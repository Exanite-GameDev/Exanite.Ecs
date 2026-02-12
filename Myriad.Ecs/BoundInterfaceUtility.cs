namespace Exanite.Myriad.Ecs;

public static class BoundInterfaceUtility
{
    public static BoundInterface<T> Bind<T>(this T interfaceComponent, Entity entity) where T : IInterfaceComponent
    {
        return new BoundInterface<T>(entity, interfaceComponent);
    }
}
