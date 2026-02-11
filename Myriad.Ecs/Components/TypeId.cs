using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Exanite.Myriad.Ecs.Components;

public readonly record struct TypeId : IComparable<TypeId>
{
    private static readonly ConcurrentBag<TypeId> registeredIds = new();

    /// <summary>
    /// All type IDs that have been discovered and registered so far.
    /// </summary>
    public static IReadOnlyCollection<TypeId> RegisteredIds => registeredIds;

    /// <summary>
    /// Raised when a new type ID is registered. May be called from any thread.
    /// </summary>
    public static event Action<TypeId>? IdRegistered;

    internal static void NotifyIdRegistered(TypeId typeId)
    {
        registeredIds.Add(typeId);
        IdRegistered?.Invoke(typeId);
    }

    /// <summary>
    /// Get the raw value of this ID.
    /// </summary>
    public readonly int Value;

    /// <summary>
    /// Whether this ID represents an interface ID.
    /// </summary>
    public bool IsInterface => Value > 0;

    /// <summary>
    /// Whether this ID represents a component ID.
    /// </summary>
    public bool IsComponent => Value < 0;

    /// <summary>
    /// The type this type ID represents.
    /// </summary>
    public Type Type
    {
        get
        {
            if (IsComponent)
            {
                return TypeRegistry.GetComponentType((ComponentId)this);
            }

            return TypeRegistry.GetInterfaceType((InterfaceId)this);
        }
    }

    internal TypeId(int value)
    {
        Value = value;
    }

    /// <inheritdoc/>
    public int CompareTo(TypeId other)
    {
        return Value.CompareTo(other.Value);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Type} ({Value})";
    }
}
