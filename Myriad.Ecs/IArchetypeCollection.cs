using System;
using System.Collections.Generic;
using Exanite.Myriad.Ecs.Worlds.Archetypes;

namespace Exanite.Myriad.Ecs;

public interface IArchetypeCollection
{
    /// <summary>
    /// Get a span of all archetypes in this collection.
    /// </summary>
    public ReadOnlySpan<Archetype> Archetypes { get; }

    /// <summary>
    /// Get a list of all archetypes in this collection.
    /// </summary>
    /// <remarks>
    /// Enumerating over this will allocate due to the List enumerator getting boxed.
    /// </remarks>
    public IReadOnlyList<Archetype> ArchetypesList { get; }
}
