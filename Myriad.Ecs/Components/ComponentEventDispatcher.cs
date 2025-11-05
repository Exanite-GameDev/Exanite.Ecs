using System;
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
        ref var component = ref entity.GetComponent<T>();

        // ReSharper disable once MergeCastWithTypeCheck
        if (component is IComponentSelfReference<T>)
        {
            ((IComponentSelfReference<T>)component).Self = entity.GetStorableComponent<T>();
        }

        // ReSharper disable once MergeCastWithTypeCheck
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (component is IComponentSetup)
        {
            ((IComponentSetup)component).Setup();
        }

        world.EventBus.Raise(new ComponentAdded<T>(recursiveCommandBuffer, entity, ref component));
    }

    public override void OnComponentModified(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity)
    {
        ref var component = ref entity.GetComponent<T>();
        world.EventBus.Raise(new ComponentModified<T>(recursiveCommandBuffer, entity, ref component));
    }

    public override void OnComponentRemoved(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity)
    {
        ref var component = ref entity.GetComponent<T>();
        world.EventBus.Raise(new ComponentRemoved<T>(recursiveCommandBuffer, entity, ref component));

        // ReSharper disable once MergeCastWithTypeCheck
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (component is IComponentTeardown)
        {
            ((IComponentTeardown)component).Teardown();
        }

        // ReSharper disable once MergeCastWithTypeCheck
        if (component is IDisposable)
        {
            ((IDisposable)component).Dispose();
        }
    }
}
