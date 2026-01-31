using System;
using Exanite.Myriad.Ecs.Worlds.Archetypes;

namespace Exanite.Myriad.Ecs;

public interface IArchetypeView
{
    /// <summary>
    /// All archetypes in this collection.
    /// </summary>
    public ReadOnlySpan<Archetype> Archetypes { get; }
}
