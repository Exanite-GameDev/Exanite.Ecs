using System;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Queries;

namespace Exanite.Myriad.Ecs.Worlds;

/// <summary>
/// Represents a registration for an interface resolver.
/// See <see cref="EcsWorld.RegisterInterfaceResolver{T}"/>
/// </summary>
public struct InterfaceResolverRegistration
{
    public readonly InterfaceId Id;
    public readonly QueryFilter Filter;
    public readonly Func<ImmutableOrderedListSet<ComponentId>, object> Factory;

    internal InterfaceResolverRegistration(InterfaceId id, QueryFilter filter, Func<ImmutableOrderedListSet<ComponentId>, object> factory)
    {
        Id = id;
        Filter = filter;
        Factory = factory;
    }
}
