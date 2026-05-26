using System;
using System.Runtime.CompilerServices;
using Exanite.Core.Utilities;

namespace Exanite.Ecs.Components;

/// <summary>
/// Unique ID for a type that implements <see cref="IEcsComponent"/>.
/// </summary>
public readonly record struct ComponentId : IComparable<ComponentId>
{
    /// <summary>
    /// Get the raw value of this ID.
    /// </summary>
    public readonly int Value;

    /// <summary>
    /// The type this component ID represents.
    /// </summary>
    public Type Type => TypeRegistry.GetComponentType(this);

    internal ComponentId(int value)
    {
        Value = value;
    }

    public static implicit operator TypeId(ComponentId value)
    {
        return new TypeId(value.Value);
    }

    public static explicit operator ComponentId(TypeId value)
    {
        GuardUtility.IsTrue(value.IsComponent, "The specified type ID does not represent a component ID");
        return new ComponentId(value.Value);
    }

    /// <summary>
    /// Gets the dispatcher for this component ID.
    /// </summary>
    /// <remarks>
    /// The dispatcher can be used to invoke generic methods given only a component ID.
    /// </remarks>
    public ComponentDispatcher GetDispatcher()
    {
        return TypeRegistry.GetComponentDispatcher(this);
    }

    /// <summary>
    /// Get the component ID for the given type.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if <see cref="type"/> does not implement <see cref="IEcsComponent"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComponentId Get(Type type)
    {
        return TypeRegistry.GetComponentId(type);
    }

    /// <summary>
    /// Get the component ID for the given type.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComponentId Get<T>() where T : IEcsComponent
    {
        return ComponentId<T>.Id;
    }

    /// <inheritdoc/>
    public int CompareTo(ComponentId other)
    {
        return Value.CompareTo(other.Value);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{TypeUtility.FormatConciseName(Type)} (ID={Value})";
    }
}

internal static class ComponentId<T> where T : IEcsComponent
{
    /// <summary>
    /// The component ID for <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// This property is cached, making repeated accesses very efficient.
    /// </remarks>
    public static readonly ComponentId Id = TypeRegistry.GetComponentId<T>();
}
