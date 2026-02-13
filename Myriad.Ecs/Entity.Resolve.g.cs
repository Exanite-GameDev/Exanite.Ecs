#nullable enable

using System.Runtime.CompilerServices;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs;

namespace Exanite.Myriad.Ecs;

public readonly partial record struct Entity
{
    /// <summary>
    /// Resolves the specified interface components from the entity's archetype.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Resolve<T0>(
        out T0 instance0)
        where T0 : class, IInterfaceComponent
    {
        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        var archetype = location.Archetype;

        instance0 = archetype.Resolve<T0>();
    }

    /// <summary>
    /// Resolves the specified interface components from the entity's archetype.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Resolve<T0, T1>(
        out T0 instance0,
        out T1 instance1)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
    {
        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        var archetype = location.Archetype;

        instance0 = archetype.Resolve<T0>();
        instance1 = archetype.Resolve<T1>();
    }

    /// <summary>
    /// Resolves the specified interface components from the entity's archetype.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Resolve<T0, T1, T2>(
        out T0 instance0,
        out T1 instance1,
        out T2 instance2)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
    {
        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        var archetype = location.Archetype;

        instance0 = archetype.Resolve<T0>();
        instance1 = archetype.Resolve<T1>();
        instance2 = archetype.Resolve<T2>();
    }

    /// <summary>
    /// Resolves the specified interface components from the entity's archetype.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Resolve<T0, T1, T2, T3>(
        out T0 instance0,
        out T1 instance1,
        out T2 instance2,
        out T3 instance3)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
        where T3 : class, IInterfaceComponent
    {
        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        var archetype = location.Archetype;

        instance0 = archetype.Resolve<T0>();
        instance1 = archetype.Resolve<T1>();
        instance2 = archetype.Resolve<T2>();
        instance3 = archetype.Resolve<T3>();
    }

    /// <summary>
    /// Resolves the specified interface components from the entity's archetype.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Resolve<T0, T1, T2, T3, T4>(
        out T0 instance0,
        out T1 instance1,
        out T2 instance2,
        out T3 instance3,
        out T4 instance4)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
        where T3 : class, IInterfaceComponent
        where T4 : class, IInterfaceComponent
    {
        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        var archetype = location.Archetype;

        instance0 = archetype.Resolve<T0>();
        instance1 = archetype.Resolve<T1>();
        instance2 = archetype.Resolve<T2>();
        instance3 = archetype.Resolve<T3>();
        instance4 = archetype.Resolve<T4>();
    }

    /// <summary>
    /// Resolves the specified interface components from the entity's archetype.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Resolve<T0, T1, T2, T3, T4, T5>(
        out T0 instance0,
        out T1 instance1,
        out T2 instance2,
        out T3 instance3,
        out T4 instance4,
        out T5 instance5)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
        where T3 : class, IInterfaceComponent
        where T4 : class, IInterfaceComponent
        where T5 : class, IInterfaceComponent
    {
        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        var archetype = location.Archetype;

        instance0 = archetype.Resolve<T0>();
        instance1 = archetype.Resolve<T1>();
        instance2 = archetype.Resolve<T2>();
        instance3 = archetype.Resolve<T3>();
        instance4 = archetype.Resolve<T4>();
        instance5 = archetype.Resolve<T5>();
    }

    /// <summary>
    /// Resolves the specified interface components from the entity's archetype.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Resolve<T0, T1, T2, T3, T4, T5, T6>(
        out T0 instance0,
        out T1 instance1,
        out T2 instance2,
        out T3 instance3,
        out T4 instance4,
        out T5 instance5,
        out T6 instance6)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
        where T3 : class, IInterfaceComponent
        where T4 : class, IInterfaceComponent
        where T5 : class, IInterfaceComponent
        where T6 : class, IInterfaceComponent
    {
        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        var archetype = location.Archetype;

        instance0 = archetype.Resolve<T0>();
        instance1 = archetype.Resolve<T1>();
        instance2 = archetype.Resolve<T2>();
        instance3 = archetype.Resolve<T3>();
        instance4 = archetype.Resolve<T4>();
        instance5 = archetype.Resolve<T5>();
        instance6 = archetype.Resolve<T6>();
    }

    /// <summary>
    /// Resolves the specified interface components from the entity's archetype.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Resolve<T0, T1, T2, T3, T4, T5, T6, T7>(
        out T0 instance0,
        out T1 instance1,
        out T2 instance2,
        out T3 instance3,
        out T4 instance4,
        out T5 instance5,
        out T6 instance6,
        out T7 instance7)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
        where T3 : class, IInterfaceComponent
        where T4 : class, IInterfaceComponent
        where T5 : class, IInterfaceComponent
        where T6 : class, IInterfaceComponent
        where T7 : class, IInterfaceComponent
    {
        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        var archetype = location.Archetype;

        instance0 = archetype.Resolve<T0>();
        instance1 = archetype.Resolve<T1>();
        instance2 = archetype.Resolve<T2>();
        instance3 = archetype.Resolve<T3>();
        instance4 = archetype.Resolve<T4>();
        instance5 = archetype.Resolve<T5>();
        instance6 = archetype.Resolve<T6>();
        instance7 = archetype.Resolve<T7>();
    }

}
