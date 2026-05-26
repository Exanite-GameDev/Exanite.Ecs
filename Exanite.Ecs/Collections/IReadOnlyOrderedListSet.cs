using System;
using System.Collections.Generic;

namespace Exanite.Myriad.Ecs.Collections;

public interface IReadOnlyOrderedListSet<T> : IReadOnlyList<T>, IEquatable<IReadOnlyOrderedListSet<T>> where T : struct, IComparable<T>, IEquatable<T>
{
    public ReadOnlySpan<T> Items { get; }

    /// <summary>
    /// Whether the set has been marked as immutable.
    /// Attempts to modify the collection will throw if the set is marked as immutable.
    /// </summary>
    internal bool IsImmutable { get; }

    /// <summary>
    /// This makes the set immutable and returns itself.
    /// This does NOT return a new instance.
    /// </summary>
    internal IReadOnlyOrderedListSet<T> MakeSelfImmutable();

    /// <summary>
    /// This creates a new set and immediately marks it as immutable.
    /// </summary>
    public IReadOnlyOrderedListSet<T> MakeNewImmutable();

    public bool Contains(T item);

    public bool IsSubsetOf(IReadOnlyOrderedListSet<T> other);
    public bool IsProperSupersetOf(IReadOnlyOrderedListSet<T> other);
    public bool IsProperSubsetOf(IReadOnlyOrderedListSet<T> other);
    public bool IsSupersetOf(IReadOnlyOrderedListSet<T> other);

    public bool Overlaps(IReadOnlyOrderedListSet<T> other);
    public bool SetEquals(IReadOnlyOrderedListSet<T> other);
}
