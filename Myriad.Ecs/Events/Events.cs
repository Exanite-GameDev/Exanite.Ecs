namespace Exanite.Myriad.Ecs.Events;

/// <summary>
/// Raised after an entity is added to the world.
/// </summary>
public readonly ref struct EntityAddedEvent
{
    public World World => Entity.World;
    public readonly Entity Entity;

    public EntityAddedEvent(Entity entity)
    {
        Entity = entity;
    }
}

/// <summary>
/// Raised before an entity becomes a phantom entity or is destroyed.
/// </summary>
public readonly ref struct EntityRemovedEvent
{
    public World World => Entity.World;
    public readonly Entity Entity;

    public EntityRemovedEvent(Entity entity)
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
/// Raised after an existing component is set.
/// </summary>
public readonly ref struct ComponentModified<T> where T : IComponent
{
    public World World => Entity.World;
    public readonly Entity Entity;
    public readonly T OldValue;
    public readonly ref T NewValue;

    public ComponentModified(Entity entity, T oldValue, ref T newValue)
    {
        Entity = entity;
        OldValue = oldValue;
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

/// <summary>
/// Raised after a component is either removed, set, or when a command buffer set is never applied. When setting, this event will contain the old component value.
/// </summary>
/// <remarks>
/// Use this for cleaning up memory and other resources.
/// </remarks>
public readonly ref struct ComponentDestroyed<T> where T : IComponent
{
    public readonly World World;
    public readonly T Value;

    public ComponentDestroyed(World world, T value)
    {
        World = world;
        Value = value;
    }
}
