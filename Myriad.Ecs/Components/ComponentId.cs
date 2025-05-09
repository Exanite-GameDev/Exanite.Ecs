using System;
using System.Diagnostics;

namespace Exanite.Myriad.Ecs.Components;

/// <summary>
/// Unique numeric ID for a type which implements IComponent
/// </summary>
[DebuggerDisplay("{Type} ({Value})")]
public readonly record struct ComponentId : IComparable<ComponentId>
{
    /// <summary>
    /// Get the raw value of this ID
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// The <see cref="System.Type"/> of the component this ID is for
    /// </summary>
    public Type Type => ComponentRegistry.Get(this);

    internal ComponentId(int value)
    {
        Value = value;
    }

    /// <inheritdoc/>
    public int CompareTo(ComponentId other)
    {
        return Value.CompareTo(other.Value);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Type} ({Value})";
    }

    /// <summary>
    /// Get the component ID for the given type
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if 'type' does not implement <see cref="IComponent"/></exception>
    public static ComponentId Get(Type type)
    {
        return ComponentRegistry.Get(type);
    }

    /// <summary>
    /// Get the component ID for the given type
    /// </summary>
    public static ComponentId Get<T>() where T : IComponent
    {
        return ComponentRegistry.Get<T>();
    }
}
