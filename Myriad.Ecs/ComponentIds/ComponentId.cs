using System;
using System.Diagnostics;
using Myriad.Ecs.Components;

namespace Myriad.Ecs.ComponentIds;

/// <summary>
/// Unique numeric ID for a type which implements IComponent
/// </summary>
[DebuggerDisplay("{Type} ({Value})")]
public readonly record struct ComponentId
    : IComparable<ComponentId>
{
    internal const int SpecialBitsCount = 4;
    internal const int SpecialBitsMask  = ~(~0 << SpecialBitsCount);
    internal const int IsPhantomComponentMask         = 0b0001;
    internal const int IsRelationComponentMask        = 0b0010;
    internal const int IsDisposableComponentMask      = 0b0100;
    internal const int IsPhantomNotifierComponentMask = 0b1000;

    /// <summary>
    /// Get the raw value of this ID
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// The <see cref="System.Type"/> of the component this ID is for
    /// </summary>
    public Type Type => ComponentRegistry.Get(this);

    /// <summary>
    /// Indicates if this component implements <see cref="IComponentPhantom"/>
    /// </summary>
    public bool IsPhantomComponent => (Value & IsPhantomComponentMask) == IsPhantomComponentMask;

    /// <summary>
    /// Indicates if this component implements <see cref="IEntityRelationComponent"/>
    /// </summary>
    public bool IsRelationComponent => (Value & IsRelationComponentMask) == IsRelationComponentMask;

    /// <summary>
    /// Indicates if this component implements <see cref="IDisposableComponent"/>
    /// </summary>
    public bool IsDisposableComponent => (Value & IsDisposableComponentMask) == IsDisposableComponentMask;

    /// <summary>
    /// Indicates if this component implements <see cref="IDisposableComponent"/>
    /// </summary>
    public bool IsPhantomNotifierComponent => (Value & IsPhantomNotifierComponentMask) == IsPhantomNotifierComponentMask;

    internal ComponentId(int value)
    {
        Value = value;
    }

    /// <inheritdoc />
    public int CompareTo(ComponentId other)
    {
        return Value.CompareTo(other.Value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var p = IsPhantomComponent ? " (phantom)" : "";
        var d = IsDisposableComponent ? " (dispose)" : "";
        return $"C{Value}{p}{d}";
    }

    /// <summary>
    /// Get the component ID for the given type
    /// </summary>
    /// <param name="type"></param>
    /// <exception cref="ArgumentException">Thrown if 'type' does not implement <see cref="IComponent"/></exception>
    public static ComponentId Get(Type type)
    {
        return ComponentRegistry.Get(type);
    }

    /// <summary>
    /// Get the component ID for the given type
    /// </summary>
    /// <param name="type"></param>
    public static ComponentId Get<T>() where T : IComponent
    {
        return ComponentRegistry.Get<T>();
    }
}
