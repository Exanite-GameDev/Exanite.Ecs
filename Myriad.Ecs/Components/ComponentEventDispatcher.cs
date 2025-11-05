using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Events;

namespace Exanite.Myriad.Ecs.Components;

internal abstract class ComponentEventDispatcher
{
    public abstract void OnComponentAdded(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity);
    public abstract void OnComponentModified(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity);
    public abstract void OnComponentRemoved(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity);
}

internal class ComponentEventDispatcher<T> : ComponentEventDispatcher where T : IComponent
{
    public override void OnComponentAdded(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity)
    {
        world.EventBus.Raise(new ComponentAdded<T>(recursiveCommandBuffer, entity, ref entity.GetComponent<T>()));
    }

    public override void OnComponentModified(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity)
    {
        world.EventBus.Raise(new ComponentModified<T>(recursiveCommandBuffer, entity, ref entity.GetComponent<T>()));
    }

    public override void OnComponentRemoved(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity)
    {
        world.EventBus.Raise(new ComponentRemoved<T>(recursiveCommandBuffer, entity, ref entity.GetComponent<T>()));
    }
}
