using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs;

/// <summary>
/// An <see cref="Entity"/> is an ID in the <see cref="World"/> which has a set of components associated with it.
/// </summary>
[DebuggerDisplay("{EntityId}")]
public readonly partial record struct Entity : IComparable<Entity>
{
    /// <summary>
    /// Check if this Entity still exists.
    /// </summary>
    public bool IsAlive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Index != 0 && World.Entities.GetVersion(Index) == Version;
    }

    /// <summary>
    /// Check if this Entity is default initialized.
    /// </summary>
    internal bool IsDefault
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Index == 0;
    }

    /// <summary>
    /// The <see cref="World"/> this <see cref="Entity"/> is in.
    /// </summary>
    public readonly EcsWorld World;

    /// <summary>
    /// The <see cref="Entity"/> of an entity, may be re-used very quickly once an <see cref="Entity"/> is destroyed.
    /// </summary>
    public int Index => EntityId.Index;

    /// <summary>
    /// The version number of this ID, may also be re-used but only after the full 32 bit counter has been overflowed for this specific ID.
    /// </summary>
    public uint Version => EntityId.Version;

    /// <summary>
    /// The raw ID of this <see cref="Entity"/>
    /// </summary>
    internal readonly EntityId EntityId;

    /// <summary>
    /// Get the set of components which this entity currently has.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> ComponentIds => World.Entities.GetArchetype(EntityId).Components;

    /// <summary>
    /// Get a boxed array of all components.
    /// <para/>
    /// This is very slow and the returned data is a copy of the original data.
    /// Avoid using this for anything other than debugging!
    /// </summary>
    public object[] BoxedComponents => ComponentIds.Select(GetBoxedComponent).ToArray()!;

    /// <summary>
    /// Check if this entity has a component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasComponent<T>() where T : IComponent
    {
        return ComponentIds.Contains(ComponentId.Get<T>());
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetComponent<T>() where T : IComponent
    {
        ref var location = ref World.Entities.GetLocation(EntityId);
        return ref location.Chunk.Get<T>(location.IndexInChunk);
    }

    /// <summary>
    /// Get a reference to a component of the given type.
    /// If the entity does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Ref<T> GetComponentRef<T>() where T : IComponent
    {
        ref var location = ref World.Entities.GetLocation(EntityId);
        return location.Chunk.GetRef<T>(location.IndexInChunk);
    }

    /// <summary>
    /// Get a reference to a component of the given type.
    /// If the entity does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EcsRef<T> GetStorableComponent<T>() where T : IComponent
    {
        GuardUtility.IsTrue(IsAlive, "Entity does not exist");
        GuardUtility.IsTrue(HasComponent<T>(), $"Component does not exist on entity: {GetType().Name}");

        return new EcsRef<T>(this);
    }

    /// <summary>
    /// Get a <b>boxed copy</b> of a component from this entity. Only use for debugging!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetBoxedComponent(ComponentId id)
    {
        if (!IsAlive)
        {
            return null;
        }

        if (!ComponentIds.Contains(id))
        {
            return null;
        }

        ref var location = ref World.Entities.GetLocation(EntityId);
        return location.Chunk.GetComponentArray(id).GetValue(location.IndexInChunk);
    }

    internal Entity(EntityId id, EcsWorld world)
    {
        EntityId = id;
        World = world;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return $"{EntityId}{(IsAlive ? "" : " (Dead)")}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Entity other)
    {
        return EntityId.CompareTo(other.EntityId);
    }
}
