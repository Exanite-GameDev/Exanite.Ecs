using System;
using Exanite.Ecs.Worlds;

namespace Exanite.Ecs;

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
