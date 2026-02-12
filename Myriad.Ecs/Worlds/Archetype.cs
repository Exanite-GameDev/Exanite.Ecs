using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Exanite.Core.Pooling;
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
    /// <inheritdoc cref="EntityStorage"/>
    internal EntityStorage Storage;

    /// <inheritdoc cref="ArchetypeInfo"/>
    internal ArchetypeInfo Info;

    /// <summary>
    /// The total number of entities in this archetype.
    /// </summary>
    internal int EntityCount;

    internal readonly int Id;

    /// <summary>
    /// The world which this archetype belongs to.
    /// </summary>
    public readonly EcsWorld World;

    /// <inheritdoc cref="ArchetypeInfo.Types"/>
    public ImmutableOrderedListSet<TypeId> Types => Info.Types;

    /// <inheritdoc cref="ArchetypeInfo.Components"/>
    public ImmutableOrderedListSet<ComponentId> Components => Info.Components;

    /// <inheritdoc cref="ArchetypeInfo.Interfaces"/>
    public ImmutableOrderedListSet<InterfaceId> Interfaces => Info.Interfaces;

    /// <summary>
    /// All entities in this archetype.
    /// </summary>
    public ReadOnlySpan<Entity> Entities => Storage.EntityColumn.AsSpan(0, EntityCount);

    /// <summary>
    /// The max number of entities that can be stored in this archetype without resizing.
    /// </summary>
    public int Capacity => Storage.Capacity;

    internal Archetype(int id, EcsWorld world, ImmutableOrderedListSet<ComponentId> components)
    {
        Id = id;
        World = world;

        // Build initial archetype info
        Info = new ArchetypeInfo(components);

        // Allocate storage
        Storage = new EntityStorage(in Info, EcsConstants.ArchetypeInitialCapacity);

        // Resolve interfaces
        UpdateInterfaceComponentResolutions();
    }

    internal void UpdateInterfaceComponentResolutions()
    {
        // Resolve interface components
        // Iterate forwards and provide references to previous resolvers to allow for overriding functionality
        using var __ = DictionaryPool<InterfaceId, object?>.Acquire(out var interfaceComponents);
        foreach (var registration in World.InterfaceResolvers)
        {
            if (registration.Filter.Build(World).IsMatch(Info.Types, in Info.BloomFilter))
            {
                ref var current = ref CollectionsMarshal.GetValueRefOrAddDefault(interfaceComponents, registration.Id, out _);
                current = registration.Factory.Invoke(current, Components);
            }
        }

        // Remove null interface components
        // Interface overriding allows for complete removal of the interface
        using var ___ = ListPool<InterfaceId>.Acquire(out var interfaceIdsToRemove);
        foreach (var (interfaceId, instance) in interfaceComponents)
        {
            if (instance == null)
            {
                interfaceIdsToRemove.Add(interfaceId);
            }
        }

        foreach (var interfaceId in interfaceIdsToRemove)
        {
            interfaceComponents.Remove(interfaceId);
        }

        // Build final archetype info
        Info = new ArchetypeInfo(Info, interfaceComponents!);
    }

    /// <summary>
    /// Ensures that the archetype has at least the specified capacity.
    /// </summary>
    public void EnsureCapacity(int capacity)
    {
        if (Storage.Capacity >= capacity)
        {
            return;
        }

        // Save old storage
        var oldStorage = Storage;
        var oldRange = new EntityStorageRange(in oldStorage, 0, EntityCount);

        // Reallocate storage
        capacity = M.GetNextPowerOfTwo(capacity);
        Storage = new EntityStorage(in Info, capacity);

        // Copy from old to new
        var newRange = new EntityStorageRange(in Storage, 0, EntityCount);
        oldRange.CopyAllTo(newRange);
    }

    internal void AddEntity(EntityId entityId, ref EntityLocation location)
    {
        EnsureCapacity(EntityCount + 1);

        // Use the next free slot
        var entityIndex = EntityCount++;

        // Store the entity in this archetype
        Storage.EntityColumn[entityIndex] = entityId.ToEntity(World);

        // Update the storage location to refer to this archetype
        location.IndexInArchetype = entityIndex;
        location.Archetype = this;
    }

    internal void RemoveEntity(EntityLocation location)
    {
        var currentEntityIndex = location.IndexInArchetype;
        var currentRange = new EntityStorageRange(in Storage, currentEntityIndex, 1);

        // We are guaranteed to have at least 1 entity
        var lastEntityIndex = EntityCount - 1;
        var lastRange = new EntityStorageRange(in Storage, lastEntityIndex, 1);

        var isSameLocation = currentEntityIndex == lastEntityIndex;
        if (!isSameLocation)
        {
            // Update location
            var lastEntity = Storage.EntityColumn[lastEntityIndex];
            ref var lastLocation = ref World.Entities.GetLocation(lastEntity.EntityId);
            lastLocation.IndexInArchetype = currentEntityIndex;

            // Swap last to current
            lastRange.CopyAllTo(currentRange);
        }

        // Clear last
        lastRange.Clear();

        // Update entity count
        EntityCount--;
    }

    internal void MigrateEntity(EntityId entity, Archetype dstArchetype, ref EntityLocation location)
    {
        GuardUtility.IsFalse(dstArchetype == this, "Destination archetype is the same as the source archetype");

        // Copy current location so we can use it later
        var srcLocation = location;

        // Move the entity to the new archetype
        dstArchetype.AddEntity(entity, ref location);

        // Copy across everything that exists in the destination archetype
        for (var i = 0; i < Storage.ComponentColumns.Length; i++)
        {
            var componentId = Info.ComponentIdByColumnIndex[i].Value;

            // Skip if the target archetype does not have this component
            if (componentId >= dstArchetype.Info.ColumnIndexByComponentId.Length || dstArchetype.Info.ColumnIndexByComponentId[componentId] == -1)
            {
                continue;
            }

            // Copy from source archetype to destination
            var srcArray = Storage.ComponentColumns[i];
            var dstArray = dstArchetype.Storage.ComponentColumns[dstArchetype.Info.ColumnIndexByComponentId[componentId]];
            Array.Copy(srcArray, srcLocation.IndexInArchetype, dstArray, location.IndexInArchetype, 1);
        }

        // Remove the entity from this archetype (using the old saved location)
        RemoveEntity(srcLocation);
    }

    /// <summary>
    /// Copies the component data from the source archetype as new entities.
    /// </summary>
    internal void AddFrom(Archetype srcArchetype, EntityLookup lookup)
    {
        var srcEntityCount = srcArchetype.EntityCount;
        EnsureCapacity(EntityCount + srcEntityCount);
        lookup.Entries.EnsureCapacity(lookup.Entries.Count + srcEntityCount);

        // Copy component data
        var srcRange = new EntityStorageRange(in srcArchetype.Storage, 0, srcEntityCount);
        var dstRange = new EntityStorageRange(in Storage, EntityCount, srcEntityCount);
        srcRange.CopyComponentsTo(dstRange);

        // Allocate new entity ids
        for (var i = 0; i < srcEntityCount; i++)
        {
            // Allocate an entity id and point it to this archetype
            ref var location = ref World.Entities.AcquireId(out var entityId);
            location.IndexInArchetype = dstRange.StartIndex + i;
            location.Archetype = this;

            // Store the entity in this archetype
            Storage.EntityColumn[dstRange.StartIndex + i] = entityId.ToEntity(World);

            // Add the entity pair to the lookup dictionary
            var originalEntity = srcArchetype.Entities[i];
            var newEntity = entityId.ToEntity(World);
            lookup.Add(originalEntity, newEntity);
        }

        // Update entity count
        EntityCount += srcEntityCount;
    }

    /// <summary>
    /// Destroy every entity in this archetype.
    /// </summary>
    internal void Clear()
    {
        var range = new EntityStorageRange(in Storage, 0, EntityCount);
        range.Clear();

        EntityCount = 0;
    }

    /// <summary>
    /// Resolves the specified interface component from this archetype.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Resolve<T>() where T : class, IInterfaceComponent
    {
        var interfaceId = InterfaceId.Get<T>();
        var interfaceIndex = ~interfaceId.Value;
        var interfaceInstance = Info.InterfaceByInterfaceId[interfaceIndex];

        GuardUtility.IsTrue(interfaceInstance != null, "Archetype does not have the specified interface component");
        return Unsafe.As<object, T>(ref interfaceInstance);
    }

    /// <summary>
    /// Tries to resolve the specified interface component from this archetype.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolve<T>([NotNullWhen(true)] out T? instance) where T : class, IInterfaceComponent
    {
        var interfaceId = InterfaceId.Get<T>();
        var interfaceIndex = ~interfaceId.Value;
        if ((uint)interfaceIndex >= Info.InterfaceByInterfaceId.Length)
        {
            instance = null;
            return false;
        }

        var interfaceInstance = Info.InterfaceByInterfaceId[interfaceIndex];
        if (interfaceInstance == null)
        {
            instance = null;
            return false;
        }

        instance = Unsafe.As<object, T>(ref interfaceInstance);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> GetSpan<T>() where T : IComponent
    {
        // Unsafe.As() is safe because we have to get the column index using the component id
        // If the component id is invalid, then we will get an index out of bounds
        var componentId = ComponentId.Get<T>();
        var array = GetComponentArray(componentId);
        return Unsafe.As<Array, T[]>(ref array).AsSpan(0, EntityCount);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Array GetComponentArray(ComponentId id)
    {
        return Storage.ComponentColumns[Info.ColumnIndexByComponentId[id.Value]];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ref T Get<T>(int entityIndex) where T : IComponent
    {
        return ref GetSpan<T>()[entityIndex];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Ref<T> GetRef<T>(int entityIndex) where T : IComponent
    {
        return new Ref<T>(ref Get<T>(entityIndex));
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Info.Hash.GetHashCode();
    }
}
