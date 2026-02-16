using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs;

/// <summary>
/// An <see cref="Entity"/> is an ID in the <see cref="World"/> which has a set of components associated with it.
/// </summary>
public readonly partial record struct Entity : IComparable<Entity>
{
    /// <summary>
    /// Check if this entity still exists.
    /// </summary>
    public bool IsAlive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (Index == 0)
            {
                return false;
            }

            ref var location = ref World.Entities.GetLocation(Index);
            return location.Version == Version && location.Archetype != null!;
        }
    }

    /// <summary>
    /// Check if this entity is pending creation.
    /// </summary>
    public bool IsPending
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (Index == 0)
            {
                return false;
            }

            ref var location = ref World.Entities.GetLocation(Index);
            return location.Version == Version && location.Archetype == null!;
        }
    }

    /// <summary>
    /// Check if this entity is default initialized.
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
    /// The archetype of the entity.
    /// </summary>
    public Archetype Archetype => World.Entities.GetLocation(EntityId).Archetype;

    /// <summary>
    /// The index of this entity.
    /// May be re-used very quickly once an <see cref="Entity"/> is destroyed.
    /// </summary>
    /// <remarks>
    /// This will never be 0 for a valid entity.
    /// </remarks>
    public int Index => EntityId.Index;

    /// <summary>
    /// The version of this entity.
    /// May be re-used, but only after the full 32 bit counter has been overflowed for this specific index.
    /// </summary>
    /// <remarks>
    /// This can be 0 for a valid entity.
    /// </remarks>
    public uint Version => EntityId.Version;

    /// <summary>
    /// The raw ID of this <see cref="Entity"/>.
    /// </summary>
    internal readonly EntityId EntityId;

    /// <summary>
    /// The set of components this entity has, including interface components.
    /// </summary>
    public IReadOnlyOrderedListSet<TypeId> Types => Archetype.Types;

    /// <summary>
    /// The set of components this entity has.
    /// </summary>
    public IReadOnlyOrderedListSet<ComponentId> Components => Archetype.Components;

    /// <summary>
    /// The set of interface components this entity has.
    /// </summary>
    public IReadOnlyOrderedListSet<InterfaceId> Interfaces => Archetype.Interfaces;

    /// <summary>
    /// Get a boxed array of all components.
    /// <para/>
    /// This is very slow and the returned data is a copy of the original data.
    /// Avoid using this for anything other than debugging!
    /// </summary>
    public object[] BoxedComponents => Components.Select(GetBoxed).ToArray();

    internal Entity(EntityId id, EcsWorld world)
    {
        EntityId = id;
        World = world;
    }

    /// <summary>
    /// Check if this entity has a component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Has<T>() where T : IComponent
    {
        return Components.Contains(ComponentId.Get<T>());
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T Get<T>() where T : IComponent
    {
        ref var location = ref World.Entities.GetLocation(EntityId);
        return ref location.Archetype.Get<T>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type.
    /// If the entity does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Ref<T> GetRef<T>() where T : IComponent
    {
        ref var location = ref World.Entities.GetLocation(EntityId);
        return location.Archetype.GetRef<T>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type.
    /// If the entity does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EcsRef<T> GetEcsRef<T>() where T : IComponent
    {
        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        GuardUtility.IsTrue(Has<T>(), $"Component does not exist on entity: {GetType().Name}");

        return new EcsRef<T>(this);
    }

    /// <summary>
    /// Get a reference to a component of the given type.
    /// No exception will be thrown if the entity does not have this component.
    /// </summary>
    /// <remarks>
    /// This is useful when the entity has pending command buffer changes.
    /// Accessing the component will still validate the reference.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public EcsRef<T> GetEcsRefUnchecked<T>() where T : IComponent
    {
        return new EcsRef<T>(this);
    }

    /// <summary>
    /// Get a boxed copy of a component from this entity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object GetBoxed(ComponentId id)
    {
        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        GuardUtility.IsTrue(Components.Contains(id), "Entity does not have the specified component");

        ref var location = ref World.Entities.GetLocation(EntityId);
        return location.Archetype.GetComponentArray(id).GetValue(location.IndexInArchetype)!;
    }

    /// <summary>
    /// Resolves the specified interface component from the entity's archetype.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object Resolve(InterfaceId interfaceId)
    {
        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        return location.Archetype.Resolve(interfaceId);
    }

    /// <summary>
    /// Resolves the specified interface component from the entity's archetype.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Resolve<T>() where T : class, IInterfaceComponent
    {
        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        return location.Archetype.Resolve<T>();
    }

    /// <summary>
    /// Resolves the specified interface component from the entity's archetype as an interface bound to this entity.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public InterfaceBinding<T> ResolveBinding<T>() where T : class, IInterfaceComponent
    {
        return Resolve<T>().Bind(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        if (World == null)
        {
            return "0:0:0";
        }

        var result = $"{World.Id}:{Index}:{Version}";
        var location = World.Entities.GetLocation(EntityId.Index);
        if (EntityId.Version != location.Version)
        {
            result += " (Destroyed)";
        }
        else if (location.Archetype == null!)
        {
            result += " (Pending)";
        }

        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(Entity other)
    {
        return EntityId.CompareTo(other.EntityId);
    }
}
