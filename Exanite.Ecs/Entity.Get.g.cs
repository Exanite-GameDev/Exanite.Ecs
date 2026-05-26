#nullable enable

using System.Runtime.CompilerServices;
using Exanite.Core.Runtime;

namespace Exanite.Ecs;

public readonly partial record struct Entity
{
    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Get<T0>(
        out Ref<T0> ref0)
        where T0 : IEcsComponent
    {
        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            throw new GuardException("Entity is not alive");
        }

        ref var location = ref locationRef.Value;
        var archetype = location.Archetype;
        ref0 = archetype.GetRef<T0>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Get<T0, T1>(
        out Ref<T0> ref0,
        out Ref<T1> ref1)
        where T0 : IEcsComponent
        where T1 : IEcsComponent
    {
        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            throw new GuardException("Entity is not alive");
        }

        ref var location = ref locationRef.Value;
        var archetype = location.Archetype;
        ref0 = archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = archetype.GetRef<T1>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Get<T0, T1, T2>(
        out Ref<T0> ref0,
        out Ref<T1> ref1,
        out Ref<T2> ref2)
        where T0 : IEcsComponent
        where T1 : IEcsComponent
        where T2 : IEcsComponent
    {
        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            throw new GuardException("Entity is not alive");
        }

        ref var location = ref locationRef.Value;
        var archetype = location.Archetype;
        ref0 = archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = archetype.GetRef<T2>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Get<T0, T1, T2, T3>(
        out Ref<T0> ref0,
        out Ref<T1> ref1,
        out Ref<T2> ref2,
        out Ref<T3> ref3)
        where T0 : IEcsComponent
        where T1 : IEcsComponent
        where T2 : IEcsComponent
        where T3 : IEcsComponent
    {
        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            throw new GuardException("Entity is not alive");
        }

        ref var location = ref locationRef.Value;
        var archetype = location.Archetype;
        ref0 = archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = archetype.GetRef<T3>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Get<T0, T1, T2, T3, T4>(
        out Ref<T0> ref0,
        out Ref<T1> ref1,
        out Ref<T2> ref2,
        out Ref<T3> ref3,
        out Ref<T4> ref4)
        where T0 : IEcsComponent
        where T1 : IEcsComponent
        where T2 : IEcsComponent
        where T3 : IEcsComponent
        where T4 : IEcsComponent
    {
        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            throw new GuardException("Entity is not alive");
        }

        ref var location = ref locationRef.Value;
        var archetype = location.Archetype;
        ref0 = archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = archetype.GetRef<T4>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Get<T0, T1, T2, T3, T4, T5>(
        out Ref<T0> ref0,
        out Ref<T1> ref1,
        out Ref<T2> ref2,
        out Ref<T3> ref3,
        out Ref<T4> ref4,
        out Ref<T5> ref5)
        where T0 : IEcsComponent
        where T1 : IEcsComponent
        where T2 : IEcsComponent
        where T3 : IEcsComponent
        where T4 : IEcsComponent
        where T5 : IEcsComponent
    {
        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            throw new GuardException("Entity is not alive");
        }

        ref var location = ref locationRef.Value;
        var archetype = location.Archetype;
        ref0 = archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = archetype.GetRef<T5>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Get<T0, T1, T2, T3, T4, T5, T6>(
        out Ref<T0> ref0,
        out Ref<T1> ref1,
        out Ref<T2> ref2,
        out Ref<T3> ref3,
        out Ref<T4> ref4,
        out Ref<T5> ref5,
        out Ref<T6> ref6)
        where T0 : IEcsComponent
        where T1 : IEcsComponent
        where T2 : IEcsComponent
        where T3 : IEcsComponent
        where T4 : IEcsComponent
        where T5 : IEcsComponent
        where T6 : IEcsComponent
    {
        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            throw new GuardException("Entity is not alive");
        }

        ref var location = ref locationRef.Value;
        var archetype = location.Archetype;
        ref0 = archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = archetype.GetRef<T6>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Get<T0, T1, T2, T3, T4, T5, T6, T7>(
        out Ref<T0> ref0,
        out Ref<T1> ref1,
        out Ref<T2> ref2,
        out Ref<T3> ref3,
        out Ref<T4> ref4,
        out Ref<T5> ref5,
        out Ref<T6> ref6,
        out Ref<T7> ref7)
        where T0 : IEcsComponent
        where T1 : IEcsComponent
        where T2 : IEcsComponent
        where T3 : IEcsComponent
        where T4 : IEcsComponent
        where T5 : IEcsComponent
        where T6 : IEcsComponent
        where T7 : IEcsComponent
    {
        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            throw new GuardException("Entity is not alive");
        }

        ref var location = ref locationRef.Value;
        var archetype = location.Archetype;
        ref0 = archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = archetype.GetRef<T7>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Get<T0, T1, T2, T3, T4, T5, T6, T7, T8>(
        out Ref<T0> ref0,
        out Ref<T1> ref1,
        out Ref<T2> ref2,
        out Ref<T3> ref3,
        out Ref<T4> ref4,
        out Ref<T5> ref5,
        out Ref<T6> ref6,
        out Ref<T7> ref7,
        out Ref<T8> ref8)
        where T0 : IEcsComponent
        where T1 : IEcsComponent
        where T2 : IEcsComponent
        where T3 : IEcsComponent
        where T4 : IEcsComponent
        where T5 : IEcsComponent
        where T6 : IEcsComponent
        where T7 : IEcsComponent
        where T8 : IEcsComponent
    {
        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            throw new GuardException("Entity is not alive");
        }

        ref var location = ref locationRef.Value;
        var archetype = location.Archetype;
        ref0 = archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = archetype.GetRef<T7>(location.IndexInArchetype);
        ref8 = archetype.GetRef<T8>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Get<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        out Ref<T0> ref0,
        out Ref<T1> ref1,
        out Ref<T2> ref2,
        out Ref<T3> ref3,
        out Ref<T4> ref4,
        out Ref<T5> ref5,
        out Ref<T6> ref6,
        out Ref<T7> ref7,
        out Ref<T8> ref8,
        out Ref<T9> ref9)
        where T0 : IEcsComponent
        where T1 : IEcsComponent
        where T2 : IEcsComponent
        where T3 : IEcsComponent
        where T4 : IEcsComponent
        where T5 : IEcsComponent
        where T6 : IEcsComponent
        where T7 : IEcsComponent
        where T8 : IEcsComponent
        where T9 : IEcsComponent
    {
        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            throw new GuardException("Entity is not alive");
        }

        ref var location = ref locationRef.Value;
        var archetype = location.Archetype;
        ref0 = archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = archetype.GetRef<T7>(location.IndexInArchetype);
        ref8 = archetype.GetRef<T8>(location.IndexInArchetype);
        ref9 = archetype.GetRef<T9>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Get<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        out Ref<T0> ref0,
        out Ref<T1> ref1,
        out Ref<T2> ref2,
        out Ref<T3> ref3,
        out Ref<T4> ref4,
        out Ref<T5> ref5,
        out Ref<T6> ref6,
        out Ref<T7> ref7,
        out Ref<T8> ref8,
        out Ref<T9> ref9,
        out Ref<T10> ref10)
        where T0 : IEcsComponent
        where T1 : IEcsComponent
        where T2 : IEcsComponent
        where T3 : IEcsComponent
        where T4 : IEcsComponent
        where T5 : IEcsComponent
        where T6 : IEcsComponent
        where T7 : IEcsComponent
        where T8 : IEcsComponent
        where T9 : IEcsComponent
        where T10 : IEcsComponent
    {
        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            throw new GuardException("Entity is not alive");
        }

        ref var location = ref locationRef.Value;
        var archetype = location.Archetype;
        ref0 = archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = archetype.GetRef<T7>(location.IndexInArchetype);
        ref8 = archetype.GetRef<T8>(location.IndexInArchetype);
        ref9 = archetype.GetRef<T9>(location.IndexInArchetype);
        ref10 = archetype.GetRef<T10>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Get<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        out Ref<T0> ref0,
        out Ref<T1> ref1,
        out Ref<T2> ref2,
        out Ref<T3> ref3,
        out Ref<T4> ref4,
        out Ref<T5> ref5,
        out Ref<T6> ref6,
        out Ref<T7> ref7,
        out Ref<T8> ref8,
        out Ref<T9> ref9,
        out Ref<T10> ref10,
        out Ref<T11> ref11)
        where T0 : IEcsComponent
        where T1 : IEcsComponent
        where T2 : IEcsComponent
        where T3 : IEcsComponent
        where T4 : IEcsComponent
        where T5 : IEcsComponent
        where T6 : IEcsComponent
        where T7 : IEcsComponent
        where T8 : IEcsComponent
        where T9 : IEcsComponent
        where T10 : IEcsComponent
        where T11 : IEcsComponent
    {
        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            throw new GuardException("Entity is not alive");
        }

        ref var location = ref locationRef.Value;
        var archetype = location.Archetype;
        ref0 = archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = archetype.GetRef<T7>(location.IndexInArchetype);
        ref8 = archetype.GetRef<T8>(location.IndexInArchetype);
        ref9 = archetype.GetRef<T9>(location.IndexInArchetype);
        ref10 = archetype.GetRef<T10>(location.IndexInArchetype);
        ref11 = archetype.GetRef<T11>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Get<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        out Ref<T0> ref0,
        out Ref<T1> ref1,
        out Ref<T2> ref2,
        out Ref<T3> ref3,
        out Ref<T4> ref4,
        out Ref<T5> ref5,
        out Ref<T6> ref6,
        out Ref<T7> ref7,
        out Ref<T8> ref8,
        out Ref<T9> ref9,
        out Ref<T10> ref10,
        out Ref<T11> ref11,
        out Ref<T12> ref12)
        where T0 : IEcsComponent
        where T1 : IEcsComponent
        where T2 : IEcsComponent
        where T3 : IEcsComponent
        where T4 : IEcsComponent
        where T5 : IEcsComponent
        where T6 : IEcsComponent
        where T7 : IEcsComponent
        where T8 : IEcsComponent
        where T9 : IEcsComponent
        where T10 : IEcsComponent
        where T11 : IEcsComponent
        where T12 : IEcsComponent
    {
        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            throw new GuardException("Entity is not alive");
        }

        ref var location = ref locationRef.Value;
        var archetype = location.Archetype;
        ref0 = archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = archetype.GetRef<T7>(location.IndexInArchetype);
        ref8 = archetype.GetRef<T8>(location.IndexInArchetype);
        ref9 = archetype.GetRef<T9>(location.IndexInArchetype);
        ref10 = archetype.GetRef<T10>(location.IndexInArchetype);
        ref11 = archetype.GetRef<T11>(location.IndexInArchetype);
        ref12 = archetype.GetRef<T12>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Get<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        out Ref<T0> ref0,
        out Ref<T1> ref1,
        out Ref<T2> ref2,
        out Ref<T3> ref3,
        out Ref<T4> ref4,
        out Ref<T5> ref5,
        out Ref<T6> ref6,
        out Ref<T7> ref7,
        out Ref<T8> ref8,
        out Ref<T9> ref9,
        out Ref<T10> ref10,
        out Ref<T11> ref11,
        out Ref<T12> ref12,
        out Ref<T13> ref13)
        where T0 : IEcsComponent
        where T1 : IEcsComponent
        where T2 : IEcsComponent
        where T3 : IEcsComponent
        where T4 : IEcsComponent
        where T5 : IEcsComponent
        where T6 : IEcsComponent
        where T7 : IEcsComponent
        where T8 : IEcsComponent
        where T9 : IEcsComponent
        where T10 : IEcsComponent
        where T11 : IEcsComponent
        where T12 : IEcsComponent
        where T13 : IEcsComponent
    {
        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            throw new GuardException("Entity is not alive");
        }

        ref var location = ref locationRef.Value;
        var archetype = location.Archetype;
        ref0 = archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = archetype.GetRef<T7>(location.IndexInArchetype);
        ref8 = archetype.GetRef<T8>(location.IndexInArchetype);
        ref9 = archetype.GetRef<T9>(location.IndexInArchetype);
        ref10 = archetype.GetRef<T10>(location.IndexInArchetype);
        ref11 = archetype.GetRef<T11>(location.IndexInArchetype);
        ref12 = archetype.GetRef<T12>(location.IndexInArchetype);
        ref13 = archetype.GetRef<T13>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Get<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        out Ref<T0> ref0,
        out Ref<T1> ref1,
        out Ref<T2> ref2,
        out Ref<T3> ref3,
        out Ref<T4> ref4,
        out Ref<T5> ref5,
        out Ref<T6> ref6,
        out Ref<T7> ref7,
        out Ref<T8> ref8,
        out Ref<T9> ref9,
        out Ref<T10> ref10,
        out Ref<T11> ref11,
        out Ref<T12> ref12,
        out Ref<T13> ref13,
        out Ref<T14> ref14)
        where T0 : IEcsComponent
        where T1 : IEcsComponent
        where T2 : IEcsComponent
        where T3 : IEcsComponent
        where T4 : IEcsComponent
        where T5 : IEcsComponent
        where T6 : IEcsComponent
        where T7 : IEcsComponent
        where T8 : IEcsComponent
        where T9 : IEcsComponent
        where T10 : IEcsComponent
        where T11 : IEcsComponent
        where T12 : IEcsComponent
        where T13 : IEcsComponent
        where T14 : IEcsComponent
    {
        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            throw new GuardException("Entity is not alive");
        }

        ref var location = ref locationRef.Value;
        var archetype = location.Archetype;
        ref0 = archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = archetype.GetRef<T7>(location.IndexInArchetype);
        ref8 = archetype.GetRef<T8>(location.IndexInArchetype);
        ref9 = archetype.GetRef<T9>(location.IndexInArchetype);
        ref10 = archetype.GetRef<T10>(location.IndexInArchetype);
        ref11 = archetype.GetRef<T11>(location.IndexInArchetype);
        ref12 = archetype.GetRef<T12>(location.IndexInArchetype);
        ref13 = archetype.GetRef<T13>(location.IndexInArchetype);
        ref14 = archetype.GetRef<T14>(location.IndexInArchetype);
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Get<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
        out Ref<T0> ref0,
        out Ref<T1> ref1,
        out Ref<T2> ref2,
        out Ref<T3> ref3,
        out Ref<T4> ref4,
        out Ref<T5> ref5,
        out Ref<T6> ref6,
        out Ref<T7> ref7,
        out Ref<T8> ref8,
        out Ref<T9> ref9,
        out Ref<T10> ref10,
        out Ref<T11> ref11,
        out Ref<T12> ref12,
        out Ref<T13> ref13,
        out Ref<T14> ref14,
        out Ref<T15> ref15)
        where T0 : IEcsComponent
        where T1 : IEcsComponent
        where T2 : IEcsComponent
        where T3 : IEcsComponent
        where T4 : IEcsComponent
        where T5 : IEcsComponent
        where T6 : IEcsComponent
        where T7 : IEcsComponent
        where T8 : IEcsComponent
        where T9 : IEcsComponent
        where T10 : IEcsComponent
        where T11 : IEcsComponent
        where T12 : IEcsComponent
        where T13 : IEcsComponent
        where T14 : IEcsComponent
        where T15 : IEcsComponent
    {
        if (IsDefault || !World.Entities.TryGetLocation(EntityId, out var locationRef))
        {
            throw new GuardException("Entity is not alive");
        }

        ref var location = ref locationRef.Value;
        var archetype = location.Archetype;
        ref0 = archetype.GetRef<T0>(location.IndexInArchetype);
        ref1 = archetype.GetRef<T1>(location.IndexInArchetype);
        ref2 = archetype.GetRef<T2>(location.IndexInArchetype);
        ref3 = archetype.GetRef<T3>(location.IndexInArchetype);
        ref4 = archetype.GetRef<T4>(location.IndexInArchetype);
        ref5 = archetype.GetRef<T5>(location.IndexInArchetype);
        ref6 = archetype.GetRef<T6>(location.IndexInArchetype);
        ref7 = archetype.GetRef<T7>(location.IndexInArchetype);
        ref8 = archetype.GetRef<T8>(location.IndexInArchetype);
        ref9 = archetype.GetRef<T9>(location.IndexInArchetype);
        ref10 = archetype.GetRef<T10>(location.IndexInArchetype);
        ref11 = archetype.GetRef<T11>(location.IndexInArchetype);
        ref12 = archetype.GetRef<T12>(location.IndexInArchetype);
        ref13 = archetype.GetRef<T13>(location.IndexInArchetype);
        ref14 = archetype.GetRef<T14>(location.IndexInArchetype);
        ref15 = archetype.GetRef<T15>(location.IndexInArchetype);
    }

}
