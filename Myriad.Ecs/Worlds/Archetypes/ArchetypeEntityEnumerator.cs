using System;
using System.Collections.Generic;
using Myriad.Ecs.Worlds.Chunks;

namespace Myriad.Ecs.Worlds.Archetypes;

/// <summary>
/// Enumerable of all the entities in a single archetype
/// </summary>
public readonly struct ArchetypeEntityEnumerable
{
    /// <summary>
    /// The <see cref="Archetype"/> this enumerable is over
    /// </summary>
    public Archetype Archetype { get; }

    /// <summary>
    /// Create a new enumerable for the given archetype
    /// </summary>
    /// <param name="archetype"></param>
    public ArchetypeEntityEnumerable(Archetype archetype)
    {
        Archetype = archetype;
    }

    /// <summary>
    /// Get an enumerator from this enumerable
    /// </summary>
    /// <returns></returns>
    public ArchetypeEntityEnumerator GetEnumerator()
    {
        return new ArchetypeEntityEnumerator(Archetype);
    }

    internal int Count()
    {
        var count = 0;
        foreach (var _ in this)
            count++;
        return count;
    }
}

/// <summary>
/// Enumerator over the entities in an archetype
/// </summary>
public struct ArchetypeEntityEnumerator
    : IDisposable
{
    private List<Chunk>.Enumerator chunksEnumerator;
    private int entityIndex = -1;

    private Chunk? chunk;

    internal ArchetypeEntityEnumerator(Archetype archetype)
    {
        chunksEnumerator = archetype.GetChunkEnumerator();
    }

    /// <summary>
    /// Get the current item from this enumerator
    /// </summary>
    public readonly Entity Current => chunk!.Entities.Span[entityIndex];

    private bool NextChunk()
    {
        if (!chunksEnumerator.MoveNext())
        {
            return false;
        }

        chunk = chunksEnumerator.Current;
        entityIndex = 0;
        return true;
    }

    /// <summary>
    /// Move to the next item
    /// </summary>
    /// <returns></returns>
    public bool MoveNext()
    {
        entityIndex++;
        if (chunk != null && entityIndex < chunk.EntityCount)
        {
            return true;
        }

        if (!NextChunk())
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        chunksEnumerator.Dispose();
    }
}
