namespace Exanite.Myriad.Ecs.Events;

/// <summary>
/// Raised after an entity is created and added to the world.
/// </summary>
public readonly ref struct EntityCreatedEvent
{
    public World World => Entity.World;
    public readonly Entity Entity;

    public EntityCreatedEvent(Entity entity)
    {
        Entity = entity;
    }
}

/// <summary>
/// Raised before an entity is destroyed and removed from the world.
/// </summary>
public readonly ref struct EntityDestroyedEvent
{
    public World World => Entity.World;
    public readonly Entity Entity;

    public EntityDestroyedEvent(Entity entity)
    {
        Entity = entity;
    }
}

/// <summary>
/// Raised after a component is either added or set. When setting, this event will contain the new component value.
/// </summary>
public readonly ref struct ComponentAdded<T> where T : IComponent
{
    public World World => Entity.World;
    public readonly Entity Entity;
    public readonly ref T Value;

    public ComponentAdded(Entity entity, ref T value)
    {
        Entity = entity;
        Value = ref value;
    }
}

/// <summary>
/// Raised after an existing component is explicitly set.
/// <br/>
/// Warning: Modifications without setting through the command buffer will NOT raise this event.
/// </summary>
/// <remarks>
/// This event does not provide the old value because the event cannot guarantee that
/// the component has not been modified since the previous <see cref="ComponentModified{T}"/> event.
/// <para/>
/// Code that relies on the previous component value should store it manually.
/// </remarks>
public readonly ref struct ComponentModified<T> where T : IComponent
{
    public World World => Entity.World;
    public readonly Entity Entity;
    public readonly ref T NewValue;

    public ComponentModified(Entity entity, ref T newValue)
    {
        Entity = entity;
        NewValue = ref newValue;
    }
}

/// <summary>
/// Raised after a component is either removed or set. When setting, this event will contain the old component value.
/// </summary>
public readonly ref struct ComponentRemoved<T> where T : IComponent
{
    public World World => Entity.World;
    public readonly Entity Entity;
    public readonly T Value;

    public ComponentRemoved(Entity entity, T value)
    {
        Entity = entity;
        Value = value;
    }
}
