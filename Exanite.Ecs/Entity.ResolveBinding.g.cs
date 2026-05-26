#nullable enable

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Exanite.Myriad.Ecs;

namespace Exanite.Myriad.Ecs;

public readonly partial record struct Entity
{
    /// <summary>
    /// Resolves the specified interface components from the entity's archetype as interfaces bound to this entity.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResolveBinding<T0>(
        out InterfaceBinding<T0> binding0)
        where T0 : class, IEcsInterface
    {
        Resolve(
            out T0 instance0
        );

        binding0 = instance0.Bind(this);
    }

    /// <summary>
    /// Resolves the specified interface components from the entity's archetype as interfaces bound to this entity.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResolveBinding<T0, T1>(
        out InterfaceBinding<T0> binding0,
        out InterfaceBinding<T1> binding1)
        where T0 : class, IEcsInterface
        where T1 : class, IEcsInterface
    {
        Resolve(
            out T0 instance0,
            out T1 instance1
        );

        binding0 = instance0.Bind(this);
        binding1 = instance1.Bind(this);
    }

    /// <summary>
    /// Resolves the specified interface components from the entity's archetype as interfaces bound to this entity.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResolveBinding<T0, T1, T2>(
        out InterfaceBinding<T0> binding0,
        out InterfaceBinding<T1> binding1,
        out InterfaceBinding<T2> binding2)
        where T0 : class, IEcsInterface
        where T1 : class, IEcsInterface
        where T2 : class, IEcsInterface
    {
        Resolve(
            out T0 instance0,
            out T1 instance1,
            out T2 instance2
        );

        binding0 = instance0.Bind(this);
        binding1 = instance1.Bind(this);
        binding2 = instance2.Bind(this);
    }

    /// <summary>
    /// Resolves the specified interface components from the entity's archetype as interfaces bound to this entity.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResolveBinding<T0, T1, T2, T3>(
        out InterfaceBinding<T0> binding0,
        out InterfaceBinding<T1> binding1,
        out InterfaceBinding<T2> binding2,
        out InterfaceBinding<T3> binding3)
        where T0 : class, IEcsInterface
        where T1 : class, IEcsInterface
        where T2 : class, IEcsInterface
        where T3 : class, IEcsInterface
    {
        Resolve(
            out T0 instance0,
            out T1 instance1,
            out T2 instance2,
            out T3 instance3
        );

        binding0 = instance0.Bind(this);
        binding1 = instance1.Bind(this);
        binding2 = instance2.Bind(this);
        binding3 = instance3.Bind(this);
    }

    /// <summary>
    /// Resolves the specified interface components from the entity's archetype as interfaces bound to this entity.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResolveBinding<T0, T1, T2, T3, T4>(
        out InterfaceBinding<T0> binding0,
        out InterfaceBinding<T1> binding1,
        out InterfaceBinding<T2> binding2,
        out InterfaceBinding<T3> binding3,
        out InterfaceBinding<T4> binding4)
        where T0 : class, IEcsInterface
        where T1 : class, IEcsInterface
        where T2 : class, IEcsInterface
        where T3 : class, IEcsInterface
        where T4 : class, IEcsInterface
    {
        Resolve(
            out T0 instance0,
            out T1 instance1,
            out T2 instance2,
            out T3 instance3,
            out T4 instance4
        );

        binding0 = instance0.Bind(this);
        binding1 = instance1.Bind(this);
        binding2 = instance2.Bind(this);
        binding3 = instance3.Bind(this);
        binding4 = instance4.Bind(this);
    }

    /// <summary>
    /// Resolves the specified interface components from the entity's archetype as interfaces bound to this entity.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResolveBinding<T0, T1, T2, T3, T4, T5>(
        out InterfaceBinding<T0> binding0,
        out InterfaceBinding<T1> binding1,
        out InterfaceBinding<T2> binding2,
        out InterfaceBinding<T3> binding3,
        out InterfaceBinding<T4> binding4,
        out InterfaceBinding<T5> binding5)
        where T0 : class, IEcsInterface
        where T1 : class, IEcsInterface
        where T2 : class, IEcsInterface
        where T3 : class, IEcsInterface
        where T4 : class, IEcsInterface
        where T5 : class, IEcsInterface
    {
        Resolve(
            out T0 instance0,
            out T1 instance1,
            out T2 instance2,
            out T3 instance3,
            out T4 instance4,
            out T5 instance5
        );

        binding0 = instance0.Bind(this);
        binding1 = instance1.Bind(this);
        binding2 = instance2.Bind(this);
        binding3 = instance3.Bind(this);
        binding4 = instance4.Bind(this);
        binding5 = instance5.Bind(this);
    }

    /// <summary>
    /// Resolves the specified interface components from the entity's archetype as interfaces bound to this entity.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResolveBinding<T0, T1, T2, T3, T4, T5, T6>(
        out InterfaceBinding<T0> binding0,
        out InterfaceBinding<T1> binding1,
        out InterfaceBinding<T2> binding2,
        out InterfaceBinding<T3> binding3,
        out InterfaceBinding<T4> binding4,
        out InterfaceBinding<T5> binding5,
        out InterfaceBinding<T6> binding6)
        where T0 : class, IEcsInterface
        where T1 : class, IEcsInterface
        where T2 : class, IEcsInterface
        where T3 : class, IEcsInterface
        where T4 : class, IEcsInterface
        where T5 : class, IEcsInterface
        where T6 : class, IEcsInterface
    {
        Resolve(
            out T0 instance0,
            out T1 instance1,
            out T2 instance2,
            out T3 instance3,
            out T4 instance4,
            out T5 instance5,
            out T6 instance6
        );

        binding0 = instance0.Bind(this);
        binding1 = instance1.Bind(this);
        binding2 = instance2.Bind(this);
        binding3 = instance3.Bind(this);
        binding4 = instance4.Bind(this);
        binding5 = instance5.Bind(this);
        binding6 = instance6.Bind(this);
    }

    /// <summary>
    /// Resolves the specified interface components from the entity's archetype as interfaces bound to this entity.
    /// Throws if it fails.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResolveBinding<T0, T1, T2, T3, T4, T5, T6, T7>(
        out InterfaceBinding<T0> binding0,
        out InterfaceBinding<T1> binding1,
        out InterfaceBinding<T2> binding2,
        out InterfaceBinding<T3> binding3,
        out InterfaceBinding<T4> binding4,
        out InterfaceBinding<T5> binding5,
        out InterfaceBinding<T6> binding6,
        out InterfaceBinding<T7> binding7)
        where T0 : class, IEcsInterface
        where T1 : class, IEcsInterface
        where T2 : class, IEcsInterface
        where T3 : class, IEcsInterface
        where T4 : class, IEcsInterface
        where T5 : class, IEcsInterface
        where T6 : class, IEcsInterface
        where T7 : class, IEcsInterface
    {
        Resolve(
            out T0 instance0,
            out T1 instance1,
            out T2 instance2,
            out T3 instance3,
            out T4 instance4,
            out T5 instance5,
            out T6 instance6,
            out T7 instance7
        );

        binding0 = instance0.Bind(this);
        binding1 = instance1.Bind(this);
        binding2 = instance2.Bind(this);
        binding3 = instance3.Bind(this);
        binding4 = instance4.Bind(this);
        binding5 = instance5.Bind(this);
        binding6 = instance6.Bind(this);
        binding7 = instance7.Bind(this);
    }

}
