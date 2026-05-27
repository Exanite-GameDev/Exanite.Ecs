using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Exanite.Core.Utilities;

namespace Exanite.Ecs.Collections;

/// <summary>
/// A set built out of an ordered list. This allows allocation free enumeration of the set.
/// </summary>
/// <remarks>
/// This represents both immutable and mutable versions of the data structure.
/// Be very careful about whether the usage expects the data to be immutable.
/// </remarks>
internal class OrderedListSet<T> : IReadOnlyOrderedListSet<T> where T : struct, IComparable<T>, IEquatable<T>
{
    public static readonly IReadOnlyOrderedListSet<T> Empty = new OrderedListSet<T>().MakeSelfImmutable();

    private readonly List<T> items = [];
    private int? hashCode;

    public int Count => items.Count;
    public T this[int i] => items[i];
    public ReadOnlySpan<T> Items => items.AsSpan();

    /// <inheritdoc/>
    public bool IsImmutable { get; private set; }

    public OrderedListSet() {}

    public OrderedListSet(ReadOnlySpan<T> other)
    {
        items.EnsureCapacity(other.Length);
        foreach (var item in other)
        {
            Add(item);
        }
    }

    public OrderedListSet(IReadOnlyOrderedListSet<T> other)
    {
        items = [..other.Items];
    }

    public static OrderedListSet<T> Create<TValue>(Dictionary<T, TValue> other)
    {
        var set = new OrderedListSet<T>();
        set.items.AddRange(other.Keys);
        set.items.Sort();
        return set;
    }

    /// <inheritdoc/>
    public IReadOnlyOrderedListSet<T> MakeSelfImmutable()
    {
        IsImmutable = true;
        return this;
    }

    /// <inheritdoc/>
    public IReadOnlyOrderedListSet<T> MakeNewImmutable()
    {
        return new OrderedListSet<T>(this).MakeSelfImmutable();
    }

    public void EnsureCapacity(int capacity)
    {
        AssertUtility.IsFalse(IsImmutable, "Set has been marked as immutable");
        items.EnsureCapacity(capacity);
    }

    public bool Add(T item)
    {
        AssertUtility.IsFalse(IsImmutable, "Set has been marked as immutable");
        var index = items.BinarySearch(item);
        if (index >= 0)
        {
            return false;
        }

        hashCode = null;
        items.Insert(~index, item);
        return true;
    }

    public void UnionWith<TValue>(Dictionary<T, TValue> other)
    {
        AssertUtility.IsFalse(IsImmutable, "Set has been marked as immutable");
        if (other.Count == 0)
        {
            return;
        }

        hashCode = null;

        EnsureCapacity(Count + other.Count);
        if (items.Count == 0)
        {
            // Since this is a key collection we know all the items must be
            // unique, therefore we can just add and sort
            items.AddRange(other.Keys);
            items.Sort();
        }
        else
        {
            foreach (var key in other.Keys)
            {
                Add(key);
            }
        }
    }

    public void UnionWith(IReadOnlyOrderedListSet<T> other)
    {
        AssertUtility.IsFalse(IsImmutable, "Set has been marked as immutable");
        if (other.Count == 0)
        {
            return;
        }

        hashCode = null;

        if (items.Count == 0)
        {
            items.AddRange(other.Items);
        }
        else
        {
            items.EnsureCapacity(items.Count + other.Count);
            foreach (var item in other)
            {
                Add(item);
            }
        }
    }

    public void IntersectWith(IReadOnlyOrderedListSet<T> other)
    {
        AssertUtility.IsFalse(IsImmutable, "Set has been marked as immutable");
        if (other.Count == 0)
        {
            return;
        }

        hashCode = null;

        for (var i = items.Count - 1; i >= 0; i--)
        {
            if (!other.Contains(items[i]))
            {
                items.RemoveAt(i);
            }
        }
    }

    public bool Remove(T item)
    {
        AssertUtility.IsFalse(IsImmutable, "Set has been marked as immutable");
        var index = items.BinarySearch(item);
        if (index < 0)
        {
            return false;
        }

        hashCode = null;
        items.RemoveAt(index);
        return true;
    }

    public void Clear()
    {
        AssertUtility.IsFalse(IsImmutable, "Set has been marked as immutable");
        hashCode = null;
        items.Clear();
    }

    public bool Contains(T item)
    {
        return items.BinarySearch(item) >= 0;
    }

    public bool IsProperSubsetOf(IReadOnlyOrderedListSet<T> other)
    {
        return Count < other.Count && IsSubsetOf(other);
    }

    public bool IsProperSupersetOf(IReadOnlyOrderedListSet<T> other)
    {
        return Count > other.Count && IsSupersetOf(other);
    }

    public bool IsSubsetOf(IReadOnlyOrderedListSet<T> other)
    {
        return other.IsSupersetOf(this);
    }

    public bool IsSupersetOf(IReadOnlyOrderedListSet<T> other)
    {
        var selfSpan = Items;
        var otherSpan = other.Items;
        if (otherSpan.Length > selfSpan.Length)
        {
            return false;
        }

        // Move forward through both lists, checking that all items in `other` are in `this`
        var i = 0;
        var j = 0;
        while (i < selfSpan.Length && j < otherSpan.Length)
        {
            var comparison = selfSpan[i].CompareTo(otherSpan[j]);

            if (comparison < 0)
            {
                // Item in `this` < `other`. That's acceptable, it means the item is in the superset and not in the subset.
                // Move to the next item in the superset.
                i++;
            }
            else if (comparison == 0)
            {
                // Items are equal, move to the next item in both
                i++;
                j++;
            }
            else
            {
                // Item in `other` < `this`. That means `other` is not a subset!
                return false;
            }
        }

        return j == otherSpan.Length;
    }

    public bool Overlaps(IReadOnlyOrderedListSet<T> other)
    {
        var selfSpan = Items;
        var otherSpan = other.Items;

        if (selfSpan.Length == 0 || otherSpan.Length == 0)
        {
            return false;
        }

        // Move forward through both lists, checking if any item in `other` is in `this`
        var i = 0;
        var j = 0;
        while (i < selfSpan.Length && j < otherSpan.Length)
        {
            var comparison = selfSpan[i].CompareTo(otherSpan[j]);

            if (comparison < 0)
            {
                i++;
            }
            else if (comparison > 0)
            {
                j++;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

    public bool SetEquals(IReadOnlyOrderedListSet<T> other)
    {
        return Items.SequenceEqual(other.Items);
    }

    public bool Equals(IReadOnlyOrderedListSet<T>? other)
    {
        return other != null && SetEquals(other);
    }

    public override bool Equals(object? obj)
    {
        return obj is IReadOnlyOrderedListSet<T> other && Equals(other);
    }

    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
    {
        if (hashCode.HasValue)
        {
            return hashCode.Value;
        }

        var intermediate = new HashCode();
        foreach (var item in items)
        {
            intermediate.Add(item);
        }

        var result = intermediate.ToHashCode();

        hashCode = result;
        return result;
    }

    public List<T>.Enumerator GetEnumerator()
    {
        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return items.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return items.GetEnumerator();
    }
}
