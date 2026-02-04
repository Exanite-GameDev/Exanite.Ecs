using System.Reflection;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Events;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.Components;

internal abstract class ComponentEventDispatcher
{
    public abstract void OnComponentCopied(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype, IEntityLookup lookup);
    public abstract void OnComponentCopied(EcsCommandBuffer recursiveCommandBuffer, Entity entity, IEntityLookup lookup);
    public abstract void OnComponentAdded(EcsCommandBuffer recursiveCommandBuffer, Entity entity);
    public abstract void OnComponentModified(EcsCommandBuffer recursiveCommandBuffer, Entity entity);
    public abstract void OnComponentRemoved(EcsCommandBuffer recursiveCommandBuffer, Entity entity);

    internal static void ComponentSelfReference<T>(ref T component, Entity entity) where T : IComponent, IComponentSelfReference<T>
    {
        component.Self = entity.GetEcsRef<T>();
    }

    internal static void ComponentCopied<T>(ref T component, EcsWorld newWorld, IEntityLookup lookup) where T : IComponent, IComponentCopied
    {
        component.OnCopied(newWorld, lookup);
    }

    internal static void ComponentAdded<T>(ref T component) where T : IComponent, IComponentAdded
    {
        component.OnAdded();
    }

    internal static void ComponentSet<T>(ref T component) where T : IComponent, IComponentSet
    {
        component.OnSet();
    }

    internal static void ComponentRemoved<T>(ref T component) where T : IComponent, IComponentRemoved
    {
        component.OnRemoved();
    }
}

internal class ComponentEventDispatcher<T> : ComponentEventDispatcher where T : IComponent
{
    private delegate void ComponentSelfReferenceAction(ref T component, Entity entity);

    private delegate void ComponentCopiedAction(ref T component, EcsWorld newWorld, IEntityLookup lookup);

    private delegate void ComponentAction(ref T component);

    private readonly ComponentSelfReferenceAction? componentSelfReference;
    private readonly ComponentCopiedAction? componentCopied;
    private readonly ComponentAction? componentAdded;
    private readonly ComponentAction? componentSet;
    private readonly ComponentAction? componentRemoved;

    public ComponentEventDispatcher()
    {
        if (typeof(T).IsAssignableTo(typeof(IComponentSelfReference<T>)))
        {
            componentSelfReference = (ComponentSelfReferenceAction)typeof(ComponentEventDispatcher).GetMethod(nameof(ComponentSelfReference), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(ComponentSelfReferenceAction), null);
        }

        if (typeof(T).IsAssignableTo(typeof(IComponentCopied)))
        {
            componentCopied = (ComponentCopiedAction)typeof(ComponentEventDispatcher).GetMethod(nameof(ComponentCopied), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(ComponentCopiedAction), null);
        }

        if (typeof(T).IsAssignableTo(typeof(IComponentAdded)))
        {
            componentAdded = (ComponentAction)typeof(ComponentEventDispatcher).GetMethod(nameof(ComponentAdded), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(ComponentAction), null);
        }

        if (typeof(T).IsAssignableTo(typeof(IComponentSet)))
        {
            componentSet = (ComponentAction)typeof(ComponentEventDispatcher).GetMethod(nameof(ComponentSet), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(ComponentAction), null);
        }

        if (typeof(T).IsAssignableTo(typeof(IComponentRemoved)))
        {
            componentRemoved = (ComponentAction)typeof(ComponentEventDispatcher).GetMethod(nameof(ComponentRemoved), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(typeof(T))
                .CreateDelegate(typeof(ComponentAction), null);
        }
    }

    public override void OnComponentCopied(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype, IEntityLookup lookup)
    {
        if (archetype.EntityCount == 0)
        {
            return;
        }

        if (componentCopied != null)
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    ref var component = ref entity.Get<T>();

                    componentCopied!.Invoke(ref component, entity.World, lookup);
                }
            }
        }

        if (componentSelfReference != null)
        {
            foreach (var chunk in archetype.Chunks)
            {
                var components = chunk.GetSpan<T>();
                var entities = chunk.Entities;

                for (var i = 0; i < entities.Length; i++)
                {
                    componentSelfReference!.Invoke(ref components[i], entities[i]);
                }
            }
        }

        foreach (var chunk in archetype.Chunks)
        {
            foreach (var entity in chunk.Entities)
            {
                ref var component = ref entity.Get<T>();

                entity.World.EventBus.Raise(new ComponentCopiedEvent<T>(recursiveCommandBuffer, entity, ref component, lookup));
            }
        }
    }

    public override void OnComponentCopied(EcsCommandBuffer recursiveCommandBuffer, Entity entity, IEntityLookup lookup)
    {
        ref var component = ref entity.Get<T>();

        if (component is IComponentSelfReference<T>)
        {
            componentSelfReference!.Invoke(ref component, entity);
        }

        if (component is IComponentCopied)
        {
            componentCopied!.Invoke(ref component, entity.World, lookup);
        }

        entity.World.EventBus.Raise(new ComponentCopiedEvent<T>(recursiveCommandBuffer, entity, ref component, lookup));
    }

    public override void OnComponentAdded(EcsCommandBuffer recursiveCommandBuffer, Entity entity)
    {
        ref var component = ref entity.Get<T>();

        if (component is IComponentSelfReference<T>)
        {
            componentSelfReference!.Invoke(ref component, entity);
        }

        if (component is IComponentAdded)
        {
            componentAdded!.Invoke(ref component);
        }

        entity.World.EventBus.Raise(new ComponentAddedEvent<T>(recursiveCommandBuffer, entity, ref component));
    }

    public override void OnComponentModified(EcsCommandBuffer recursiveCommandBuffer, Entity entity)
    {
        ref var component = ref entity.Get<T>();

        if (component is IComponentSet)
        {
            componentSet!.Invoke(ref component);
        }

        entity.World.EventBus.Raise(new ComponentModifiedEvent<T>(recursiveCommandBuffer, entity, ref component));
    }

    public override void OnComponentRemoved(EcsCommandBuffer recursiveCommandBuffer, Entity entity)
    {
        ref var component = ref entity.Get<T>();
        entity.World.EventBus.Raise(new ComponentRemoved<T>(recursiveCommandBuffer, entity, ref component));

        if (component is IComponentRemoved)
        {
            componentRemoved!.Invoke(ref component);
        }
    }
}
