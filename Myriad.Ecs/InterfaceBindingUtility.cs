namespace Exanite.Myriad.Ecs;

public static class InterfaceBindingUtility
{
    public static InterfaceBinding<T> Bind<T>(this T interfaceComponent, Entity entity) where T : IEcsInterface
    {
        return new InterfaceBinding<T>(entity, interfaceComponent);
    }
}
