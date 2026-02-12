namespace Exanite.Myriad.Ecs;

public static class InterfaceBindingUtility
{
    public static InterfaceBinding<T> Bind<T>(this T interfaceComponent, Entity entity) where T : IInterfaceComponent
    {
        return new InterfaceBinding<T>(entity, interfaceComponent);
    }
}
