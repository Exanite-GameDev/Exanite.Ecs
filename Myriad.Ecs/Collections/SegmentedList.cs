using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;

namespace Exanite.Myriad.Ecs.Collections;

/// <summary>
/// A list which stores data in "segments", this removes the need for copying data when the list grows.
/// </summary>
internal class SegmentedList<T>
{
    private readonly Lock growLock = new();
    private int totalCapacity;

    private T[][] segments = [];
    private readonly int shift;
    private readonly int mask;

    /// <summary>
    /// How many items are stored within a single segment
    /// </summary>
    public int SegmentCapacity { get; }

    /// <summary>
    /// Total capacity in all segments
    /// </summary>
    public int TotalCapacity => totalCapacity;

    public SegmentedList(int segmentCapacity)
    {
        GuardUtility.IsTrue(segmentCapacity.IsPowerOfTwo(), "Segment capacity must be a power of 2");

        SegmentCapacity = segmentCapacity;
        shift = BitOperations.TrailingZeroCount(segmentCapacity);
        mask = segmentCapacity - 1;

        Grow();
    }

    /// <summary>
    /// Get the item with the given index (mutable)
    /// </summary>
    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            var segmentIndex = index >> shift;
            var rowIndex = index & mask;
            var segment = segments[segmentIndex];

            return ref segment[rowIndex];
        }
    }

    /// <summary>
    /// Add another segment
    /// </summary>
    public void Grow()
    {
        using var _ = growLock.EnterScope();

        T[][] newSegments = [..segments, new T[SegmentCapacity]];
        Interlocked.Exchange(ref segments, newSegments);
        Interlocked.Exchange(ref totalCapacity, totalCapacity + SegmentCapacity);
    }
}
