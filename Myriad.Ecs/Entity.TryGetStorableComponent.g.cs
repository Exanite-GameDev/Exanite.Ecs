#nullable enable

using System.Runtime.CompilerServices;
using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs;

public readonly partial record struct Entity
{
    /// <summary>
    /// Try to get a storable reference to a component of the given type.
    /// It is safe to use the returned ComponentRefs even when this method returns false,
    /// the ComponentRefs are just not guaranteed to point to a component in this case.
    /// This is because ComponentRef can check for the existence of the component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetStorableComponent<T0>(out CRef<T0> ref0) where T0 : IComponent
    {
        ref0 = new CRef<T0>(this);

        if (IsDefault || !World.TryGetStorageLocation(EntityId, out var storageLocation))
        {
            return false;
        }

        var components = storageLocation.Value.Chunk.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Try to get a storable reference to a component of the given type.
    /// It is safe to use the returned ComponentRefs even when this method returns false,
    /// the ComponentRefs are just not guaranteed to point to a component in this case.
    /// This is because ComponentRef can check for the existence of the component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetStorableComponent<T0, T1>(out CRef<T0> ref0, out CRef<T1> ref1) where T0 : IComponent where T1 : IComponent
    {
        ref0 = new CRef<T0>(this);
        ref1 = new CRef<T1>(this);

        if (IsDefault || !World.TryGetStorageLocation(EntityId, out var storageLocation))
        {
            return false;
        }

        var components = storageLocation.Value.Chunk.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Try to get a storable reference to a component of the given type.
    /// It is safe to use the returned ComponentRefs even when this method returns false,
    /// the ComponentRefs are just not guaranteed to point to a component in this case.
    /// This is because ComponentRef can check for the existence of the component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetStorableComponent<T0, T1, T2>(out CRef<T0> ref0, out CRef<T1> ref1, out CRef<T2> ref2) where T0 : IComponent where T1 : IComponent where T2 : IComponent
    {
        ref0 = new CRef<T0>(this);
        ref1 = new CRef<T1>(this);
        ref2 = new CRef<T2>(this);

        if (IsDefault || !World.TryGetStorageLocation(EntityId, out var storageLocation))
        {
            return false;
        }

        var components = storageLocation.Value.Chunk.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Try to get a storable reference to a component of the given type.
    /// It is safe to use the returned ComponentRefs even when this method returns false,
    /// the ComponentRefs are just not guaranteed to point to a component in this case.
    /// This is because ComponentRef can check for the existence of the component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetStorableComponent<T0, T1, T2, T3>(out CRef<T0> ref0, out CRef<T1> ref1, out CRef<T2> ref2, out CRef<T3> ref3) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent
    {
        ref0 = new CRef<T0>(this);
        ref1 = new CRef<T1>(this);
        ref2 = new CRef<T2>(this);
        ref3 = new CRef<T3>(this);

        if (IsDefault || !World.TryGetStorageLocation(EntityId, out var storageLocation))
        {
            return false;
        }

        var components = storageLocation.Value.Chunk.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Try to get a storable reference to a component of the given type.
    /// It is safe to use the returned ComponentRefs even when this method returns false,
    /// the ComponentRefs are just not guaranteed to point to a component in this case.
    /// This is because ComponentRef can check for the existence of the component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetStorableComponent<T0, T1, T2, T3, T4>(out CRef<T0> ref0, out CRef<T1> ref1, out CRef<T2> ref2, out CRef<T3> ref3, out CRef<T4> ref4) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent
    {
        ref0 = new CRef<T0>(this);
        ref1 = new CRef<T1>(this);
        ref2 = new CRef<T2>(this);
        ref3 = new CRef<T3>(this);
        ref4 = new CRef<T4>(this);

        if (IsDefault || !World.TryGetStorageLocation(EntityId, out var storageLocation))
        {
            return false;
        }

        var components = storageLocation.Value.Chunk.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Try to get a storable reference to a component of the given type.
    /// It is safe to use the returned ComponentRefs even when this method returns false,
    /// the ComponentRefs are just not guaranteed to point to a component in this case.
    /// This is because ComponentRef can check for the existence of the component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetStorableComponent<T0, T1, T2, T3, T4, T5>(out CRef<T0> ref0, out CRef<T1> ref1, out CRef<T2> ref2, out CRef<T3> ref3, out CRef<T4> ref4, out CRef<T5> ref5) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent
    {
        ref0 = new CRef<T0>(this);
        ref1 = new CRef<T1>(this);
        ref2 = new CRef<T2>(this);
        ref3 = new CRef<T3>(this);
        ref4 = new CRef<T4>(this);
        ref5 = new CRef<T5>(this);

        if (IsDefault || !World.TryGetStorageLocation(EntityId, out var storageLocation))
        {
            return false;
        }

        var components = storageLocation.Value.Chunk.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Try to get a storable reference to a component of the given type.
    /// It is safe to use the returned ComponentRefs even when this method returns false,
    /// the ComponentRefs are just not guaranteed to point to a component in this case.
    /// This is because ComponentRef can check for the existence of the component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetStorableComponent<T0, T1, T2, T3, T4, T5, T6>(out CRef<T0> ref0, out CRef<T1> ref1, out CRef<T2> ref2, out CRef<T3> ref3, out CRef<T4> ref4, out CRef<T5> ref5, out CRef<T6> ref6) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent
    {
        ref0 = new CRef<T0>(this);
        ref1 = new CRef<T1>(this);
        ref2 = new CRef<T2>(this);
        ref3 = new CRef<T3>(this);
        ref4 = new CRef<T4>(this);
        ref5 = new CRef<T5>(this);
        ref6 = new CRef<T6>(this);

        if (IsDefault || !World.TryGetStorageLocation(EntityId, out var storageLocation))
        {
            return false;
        }

        var components = storageLocation.Value.Chunk.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Try to get a storable reference to a component of the given type.
    /// It is safe to use the returned ComponentRefs even when this method returns false,
    /// the ComponentRefs are just not guaranteed to point to a component in this case.
    /// This is because ComponentRef can check for the existence of the component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetStorableComponent<T0, T1, T2, T3, T4, T5, T6, T7>(out CRef<T0> ref0, out CRef<T1> ref1, out CRef<T2> ref2, out CRef<T3> ref3, out CRef<T4> ref4, out CRef<T5> ref5, out CRef<T6> ref6, out CRef<T7> ref7) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent
    {
        ref0 = new CRef<T0>(this);
        ref1 = new CRef<T1>(this);
        ref2 = new CRef<T2>(this);
        ref3 = new CRef<T3>(this);
        ref4 = new CRef<T4>(this);
        ref5 = new CRef<T5>(this);
        ref6 = new CRef<T6>(this);
        ref7 = new CRef<T7>(this);

        if (IsDefault || !World.TryGetStorageLocation(EntityId, out var storageLocation))
        {
            return false;
        }

        var components = storageLocation.Value.Chunk.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Try to get a storable reference to a component of the given type.
    /// It is safe to use the returned ComponentRefs even when this method returns false,
    /// the ComponentRefs are just not guaranteed to point to a component in this case.
    /// This is because ComponentRef can check for the existence of the component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetStorableComponent<T0, T1, T2, T3, T4, T5, T6, T7, T8>(out CRef<T0> ref0, out CRef<T1> ref1, out CRef<T2> ref2, out CRef<T3> ref3, out CRef<T4> ref4, out CRef<T5> ref5, out CRef<T6> ref6, out CRef<T7> ref7, out CRef<T8> ref8) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent
    {
        ref0 = new CRef<T0>(this);
        ref1 = new CRef<T1>(this);
        ref2 = new CRef<T2>(this);
        ref3 = new CRef<T3>(this);
        ref4 = new CRef<T4>(this);
        ref5 = new CRef<T5>(this);
        ref6 = new CRef<T6>(this);
        ref7 = new CRef<T7>(this);
        ref8 = new CRef<T8>(this);

        if (IsDefault || !World.TryGetStorageLocation(EntityId, out var storageLocation))
        {
            return false;
        }

        var components = storageLocation.Value.Chunk.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        var componentId8 = ComponentId.Get<T8>();
        if (!components.Contains(componentId8))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Try to get a storable reference to a component of the given type.
    /// It is safe to use the returned ComponentRefs even when this method returns false,
    /// the ComponentRefs are just not guaranteed to point to a component in this case.
    /// This is because ComponentRef can check for the existence of the component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetStorableComponent<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(out CRef<T0> ref0, out CRef<T1> ref1, out CRef<T2> ref2, out CRef<T3> ref3, out CRef<T4> ref4, out CRef<T5> ref5, out CRef<T6> ref6, out CRef<T7> ref7, out CRef<T8> ref8, out CRef<T9> ref9) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent
    {
        ref0 = new CRef<T0>(this);
        ref1 = new CRef<T1>(this);
        ref2 = new CRef<T2>(this);
        ref3 = new CRef<T3>(this);
        ref4 = new CRef<T4>(this);
        ref5 = new CRef<T5>(this);
        ref6 = new CRef<T6>(this);
        ref7 = new CRef<T7>(this);
        ref8 = new CRef<T8>(this);
        ref9 = new CRef<T9>(this);

        if (IsDefault || !World.TryGetStorageLocation(EntityId, out var storageLocation))
        {
            return false;
        }

        var components = storageLocation.Value.Chunk.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        var componentId8 = ComponentId.Get<T8>();
        if (!components.Contains(componentId8))
        {
            return false;
        }

        var componentId9 = ComponentId.Get<T9>();
        if (!components.Contains(componentId9))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Try to get a storable reference to a component of the given type.
    /// It is safe to use the returned ComponentRefs even when this method returns false,
    /// the ComponentRefs are just not guaranteed to point to a component in this case.
    /// This is because ComponentRef can check for the existence of the component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetStorableComponent<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(out CRef<T0> ref0, out CRef<T1> ref1, out CRef<T2> ref2, out CRef<T3> ref3, out CRef<T4> ref4, out CRef<T5> ref5, out CRef<T6> ref6, out CRef<T7> ref7, out CRef<T8> ref8, out CRef<T9> ref9, out CRef<T10> ref10) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent
    {
        ref0 = new CRef<T0>(this);
        ref1 = new CRef<T1>(this);
        ref2 = new CRef<T2>(this);
        ref3 = new CRef<T3>(this);
        ref4 = new CRef<T4>(this);
        ref5 = new CRef<T5>(this);
        ref6 = new CRef<T6>(this);
        ref7 = new CRef<T7>(this);
        ref8 = new CRef<T8>(this);
        ref9 = new CRef<T9>(this);
        ref10 = new CRef<T10>(this);

        if (IsDefault || !World.TryGetStorageLocation(EntityId, out var storageLocation))
        {
            return false;
        }

        var components = storageLocation.Value.Chunk.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        var componentId8 = ComponentId.Get<T8>();
        if (!components.Contains(componentId8))
        {
            return false;
        }

        var componentId9 = ComponentId.Get<T9>();
        if (!components.Contains(componentId9))
        {
            return false;
        }

        var componentId10 = ComponentId.Get<T10>();
        if (!components.Contains(componentId10))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Try to get a storable reference to a component of the given type.
    /// It is safe to use the returned ComponentRefs even when this method returns false,
    /// the ComponentRefs are just not guaranteed to point to a component in this case.
    /// This is because ComponentRef can check for the existence of the component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetStorableComponent<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(out CRef<T0> ref0, out CRef<T1> ref1, out CRef<T2> ref2, out CRef<T3> ref3, out CRef<T4> ref4, out CRef<T5> ref5, out CRef<T6> ref6, out CRef<T7> ref7, out CRef<T8> ref8, out CRef<T9> ref9, out CRef<T10> ref10, out CRef<T11> ref11) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent where T11 : IComponent
    {
        ref0 = new CRef<T0>(this);
        ref1 = new CRef<T1>(this);
        ref2 = new CRef<T2>(this);
        ref3 = new CRef<T3>(this);
        ref4 = new CRef<T4>(this);
        ref5 = new CRef<T5>(this);
        ref6 = new CRef<T6>(this);
        ref7 = new CRef<T7>(this);
        ref8 = new CRef<T8>(this);
        ref9 = new CRef<T9>(this);
        ref10 = new CRef<T10>(this);
        ref11 = new CRef<T11>(this);

        if (IsDefault || !World.TryGetStorageLocation(EntityId, out var storageLocation))
        {
            return false;
        }

        var components = storageLocation.Value.Chunk.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        var componentId8 = ComponentId.Get<T8>();
        if (!components.Contains(componentId8))
        {
            return false;
        }

        var componentId9 = ComponentId.Get<T9>();
        if (!components.Contains(componentId9))
        {
            return false;
        }

        var componentId10 = ComponentId.Get<T10>();
        if (!components.Contains(componentId10))
        {
            return false;
        }

        var componentId11 = ComponentId.Get<T11>();
        if (!components.Contains(componentId11))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Try to get a storable reference to a component of the given type.
    /// It is safe to use the returned ComponentRefs even when this method returns false,
    /// the ComponentRefs are just not guaranteed to point to a component in this case.
    /// This is because ComponentRef can check for the existence of the component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetStorableComponent<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(out CRef<T0> ref0, out CRef<T1> ref1, out CRef<T2> ref2, out CRef<T3> ref3, out CRef<T4> ref4, out CRef<T5> ref5, out CRef<T6> ref6, out CRef<T7> ref7, out CRef<T8> ref8, out CRef<T9> ref9, out CRef<T10> ref10, out CRef<T11> ref11, out CRef<T12> ref12) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent where T11 : IComponent where T12 : IComponent
    {
        ref0 = new CRef<T0>(this);
        ref1 = new CRef<T1>(this);
        ref2 = new CRef<T2>(this);
        ref3 = new CRef<T3>(this);
        ref4 = new CRef<T4>(this);
        ref5 = new CRef<T5>(this);
        ref6 = new CRef<T6>(this);
        ref7 = new CRef<T7>(this);
        ref8 = new CRef<T8>(this);
        ref9 = new CRef<T9>(this);
        ref10 = new CRef<T10>(this);
        ref11 = new CRef<T11>(this);
        ref12 = new CRef<T12>(this);

        if (IsDefault || !World.TryGetStorageLocation(EntityId, out var storageLocation))
        {
            return false;
        }

        var components = storageLocation.Value.Chunk.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        var componentId8 = ComponentId.Get<T8>();
        if (!components.Contains(componentId8))
        {
            return false;
        }

        var componentId9 = ComponentId.Get<T9>();
        if (!components.Contains(componentId9))
        {
            return false;
        }

        var componentId10 = ComponentId.Get<T10>();
        if (!components.Contains(componentId10))
        {
            return false;
        }

        var componentId11 = ComponentId.Get<T11>();
        if (!components.Contains(componentId11))
        {
            return false;
        }

        var componentId12 = ComponentId.Get<T12>();
        if (!components.Contains(componentId12))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Try to get a storable reference to a component of the given type.
    /// It is safe to use the returned ComponentRefs even when this method returns false,
    /// the ComponentRefs are just not guaranteed to point to a component in this case.
    /// This is because ComponentRef can check for the existence of the component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetStorableComponent<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(out CRef<T0> ref0, out CRef<T1> ref1, out CRef<T2> ref2, out CRef<T3> ref3, out CRef<T4> ref4, out CRef<T5> ref5, out CRef<T6> ref6, out CRef<T7> ref7, out CRef<T8> ref8, out CRef<T9> ref9, out CRef<T10> ref10, out CRef<T11> ref11, out CRef<T12> ref12, out CRef<T13> ref13) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent where T11 : IComponent where T12 : IComponent where T13 : IComponent
    {
        ref0 = new CRef<T0>(this);
        ref1 = new CRef<T1>(this);
        ref2 = new CRef<T2>(this);
        ref3 = new CRef<T3>(this);
        ref4 = new CRef<T4>(this);
        ref5 = new CRef<T5>(this);
        ref6 = new CRef<T6>(this);
        ref7 = new CRef<T7>(this);
        ref8 = new CRef<T8>(this);
        ref9 = new CRef<T9>(this);
        ref10 = new CRef<T10>(this);
        ref11 = new CRef<T11>(this);
        ref12 = new CRef<T12>(this);
        ref13 = new CRef<T13>(this);

        if (IsDefault || !World.TryGetStorageLocation(EntityId, out var storageLocation))
        {
            return false;
        }

        var components = storageLocation.Value.Chunk.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        var componentId8 = ComponentId.Get<T8>();
        if (!components.Contains(componentId8))
        {
            return false;
        }

        var componentId9 = ComponentId.Get<T9>();
        if (!components.Contains(componentId9))
        {
            return false;
        }

        var componentId10 = ComponentId.Get<T10>();
        if (!components.Contains(componentId10))
        {
            return false;
        }

        var componentId11 = ComponentId.Get<T11>();
        if (!components.Contains(componentId11))
        {
            return false;
        }

        var componentId12 = ComponentId.Get<T12>();
        if (!components.Contains(componentId12))
        {
            return false;
        }

        var componentId13 = ComponentId.Get<T13>();
        if (!components.Contains(componentId13))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Try to get a storable reference to a component of the given type.
    /// It is safe to use the returned ComponentRefs even when this method returns false,
    /// the ComponentRefs are just not guaranteed to point to a component in this case.
    /// This is because ComponentRef can check for the existence of the component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetStorableComponent<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(out CRef<T0> ref0, out CRef<T1> ref1, out CRef<T2> ref2, out CRef<T3> ref3, out CRef<T4> ref4, out CRef<T5> ref5, out CRef<T6> ref6, out CRef<T7> ref7, out CRef<T8> ref8, out CRef<T9> ref9, out CRef<T10> ref10, out CRef<T11> ref11, out CRef<T12> ref12, out CRef<T13> ref13, out CRef<T14> ref14) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent where T11 : IComponent where T12 : IComponent where T13 : IComponent where T14 : IComponent
    {
        ref0 = new CRef<T0>(this);
        ref1 = new CRef<T1>(this);
        ref2 = new CRef<T2>(this);
        ref3 = new CRef<T3>(this);
        ref4 = new CRef<T4>(this);
        ref5 = new CRef<T5>(this);
        ref6 = new CRef<T6>(this);
        ref7 = new CRef<T7>(this);
        ref8 = new CRef<T8>(this);
        ref9 = new CRef<T9>(this);
        ref10 = new CRef<T10>(this);
        ref11 = new CRef<T11>(this);
        ref12 = new CRef<T12>(this);
        ref13 = new CRef<T13>(this);
        ref14 = new CRef<T14>(this);

        if (IsDefault || !World.TryGetStorageLocation(EntityId, out var storageLocation))
        {
            return false;
        }

        var components = storageLocation.Value.Chunk.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        var componentId8 = ComponentId.Get<T8>();
        if (!components.Contains(componentId8))
        {
            return false;
        }

        var componentId9 = ComponentId.Get<T9>();
        if (!components.Contains(componentId9))
        {
            return false;
        }

        var componentId10 = ComponentId.Get<T10>();
        if (!components.Contains(componentId10))
        {
            return false;
        }

        var componentId11 = ComponentId.Get<T11>();
        if (!components.Contains(componentId11))
        {
            return false;
        }

        var componentId12 = ComponentId.Get<T12>();
        if (!components.Contains(componentId12))
        {
            return false;
        }

        var componentId13 = ComponentId.Get<T13>();
        if (!components.Contains(componentId13))
        {
            return false;
        }

        var componentId14 = ComponentId.Get<T14>();
        if (!components.Contains(componentId14))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Try to get a storable reference to a component of the given type.
    /// It is safe to use the returned ComponentRefs even when this method returns false,
    /// the ComponentRefs are just not guaranteed to point to a component in this case.
    /// This is because ComponentRef can check for the existence of the component.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetStorableComponent<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(out CRef<T0> ref0, out CRef<T1> ref1, out CRef<T2> ref2, out CRef<T3> ref3, out CRef<T4> ref4, out CRef<T5> ref5, out CRef<T6> ref6, out CRef<T7> ref7, out CRef<T8> ref8, out CRef<T9> ref9, out CRef<T10> ref10, out CRef<T11> ref11, out CRef<T12> ref12, out CRef<T13> ref13, out CRef<T14> ref14, out CRef<T15> ref15) where T0 : IComponent where T1 : IComponent where T2 : IComponent where T3 : IComponent where T4 : IComponent where T5 : IComponent where T6 : IComponent where T7 : IComponent where T8 : IComponent where T9 : IComponent where T10 : IComponent where T11 : IComponent where T12 : IComponent where T13 : IComponent where T14 : IComponent where T15 : IComponent
    {
        ref0 = new CRef<T0>(this);
        ref1 = new CRef<T1>(this);
        ref2 = new CRef<T2>(this);
        ref3 = new CRef<T3>(this);
        ref4 = new CRef<T4>(this);
        ref5 = new CRef<T5>(this);
        ref6 = new CRef<T6>(this);
        ref7 = new CRef<T7>(this);
        ref8 = new CRef<T8>(this);
        ref9 = new CRef<T9>(this);
        ref10 = new CRef<T10>(this);
        ref11 = new CRef<T11>(this);
        ref12 = new CRef<T12>(this);
        ref13 = new CRef<T13>(this);
        ref14 = new CRef<T14>(this);
        ref15 = new CRef<T15>(this);

        if (IsDefault || !World.TryGetStorageLocation(EntityId, out var storageLocation))
        {
            return false;
        }

        var components = storageLocation.Value.Chunk.Archetype.Components;

        var componentId0 = ComponentId.Get<T0>();
        if (!components.Contains(componentId0))
        {
            return false;
        }

        var componentId1 = ComponentId.Get<T1>();
        if (!components.Contains(componentId1))
        {
            return false;
        }

        var componentId2 = ComponentId.Get<T2>();
        if (!components.Contains(componentId2))
        {
            return false;
        }

        var componentId3 = ComponentId.Get<T3>();
        if (!components.Contains(componentId3))
        {
            return false;
        }

        var componentId4 = ComponentId.Get<T4>();
        if (!components.Contains(componentId4))
        {
            return false;
        }

        var componentId5 = ComponentId.Get<T5>();
        if (!components.Contains(componentId5))
        {
            return false;
        }

        var componentId6 = ComponentId.Get<T6>();
        if (!components.Contains(componentId6))
        {
            return false;
        }

        var componentId7 = ComponentId.Get<T7>();
        if (!components.Contains(componentId7))
        {
            return false;
        }

        var componentId8 = ComponentId.Get<T8>();
        if (!components.Contains(componentId8))
        {
            return false;
        }

        var componentId9 = ComponentId.Get<T9>();
        if (!components.Contains(componentId9))
        {
            return false;
        }

        var componentId10 = ComponentId.Get<T10>();
        if (!components.Contains(componentId10))
        {
            return false;
        }

        var componentId11 = ComponentId.Get<T11>();
        if (!components.Contains(componentId11))
        {
            return false;
        }

        var componentId12 = ComponentId.Get<T12>();
        if (!components.Contains(componentId12))
        {
            return false;
        }

        var componentId13 = ComponentId.Get<T13>();
        if (!components.Contains(componentId13))
        {
            return false;
        }

        var componentId14 = ComponentId.Get<T14>();
        if (!components.Contains(componentId14))
        {
            return false;
        }

        var componentId15 = ComponentId.Get<T15>();
        if (!components.Contains(componentId15))
        {
            return false;
        }

        return true;
    }

}
