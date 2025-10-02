using System;
using Exanite.Myriad.Ecs.Collections;
using NUnit.Framework;

namespace Exanite.Myriad.Ecs.Tests.Collections;

[TestFixture]
public class SegmentListTests
{
    [Test]
    public void Create()
    {
        var list = new SegmentedList<int>(128);

        Assert.That(list.SegmentCapacity, Is.EqualTo(128));
    }

    [Test]
    public void IndexSingleSegment()
    {
        var list = new SegmentedList<int>(16);

        // Write index to each slot
        for (var i = 0; i < list.SegmentCapacity; i++)
        {
            list[i] = i;
        }

        // Read index from each slot
        for (var i = 0; i < list.SegmentCapacity; i++)
        {
            Assert.That(list[i], Is.EqualTo(i));
        }
    }

    [Test]
    public void IndexSingleSegmentOutOfRange()
    {
        var list = new SegmentedList<int>(16);

        Assert.Throws<IndexOutOfRangeException>(() => { _ = list[-1]; });
        Assert.Throws<ArgumentOutOfRangeException>(() => { _ = list[16]; });
    }

    [Test]
    public void Grow()
    {
        var list = new SegmentedList<int>(16);

        Assert.That(list.SegmentCapacity, Is.EqualTo(16));
        Assert.That(list.TotalCapacity, Is.EqualTo(16));
        Assert.That(list[15], Is.EqualTo(0));

        list.Grow();
        Assert.That(list.SegmentCapacity, Is.EqualTo(16));
        Assert.That(list.TotalCapacity, Is.EqualTo(32));
        Assert.That(list[31], Is.EqualTo(0));

        list.Grow();
        Assert.That(list.SegmentCapacity, Is.EqualTo(16));
        Assert.That(list.TotalCapacity, Is.EqualTo(48));
        Assert.That(list[47], Is.EqualTo(0));
    }
}
