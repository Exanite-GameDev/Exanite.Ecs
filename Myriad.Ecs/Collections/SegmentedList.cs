using System;
using System.Threading;

namespace Exanite.Myriad.Ecs.Collections;

/// <summary>
/// A list which stores data in "segments", this removes the need for copying data when the list grows.
/// </summary>
internal class SegmentedList<T>
{
    private readonly Lock growLock = new();

    /// <summary>
    /// How many items are stored within a single segment
    /// </summary>
    public int SegmentCapacity { get; }

    /// <summary>
    /// Total capacity in all segments
    /// </summary>
    public int TotalCapacity { get; private set; }

    private T[][] segments = [];

    public SegmentedList(int segmentCapacity)
    {
        SegmentCapacity = segmentCapacity;
        Grow();
    }

    /// <summary>
    /// Get the item with the given index (mutable)
    /// </summary>
    public ref T this[int index]
    {
        get
        {
            var (rowIndex, segment) = GetSegment(index);
            return ref segment[rowIndex];
        }
    }

    /// <summary>
    /// Get the segment and index within the segment for the item with the given index
    /// </summary>
    /// <exception cref="IndexOutOfRangeException"/>
    private (int, T[]) GetSegment(int index)
    {
        var segIndex = index / SegmentCapacity;
        var rowIndex = index - segIndex * SegmentCapacity;

        return (rowIndex, segments[segIndex]);
    }

    /// <summary>
    /// Add another segment
    /// </summary>
    public void Grow()
    {
        using var _ = growLock.EnterScope();

        T[][] newSegments = [..segments, new T[SegmentCapacity]];
        Interlocked.Exchange(ref segments, newSegments);
    }
}
