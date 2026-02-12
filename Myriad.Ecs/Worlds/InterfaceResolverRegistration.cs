using System;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Queries;

namespace Exanite.Myriad.Ecs.Worlds;

/// <summary>
/// Represents a registration for an interface resolver.
/// See <see cref="EcsWorld.RegisterInterfaceResolver{T}"/>
/// </summary>
public record struct InterfaceResolverRegistration(InterfaceId Id, QueryFilter Filter, Func<ImmutableOrderedListSet<ComponentId>, object> Factory);
