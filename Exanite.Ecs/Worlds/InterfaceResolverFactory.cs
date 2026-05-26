using Exanite.Ecs.Collections;
using Exanite.Ecs.Components;

namespace Exanite.Ecs.Worlds;

/// <summary>
/// Get or create the concrete implementation of the interface component for the specified archetype.
/// </summary>
/// <param name="previous">The previous instance of the resolver. This can be wrapped by the new resolver to allow for override-like functionality.</param>
/// <param name="components">The components of the matched archetype.</param>
public delegate T? InterfaceResolverFactory<T>(T? previous, IReadOnlyOrderedListSet<TypeId> components);
