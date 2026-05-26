using Exanite.Myriad.Ecs.Collections;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests.Collections;

public class SegmentListTests
{
    [Fact]
    public void Create()
    {
        var capacity = 10000;
        var list = new SegmentedList<int>();
        list.EnsureCapacity(capacity);

        Assert.True(list.Capacity >= capacity);
    }

    [Fact]
    public void IndexSingleSegment()
    {
        var capacity = 1000;
        var list = new SegmentedList<int>();
        list.EnsureCapacity(capacity);

        // Write index to each slot
        for (var i = 0; i < capacity; i++)
        {
            list[i] = i;
        }

        // Read index from each slot
        for (var i = 0; i < capacity; i++)
        {
            Assert.Equal(i, list[i]);
        }
    }

    [Fact]
    public void Grow()
    {
        var baseShift = EcsConstants.SegmentedListBaseCapacityPowerOf2;
        var list = new SegmentedList<int>();

        Assert.Equal(1 << baseShift, list.Capacity);
        Assert.Equal(0, list[list.Capacity - 1]);

        list.Grow();
        Assert.Equal((1 << baseShift) + (1 << baseShift + 1), list.Capacity);
        Assert.Equal(0, list[list.Capacity - 1]);

        list.Grow();
        Assert.Equal((1 << baseShift) + (1 << baseShift + 1) + (1 << baseShift + 2), list.Capacity);
        Assert.Equal(0, list[list.Capacity - 1]);
    }
}
