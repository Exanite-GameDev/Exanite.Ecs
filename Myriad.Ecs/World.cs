using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Exanite.Core.Pooling;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;
using Exanite.Myriad.Ecs.Worlds.Archetypes;

namespace Exanite.Myriad.Ecs;

/// <summary>
/// A world contains all entities.
/// </summary>
public sealed class World : IDisposable, IPool<EcsCommandBuffer>
{
    private readonly List<Archetype> archetypes = [];
    private readonly Dictionary<ArchetypeHash, List<Archetype>> archetypesByHash = [];

    // Keep track of dead entities so their ID can be re-used
    private readonly List<EntityId> deadEntities = [];
    private int nextEntityId = 1;

    private readonly SegmentedList<StorageLocation> entities = new(1024);

    /// <summary>
    /// Get a list of all archetypes in this <see cref="World"/>
    /// </summary>
    public IReadOnlyList<Archetype> Archetypes => archetypes;
    internal int ArchetypesCount => archetypes.Count;

    private readonly Pool<EcsCommandBuffer> commandBufferPool;

    public World()
    {
        commandBufferPool = new Pool<EcsCommandBuffer>(
            create: () => new EcsCommandBuffer(this),
            onRelease: commandBuffer => commandBuffer.Clear());
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // TODO: Make sure events are properly sent out
    }

    #region Command Buffer Pool

    public Pool<EcsCommandBuffer>.Handle Acquire(out EcsCommandBuffer value)
    {
        lock (commandBufferPool)
        {
            return commandBufferPool.Acquire(out value);
        }
    }

    public EcsCommandBuffer Acquire()
    {
        lock (commandBufferPool)
        {
            return commandBufferPool.Acquire();
        }
    }

    public void Release(EcsCommandBuffer value)
    {
        lock (commandBufferPool)
        {
            commandBufferPool.Release(value);
        }
    }

    #endregion

    internal void DeleteImmediate(EntityId delete)
    {
        // Get the EntityInfo for this entity
        ref var entityInfo = ref entities[delete.Id];

        // Check this is still a valid entity reference. Early exit if the entity
        // is already dead.
        if (entityInfo.Version != delete.Version)
        {
            return;
        }

        // Notify archetype this entity is dead
        entityInfo.Chunk.Archetype.RemoveEntity(entityInfo);

        // Increment version, this will invalid the handle
        entityInfo.Version++;

        // Store this ID for re-use later
        deadEntities.Add(delete);
    }

    internal void DeleteImmediate(Archetype archetype)
    {
        // Mark all of the IDs as dead (as long as they haven't become phantoms)
        if (archetype is { HasPhantomComponents: false, IsPhantom: false })
        {
            deadEntities.EnsureCapacity(deadEntities.Count + archetype.EntityCount);
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities.Span)
                {
                    // Get the entityinfo for this entity
                    ref var entityInfo = ref entities[entity.EntityId.Id];

                    // Increment version, this will invalidate the handle
                    entityInfo.Version++;

                    // Store this ID for re-use later
                    deadEntities.Add(entity.EntityId);
                }
            }
        }

        // Clear the archetype
        archetype.Clear();
    }

    internal Archetype GetArchetype(EntityId entity)
    {
        if (entity.Id < 0 || entity.Id >= entities.TotalCapacity)
        {
            throw new ArgumentException("Invalid entity ID", nameof(entity));
        }

        return GetStorageLocation(entity).Chunk.Archetype;
    }

    /// <summary>
    /// Get the current version for a given entity ID
    /// </summary>
    /// <returns>The entity ID, or zero if the entity does not exist</returns>
    internal uint GetVersion(int entityId)
    {
        if (entityId <= 0 || entityId >= entities.TotalCapacity)
        {
            return 0;
        }

        return entities[entityId].Version;
    }

    #region Get/Create Archetype

    /// <summary>
    /// Find an archetype with the given set of components, using a precomputed archetype hash.
    /// </summary>
    internal Archetype GetOrCreateArchetype(OrderedListSet<ComponentId> components, ArchetypeHash hash)
    {
        // Get list of all archetypes with this hash
        if (!archetypesByHash.TryGetValue(hash, out var candidates))
        {
            candidates = [];
            archetypesByHash.Add(hash, candidates);
        }

        // Check if any of the candidates are the one we need
        foreach (var archetype in candidates)
        {
            if (archetype.SetEquals(components))
            {
                return archetype;
            }
        }

        // Didn't find one, create the new archetype
        var a = new Archetype(this, ImmutableOrderedListSet<ComponentId>.Create(components));

        // Add it to the relevant lists
        archetypes.Add(a);
        candidates.Add(a);

        return a;
    }

    internal Archetype GetOrCreateArchetype<TV>(Dictionary<ComponentId, TV> components, ArchetypeHash hash)
    {
        // Get list of all archetypes with this hash
        if (!archetypesByHash.TryGetValue(hash, out var candidates))
        {
            candidates = [];
            archetypesByHash.Add(hash, candidates);
        }

        // Check if any of the candidates are the one we need
        foreach (var archetype in candidates)
        {
            if (archetype.SetEquals(components))
            {
                return archetype;
            }
        }

        // Didn't find one, create the new archetype
        var set = ImmutableOrderedListSet<ComponentId>.Create(components);
        var a = new Archetype(this, set);

        // Add it to the relevant lists
        archetypes.Add(a);
        candidates.Add(a);

        return a;
    }

    internal Archetype GetOrCreateArchetype(OrderedListSet<ComponentId> components)
    {
        return GetOrCreateArchetype(components, ArchetypeHash.Create(components));
    }

    internal Archetype GetOrCreateArchetype<TV>(Dictionary<ComponentId, TV> components)
    {
        return GetOrCreateArchetype(components, ArchetypeHash.Create(components));
    }

    #endregion

    internal EntityStorageLocation MigrateEntity(EntityId entity, Archetype dstArchetype)
    {
        ref var location = ref GetStorageLocation(entity);
        return location.Chunk.Archetype.MigrateTo(entity, ref location, dstArchetype);
    }

    internal ref StorageLocation AllocateEntity(out EntityId entity)
    {
        if (deadEntities.Count > 0)
        {
            var previousId = deadEntities[^1];
            deadEntities.RemoveAt(deadEntities.Count - 1);

            var version = unchecked(previousId.Version + 1);

            // Ensure ID 0 is not assigned even after wrapping around 2^32 entities
            if (version == 0)
            {
                version += 1;
            }

            entity = new EntityId(previousId.Id, version);
        }
        else
        {
            // Allocate a new ID. This **must not** overflow!
            entity = new EntityId(checked(nextEntityId++), 1);

            // Check if the collection of all entities needs to grow
            if (entity.Id >= entities.TotalCapacity)
            {
                entities.Grow();
            }
        }

        // Update the version
        ref var location = ref entities[entity.Id];
        location.Version = entity.Version;

        return ref location;
    }

    internal EntityStorageLocation GetEntityStorageLocation(EntityId entity)
    {
        var location = GetStorageLocation(entity);
        return new EntityStorageLocation(entity, location.IndexInChunk, location.Chunk);
    }

    internal ref StorageLocation GetStorageLocation(EntityId entity)
    {
        ref var location = ref entities[entity.Id];

        if (location.Version != entity.Version)
        {
            throw new ArgumentException("Entity is not alive", nameof(entity));
        }

        return ref location;
    }
}
