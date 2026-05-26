using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Exanite.Core.Utilities;

namespace Exanite.Ecs.Components;

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
    /// Whether this ID represents a component ID.
    /// </summary>
    public bool IsComponent => Value > 0;

    /// <summary>
    /// Whether this ID represents an interface ID.
    /// </summary>
    public bool IsInterface => Value < 0;

    /// <summary>
    /// The type this type ID represents.
    /// </summary>
    public Type Type => TypeRegistry.GetBackingType(this);

    internal TypeId(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Get the type ID for the given type.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if <see cref="type"/> does not implement <see cref="IEcsComponent"/> nor <see cref="IEcsInterface"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TypeId Get(Type type)
    {
        return TypeRegistry.GetTypeId(type);
    }

    /// <summary>
    /// Get the type ID for the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TypeId Get<T>() where T : IEcsType
    {
        return TypeId<T>.Id;
    }

    /// <inheritdoc/>
    public int CompareTo(TypeId other)
    {
        return Value.CompareTo(other.Value);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{TypeUtility.FormatConciseName(Type)} (ID={Value})";
    }
}

internal static class TypeId<T> where T : IEcsType
{
    /// <summary>
    /// The type ID for <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// This property is cached, making repeated accesses very efficient.
    /// </remarks>
    public static readonly TypeId Id = TypeRegistry.GetTypeId<T>();
}
