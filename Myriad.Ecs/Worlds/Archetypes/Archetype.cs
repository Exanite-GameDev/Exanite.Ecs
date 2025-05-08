using System;
using System.Collections.Generic;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Utilities;
using Exanite.Myriad.Ecs.Worlds.Chunks;

namespace Exanite.Myriad.Ecs.Worlds.Archetypes;

/// <summary>
/// An archetype contains all entities which share exactly the same set of components.
/// </summary>
public sealed class Archetype
{
    /// <summary>
    /// Number of entities in a single chunk.
    /// </summary>
    internal const int ChunkSize = 1024;

    /// <summary>
    /// How many empty chunks to keep as spares.
    /// </summary>
    private const int ChunkHotSpares = 4;

    /// <summary>
    /// The world which this archetype belongs to.
    /// </summary>
    public World World { get; }

    /// <summary>
    /// The chunks contained in this archetype.
    /// </summary>
    public IReadOnlyList<Chunk> Chunks => chunks;

    /// <summary>
    /// The components of entities in this archetype.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> Components { get; }

    /// <summary>
    /// The total number of entities in this archetype.
    /// </summary>
    public int EntityCount { get; private set; }

    /// <summary>
    /// True if any of the components in this Archetype implement <see cref="IComponentPhantom"/>.
    /// </summary>
    public bool HasPhantomComponents { get; }

    /// <summary>
    /// True if any of the components in this Archetype is <see cref="ComponentPhantom"/>.
    /// </summary>
    public bool IsPhantom { get; }

    /// <summary>
    /// A bloom filter of all the components in this archetype.
    /// </summary>
    internal readonly ComponentBloomFilter ComponentsBloomFilter;

    /// <summary>
    /// The hash of all components IDs in this archetype.
    /// </summary>
    internal ArchetypeHash Hash { get; }

    /// <summary>
    /// Map from component index to component type for chunks in this archetype.
    /// </summary>
    private readonly Type[] componentTypesByComponentIndex;

    /// <summary>
    /// Map from component index to component ID for chunks in this archetype.
    /// </summary>
    private readonly ComponentId[] componentIdByComponentIndex;

    /// <summary>
    /// Sparse map from component ID to component index in chunk for chunks in this archetype.
    /// </summary>
    private readonly int[] componentIndexByComponentId;

    /// <summary>
    /// All chunks in this archetype.
    /// </summary>
    private readonly List<Chunk> chunks = [];

    /// <summary>
    /// A list of chunks which might have space to put an entity in.
    /// </summary>
    private readonly List<Chunk> chunksWithSpace = [];

    /// <summary>
    /// A list of empty chunks that have been removed from this archetype.
    /// </summary>
    private readonly Stack<Chunk> spareChunks = new(ChunkHotSpares);

    /// <summary>
    /// The archetype that entities should be moved to when deleted. Only non-null if <c>HasPhantomComponents &amp; !IsPhantom</c>.
    /// </summary>
    private readonly Archetype? phantomDestination;

    internal Archetype(World world, ImmutableOrderedListSet<ComponentId> components)
    {
        World = world;
        Components = components;
        ComponentsBloomFilter = components.ToBloomFilter();

        // Create arrays to fills in below
        componentTypesByComponentIndex = new Type[components.Count];
        componentIdByComponentIndex = new ComponentId[components.Count];

        // Calculate archetype hash and also keep track of the max component ID ever seen
        var maxComponentId = int.MinValue;
        foreach (var component in components)
        {
            Hash = Hash.Toggle(component);
            if (component.Value > maxComponentId)
            {
                maxComponentId = component.Value;
            }
        }

        // Build an array where the number at a given index is the index of the component with that ID
        componentIndexByComponentId = maxComponentId == int.MinValue ? [] : new int[maxComponentId + 1];
        Array.Fill(componentIndexByComponentId, -1);
        var idx = 0;
        foreach (var component in components)
        {
            componentTypesByComponentIndex[idx] = component.Type;
            componentIndexByComponentId[component.Value] = idx;
            componentIdByComponentIndex[idx] = component;

            idx++;
        }

        // Gather flags for special components
        foreach (var component in components)
        {
            IsPhantom |= component == ComponentId.Get<ComponentPhantom>();
            HasPhantomComponents |= component.IsPhantomComponent;
        }

        // Get the destination archetype for deleted entities, if they become phantoms
        if (HasPhantomComponents && !IsPhantom)
        {
            var c = new OrderedListSet<ComponentId>(components)
            {
                ComponentId.Get<ComponentPhantom>()
            };
            phantomDestination = World.GetOrCreateArchetype(c);
        }
    }

    internal EntityStorageLocation CreateEntity()
    {
        // Allocate an entity in the world
        ref var location = ref World.AllocateEntity(out var entity);

        // Add it to this archetype, find a location to put components into
        return AddEntity(entity, ref location);
    }

    /// <summary>
    /// Delete every Entity in this archetype
    /// </summary>
    internal void Clear()
    {
        if (HasPhantomComponents && !IsPhantom)
        {
            AssertUtility.NotNull(phantomDestination);

            // Migrate all entities in all chunks to the new archetype. Doing this does all of the bookkeeping like chunk management and entity count.
            // This could be better, at the moment it just does the work on a per-entity basis, instead of doing it all in one batch.
            while (chunks.Count > 0)
            {
                var chunk = chunks[^1];

                while (chunk.EntityCount > 0)
                {
                    var entity = chunk.Entities.Span[^1].EntityId;
                    ref var location = ref World.GetStorageLocation(entity);

                    MigrateTo(entity, ref location, phantomDestination!);
                }
            }
        }
        else
        {
            // Clear all the chunks
            foreach (var chunk in chunks)
            {
                chunk.Clear();
            }

            // Move some chunks to hot spares and then delete the rest
            foreach (var chunk in chunks)
            {
                if (spareChunks.Count < ChunkHotSpares)
                {
                    spareChunks.Push(chunk);
                }
                else
                {
                    break;
                }
            }
            chunksWithSpace.Clear();
            chunks.Clear();

            // Done! No entities left.
            EntityCount = 0;
        }

        AssertUtility.IsTrue(EntityCount == 0, "Expected EntityCount to equal 0");
    }

    /// <summary>
    /// Find a chunk with space and add the given entity to it.
    /// </summary>
    /// <param name="entity">Entity to add to a chunk</param>
    /// <param name="location">Location will be mutated to point to the new location</param>
    internal EntityStorageLocation AddEntity(EntityId entity, ref StorageLocation location)
    {
        // Increase archetype entity count
        EntityCount++;

        // Trim chunks with space collection to remove items
        chunksWithSpace.RemoveAll(static c => c.EntityCount == ChunkSize);

        // If there's one with space, use it
        if (chunksWithSpace.Count > 0)
        {
            return chunksWithSpace[0].AddEntity(entity, ref location);
        }

        // No space in any chunks, create a new chunk
        var newChunk = spareChunks.Count > 0 ? spareChunks.Pop() : new Chunk(this, ChunkSize, componentTypesByComponentIndex, componentIdByComponentIndex, componentIndexByComponentId);
        chunks.Add(newChunk);
        chunksWithSpace.Add(newChunk);

        // The chunk obviously has space, so this cannot fail!
        return newChunk.AddEntity(entity, ref location);
    }

    internal void RemoveEntity(StorageLocation location)
    {
        // Remove the entity from the chunk, component data is lost after this point
        location.Chunk.RemoveEntity(location);

        // Execute handler for when an entity is removed from a chunk
        OnChunkEntityRemoved(location.Chunk);
    }

    internal EntityStorageLocation MigrateTo(EntityId entity, ref StorageLocation location, Archetype dstArchetype)
    {
        // Early exit if we're migrating to where we already are!
        if (dstArchetype == this)
        {
            return location.GetEntityStorageLocation(entity);
        }

        // Do the actual copying
        var srcChunk = location.Chunk;
        var newEntityLocation = srcChunk.MigrateTo(entity, ref location, dstArchetype);

        // Execute handler for when an entity is removed from a chunk
        OnChunkEntityRemoved(srcChunk);

        return newEntityLocation;
    }

    private void OnChunkEntityRemoved(Chunk chunk)
    {
        // Decrease archetype entity count
        EntityCount--;

        switch (chunk.EntityCount)
        {
            // If the chunk is empty remove it from this archetype entirely
            case 0:
            {
                chunksWithSpace.Remove(chunk);
                chunks.Remove(chunk);
                if (spareChunks.Count < ChunkHotSpares)
                {
                    spareChunks.Push(chunk);
                }

                break;
            }

            // If the chunk was previously full and now isn't, add it to the set of chunks with space
            case ChunkSize - 1:
            {
                chunksWithSpace.Add(chunk);
                break;
            }
        }
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Hash.GetHashCode();
    }

    internal bool SetEquals(OrderedListSet<ComponentId> query)
    {
        return Components.SetEquals(query);
    }

    internal bool SetEquals<TV>(Dictionary<ComponentId, TV> query)
    {
        return Components.SetEquals(query);
    }
}
