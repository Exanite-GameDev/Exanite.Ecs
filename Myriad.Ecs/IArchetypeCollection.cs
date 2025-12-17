using System;
using System.Collections.Generic;
using Exanite.Myriad.Ecs.Worlds.Archetypes;

namespace Exanite.Myriad.Ecs;

public interface IArchetypeCollection
{
    /// <summary>
    /// All archetypes in this collection.
    /// </summary>
    public ReadOnlySpan<Archetype> Archetypes { get; }

    /// <inheritdoc cref="Archetypes"/>
    public IReadOnlyList<Archetype> ArchetypesList { get; }
}
