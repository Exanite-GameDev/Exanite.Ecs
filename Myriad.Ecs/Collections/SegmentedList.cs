using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Exanite.Myriad.Ecs.Collections;

/// <summary>
/// Implementation of a doubling segmented list for use by the <see cref="EntityManager"/>.
/// </summary>
/// <remarks>
/// Grow is assumed to be accessed under a lock owned by <see cref="EntityManager"/>.
/// <para/>
/// Behavior is undefined if an invalid index is accessed.
/// <para/>
/// Reference stability to values returned by the indexer is guaranteed.
/// </remarks>
public struct SegmentedList<T>
{
    private const int BaseShift = EcsConstants.SegmentedListBaseCapacityPowerOf2;
    private const int BaseCapacity = 1 << BaseShift;
    private const int MaxSegments = 31 - BaseShift;

    private SegmentArray segments;
    private int segmentCount = 0;

    /// <summary>
    /// The total capacity of the list.
    /// </summary>
    public int Capacity => (1 << (segmentCount + BaseShift)) - BaseCapacity;

    public SegmentedList()
    {
        Grow();
    }

    public ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            // Calculate the segment index
            var adjustedIndex = (uint)index + BaseCapacity;
            var leadingZeroCount = BitOperations.LeadingZeroCount(adjustedIndex);
            var segmentIndex = 31 - leadingZeroCount - BaseShift;

            // Use a mask to calculate the index within the segment
            var indexMask = (1u << (31 - leadingZeroCount)) - 1;
            var indexInSegment = (int)(adjustedIndex & indexMask);

            var segment = GetSegment(segmentIndex);
            return ref segment[indexInSegment];
        }
    }

    public void EnsureCapacity(int capacity)
    {
        var adjustedCapacity = (uint)capacity + BaseCapacity;
        var leadingZeroCount = BitOperations.LeadingZeroCount(adjustedCapacity);
        var requiredSegmentCount = 32 - leadingZeroCount - BaseShift;
        while (requiredSegmentCount > segmentCount)
        {
            Grow();
        }
    }

    public void Grow()
    {
        if (segmentCount >= MaxSegments)
        {
            throw new InvalidOperationException("Segmented list has reached maximum capacity.");
        }

        var nextSegmentIndex = segmentCount;

        // Size doubles: 1024, 2048, 4096, 8192...
        var newSegmentSize = 1 << (nextSegmentIndex + BaseShift);

        var newSegment = new T[newSegmentSize];
        Volatile.Write(ref GetSegment(nextSegmentIndex), newSegment);
        Interlocked.Increment(ref segmentCount);
    }

    private ref T[] GetSegment(int segmentIndex)
    {
        ref var arrayBase = ref Unsafe.As<SegmentArray, T[]>(ref segments);
        ref var element = ref Unsafe.Add(ref arrayBase, segmentIndex);
        return ref Unsafe.AsRef(ref element);
    }

    [InlineArray(MaxSegments)]
    private struct SegmentArray
    {
        private T[] element0;
    }
}
