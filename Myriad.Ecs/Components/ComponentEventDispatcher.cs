using Exanite.Core.Pooling;
using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs.Events;

namespace Exanite.Myriad.Ecs.Components;

internal abstract class ComponentEventDispatcher
{
    public abstract void RaiseComponentAdded(World world, Entity entity);
    public abstract void RaiseComponentModified(World world, Entity entity);
    public abstract void RaiseComponentRemoved(World world, Entity entity);
}

internal class ComponentEventDispatcher<T> : ComponentEventDispatcher where T : IComponent
{
    public override void RaiseComponentAdded(World world, Entity entity)
    {
        world.EventBus.Raise(new ComponentAdded<T>(entity, ref entity.GetComponent<T>()));
    }

    public override void RaiseComponentModified(World world, Entity entity)
    {
        world.EventBus.Raise(new ComponentModified<T>(entity, ref entity.GetComponent<T>()));
    }

    public override void RaiseComponentRemoved(World world, Entity entity)
    {
        world.EventBus.Raise(new ComponentRemoved<T>(entity, entity.GetComponent<T>()));
    }
}
