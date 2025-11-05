using System;
using System.Reflection;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Events;

namespace Exanite.Myriad.Ecs.Components;

internal abstract class ComponentEventDispatcher
{
    public abstract void OnComponentAdded(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity);
    public abstract void OnComponentModified(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity);
    public abstract void OnComponentRemoved(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity);

    internal static void SetSelf<T>(ref T component, Entity entity) where T : IComponent, IComponentSelfReference<T>
    {
        component.Self = entity.GetStorableComponent<T>();
    }

    internal static void Setup<T>(ref T component) where T : IComponent, IComponentSetup
    {
        component.Setup();
    }

    internal static void Teardown<T>(ref T component) where T : IComponent, IComponentTeardown
    {
        component.Teardown();
    }

    internal static void Dispose<T>(ref T component) where T : IComponent, IDisposable
    {
        component.Dispose();
    }
}

internal class ComponentEventDispatcher<T> : ComponentEventDispatcher where T : IComponent
{
    private delegate void SetSelfAction(ref T component, Entity entity);
    private delegate void SetupAction(ref T component);
    private delegate void TeardownAction(ref T component);
    private delegate void DisposeAction(ref T component);

    private readonly SetSelfAction? setSelf;
    private readonly SetupAction? setup;
    private readonly TeardownAction? teardown;
    private readonly DisposeAction? dispose;

    public ComponentEventDispatcher()
    {
        if (typeof(T).IsAssignableTo(typeof(IComponentSelfReference<T>)))
        {
            setSelf = (SetSelfAction)typeof(ComponentEventDispatcher).GetMethod(nameof(SetSelf), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(SetSelfAction), null);
        }

        if (typeof(T).IsAssignableTo(typeof(IComponentSetup)))
        {
            setup = (SetupAction)typeof(ComponentEventDispatcher).GetMethod(nameof(Setup), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(SetupAction), null);
        }

        if (typeof(T).IsAssignableTo(typeof(IComponentTeardown)))
        {
            teardown = (TeardownAction)typeof(ComponentEventDispatcher).GetMethod(nameof(Teardown), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(TeardownAction), null);
        }

        if (typeof(T).IsAssignableTo(typeof(IDisposable)))
        {
            dispose = (DisposeAction)typeof(ComponentEventDispatcher).GetMethod(nameof(Dispose), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(DisposeAction), null);
        }
    }

    public override void OnComponentAdded(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity)
    {
        ref var component = ref entity.GetComponent<T>();

        if (component is IComponentSelfReference<T>)
        {
            setSelf!.Invoke(ref component, entity);
        }

        if (component is IComponentSetup)
        {
            setup!.Invoke(ref component);
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

        if (component is IComponentTeardown)
        {
            teardown!.Invoke(ref component);
        }

        if (component is IDisposable)
        {
            dispose!.Invoke(ref component);
        }
    }
}
