using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Myriad.Ecs.Components;

namespace Myriad.Ecs.Collections;

/// <summary>
/// A set built out of an ordered list. This allows allocation free enumeration of the set.
/// </summary>
/// <typeparam name="TItem"></typeparam>
internal class OrderedListSet<TItem> : IReadOnlyList<TItem> where TItem : struct, IComparable<TItem>, IEquatable<TItem>
{
    private readonly List<TItem> items = [];
    public int Count => items.Count;

    public TItem this[int i] => items[i];

    #region constructors
    public OrderedListSet()
    {
    }

    // ReSharper disable once ParameterTypeCanBeEnumerable.Local
    public OrderedListSet(List<TItem> items)
    {
        this.items.EnsureCapacity(items.Count);
        foreach (var item in items)
        {
            Add(item);
        }
    }

    public OrderedListSet(ReadOnlySpan<TItem> items)
    {
        this.items.EnsureCapacity(items.Length);
        foreach (var item in items)
        {
            Add(item);
        }
    }

    internal OrderedListSet(HashSet<TItem> items)
    {
        this.items.AddRange(items);
        this.items.Sort();
    }

    public OrderedListSet(OrderedListSet<TItem> items)
    {
        this.items = [..items.items];
    }

    public OrderedListSet(FrozenOrderedListSet<TItem> items)
    {
        this.items.EnsureCapacity(items.Count);
        foreach (var item in items)
        {
            this.items.Add(item);
        }
    }
    #endregion

    internal void CopyTo(List<TItem> dest)
    {
        dest.AddRange(items);
    }

    #region add
    internal void EnsureCapacity(int capacity)
    {
        items.EnsureCapacity(capacity);
    }

    public bool Add(TItem item)
    {
        var index = items.BinarySearch(item);
        if (index >= 0)
        {
            return false;
        }

        items.Insert(~index, item);
        return true;
    }

    public void AddRange<TValue>(Dictionary<TItem, TValue>.KeyCollection keys)
    {
        EnsureCapacity(Count + keys.Count);

        if (items.Count == 0)
        {
            // Since this is a key collection we know all the items must be
            // unique, therefore we can just add and sort
            items.AddRange(keys);
            items.Sort();
        }
        else
        {
            foreach (var key in keys)
            {
                Add(key);
            }
        }
    }
    #endregion

    #region unionwith
    internal void UnionWith(FrozenOrderedListSet<TItem> set)
    {
        if (items.Count == 0)
        {
            set.CopyTo(items);
        }
        else
        {
            items.EnsureCapacity(items.Count + set.Count);
            foreach (var item in set)
            {
                Add(item);
            }
        }
    }
    #endregion

    public void IntersectWith(FrozenOrderedListSet<TItem> other)
    {
        for (var i = items.Count - 1; i >= 0; i--)
            if (!other.Contains(items[i]))
            {
                items.RemoveAt(i);
            }
    }

    public bool Remove(TItem item)
    {
        var index = items.BinarySearch(item);
        if (index < 0)
        {
            return false;
        }

        items.RemoveAt(index);
        return true;
    }

    public void Clear()
    {
        items.Clear();
    }

    /// <summary>
    /// Copy this set to a new frozen set
    /// </summary>
    /// <returns></returns>
    public FrozenOrderedListSet<TItem> Freeze()
    {
        return FrozenOrderedListSet<TItem>.Create(this);
    }

    #region GetEnumerator
    public List<TItem>.Enumerator GetEnumerator()
    {
        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return items.GetEnumerator();
    }

    IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator()
    {
        return items.GetEnumerator();
    }
    #endregion

    public bool Contains(TItem item)
    {
        return items.BinarySearch(item) >= 0;
    }

    //todo: other set methods when needed

    //public bool IsProperSubsetOf(IEnumerable<TItem> other)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool IsProperSubsetOf(OrderedListSet<TItem> other)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool IsProperSupersetOf(IEnumerable<TItem> other)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool IsProperSupersetOf(OrderedListSet<TItem> other)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool IsSubsetOf(IEnumerable<TItem> other)
    //{
    //    throw new NotImplementedException();
    //}

    //public bool IsSubsetOf(OrderedListSet<TItem> other)
    //{
    //    throw new NotImplementedException();
    //}

    #region IsSupersetOf
    public bool IsSupersetOf(OrderedListSet<TItem> other)
    {
        if (other.Count > Count)
        {
            return false;
        }

        // Move forward through both lists, checking that all items in `other` are in `this`
        var i = 0;
        var j = 0;
        while (i < items.Count && j < other.Count)
        {
            var cmp = items[i].CompareTo(other.items[j]);

            if (cmp < 0)
            {
                // Item in `this` < `other`. That's acceptable, it means the item is in the superset and not in the subset.
                // Move to the next item in the superset.
                i++;
            }
            else if (cmp == 0)
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

        return j == other.Count;
    }
    #endregion

    #region Overlaps
    public bool Overlaps(OrderedListSet<TItem> other)
    {
        if (Count == 0)
        {
            return false;
        }

        if (other.Count == 0)
        {
            return false;
        }

        // Move forward through both lists, checking if any item in `other` is in `this`
        var i = 0;
        var j = 0;
        while (i < items.Count && j < other.Count)
        {
            var cmp = items[i].CompareTo(other.items[j]);

            if (cmp < 0)
            {
                i++;
            }
            else if (cmp > 0)
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
    #endregion

    #region SetEquals
    public bool SetEquals(HashSet<TItem> other)
    {
        // Can't be equal if counts are different
        if (other.Count != Count)
        {
            return false;
        }

        // Ensure every item in this is in other
        foreach (var item in items)
        {
            if (!other.Contains(item))
            {
                return false;
            }
        }

        return true;
    }

    public bool SetEquals(OrderedListSet<TItem> other)
    {
        if (Count != other.Count)
        {
            return false;
        }

        var a = CollectionsMarshal.AsSpan(items);
        var b = CollectionsMarshal.AsSpan(other.items);

        // Add a specialization for ComponentId. This allows it to be compared with fast SIMD equality
        // instead of calling the equality implementation for every item individually.
        if (typeof(TItem) == typeof(ComponentId))
        {
            var aa = MemoryMarshal.Cast<TItem, int>(a);
            var bb = MemoryMarshal.Cast<TItem, int>(b);
            return aa.SequenceEqual(bb);
        }

        return a.SequenceEqual(b);
    }

    public bool SetEquals<TV>(Dictionary<TItem, TV> other)
    {
        // Can't be equal if counts are different
        if (other.Count != Count)
        {
            return false;
        }

        // Ensure every item in this is in other
        foreach (var item in other.Keys)
        {
            if (items.BinarySearch(item) < 0)
            {
                return false;
            }
        }

        return true;
    }
    #endregion

    #region LINQ
    internal IReadOnlyCollection<TItem> Linq()
    {
        return items;
    }

    internal TItem Single()
    {
        if (items.Count != 1)
        {
            throw new InvalidOperationException($"Cannot get single item, there are {items.Count} items");
        }

        return items[0];
    }

    internal bool Any(Func<TItem, bool> predicate)
    {
        foreach (var item in items)
        {
            if (predicate(item))
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}
