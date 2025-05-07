using System;
using System.Collections.Generic;
using System.Diagnostics;
using Myriad.Ecs.Allocations;
using Myriad.Ecs.Collections;
using Myriad.Ecs.Components;
using Myriad.Ecs.Worlds.Archetypes;

namespace Myriad.Ecs.Worlds.Chunks;

public sealed class Chunk
{
    /// <summary>
    /// The archetype which contains this chunk
    /// </summary>
    public Archetype Archetype { get; }

    // Map from component ID (index) to index in chunk
    private readonly int[] componentIndexLookup;

    /// <summary>
    /// Map from index to component ID
    /// </summary>
    private readonly IReadOnlyList<ComponentId> componentIdLookup;

    private readonly Entity[] entities;
    private readonly Array[] components;

    /// <summary>
    /// Get the number of entities currently in this chunk
    /// </summary>
    public int EntityCount { get; private set; }

    /// <summary>
    /// Get all of the entities in this chunk
    /// </summary>
    public ReadOnlyMemory<Entity> Entities => entities.AsMemory(0, EntityCount);

    internal Chunk(Archetype archetype, int size, int[] componentIndexLookup, IReadOnlyList<Type> componentTypes, IReadOnlyList<ComponentId> ids)
    {
        Archetype = archetype;
        this.componentIndexLookup = componentIndexLookup;
        entities = new Entity[size];
        componentIdLookup = ids;

        components = new Array[componentTypes.Count];
        for (var i = 0; i < components.Length; i++)
            components[i] = ArrayFactory.Create(componentTypes[i], size);
    }

    #region get component
    internal ref T Get<T>(EntityId entityId, int rowIndex) where T : IComponent
    {
        Debug.Assert(entities[rowIndex].EntityId == entityId, "Mismatched entities in chunk");
        return ref Get<T>(rowIndex);
    }

    internal Ref<T> GetRef<T>(EntityId entityId, int rowIndex) where T : IComponent
    {
        Debug.Assert(entities[rowIndex].EntityId == entityId, "Mismatched entities in chunk");

        return new Ref<T>(ref Get<T>(rowIndex));
    }

    internal ref T Get<T>(int rowIndex) where T : IComponent
    {
        return ref Get<T>(rowIndex, ComponentId.Get<T>());
    }

    internal ref T Get<T>(int rowIndex, ComponentId id) where T : IComponent
    {
        return ref GetSpan<T>(id)[rowIndex];
    }

    public Span<T> GetSpan<T>() where T : IComponent
    {
        return GetSpan<T>(ComponentId.Get<T>());
    }

    public Span<T> GetSpan<T>(ComponentId id) where T : IComponent
    {
        return GetComponentArray<T>(id).AsSpan(0, EntityCount);
    }

    internal T[] GetComponentArray<T>() where T : IComponent
    {
        return GetComponentArray<T>(ComponentId.Get<T>());
    }

    /// <summary>
    /// Get the component array, providing the component ID if it is known.
    /// </summary>
    internal T[] GetComponentArray<T>(ComponentId id) where T : IComponent
    {
        return (GetComponentArray(id) as T[])!;
    }

    internal Array GetComponentArray(ComponentId id)
    {
        return components[componentIndexLookup[id.Value]];
    }
    #endregion

    #region add/remove entity
    // Note that these must be called only from Archetype! The Archetype needs to do some bookeeping on create/destroy.

    internal void Clear()
    {
        Debug.Assert(!Archetype.HasPhantomComponents);

        // Clear out the components. This prevents chunks holding
        // onto references to dead managed components, and keeping them in memory.
        foreach (var component in components)
        {
            Array.Clear(component, 0, component.Length);
        }

        // Not strictly necessary, clean up all the IDs so they're default instead of some invalid value.
        Array.Clear(entities, 0, entities.Length);

        EntityCount = 0;
    }

    internal Row AddEntity(EntityId entity, ref EntityInfo info)
    {
        // It is safe to only debug assert here. It should never happen if Myriad is working
        // correctly. If it does somehow go wrong you'll get an index out of range exception
        // below so it still fails in a sensible way.
        Debug.Assert(EntityCount < entities.Length, "Cannot add entity to full chunk");

        // Use the next free slot
        var index = EntityCount++;

        // Occupy this row
        entities[index] = entity.ToEntity(Archetype.World);

        // Update global entity info to refer to this location
        info.RowIndex = index;
        info.Chunk = this;

        return new Row(entity, index, this);
    }

    internal void RemoveEntity(EntityInfo info)
    {
        var index = info.RowIndex;

        // Clear out the components. This prevents chunks holding
        // onto references to dead managed components, and keeping them in memory.
        foreach (var component in components)
        {
            Array.Clear(component, index, 1);
        }

        // No work to do if there are no other entities
        EntityCount -= 1;
        if (EntityCount == 0)
        {
            entities[index] = default;
            return;
        }

        // If we did not just delete the top entity into place then swap the top
        // entity down into this slot to keep the chunk continuous.
        if (index != EntityCount)
        {
            var lastEntity = entities[EntityCount];
            var lastEntityIndex = EntityCount;
            ref var lastInfo = ref Archetype.World.GetEntityInfo(lastEntity.EntityId);
            entities[index] = lastEntity;
            entities[lastEntityIndex] = default;
            lastInfo.RowIndex = index;

            // Copy top entity components into place
            foreach (var component in components)
            {
                Array.Copy(component, lastEntityIndex, component, index, 1);

                // Clear out the components we just moved. This prevents chunks holding
                // onto references to dead managed components, and keeping them in memory.
                Array.Clear(component, lastEntityIndex, 1);
            }
        }
    }

    internal Row MigrateTo(EntityId entity, ref EntityInfo info, Archetype to)
    {
        // Copy current entity info so we can use it later
        var oldInfo = info;

        // Get a reference to the row currently storing this entity
        var srcRow = info.GetRow(entity);

        // Move the entity to the new archetype
        var destRow = to.AddEntity(entity, ref info);
        var destChunk = destRow.Chunk;

        // Copy across everything that exists in the destination archetype
        for (var i = 0; i < components.Length; i++)
        {
            var id = componentIdLookup[i].Value;

            // Check if the component is not in the destination, in which case just don't copy it
            if (id >= destChunk.componentIndexLookup.Length || destChunk.componentIndexLookup[id] == -1)
            {
                continue;
            }

            // Get the two arrays
            var srcArr = components[i];
            var destArr = destChunk.components[destChunk.componentIndexLookup[id]];

            // Copy!
            Array.Copy(srcArr, srcRow.RowIndex, destArr, destRow.RowIndex, 1);
        }

        // Remove the entity from this chunk (using the old saved info)
        RemoveEntity(oldInfo);

        return destRow;
    }
    #endregion
}
