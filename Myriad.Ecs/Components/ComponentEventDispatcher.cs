using Exanite.Core.Pooling;
using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs.Events;

namespace Exanite.Myriad.Ecs.Components;

internal abstract class ComponentEventDispatcher
{
    public abstract ValueBox AcquireValueBox();
    public abstract void ReleaseValueBox(ValueBox box);

    public abstract void RaiseComponentAdded(World world, Entity entity);
    public abstract void RaiseComponentModified(World world, Entity entity, ValueBox oldValue);
    public abstract void RaiseComponentRemoved(World world, Entity entity);
    public abstract void RaiseComponentDestroyed(World world, ValueBox value);
}

internal class ComponentEventDispatcher<T> : ComponentEventDispatcher where T : IComponent
{
    private readonly Pool<ValueBox<T?>> pool = new(
        create: () => new ValueBox<T?>(default),
        onRelease: box => box.Value = default);

    public override ValueBox AcquireValueBox()
    {
        lock (pool)
        {
            return pool.Acquire();
        }
    }

    public override void ReleaseValueBox(ValueBox box)
    {
        lock (pool)
        {
            pool.Release((ValueBox<T?>)box);
        }
    }

    public override void RaiseComponentAdded(World world, Entity entity)
    {
        world.EventBus.Raise(new ComponentAdded<T>(entity, ref entity.GetComponent<T>()));
    }

    public override void RaiseComponentModified(World world, Entity entity, ValueBox oldValue)
    {
        world.EventBus.Raise(new ComponentModified<T>(entity, ((ValueBox<T?>)oldValue).Value!, ref entity.GetComponent<T>()));
    }

    public override void RaiseComponentRemoved(World world, Entity entity)
    {
        world.EventBus.Raise(new ComponentRemoved<T>(entity, entity.GetComponent<T>()));
    }

    public override void RaiseComponentDestroyed(World world, ValueBox value)
    {
        world.EventBus.Raise(new ComponentDestroyed<T>(world, ((ValueBox<T?>)value).Value!));
    }
}
