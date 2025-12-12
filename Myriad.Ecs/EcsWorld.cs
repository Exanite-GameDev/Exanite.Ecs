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
public sealed class EcsWorld : ITrackedDisposable
{
    public bool IsDisposing { get; private set; }
    public bool IsDisposed { get; private set; }

    private readonly List<Archetype> archetypes = [];
    private readonly Dictionary<ArchetypeHash, List<Archetype>> archetypesByHash = [];

    // Keep track of dead entities so their ID can be re-used
    internal readonly List<EntityId> DeadEntities = [];
    private int nextEntityId = 1;

    internal readonly SegmentedList<StorageLocation> Entities = new(1024);

    /// <summary>
    /// Get a span of all archetypes in this <see cref="EcsWorld"/>
    /// </summary>
    public ReadOnlySpan<Archetype> Archetypes => archetypes.AsSpan();

    /// <summary>
    /// Get a list of all archetypes in this <see cref="EcsWorld"/>
    /// </summary>
    /// <remarks>
    /// Enumerating over this will allocate due to the List enumerator being boxed.
    /// </remarks>
    public IReadOnlyList<Archetype> ArchetypesList => archetypes;

    internal int ArchetypesCount => archetypes.Count;

    internal readonly Dictionary<QueryCacheKey, QueryDescription> QueryDescriptionCache = new();

    private readonly Pool<EcsCommandBuffer> commandBufferPool;
    private readonly HashSet<EcsCommandBuffer> activeCommandBuffers = new();

    public EventBus EventBus { get; } = new();

    public EcsWorld()
    {
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

    /// <inheritdoc/>
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposing = true;

        // Destroy all entities
        using var _ = AcquireCommandBuffer(out var commandBuffer);
        var allEntitiesQuery = new QueryBuilder().Build(this);
        commandBuffer.Destroy(allEntitiesQuery);
        commandBuffer.Execute();

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

    #region Command Buffer Pool

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

    #endregion

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

    internal bool TryGetStorageLocation(EntityId entity, out VRef<StorageLocation> storageLocation)
    {
        storageLocation = default;

        ref var location = ref Entities[entity.Id];
        if (location.Version != entity.Version)
        {
            return false;
        }

        storageLocation = new VRef<StorageLocation>(ref location);
        return true;
    }
}
