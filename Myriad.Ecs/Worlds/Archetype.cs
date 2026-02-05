using System;
using System.Collections.Generic;
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
    /// The total number of entities in this archetype.
    /// </summary>
    public int EntityCount { get; private set; }

    /// <summary>
    /// The chunks contained in this archetype.
    /// </summary>
    /// <remarks>
    /// This will never include empty chunks.
    /// </remarks>
    public ReadOnlySpan<Chunk> Chunks => chunksList.AsSpan();

    /// <summary>
    /// The chunks contained in this archetype.
    /// </summary>
    /// <remarks>
    /// Enumerating over this will allocate due to the List enumerator being boxed.
    /// </remarks>
    public IReadOnlyList<Chunk> ChunksList => chunksList;

    /// <summary>
    /// The components of entities in this archetype.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> Components { get; }

    /// <inheritdoc cref="ArchetypeComponentLookup"/>
    internal readonly ArchetypeComponentLookup Lookup;

    /// <summary>
    /// A bloom filter of all the components in this archetype.
    /// </summary>
    internal readonly ComponentBloomFilter ComponentsBloomFilter;

    /// <summary>
    /// The hash of all components IDs in this archetype.
    /// </summary>
    internal ArchetypeHash Hash { get; }

    /// <summary>
    /// All chunks in this archetype.
    /// </summary>
    private readonly List<Chunk> chunksList = [];

    /// <summary>
    /// A list of empty chunks that have been removed from this archetype.
    /// </summary>
    private readonly Stack<Chunk> spareChunks = new(EcsConstants.ChunkHotSpareCount);

    internal Archetype(EcsWorld world, ImmutableOrderedListSet<ComponentId> components)
    {
        World = world;
        Components = components;
        ComponentsBloomFilter = components.ToBloomFilter();

        // Calculate archetype hash
        foreach (var component in components)
        {
            Hash = Hash.Toggle(component);
        }

        // Initialize component lookup
        Lookup = new ArchetypeComponentLookup(components);
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
    /// Destroy every Entity in this archetype
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
}
