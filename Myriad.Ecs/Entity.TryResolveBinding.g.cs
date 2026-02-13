#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Exanite.Myriad.Ecs;

namespace Exanite.Myriad.Ecs;

public readonly partial record struct Entity
{
    /// <summary>
    /// Tries to resolve the specified interface components from the entity's archetype as interfaces bound to this entity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolveBinding<T0>(
        [NotNullWhen(true)]out InterfaceBinding<T0> binding0)
        where T0 : class, IInterfaceComponent
    {
        if (!TryResolve(
            out T0? instance0
        ))
        {
            binding0 = default;
            return false;
        }

        binding0 = instance0.Bind(this);

        return true;
    }

    /// <summary>
    /// Tries to resolve the specified interface components from the entity's archetype as interfaces bound to this entity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolveBinding<T0, T1>(
        [NotNullWhen(true)]out InterfaceBinding<T0> binding0,
        [NotNullWhen(true)]out InterfaceBinding<T1> binding1)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
    {
        if (!TryResolve(
            out T0? instance0,
            out T1? instance1
        ))
        {
            binding0 = default;
            binding1 = default;
            return false;
        }

        binding0 = instance0.Bind(this);
        binding1 = instance1.Bind(this);

        return true;
    }

    /// <summary>
    /// Tries to resolve the specified interface components from the entity's archetype as interfaces bound to this entity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolveBinding<T0, T1, T2>(
        [NotNullWhen(true)]out InterfaceBinding<T0> binding0,
        [NotNullWhen(true)]out InterfaceBinding<T1> binding1,
        [NotNullWhen(true)]out InterfaceBinding<T2> binding2)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
    {
        if (!TryResolve(
            out T0? instance0,
            out T1? instance1,
            out T2? instance2
        ))
        {
            binding0 = default;
            binding1 = default;
            binding2 = default;
            return false;
        }

        binding0 = instance0.Bind(this);
        binding1 = instance1.Bind(this);
        binding2 = instance2.Bind(this);

        return true;
    }

    /// <summary>
    /// Tries to resolve the specified interface components from the entity's archetype as interfaces bound to this entity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolveBinding<T0, T1, T2, T3>(
        [NotNullWhen(true)]out InterfaceBinding<T0> binding0,
        [NotNullWhen(true)]out InterfaceBinding<T1> binding1,
        [NotNullWhen(true)]out InterfaceBinding<T2> binding2,
        [NotNullWhen(true)]out InterfaceBinding<T3> binding3)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
        where T3 : class, IInterfaceComponent
    {
        if (!TryResolve(
            out T0? instance0,
            out T1? instance1,
            out T2? instance2,
            out T3? instance3
        ))
        {
            binding0 = default;
            binding1 = default;
            binding2 = default;
            binding3 = default;
            return false;
        }

        binding0 = instance0.Bind(this);
        binding1 = instance1.Bind(this);
        binding2 = instance2.Bind(this);
        binding3 = instance3.Bind(this);

        return true;
    }

    /// <summary>
    /// Tries to resolve the specified interface components from the entity's archetype as interfaces bound to this entity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolveBinding<T0, T1, T2, T3, T4>(
        [NotNullWhen(true)]out InterfaceBinding<T0> binding0,
        [NotNullWhen(true)]out InterfaceBinding<T1> binding1,
        [NotNullWhen(true)]out InterfaceBinding<T2> binding2,
        [NotNullWhen(true)]out InterfaceBinding<T3> binding3,
        [NotNullWhen(true)]out InterfaceBinding<T4> binding4)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
        where T3 : class, IInterfaceComponent
        where T4 : class, IInterfaceComponent
    {
        if (!TryResolve(
            out T0? instance0,
            out T1? instance1,
            out T2? instance2,
            out T3? instance3,
            out T4? instance4
        ))
        {
            binding0 = default;
            binding1 = default;
            binding2 = default;
            binding3 = default;
            binding4 = default;
            return false;
        }

        binding0 = instance0.Bind(this);
        binding1 = instance1.Bind(this);
        binding2 = instance2.Bind(this);
        binding3 = instance3.Bind(this);
        binding4 = instance4.Bind(this);

        return true;
    }

    /// <summary>
    /// Tries to resolve the specified interface components from the entity's archetype as interfaces bound to this entity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolveBinding<T0, T1, T2, T3, T4, T5>(
        [NotNullWhen(true)]out InterfaceBinding<T0> binding0,
        [NotNullWhen(true)]out InterfaceBinding<T1> binding1,
        [NotNullWhen(true)]out InterfaceBinding<T2> binding2,
        [NotNullWhen(true)]out InterfaceBinding<T3> binding3,
        [NotNullWhen(true)]out InterfaceBinding<T4> binding4,
        [NotNullWhen(true)]out InterfaceBinding<T5> binding5)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
        where T3 : class, IInterfaceComponent
        where T4 : class, IInterfaceComponent
        where T5 : class, IInterfaceComponent
    {
        if (!TryResolve(
            out T0? instance0,
            out T1? instance1,
            out T2? instance2,
            out T3? instance3,
            out T4? instance4,
            out T5? instance5
        ))
        {
            binding0 = default;
            binding1 = default;
            binding2 = default;
            binding3 = default;
            binding4 = default;
            binding5 = default;
            return false;
        }

        binding0 = instance0.Bind(this);
        binding1 = instance1.Bind(this);
        binding2 = instance2.Bind(this);
        binding3 = instance3.Bind(this);
        binding4 = instance4.Bind(this);
        binding5 = instance5.Bind(this);

        return true;
    }

    /// <summary>
    /// Tries to resolve the specified interface components from the entity's archetype as interfaces bound to this entity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolveBinding<T0, T1, T2, T3, T4, T5, T6>(
        [NotNullWhen(true)]out InterfaceBinding<T0> binding0,
        [NotNullWhen(true)]out InterfaceBinding<T1> binding1,
        [NotNullWhen(true)]out InterfaceBinding<T2> binding2,
        [NotNullWhen(true)]out InterfaceBinding<T3> binding3,
        [NotNullWhen(true)]out InterfaceBinding<T4> binding4,
        [NotNullWhen(true)]out InterfaceBinding<T5> binding5,
        [NotNullWhen(true)]out InterfaceBinding<T6> binding6)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
        where T3 : class, IInterfaceComponent
        where T4 : class, IInterfaceComponent
        where T5 : class, IInterfaceComponent
        where T6 : class, IInterfaceComponent
    {
        if (!TryResolve(
            out T0? instance0,
            out T1? instance1,
            out T2? instance2,
            out T3? instance3,
            out T4? instance4,
            out T5? instance5,
            out T6? instance6
        ))
        {
            binding0 = default;
            binding1 = default;
            binding2 = default;
            binding3 = default;
            binding4 = default;
            binding5 = default;
            binding6 = default;
            return false;
        }

        binding0 = instance0.Bind(this);
        binding1 = instance1.Bind(this);
        binding2 = instance2.Bind(this);
        binding3 = instance3.Bind(this);
        binding4 = instance4.Bind(this);
        binding5 = instance5.Bind(this);
        binding6 = instance6.Bind(this);

        return true;
    }

    /// <summary>
    /// Tries to resolve the specified interface components from the entity's archetype as interfaces bound to this entity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryResolveBinding<T0, T1, T2, T3, T4, T5, T6, T7>(
        [NotNullWhen(true)]out InterfaceBinding<T0> binding0,
        [NotNullWhen(true)]out InterfaceBinding<T1> binding1,
        [NotNullWhen(true)]out InterfaceBinding<T2> binding2,
        [NotNullWhen(true)]out InterfaceBinding<T3> binding3,
        [NotNullWhen(true)]out InterfaceBinding<T4> binding4,
        [NotNullWhen(true)]out InterfaceBinding<T5> binding5,
        [NotNullWhen(true)]out InterfaceBinding<T6> binding6,
        [NotNullWhen(true)]out InterfaceBinding<T7> binding7)
        where T0 : class, IInterfaceComponent
        where T1 : class, IInterfaceComponent
        where T2 : class, IInterfaceComponent
        where T3 : class, IInterfaceComponent
        where T4 : class, IInterfaceComponent
        where T5 : class, IInterfaceComponent
        where T6 : class, IInterfaceComponent
        where T7 : class, IInterfaceComponent
    {
        if (!TryResolve(
            out T0? instance0,
            out T1? instance1,
            out T2? instance2,
            out T3? instance3,
            out T4? instance4,
            out T5? instance5,
            out T6? instance6,
            out T7? instance7
        ))
        {
            binding0 = default;
            binding1 = default;
            binding2 = default;
            binding3 = default;
            binding4 = default;
            binding5 = default;
            binding6 = default;
            binding7 = default;
            return false;
        }

        binding0 = instance0.Bind(this);
        binding1 = instance1.Bind(this);
        binding2 = instance2.Bind(this);
        binding3 = instance3.Bind(this);
        binding4 = instance4.Bind(this);
        binding5 = instance5.Bind(this);
        binding6 = instance6.Bind(this);
        binding7 = instance7.Bind(this);

        return true;
    }

}
