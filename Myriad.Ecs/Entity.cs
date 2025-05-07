using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Exanite.Core.Runtime;
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
    /// The <see cref="World"/> this <see cref="Entity"/> is in
    /// </summary>
    public readonly World World;

    /// <summary>
    /// The <see cref="Ecs.Entity"/> of an entity, may be re-used very quickly once an <see cref="Ecs.Entity"/> is destroyed.
    /// </summary>
    public int Id => EntityId.Id;

    /// <summary>
    /// The version number of this ID, may also be re-used but only after the full 32 bit counter has been overflowed for this specific ID.
    /// </summary>
    public uint Version => EntityId.Version;

    /// <summary>
    /// The raw ID of this <see cref="Entity"/>
    /// </summary>
    internal readonly EntityId EntityId;

    /// <summary>
    /// Get the set of components which this entity currently has
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> ComponentTypes => EntityId.GetComponents(World);

    /// <summary>
    /// Get a boxed array of all components. <b>DO NOT</b> use this for anything other than debugging!
    /// </summary>
    public object[] BoxedComponents => ComponentTypes.Select(GetBoxedComponent).ToArray()!;

    internal Entity(EntityId id, World world)
    {
        EntityId = id;
        World = world;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return EntityId.ToString();
    }

    /// <summary>
    /// Check if this Entity still exists.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Exists() => EntityId.Exists(World);

    /// <summary>
    /// Check if this Entity still exists and is not a phantom.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAlive() => EntityId.IsAlive(World);

    /// <summary>
    /// Check if this Entity is in a phantom state. i.e. automatically excluded from queries
    /// and automatically deleted when the last IPhantomComponent component is removed.
    /// </summary>
    /// <returns>true if this entity is a phantom. False is it does not exist or is not a phantom.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsPhantom() => EntityId.IsPhantom(World);

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Entity other)
    {
        return EntityId.CompareTo(other.EntityId);
    }

    /// <summary>
    /// Get a unique 64 bit ID for this entity
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long UniqueId() => EntityId.UniqueId();

    /// <summary>
    /// Check if this entity has a component
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasComponent<T>() where T : IComponent => EntityId.HasComponent<T>(World);

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetComponent<T>() where T : IComponent => ref EntityId.GetComponent<T>(World);

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueRef<T> GetComponentRef<T>() where T : IComponent => EntityId.GetComponentRef<T>(World);

    /// <summary>
    /// Get a <b>boxed copy</b> of a component from this entity. Only use for debugging!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetBoxedComponent(ComponentId id) => EntityId.GetBoxedComponent(World, id);
}
