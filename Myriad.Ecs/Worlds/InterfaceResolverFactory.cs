using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Worlds;

/// <summary>
/// Get or create the concrete implementation of the interface component for the specified archetype.
/// </summary>
/// <param name="previous">The previous instance of the resolver. This can be wrapped by the new resolver to allow for override-like functionality.</param>
/// <param name="components">The components of the matched archetype.</param>
public delegate T? InterfaceResolverFactory<T>(T? previous, ImmutableOrderedListSet<ComponentId> components);
