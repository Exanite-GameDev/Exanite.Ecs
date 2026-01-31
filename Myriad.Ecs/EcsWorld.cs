using System;
using System.Collections.Generic;
using Exanite.Core.Events;
using Exanite.Core.Pooling;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Queries;
using Exanite.Myriad.Ecs.Worlds;
using Exanite.Myriad.Ecs.Worlds.Archetypes;

namespace Exanite.Myriad.Ecs;

/// <summary>
/// A world contains all entities.
/// </summary>
public sealed class EcsWorld : IArchetypeCollection, ITrackedDisposable
{
    public bool IsDisposing { get; private set; }
    public bool IsDisposed { get; private set; }

    private readonly List<Archetype> archetypes = [];
    private readonly Dictionary<ArchetypeHash, List<Archetype>> archetypesByHash = [];

    internal readonly Dictionary<QueryCacheKey, QueryView> QueryViewCache = new();

    internal readonly SegmentedList<StorageLocation> Entities = new(EcsConstants.StorageLocationSegmentSize);

    // Keep track of dead entities so their ID can be re-used
    internal readonly List<EntityId> DeadEntities = [];
    private int nextEntityId = 1;

    private readonly QueryView allEntitiesQuery;

    private readonly Pool<EcsCommandBuffer> commandBufferPool;
    private readonly HashSet<EcsCommandBuffer> activeCommandBuffers = new();

    /// <summary>
    /// The archetypes stored by this world.
    /// </summary>
    public ReadOnlySpan<Archetype> Archetypes => archetypes.AsSpan();

    /// <inheritdoc cref="Archetypes"/>
    public IReadOnlyList<Archetype> ArchetypesList => archetypes;

    public EventBus EventBus { get; } = new();

    public EcsWorld()
    {
        allEntitiesQuery = new QueryFilter().Build(this);

        commandBufferPool = new Pool<EcsCommandBuffer>(
            create: () => new EcsCommandBuffer(this),
            onAcquire: commandBuffer =>
            {
                activeCommandBuffers.Add(commandBuffer);
            },
            onRelease: commandBuffer =>
            {
                commandBuffer.Clear();
                activeCommandBuffers.Remove(commandBuffer);
            });
    }

    public Pool<EcsCommandBuffer>.Handle AcquireCommandBuffer(out EcsCommandBuffer value)
    {
        return commandBufferPool.Acquire(out value);
    }

    public EcsCommandBuffer AcquireCommandBuffer()
    {
        return commandBufferPool.Acquire();
    }

    public void ReleaseCommandBuffer(EcsCommandBuffer value)
    {
        commandBufferPool.Release(value);
    }

    /// <summary>
    /// Copies all entities and their components to the destination world.
    /// All data in the target world will be overwritten.
    /// <para/>
    ///
    /// <br/>
    /// The <see cref="IComponentSelfCopied"/> event will be called for all components copied from the source world.
    /// </summary>
    public EntityLookup CopyTo(EcsWorld dstWorld)
    {
        dstWorld.Clear();

        using var _ = AcquireCommandBuffer(out var commandBuffer);
        var lookup = new Dictionary<Entity, Entity>();
        foreach (var srcArchetype in archetypes)
        {
            if (srcArchetype.EntityCount == 0)
            {
                continue;
            }

            var dstArchetype = dstWorld.GetOrCreateArchetype(srcArchetype.Components.AsComponentIdSet(), srcArchetype.Hash);
            foreach (var srcChunk in srcArchetype.Chunks)
            {
                dstArchetype.CreateChunkFrom(srcChunk, commandBuffer, lookup);
            }
        }

        // TODO: Events

        commandBuffer.Execute();

        return new EntityLookup(lookup);
    }

    /// <summary>
    /// Clears the world by destroying all entities.
    /// </summary>
    public void Clear()
    {
        using var _ = AcquireCommandBuffer(out var commandBuffer);
        commandBuffer.Destroy(allEntitiesQuery);
        commandBuffer.Execute();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposing = true;

        // Destroy all entities
        Clear();

        // Clear all active command buffers
        foreach (var activeCommandBuffer in activeCommandBuffers)
        {
            activeCommandBuffer.Clear();
        }
        activeCommandBuffers.Clear();

        // Clear event handlers
        EventBus.Dispose();

        IsDisposing = false;
        IsDisposed = true;

        GuardUtility.IsTrue(allEntitiesQuery.Count() == 0, "Expected entity count to be 0 after world disposal");
    }

    internal Archetype GetArchetype(EntityId entity)
    {
        if (entity.Id < 0 || entity.Id >= Entities.TotalCapacity)
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
        if (entityId <= 0 || entityId >= Entities.TotalCapacity)
        {
            return 0;
        }

        return Entities[entityId].Version;
    }

    /// <summary>
    /// Find an archetype with the given set of components, using a precomputed archetype hash.
    /// </summary>
    internal Archetype GetOrCreateArchetype<T>(T components, ArchetypeHash hash) where T : IComponentIdSet
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
            if (components.SetEquals(archetype.Components))
            {
                return archetype;
            }
        }

        // Didn't find one, create the new archetype
        var a = new Archetype(this, components.ToImmutableOrderedListSet());

        // Add it to the relevant lists
        archetypes.Add(a);
        candidates.Add(a);

        return a;
    }

    /// <summary>
    /// Find an archetype with the given set of components.
    /// </summary>
    internal Archetype GetOrCreateArchetype<T>(T components) where T : IComponentIdSet
    {
        return GetOrCreateArchetype(components, components.CreateArchetypeHash());
    }

    /// <inheritdoc cref="GetOrCreateArchetype{T}(T)"/>
    internal Archetype GetOrCreateArchetype(OrderedListSet<ComponentId> components)
    {
        return GetOrCreateArchetype(components.AsComponentIdSet());
    }

    internal EntityStorageLocation MigrateEntity(EntityId entity, Archetype dstArchetype)
    {
        ref var location = ref GetStorageLocation(entity);
        return location.Chunk.Archetype.MigrateTo(entity, ref location, dstArchetype);
    }

    internal ref StorageLocation AllocateEntity(out EntityId entity)
    {
        if (DeadEntities.Count > 0)
        {
            var previousId = DeadEntities[^1];
            DeadEntities.RemoveAt(DeadEntities.Count - 1);

            var version = previousId.Version + 1;

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
            if (entity.Id >= Entities.TotalCapacity)
            {
                Entities.Grow();
            }
        }

        // Update the version
        ref var location = ref Entities[entity.Id];
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
        ref var location = ref Entities[entity.Id];
        GuardUtility.IsTrue(location.Version == entity.Version, "Entity is not alive");

        return ref location;
    }

    internal bool TryGetStorageLocation(EntityId entity, out Ref<StorageLocation> storageLocation)
    {
        storageLocation = default;

        ref var location = ref Entities[entity.Id];
        if (location.Version != entity.Version)
        {
            return false;
        }

        storageLocation = new Ref<StorageLocation>(ref location);
        return true;
    }
}
