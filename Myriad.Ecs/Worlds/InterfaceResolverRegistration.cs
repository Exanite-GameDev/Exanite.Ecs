using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Queries;

namespace Exanite.Myriad.Ecs.Worlds;

/// <summary>
/// Represents a registration for an interface resolver.
/// See <see cref="EcsWorld.RegisterInterfaceResolver{T}"/>
/// </summary>
public readonly record struct InterfaceResolverRegistration
{
    public readonly InterfaceId Id;
    public readonly QueryFilter Filter;
    public readonly InterfaceResolverFactory<object> Factory;

    internal InterfaceResolverRegistration(InterfaceId id, QueryFilter filter, InterfaceResolverFactory<object> factory)
    {
        Id = id;
        Filter = filter;
        Factory = factory;
    }

    public override string ToString()
    {
        return $"{Id} {Filter}";
    }
}
