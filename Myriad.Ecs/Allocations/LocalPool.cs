using System;
using System.Collections.Generic;

namespace Myriad.Ecs.Allocations;

/// <summary>
/// A non-thread safe pool, backed by the global thread safe pool.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct LocalPool<T> : IDisposable where T : class, new()
{
    private readonly List<T> items = Pool<List<T>>.Get();
    private readonly int maxSize;

    /// <summary>
    /// Create a new <see cref="LocalPool{T}"/>
    /// </summary>
    public LocalPool() : this(16)
    {
    }

    /// <summary>
    /// Create a new <see cref="LocalPool{T}"/>
    /// </summary>
    public LocalPool(int maxSize)
    {
        items = Pool<List<T>>.Get();
        this.maxSize = maxSize;
    }

    /// <summary>
    /// Get an item from this pool, fetches from the global pool if none are available
    /// </summary>
    /// <returns></returns>
    public T Get()
    {
        if (items.Count == 0)
        {
            return Pool<T>.Get();
        }

        var item = items[^1];
        items.RemoveAt(items.Count - 1);
        return item;
    }

    /// <summary>
    /// Return an item to this pool
    /// </summary>
    /// <param name="item"></param>
    public void Return(T item)
    {
        if (items.Count < maxSize)
        {
            items.Add(item);
        }
        else
        {
            Pool<T>.Return(item);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var item in items)
            Pool<T>.Return(item);
        items.Clear();

        Pool<List<T>>.Return(items);
    }
}
