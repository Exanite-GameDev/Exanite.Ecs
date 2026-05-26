namespace Exanite.Ecs;

/// <summary>
/// Represents an <see cref="IEcsInterface"/> that is bound to a specific entity.
/// </summary>
public readonly ref struct InterfaceBinding<T> where T : IEcsInterface
{
    public readonly Entity Entity;
    public readonly T Interface;

    public InterfaceBinding(Entity entity, T interfaceComponent)
    {
        Entity = entity;
        Interface = interfaceComponent;
    }
}
