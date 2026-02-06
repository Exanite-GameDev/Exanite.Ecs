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
    public ImmutableOrderedListSet<ComponentId> Components { get; }

    /// <inheritdoc cref="ArchetypeInfo"/>
    internal readonly ArchetypeInfo Info;

    /// <remarks>
    /// Indexed using entity index.
    /// </remarks>>
    private readonly Entity[] entityColumn;

    /// <remarks>
    /// Indexed using column index, then entity index.
    /// </remarks>>
    private readonly Array[] componentColumns;

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
    public ReadOnlySpan<Entity> Entities => entityColumn.AsSpan(0, EntityCount);

    internal Archetype(EcsWorld world, ImmutableOrderedListSet<ComponentId> components)
    {
        World = world;
        Components = components;

        // Initialize component lookup
        Info = new ArchetypeInfo(components);

        // Allocate initial storage arrays
        EntityCapacity = EcsConstants.ArchetypeInitialCapacity;

        entityColumn = new Entity[EntityCapacity];
        componentColumns = new Array[Info.ComponentIdByColumnIndex.Length];

        var arrayFactory = new ArrayFactory();
        for (var i = 0; i < componentColumns.Length; i++)
        {
            var dispatcher = Info.ComponentDispatcherByComponentId[Info.ComponentIdByColumnIndex[i].Value];
            componentColumns[i] = dispatcher.Create<ArrayFactory, int, Array>(arrayFactory, EntityCapacity);
        }
    }

    /// <summary>
    /// Find a chunk with space and add the given entity to it.
    /// </summary>
    /// <param name="entity">Entity to add to a chunk</param>
    /// <param name="location">Location will be mutated to point to the new location</param>
    internal void AddEntity(EntityId entity, ref EntityLocation location)
    {
        EntityCount++;

        var chunk = GetChunkWithSpace();
        chunk.AddEntity(entity, ref location);
    }

    internal void RemoveEntity(EntityLocation location)
    {
        // Remove the entity from the chunk, component data is lost after this point
        location.Chunk.RemoveEntity(location);

        // Execute handler for when an entity is removed from a chunk
        OnChunkEntityRemoved();
    }

    internal void MigrateEntity(EntityId entity, Archetype dstArchetype, ref EntityLocation location)
    {
        GuardUtility.IsFalse(dstArchetype == this, "Destination archetype is the same as the source archetype");

        // Do the actual copying
        var srcChunk = location.Chunk;
        srcChunk.MigrateTo(entity, ref location, dstArchetype);

        // Execute handler for when an entity is removed from a chunk
        OnChunkEntityRemoved();
    }

    /// <summary>
    /// Copies the entities from the source chunk to a new chunk in this archetype.
    /// The source chunk must have the same component set.
    /// </summary>
    /// <remarks>
    /// This is designed to be called by <see cref="EcsWorld.AddTo(EcsWorld, IArchetypeView)"/>.
    /// </remarks>
    internal Chunk CreateChunkFrom(Chunk srcChunk, EntityLookup lookup)
    {
        EntityCount += srcChunk.Entities.Length;

        var newChunk = GetEmptyChunk();
        newChunk.CopyFrom(srcChunk, lookup);

        return newChunk;
    }

    /// <summary>
    /// Compacts the chunk contained in this archetype,
    /// ensuring that, at most, only the last chunk is left partially filled.
    /// </summary>
    internal void Compact()
    {
        if (chunksList.Count <= 1)
        {
            return;
        }

        var srcIndex = chunksList.Count - 1;
        var dstIndex = 0;

        while (dstIndex < srcIndex)
        {
            var dst = chunksList[dstIndex];
            var src = chunksList[srcIndex];

            if (dst.IsFull)
            {
                dstIndex++;
                continue;
            }

            src.CompactInto(dst);

            if (dst.IsFull)
            {
                dstIndex++;
            }

            if (src.IsEmpty)
            {
                // Add chunk back to pool if needed
                if (spareChunks.Count < EcsConstants.ChunkHotSpareCount)
                {
                    spareChunks.Push(src);
                }

                chunksList.RemoveAt(srcIndex);
                srcIndex--;
            }
        }
    }

    /// <summary>
    /// Destroy every entity in this archetype
    /// </summary>
    internal void Clear()
    {
        // Clear all the chunks
        foreach (var chunk in chunksList)
        {
            chunk.Clear();
        }

        // Move some chunks to hot spares and then destroy the rest
        foreach (var chunk in chunksList)
        {
            if (spareChunks.Count < EcsConstants.ChunkHotSpareCount)
            {
                spareChunks.Push(chunk);
            }
            else
            {
                break;
            }
        }
        chunksList.Clear();

        // Done! No entities left.
        EntityCount = 0;
    }

    /// <summary>
    /// Returns a chunk that is not full.
    /// </summary>
    private Chunk GetChunkWithSpace()
    {
        if (chunksList.Count > 0)
        {
            var lastChunk = chunksList[^1];
            if (!lastChunk.IsFull)
            {
                return lastChunk;
            }
        }

        return GetEmptyChunk();
    }

    /// <summary>
    /// Returns a chunk that is completely empty.
    /// </summary>
    private Chunk GetEmptyChunk()
    {
        var newChunk = spareChunks.Count > 0 ? spareChunks.Pop() : new Chunk(this);
        chunksList.Add(newChunk);

        return newChunk;
    }

    private void OnChunkEntityRemoved()
    {
        // Decrease archetype entity count
        EntityCount--;

        // Check if last chunk is empty
        var lastChunk = chunksList[^1];
        if (lastChunk.IsEmpty)
        {
            chunksList.RemoveAt(chunksList.Count - 1);
            if (spareChunks.Count < EcsConstants.ChunkHotSpareCount)
            {
                spareChunks.Push(lastChunk);
            }
        }
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Hash.GetHashCode();
    }

        internal Chunk(Archetype archetype)
    {
        Archetype = archetype;
        Info = archetype.Info;

        entityColumn = new Entity[EcsConstants.ChunkEntityCount];
        componentColumns = new Array[Info.ComponentIdByColumnIndex.Length];
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
        return componentColumns[Info.ColumnIndexByComponentId[id.Value]];
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

    /// <remarks>
    /// Must only be called by <see cref="Archetype"/> because <see cref="Archetype"/> needs to update its internal state.
    /// </remarks>
    internal void Clear()
    {
        // Clear out the components. This prevents chunks holding
        // onto references to dead managed components, and keeping them in memory.
        foreach (var componentColumn in componentColumns)
        {
            Array.Clear(componentColumn, 0, componentColumn.Length);
        }

        // Not strictly necessary, clean up all the IDs so they're default instead of some invalid value.
        Array.Clear(entityColumn, 0, entityColumn.Length);

        EntityCount = 0;
    }

    /// <remarks>
    /// Must only be called by <see cref="Archetype"/> because <see cref="Archetype"/> needs to update its internal state.
    /// </remarks>
    internal void CopyFrom(Chunk srcChunk, EntityLookup lookup)
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
            var originalEntity = srcChunk.Entities[location.IndexInChunk];
            var newEntity = entityId.ToEntity(world);
            lookup.Add(originalEntity, newEntity);
        }
    }

    /// <remarks>
    /// Must only be called by <see cref="Archetype"/> because <see cref="Archetype"/> needs to update its internal state.
    /// </remarks>
    internal void CompactInto(Chunk dstChunk)
    {
        GuardUtility.IsTrue(dstChunk.Archetype == Archetype, "Internal: Cannot compact chunks that have different archetypes");

        var availableSpace = EcsConstants.ChunkEntityCount - dstChunk.EntityCount;
        var copyCount = int.Min(availableSpace, EntityCount);
        var srcIndex = EntityCount - copyCount;
        var dstIndex = dstChunk.EntityCount;

        // Move components
        for (var columnIndex = 0; columnIndex < componentColumns.Length; columnIndex++)
        {
            var srcComponentColumn = componentColumns[columnIndex];
            var dstComponentColumn = dstChunk.componentColumns[columnIndex];

            Array.Copy(srcComponentColumn, srcIndex, dstComponentColumn, dstIndex, copyCount);
            Array.Clear(srcComponentColumn, srcIndex, copyCount);
        }

        // Move entities
        // Clear is not strictly necessary since the only reference is to the world object, but keeps things clean
        Array.Copy(entityColumn, srcIndex, dstChunk.entityColumn, dstIndex, copyCount);
        Array.Clear(entityColumn, srcIndex, copyCount);

        // Update entity locations
        var world = Archetype.World;
        for (var i = dstIndex; i < dstIndex + copyCount; i++)
        {
            ref var location = ref world.Entities.GetLocation(dstChunk.entityColumn[i].Index);
            location.Chunk = dstChunk;
            location.IndexInChunk = i;
        }

        EntityCount -= copyCount;
        dstChunk.EntityCount += copyCount;
    }

    /// <remarks>
    /// Must only be called by <see cref="Archetype"/> because <see cref="Archetype"/> needs to update its internal state.
    /// </remarks>
    internal void AddEntity(EntityId entityId, ref EntityLocation location)
    {
        // It is safe to only assert here. It should never happen if Myriad is working
        // correctly. If it does somehow go wrong you'll get an index out of range exception
        // below so it still fails in a sensible way.
        AssertUtility.IsTrue(EntityCount < entityColumn.Length, "Cannot add entity to full chunk");

        // Use the next free slot
        var entityIndex = EntityCount++;

        // Store the entity in this chunk
        entityColumn[entityIndex] = entityId.ToEntity(Archetype.World);

        // Update the storage location to refer to this chunk
        location.IndexInChunk = entityIndex;
        location.Chunk = this;
    }

    /// <remarks>
    /// Must only be called by <see cref="Archetype"/> because <see cref="Archetype"/> needs to update its internal state.
    /// </remarks>
    internal void RemoveEntity(EntityLocation location)
    {
        var entityIndexInChunk = location.IndexInChunk;

        // We are guaranteed to have at least 1 chunk and 1 entity
        var lastChunk = Archetype.Chunks[^1];
        var lastEntityIndexInChunk = lastChunk.EntityCount - 1;

        var isSameLocation = this == lastChunk && entityIndexInChunk == lastEntityIndexInChunk;
        if (!isSameLocation)
        {
            // Swap first
            var lastEntity = lastChunk.entityColumn[lastEntityIndexInChunk];

            // Update location
            ref var lastLocation = ref Archetype.World.Entities.GetLocation(lastEntity.EntityId);
            lastLocation.Chunk = this;
            lastLocation.IndexInChunk = entityIndexInChunk;

            // Copy entity
            entityColumn[entityIndexInChunk] = lastEntity;

            // Copy components
            for (var i = 0; i < componentColumns.Length; i++)
            {
                Array.Copy(lastChunk.componentColumns[i], lastEntityIndexInChunk, componentColumns[i], entityIndexInChunk, 1);
            }
        }

        // Clear last
        {
            // Clear entity
            lastChunk.entityColumn[lastEntityIndexInChunk] = default;

            // Clear components
            foreach (var componentColumn in lastChunk.componentColumns)
            {
                Array.Clear(componentColumn, lastEntityIndexInChunk, 1);
            }

            // Decrement entity count
            lastChunk.EntityCount--;
        }
    }

    /// <remarks>
    /// Must only be called by <see cref="Archetype"/> because <see cref="Archetype"/> needs to update its internal state.
    /// </remarks>
    internal void MigrateTo(EntityId entity, ref EntityLocation location, Archetype to)
    {
        // Copy current location so we can use it later
        var srcLocation = location;

        // Move the entity to the new archetype
        to.AddEntity(entity, ref location);
        var dstChunk = location.Chunk;

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
            Array.Copy(srcArray, srcLocation.IndexInChunk, dstArray, location.IndexInChunk, 1);
        }

        // Remove the entity from this chunk (using the old saved location)
        RemoveEntity(srcLocation);
    }

    private readonly struct ArrayFactory : ComponentDispatcher.IComponentFactory<int, Array>
    {
        public Array Create<T>(int capacity) where T : IComponent
        {
            return capacity == 0 ? [] : new T[capacity];
        }
    }
}
