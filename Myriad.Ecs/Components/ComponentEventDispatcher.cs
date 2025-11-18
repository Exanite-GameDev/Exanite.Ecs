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

    internal static void SelfReference<T>(ref T component, Entity entity) where T : IComponent, IComponentSelfReference<T>
    {
        component.Self = entity.GetStorableComponent<T>();
    }

    internal static void SelfAdded<T>(ref T component) where T : IComponent, IComponentSelfAdded
    {
        component.OnAdded();
    }

    internal static void SelfSet<T>(ref T component) where T : IComponent, IComponentSelfSet
    {
        component.OnSet();
    }

    internal static void SelfRemoved<T>(ref T component) where T : IComponent, IComponentSelfRemoved
    {
        component.OnRemoved();
    }

    internal static void Dispose<T>(ref T component) where T : IComponent, IDisposable
    {
        component.Dispose();
    }
}

internal class ComponentEventDispatcher<T> : ComponentEventDispatcher where T : IComponent
{
    private delegate void SelfReferenceAction(ref T component, Entity entity);

    private delegate void ComponentAction(ref T component);

    private readonly SelfReferenceAction? selfReference;
    private readonly ComponentAction? selfAdded;
    private readonly ComponentAction? selfSet;
    private readonly ComponentAction? selfRemoved;
    private readonly ComponentAction? dispose;

    public ComponentEventDispatcher()
    {
        if (typeof(T).IsAssignableTo(typeof(IComponentSelfReference<T>)))
        {
            selfReference = (SelfReferenceAction)typeof(ComponentEventDispatcher).GetMethod(nameof(SelfReference), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(SelfReferenceAction), null);
        }

        if (typeof(T).IsAssignableTo(typeof(IComponentSelfAdded)))
        {
            selfAdded = (ComponentAction)typeof(ComponentEventDispatcher).GetMethod(nameof(SelfAdded), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(ComponentAction), null);
        }

        if (typeof(T).IsAssignableTo(typeof(IComponentSelfSet)))
        {
            selfSet = (ComponentAction)typeof(ComponentEventDispatcher).GetMethod(nameof(SelfSet), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(ComponentAction), null);
        }

        if (typeof(T).IsAssignableTo(typeof(IComponentSelfRemoved)))
        {
            selfRemoved = (ComponentAction)typeof(ComponentEventDispatcher).GetMethod(nameof(SelfRemoved), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(ComponentAction), null);
        }

        if (typeof(T).IsAssignableTo(typeof(IDisposable)))
        {
            dispose = (ComponentAction)typeof(ComponentEventDispatcher).GetMethod(nameof(Dispose), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(ComponentAction), null);
        }
    }

    public override void OnComponentAdded(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity)
    {
        ref var component = ref entity.GetComponent<T>();

        if (component is IComponentSelfReference<T>)
        {
            selfReference!.Invoke(ref component, entity);
        }

        if (component is IComponentSelfAdded)
        {
            selfAdded!.Invoke(ref component);
        }

        world.EventBus.Raise(new ComponentAdded<T>(recursiveCommandBuffer, entity, ref component));
    }

    public override void OnComponentModified(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity)
    {
        ref var component = ref entity.GetComponent<T>();

        if (component is IComponentSelfSet)
        {
            selfSet!.Invoke(ref component);
        }

        world.EventBus.Raise(new ComponentModified<T>(recursiveCommandBuffer, entity, ref component));
    }

    public override void OnComponentRemoved(EcsCommandBuffer recursiveCommandBuffer, EcsWorld world, Entity entity)
    {
        ref var component = ref entity.GetComponent<T>();
        world.EventBus.Raise(new ComponentRemoved<T>(recursiveCommandBuffer, entity, ref component));

        if (component is IComponentSelfRemoved)
        {
            selfRemoved!.Invoke(ref component);
        }

        if (component is IDisposable)
        {
            dispose!.Invoke(ref component);
        }
    }
}
