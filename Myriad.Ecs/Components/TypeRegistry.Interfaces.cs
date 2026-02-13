using System;

namespace Exanite.Myriad.Ecs.Components;

internal static partial class TypeRegistry
{
    /// <summary>
    /// Gets the interface ID for the given type.
    /// </summary>
    public static InterfaceId GetInterfaceId<T>() where T : IInterfaceComponent
    {
        var type = typeof(T);
        using (Lock.EnterReadLock(out var state))
        {
            if (state.Value.TryGetTypeId(type, out var typeId))
            {
                return (InterfaceId)typeId;
            }
        }

        using (Lock.EnterWriteLock(out var state))
        {
            return state.Value.GetOrAddInterfaceId(type);
        }
    }

    /// <summary>
    /// Gets the interface ID for the given type.
    /// </summary>
    public static InterfaceId GetInterfaceId(Type type)
    {
        using (Lock.EnterReadLock(out var state))
        {
            if (state.Value.TryGetTypeId(type, out var typeId))
            {
                return (InterfaceId)typeId;
            }
        }

        using (Lock.EnterWriteLock(out var state))
        {
            return state.Value.GetOrAddInterfaceId(type);
        }
    }

    /// <summary>
    /// Gets the type for a given interface ID.
    /// </summary>
    public static Type GetInterfaceType(InterfaceId interfaceId)
    {
        using (Lock.EnterReadLock(out var state))
        {
            return state.Value.GetInterfaceType(interfaceId);
        }
    }
}
