using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Myriad.Ecs.Collections;
using Myriad.Ecs.Command;
using Myriad.Ecs.ComponentIds;
using Myriad.Ecs.Worlds.Archetypes;

namespace Myriad.Ecs.Worlds;

/// <summary>
/// A world contains all entities.
/// </summary>
public sealed class World : IDisposable
{
    private readonly List<Archetype> archetypes = [];
    private readonly Dictionary<ArchetypeHash, List<Archetype>> archetypesByHash = [];

    // Keep track of dead entities so their ID can be re-used
    private readonly List<EntityId> deadEntities = [];
    private int nextEntityId = 1;

    private readonly SegmentedList<EntityInfo> entities = new(1024);

    /// <summary>
    /// Get a list of all archetypes in this <see cref="World"/>
    /// </summary>
    public IReadOnlyList<Archetype> Archetypes => archetypes;
    internal int ArchetypesCount => archetypes.Count;

    private readonly ConcurrentBag<EcsCommandBuffer> commandBufferPool = [];

    /// <inheritdoc/>
    public void Dispose()
    {
        // TODO: Make sure events are properly sent out
    }

    #region command buffer pool
    /// <summary>
    /// Get a <see cref="EcsCommandBuffer"/> from the pool or create a new one
    /// </summary>
    /// <returns></returns>
    public EcsCommandBuffer GetCommandBuffer()
    {
        if (!commandBufferPool.TryTake(out var buffer))
        {
            buffer = new EcsCommandBuffer(this);
        }

        return buffer;
    }

    /// <summary>
    /// Return a <see cref="EcsCommandBuffer"/> to the internal pool
    /// </summary>
    /// <param name="buffer"></param>
    public void ReturnCommandBuffer(EcsCommandBuffer buffer)
    {
        if (commandBufferPool.Count < 32)
        {
            buffer.Clear();
            commandBufferPool.Add(buffer);
        }
    }
    #endregion

    internal void DeleteImmediate(EntityId delete, ref LazyCommandBuffer lazy)
    {
        // Get the entityinfo for this entity
        ref var entityInfo = ref entities[delete.Id];

        // Check this is still a valid entity reference. Early exit if the entity
        // is already dead.
        if (entityInfo.Version != delete.Version)
        {
            return;
        }

        // Notify archetype this entity is dead
        entityInfo.Chunk.Archetype.RemoveEntity(entityInfo, ref lazy);

        // Increment version, this will invalid the handle
        entityInfo.Version++;

        // Store this ID for re-use later
        deadEntities.Add(delete);
    }

    internal void DeleteImmediate(Archetype archetype, ref LazyCommandBuffer lazy)
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
                    ref var entityInfo = ref entities[entity.Id.Id];

                    // Increment version, this will invalidate the handle
                    entityInfo.Version++;

                    // Store this ID for re-use later
                    deadEntities.Add(entity.Id);
                }
            }
        }

        // Clear the archetype
        archetype.Clear(ref lazy);
    }

    internal Archetype GetArchetype(EntityId entity)
    {
        if (entity.Id < 0 || entity.Id >= entities.TotalCapacity)
        {
            throw new ArgumentException("Invalid entity ID", nameof(entity));
        }

        return GetEntityInfo(entity).Chunk.Archetype;
    }

    /// <summary>
    /// Get the current version for a given entity ID
    /// </summary>
    /// <param name="entityId"></param>
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
    /// <param name="components"></param>
    /// <param name="hash"></param>
    /// <returns></returns>
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
            if (archetype.SetEquals(components))
            {
                return archetype;
            }

        // Didn't find one, create the new archetype
        var a = new Archetype(this, FrozenOrderedListSet<ComponentId>.Create(components));

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
            if (archetype.SetEquals(components))
            {
                return archetype;
            }

        // Didn't find one, create the new archetype
        var set = FrozenOrderedListSet<ComponentId>.Create(components);
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

    internal Row MigrateEntity(EntityId entity, Archetype to, ref LazyCommandBuffer lazy)
    {
        ref var info = ref GetEntityInfo(entity);
        return info.Chunk.Archetype.MigrateTo(entity, ref info, to, ref lazy);
    }

    internal ref EntityInfo AllocateEntity(out EntityId entity)
    {
        if (deadEntities.Count > 0)
        {
            var prev = deadEntities[^1];
            deadEntities.RemoveAt(deadEntities.Count - 1);

            var v = unchecked(prev.Version + 1);

            // Ensure ID 0 is not assigned even after wrapping around 2^32 entities
            if (v == 0)
            {
                v += 1;
            }

            entity = new EntityId(prev.Id, v);
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
        ref var slot = ref entities[entity.Id];
        slot.Version = entity.Version;

        return ref slot;
    }

    internal Row GetRow(EntityId entity)
    {
        var info = GetEntityInfo(entity);
        return new Row(entity, info.RowIndex, info.Chunk);
    }

    internal ref EntityInfo GetEntityInfo(EntityId entity)
    {
        ref var info = ref entities[entity.Id];

        if (info.Version != entity.Version)
        {
            throw new ArgumentException("entity is not alive", nameof(entity));
        }

        return ref info;
    }

    /// <summary>
    /// Get the info for the given entity, if the entity is dead returns a reference to dummy
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dummy"></param>
    /// <param name="isDummy"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    internal ref EntityInfo GetEntityInfo(EntityId entity, ref EntityInfo dummy, out bool isDummy)
    {
        if (entity.Id <= 0 || entity.Id >= entities.TotalCapacity)
        {
            isDummy = true;
            return ref dummy;
        }

        ref var info = ref entities[entity.Id];
        if (info.Version != entity.Version)
        {
            isDummy = true;
            return ref dummy;
        }

        isDummy = false;
        return ref info;
    }
}
