#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs;

namespace Exanite.Myriad.Ecs;

public readonly partial record struct Entity
{
    /// <summary>
    /// Tries to resolve the specified interface components from the entity's archetype.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolve<T0>(
        [NotNullWhen(true)] out T0? instance0)
        where T0 : class, IInterfaceComponent
    {
        instance0 = default;

        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        var archetype = location.Archetype;

        if (!archetype.TryResolve<T0>(out instance0)) return false;

        return true;
    }

    /// <summary>
    /// Tries to resolve the specified interface components from the entity's archetype.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolve<T0, T1>(
        [NotNullWhen(true)] out T0? instance0,
        [NotNullWhen(true)] out T1? instance1)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
    {
        instance0 = default;
        instance1 = default;

        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        var archetype = location.Archetype;

        if (!archetype.TryResolve<T0>(out instance0)) return false;
        if (!archetype.TryResolve<T1>(out instance1)) return false;

        return true;
    }

    /// <summary>
    /// Tries to resolve the specified interface components from the entity's archetype.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolve<T0, T1, T2>(
        [NotNullWhen(true)] out T0? instance0,
        [NotNullWhen(true)] out T1? instance1,
        [NotNullWhen(true)] out T2? instance2)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
    {
        instance0 = default;
        instance1 = default;
        instance2 = default;

        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        var archetype = location.Archetype;

        if (!archetype.TryResolve<T0>(out instance0)) return false;
        if (!archetype.TryResolve<T1>(out instance1)) return false;
        if (!archetype.TryResolve<T2>(out instance2)) return false;

        return true;
    }

    /// <summary>
    /// Tries to resolve the specified interface components from the entity's archetype.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolve<T0, T1, T2, T3>(
        [NotNullWhen(true)] out T0? instance0,
        [NotNullWhen(true)] out T1? instance1,
        [NotNullWhen(true)] out T2? instance2,
        [NotNullWhen(true)] out T3? instance3)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
        where T3 : class, IInterfaceComponent
    {
        instance0 = default;
        instance1 = default;
        instance2 = default;
        instance3 = default;

        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        var archetype = location.Archetype;

        if (!archetype.TryResolve<T0>(out instance0)) return false;
        if (!archetype.TryResolve<T1>(out instance1)) return false;
        if (!archetype.TryResolve<T2>(out instance2)) return false;
        if (!archetype.TryResolve<T3>(out instance3)) return false;

        return true;
    }

    /// <summary>
    /// Tries to resolve the specified interface components from the entity's archetype.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolve<T0, T1, T2, T3, T4>(
        [NotNullWhen(true)] out T0? instance0,
        [NotNullWhen(true)] out T1? instance1,
        [NotNullWhen(true)] out T2? instance2,
        [NotNullWhen(true)] out T3? instance3,
        [NotNullWhen(true)] out T4? instance4)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
        where T3 : class, IInterfaceComponent
        where T4 : class, IInterfaceComponent
    {
        instance0 = default;
        instance1 = default;
        instance2 = default;
        instance3 = default;
        instance4 = default;

        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        var archetype = location.Archetype;

        if (!archetype.TryResolve<T0>(out instance0)) return false;
        if (!archetype.TryResolve<T1>(out instance1)) return false;
        if (!archetype.TryResolve<T2>(out instance2)) return false;
        if (!archetype.TryResolve<T3>(out instance3)) return false;
        if (!archetype.TryResolve<T4>(out instance4)) return false;

        return true;
    }

    /// <summary>
    /// Tries to resolve the specified interface components from the entity's archetype.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolve<T0, T1, T2, T3, T4, T5>(
        [NotNullWhen(true)] out T0? instance0,
        [NotNullWhen(true)] out T1? instance1,
        [NotNullWhen(true)] out T2? instance2,
        [NotNullWhen(true)] out T3? instance3,
        [NotNullWhen(true)] out T4? instance4,
        [NotNullWhen(true)] out T5? instance5)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
        where T3 : class, IInterfaceComponent
        where T4 : class, IInterfaceComponent
        where T5 : class, IInterfaceComponent
    {
        instance0 = default;
        instance1 = default;
        instance2 = default;
        instance3 = default;
        instance4 = default;
        instance5 = default;

        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        var archetype = location.Archetype;

        if (!archetype.TryResolve<T0>(out instance0)) return false;
        if (!archetype.TryResolve<T1>(out instance1)) return false;
        if (!archetype.TryResolve<T2>(out instance2)) return false;
        if (!archetype.TryResolve<T3>(out instance3)) return false;
        if (!archetype.TryResolve<T4>(out instance4)) return false;
        if (!archetype.TryResolve<T5>(out instance5)) return false;

        return true;
    }

    /// <summary>
    /// Tries to resolve the specified interface components from the entity's archetype.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolve<T0, T1, T2, T3, T4, T5, T6>(
        [NotNullWhen(true)] out T0? instance0,
        [NotNullWhen(true)] out T1? instance1,
        [NotNullWhen(true)] out T2? instance2,
        [NotNullWhen(true)] out T3? instance3,
        [NotNullWhen(true)] out T4? instance4,
        [NotNullWhen(true)] out T5? instance5,
        [NotNullWhen(true)] out T6? instance6)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
        where T3 : class, IInterfaceComponent
        where T4 : class, IInterfaceComponent
        where T5 : class, IInterfaceComponent
        where T6 : class, IInterfaceComponent
    {
        instance0 = default;
        instance1 = default;
        instance2 = default;
        instance3 = default;
        instance4 = default;
        instance5 = default;
        instance6 = default;

        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        var archetype = location.Archetype;

        if (!archetype.TryResolve<T0>(out instance0)) return false;
        if (!archetype.TryResolve<T1>(out instance1)) return false;
        if (!archetype.TryResolve<T2>(out instance2)) return false;
        if (!archetype.TryResolve<T3>(out instance3)) return false;
        if (!archetype.TryResolve<T4>(out instance4)) return false;
        if (!archetype.TryResolve<T5>(out instance5)) return false;
        if (!archetype.TryResolve<T6>(out instance6)) return false;

        return true;
    }

    /// <summary>
    /// Tries to resolve the specified interface components from the entity's archetype.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolve<T0, T1, T2, T3, T4, T5, T6, T7>(
        [NotNullWhen(true)] out T0? instance0,
        [NotNullWhen(true)] out T1? instance1,
        [NotNullWhen(true)] out T2? instance2,
        [NotNullWhen(true)] out T3? instance3,
        [NotNullWhen(true)] out T4? instance4,
        [NotNullWhen(true)] out T5? instance5,
        [NotNullWhen(true)] out T6? instance6,
        [NotNullWhen(true)] out T7? instance7)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
        where T3 : class, IInterfaceComponent
        where T4 : class, IInterfaceComponent
        where T5 : class, IInterfaceComponent
        where T6 : class, IInterfaceComponent
        where T7 : class, IInterfaceComponent
    {
        instance0 = default;
        instance1 = default;
        instance2 = default;
        instance3 = default;
        instance4 = default;
        instance5 = default;
        instance6 = default;
        instance7 = default;

        GuardUtility.IsTrue(IsAlive, "Entity is not alive");
        var location = World.Entities.GetLocation(EntityId);
        var archetype = location.Archetype;

        if (!archetype.TryResolve<T0>(out instance0)) return false;
        if (!archetype.TryResolve<T1>(out instance1)) return false;
        if (!archetype.TryResolve<T2>(out instance2)) return false;
        if (!archetype.TryResolve<T3>(out instance3)) return false;
        if (!archetype.TryResolve<T4>(out instance4)) return false;
        if (!archetype.TryResolve<T5>(out instance5)) return false;
        if (!archetype.TryResolve<T6>(out instance6)) return false;
        if (!archetype.TryResolve<T7>(out instance7)) return false;

        return true;
    }

}
