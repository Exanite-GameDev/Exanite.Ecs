using System;

namespace Exanite.Ecs.Components;

internal static partial class TypeRegistry
{
    /// <summary>
    /// Gets the type ID for the given type.
    /// </summary>
    public static TypeId GetTypeId<T>() where T : IEcsType
    {
        var type = typeof(T);
        using (Lock.EnterReadLock(out var state))
        {
            if (state.Value.TryGetTypeId(type, out var typeId))
            {
                return typeId;
            }
        }

        using (Lock.EnterWriteLock(out var state))
        {
            return state.Value.GetOrAddTypeId(type);
        }
    }

    /// <summary>
    /// Gets the type ID for the given type.
    /// </summary>
    public static TypeId GetTypeId(Type type)
    {
        using (Lock.EnterReadLock(out var state))
        {
            if (state.Value.TryGetTypeId(type, out var typeId))
            {
                return typeId;
            }
        }

        using (Lock.EnterWriteLock(out var state))
        {
            return state.Value.GetOrAddTypeId(type);
        }
    }

    /// <summary>
    /// Gets the type for a given type ID.
    /// </summary>
    public static Type GetBackingType(TypeId typeId)
    {
        using (Lock.EnterReadLock(out var state))
        {
            return state.Value.GetBackingType(typeId);
        }
    }
}
