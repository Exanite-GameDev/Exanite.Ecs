using System;
using System.Runtime.CompilerServices;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Worlds;

/// <summary>
/// An archetype contains all entities which share exactly the same set of components.
/// </summary>
public sealed class Archetype
{
    /// <summary>
    /// The world which this archetype belongs to.
    /// </summary>
    public EcsWorld World { get; }

    /// <summary>
    /// The components of entities in this archetype.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> Components => Info.Components;

    /// <summary>
    /// The total number of entities in this archetype.
    /// </summary>
    public int EntityCount { get; private set; }

    /// <summary>
    /// The max number of entities that can be stored in this archetype without resizing.
    /// </summary>
    public int EntityCapacity { get; private set; }

    /// <summary>
    /// All entities in this archetype.
    /// </summary>
    public ReadOnlySpan<Entity> Entities => storage.EntityColumn.AsSpan(0, EntityCount);

    /// <inheritdoc cref="ArchetypeInfo"/>
    internal readonly ArchetypeInfo Info;

    /// <inheritdoc cref="EntityStorage"/>
    private EntityStorage storage;

    internal Archetype(EcsWorld world, ImmutableOrderedListSet<ComponentId> components)
    {
        World = world;
        Info = new ArchetypeInfo(components);
        storage = new EntityStorage(in Info, EcsConstants.ArchetypeInitialCapacity);
    }

    /// <summary>
    /// Ensures that the archetype has at least the specified capacity.
    /// </summary>
    public void EnsureCapacity(int capacity)
    {
        if (storage.Capacity >= capacity)
        {
            return;
        }

        // Save old storage
        var oldStorage = storage;
        var oldRange = new EntityStorageRange(in oldStorage, 0, EntityCount);

        // Reallocate storage
        capacity = M.GetNextPowerOfTwo(capacity);
        storage = new EntityStorage(in Info, capacity);

        // Copy from old to new
        var newRange = new EntityStorageRange(in storage, 0, EntityCount);
        oldRange.CopyTo(newRange);
    }

    /// <summary>
    /// Find a chunk with space and add the given entity to it.
    /// </summary>
    /// <param name="entityId">Entity to add to a chunk</param>
    /// <param name="location">Location will be mutated to point to the new location</param>
    internal void AddEntity(EntityId entityId, ref EntityLocation location)
    {
        EnsureCapacity(EntityCount + 1);

        // Use the next free slot
        var entityIndex = EntityCount++;

        // Store the entity in this chunk
        storage.EntityColumn[entityIndex] = entityId.ToEntity(World);

        // Update the storage location to refer to this chunk
        location.IndexInArchetype = entityIndex;
        location.Archetype = this;
    }

    internal void RemoveEntity(EntityLocation location)
    {
        var currentEntityIndex = location.IndexInArchetype;
        var currentRange = new EntityStorageRange(in storage, currentEntityIndex, 1);

        // We are guaranteed to have at least 1 entity
        var lastEntityIndex = EntityCount - 1;
        var lastRange = new EntityStorageRange(in storage, currentEntityIndex, 1);

        var isSameLocation = currentEntityIndex == lastEntityIndex;
        if (!isSameLocation)
        {
            // Update location
            var lastEntity = storage.EntityColumn[lastEntityIndex];
            ref var lastLocation = ref World.Entities.GetLocation(lastEntity.EntityId);
            lastLocation.IndexInArchetype = currentEntityIndex;

            // Swap last to current
            lastRange.CopyTo(currentRange);
        }

        // Clear last
        lastRange.Clear();

        // Decrement entity count
        EntityCount--;
    }

    internal void MigrateEntity(EntityId entity, Archetype dstArchetype, ref EntityLocation location)
    {
        GuardUtility.IsFalse(dstArchetype == this, "Destination archetype is the same as the source archetype");

        // Do the actual copying
        var srcChunk = location.Archetype;
        srcChunk.MigrateTo(entity, ref location, dstArchetype);

        {
            // Copy current location so we can use it later
            var srcLocation = location;

            // Move the entity to the new archetype
            to.AddEntity(entity, ref location);
            var dstChunk = location.Archetype;

            // Copy across everything that exists in the destination archetype
            for (var i = 0; i < componentColumns.Length; i++)
            {
                var id = Info.ComponentIdByColumnIndex[i].Value;

                // Check if the component is not in the destination, in which case just don't copy it
                if (id >= dstChunk.Lookup.ColumnIndexByComponentId.Length || dstChunk.Lookup.ColumnIndexByComponentId[id] == -1)
                {
                    continue;
                }

                // Get the two arrays
                var srcArray = componentColumns[i];
                var dstArray = dstChunk.componentColumns[dstChunk.Lookup.ColumnIndexByComponentId[id]];

                // Copy!
                Array.Copy(srcArray, srcLocation.IndexInArchetype, dstArray, location.IndexInArchetype, 1);
            }

            // Remove the entity from this chunk (using the old saved location)
            RemoveEntity(srcLocation);
        }

        // Execute handler for when an entity is removed from a chunk
        EntityCount--;
    }

    internal void AddFrom(Chunk srcChunk, EntityLookup lookup)
    {
        for (var i = 0; i < srcChunk.componentColumns.Length; i++)
        {
            var srcColumn = srcChunk.componentColumns[i];
            var dstColumn = componentColumns[i];
            Array.Copy(srcColumn, dstColumn, srcChunk.EntityCount);
        }

        var world = Archetype.World;
        while (EntityCount < srcChunk.EntityCount)
        {
            // Allocate an entity id and add it to this chunk
            ref var location = ref world.Entities.AcquireId(out var entityId);
            AddEntity(entityId, ref location);

            // Add the entity pair to the lookup dictionary
            var originalEntity = srcChunk.Entities[location.IndexInArchetype];
            var newEntity = entityId.ToEntity(world);
            lookup.Add(originalEntity, newEntity);
        }
    }

    /// <summary>
    /// Destroy every entity in this archetype.
    /// </summary>
    internal void Clear()
    {
        var range = new EntityStorageRange(in storage, 0, EntityCount);
        range.Clear();

        EntityCount = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> GetSpan<T>() where T : IComponent
    {
        return GetSpan<T>(ComponentId.Get<T>());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> GetSpan<T>(ComponentId id) where T : IComponent
    {
        return ((T[])GetComponentArray(id)).AsSpan(0, EntityCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Array GetComponentArray(ComponentId id)
    {
        return storage.ComponentColumns[Info.ColumnIndexByComponentId[id.Value]];
    }

    /// <remarks>
    /// Providing the component ID can prevent repeated component ID lookups.
    /// </remarks>>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Ref<T> GetRef<T>(int entityIndex) where T : IComponent
    {
        return new Ref<T>(ref Get<T>(entityIndex));
    }

    /// <remarks>
    /// Providing the component ID can prevent repeated component ID lookups.
    /// </remarks>>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Ref<T> GetRef<T>(int entityIndex, ComponentId id) where T : IComponent
    {
        return new Ref<T>(ref Get<T>(entityIndex, id));
    }

    /// <remarks>
    /// Providing the component ID can prevent repeated component ID lookups.
    /// </remarks>>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T Get<T>(int entityIndex) where T : IComponent
    {
        return ref Get<T>(entityIndex, ComponentId.Get<T>());
    }

    /// <remarks>
    /// Providing the component ID can prevent repeated component ID lookups.
    /// </remarks>>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T Get<T>(int entityIndex, ComponentId id) where T : IComponent
    {
        return ref GetSpan<T>(id)[entityIndex];
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Info.Hash.GetHashCode();
    }
}
