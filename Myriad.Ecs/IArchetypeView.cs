using System;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs;

/// <summary>
/// Represents a high-performance collection of archetypes.
/// </summary>
public interface IArchetypeView
{
    /// <summary>
    /// All archetypes in this collection.
    /// </summary>
    public ReadOnlySpan<Archetype> Archetypes { get; }
}
