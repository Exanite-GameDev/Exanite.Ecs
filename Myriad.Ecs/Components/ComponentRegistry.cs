using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Exanite.Myriad.Ecs.Allocations;
using Exanite.Myriad.Ecs.Threading;

namespace Exanite.Myriad.Ecs.Components;

/// <summary>
/// Store a lookup from component type to unique 32 bit ID.
/// </summary>
internal static class ComponentRegistry
{
    private static readonly RwLock<State> Lock = new(new State());

    /// <summary>
    /// Get the ID for the given type
    /// </summary>
    public static ComponentId Get<T>() where T : IComponent
    {
        var type = typeof(T);

        using (var locker = Lock.EnterReadLock())
        {
            if (locker.Value.TryGet(type, out var value))
            {
                return value;
            }
        }

        using (var locker = Lock.EnterWriteLock())
        {
            return locker.Value.GetOrAdd(type);
        }
    }

    /// <summary>
    /// Get the ID for the given type
    /// </summary>
    public static ComponentId Get(Type type)
    {
        TypeCheck(type);

        using (var locker = Lock.EnterReadLock())
        {
            if (locker.Value.TryGet(type, out var value))
            {
                return value;
            }
        }

        using (var locker = Lock.EnterWriteLock())
        {
            return locker.Value.GetOrAdd(type);
        }
    }

    /// <summary>
    /// Get the type for a given ID
    /// </summary>
    public static Type Get(ComponentId id)
    {
        using var locker = Lock.EnterReadLock();

        if (!locker.Value.TryGet(id, out var type))
        {
            throw new InvalidOperationException("Unknown component ID");
        }

        return type;
    }

    private static void TypeCheck(Type type)
    {
        if (!typeof(IComponent).IsAssignableFrom(type))
        {
            throw new ArgumentException($"Type `{type.FullName}` is not assignable to `{nameof(IComponent)}`)");
        }
    }

    private class State
    {
        private readonly Dictionary<ComponentId, Type> typeLookup = [];
        private readonly Dictionary<Type, ComponentId> idLookup = [];

        // Init the first ID to be the one after the default ID. That
        // means that default is _not_ a valid ID.
        private int nextId = 1;

        public ComponentId GetOrAdd(Type type)
        {
            if (!idLookup.TryGetValue(type, out var value))
            {
                var id = nextId++;

                // Store it for future lookups
                value = new ComponentId(id);
                idLookup[type] = value;
                typeLookup[value] = type;

                // Since we've discovered this component we're likely to need
                // arrays made for it later. Prepare the array factory for that.
                ArrayFactory.Prepare(type);
            }

            return value;
        }

        public bool TryGet(Type type, out ComponentId id)
        {
            return idLookup.TryGetValue(type, out id);
        }

        public bool TryGet(ComponentId id, [MaybeNullWhen(false)] out Type type)
        {
            return typeLookup.TryGetValue(id, out type);
        }
    }
}
