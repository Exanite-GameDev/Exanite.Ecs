using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Exanite.Core.Utilities;

// ReSharper disable StaticMemberInGenericType

namespace Exanite.Myriad.Ecs.Allocations;

/// <summary>
/// Thread safe global pool.
/// </summary>
public static class Pool<T>
    where T : class, new()
{
    [ThreadStatic] private static int MaxSize;
    [ThreadStatic] private static int Pressure;
    [ThreadStatic] private static List<T>? Items;

    [MemberNotNull(nameof(Items))]
    private static void Init()
    {
        // Initialize item. This can't be done in the field initializer for a threadstatic field!
        if (Items == null)
        {
            Items = [];
            MaxSize = 1024;
        }
    }

    /// <summary>
    /// Get an item from this pool, creates a new one if there are none in the pool
    /// </summary>
    public static T Get()
    {
        Init();
        AssertUtility.NotNull(Items);

        if (Items.Count == 0)
        {
            // Every allocation significantly increases pressure
            Pressure += 8;
            return new();
        }

        // Every hit of the pool slightly reduces pressure
        if (Pressure > 0)
        {
            Pressure--;
        }

        var item = Items[^1];
        Items.RemoveAt(Items.Count - 1);
        return item;
    }

    /// <summary>
    /// Get an item from this pool, creates a new one if there are none in the pool
    /// </summary>
    /// <returns>A <see cref="Rental"/> contains the borrowed object and will return it when disposed</returns>
    public static Rental Rent()
    {
        return new Rental(Get());
    }

    /// <summary>
    /// Return an item to the pool
    /// </summary>
    public static void Return(T item)
    {
        Init();
        AssertUtility.NotNull(Items);

        if (Pressure > MaxSize)
        {
            MaxSize *= 2;
            Pressure = 0;
        }

        if (Items.Count < MaxSize)
        {
            Items.Add(item);
        }
    }

    /// <summary>
    /// Contains an object borrowed from a pool, returns it when disposed
    /// </summary>
    public readonly struct Rental(T value) : IDisposable
    {
        /// <summary>
        /// The borrowed object
        /// </summary>
        public T Value { get; } = value;

        /// <inheritdoc/>
        public void Dispose()
        {
            Return(Value);
        }
    }
}

/// <summary>
/// Thread safe global pool.
/// </summary>
public static class Pool
{
    /// <summary>
    /// Return an item to the pool
    /// </summary>
    public static void Return<T>(T item)
        where T : class, new()
    {
        Pool<T>.Return(item);
    }
}
