using System;
using System.Runtime.CompilerServices;
using Exanite.Core.Utilities;

namespace Exanite.Myriad.Ecs.Components;

/// <summary>
/// Unique ID for a type that implements <see cref="IInterfaceComponent"/>.
/// </summary>
public readonly record struct InterfaceId : IComparable<InterfaceId>
{
    /// <summary>
    /// Get the raw value of this ID.
    /// </summary>
    public readonly int Value;

    /// <summary>
    /// The type this interface ID represents.
    /// </summary>
    public Type Type => TypeRegistry.GetInterfaceType(this);

    internal InterfaceId(int value)
    {
        Value = value;
    }

    public static implicit operator TypeId(InterfaceId value)
    {
        return new TypeId(value.Value);
    }

    public static explicit operator InterfaceId(TypeId value)
    {
        GuardUtility.IsTrue(value.IsInterface, "The specified type ID does not represent an interface ID");
        return new InterfaceId(value.Value);
    }

    /// <summary>
    /// Get the interface ID for the given type.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if <see cref="type"/> does not implement <see cref="IInterfaceComponent"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static InterfaceId Get(Type type)
    {
        return TypeRegistry.GetInterfaceId(type);
    }

    /// <summary>
    /// Get the interface ID for the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static InterfaceId Get<T>() where T : IInterfaceComponent
    {
        return InterfaceId<T>.Id;
    }

    /// <inheritdoc/>
    public int CompareTo(InterfaceId other)
    {
        return Value.CompareTo(other.Value);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{TypeUtility.FormatConciseName(Type)} ({Value})";
    }
}

internal static class InterfaceId<T> where T : IInterfaceComponent
{
    /// <summary>
    /// The interface ID for <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// This property is cached, making repeated accesses very efficient.
    /// </remarks>
    public static readonly InterfaceId Id = TypeRegistry.GetInterfaceId<T>();
}
