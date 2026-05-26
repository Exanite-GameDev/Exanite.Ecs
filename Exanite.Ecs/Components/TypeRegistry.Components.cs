using System;

namespace Exanite.Myriad.Ecs.Components;

internal static partial class TypeRegistry
{
    /// <summary>
    /// Gets the component ID for the given type.
    /// </summary>
    public static ComponentId GetComponentId<T>() where T : IEcsComponent
    {
        var type = typeof(T);
        using (Lock.EnterReadLock(out var state))
        {
            if (state.Value.TryGetTypeId(type, out var typeId))
            {
                return (ComponentId)typeId;
            }
        }

        using (Lock.EnterWriteLock(out var state))
        {
            return state.Value.GetOrAddComponentId(type);
        }
    }

    /// <summary>
    /// Gets the component ID for the given type.
    /// </summary>
    public static ComponentId GetComponentId(Type type)
    {
        using (Lock.EnterReadLock(out var state))
        {
            if (state.Value.TryGetTypeId(type, out var typeId))
            {
                return (ComponentId)typeId;
            }
        }

        using (Lock.EnterWriteLock(out var state))
        {
            return state.Value.GetOrAddComponentId(type);
        }
    }

    /// <summary>
    /// Gets the type for a given component ID.
    /// </summary>
    public static Type GetComponentType(ComponentId componentId)
    {
        using (Lock.EnterReadLock(out var state))
        {
            return state.Value.GetComponentType(componentId);
        }
    }

    /// <summary>
    /// Gets the dispatcher for a given component ID.
    /// </summary>
    internal static ComponentDispatcher GetComponentDispatcher(ComponentId componentId)
    {
        using (Lock.EnterReadLock(out var state))
        {
            return state.Value.GetComponentDispatcher(componentId);
        }
    }
}
