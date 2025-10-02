using System.Collections.Generic;
using Exanite.Myriad.Ecs.Collections;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Exanite.Myriad.Ecs.Tests.Collections;

[TestFixture]
public class OrderedListSetTests
{
    [Test]
    public void Create()
    {
        var set = new OrderedListSet<int>();

        Assert.That(set.Count, Is.EqualTo(0));
    }

    [Test]
    public void Create_NonEmpty()
    {
        var set = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        Assert.That(set.Count, Is.EqualTo(3));
        Assert.That(set.Contains(1), Is.True);
        Assert.That(set.Contains(2), Is.True);
        Assert.That(set.Contains(3), Is.True);
        Assert.That(set.Contains(4), Is.False);
    }

    [Test]
    public void UnionWith()
    {
        var set = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var ints = new OrderedListSet<int>
        {
            1,
            2,
            3,
            4,
        };

        var immutable = ImmutableOrderedListSet<int>.Create(ints);
        set.UnionWith(immutable);

        Assert.That(set.Count, Is.EqualTo(4));
        Assert.That(set.Contains(1), Is.True);
        Assert.That(set.Contains(2), Is.True);
        Assert.That(set.Contains(3), Is.True);
        Assert.That(set.Contains(4), Is.True);
    }

    [Test]
    public void AddUnique()
    {
        var set = new OrderedListSet<int>
        {
            1,
            2,
            3,
            11,
            5,
        };

        Assert.That(set.Count, Is.EqualTo(5));
    }

    [Test]
    public void AddDuplicates()
    {
        var set = new OrderedListSet<int>
        {
            1,
            1,
            2,
            3,
            2,
            2,
            11,
        };

        Assert.That(set.Count, Is.EqualTo(4));
    }

    [Test]
    public void SetEquals_True()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new OrderedListSet<int>
        {
            3,
            2,
            1,
        };

        Assert.That(a.SetEquals(b), Is.True);
    }

    [Test]
    public void SetEquals_False_SameCount()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new OrderedListSet<int>
        {
            2,
            1,
            0,
        };

        Assert.That(a.ToImmutable().SetEquals(b.ToImmutable()), Is.False);
    }

    [Test]
    public void SetEquals_False_Superset()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new OrderedListSet<int>
        {
            1,
            2,
            3,
            4,
        };

        Assert.That(a.SetEquals(b), Is.False);
    }

    [Test]
    public void SetEquals_False_Subset()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new OrderedListSet<int>
        {
            1,
            2,
        };

        Assert.That(a.SetEquals(b), Is.False);
    }

    [Test]
    public void SetEquals_Enumerable_True()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new HashSet<int>
        {
            3,
            2,
            1,
        };

        Assert.That(a.SetEquals(b), Is.True);
    }

    [Test]
    public void SetEquals_Enumerable_False_SameCount()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new HashSet<int>
        {
            2,
            1,
            0,
        };

        Assert.That(a.SetEquals(b), Is.False);
    }

    [Test]
    public void SetEquals_Enumerable_False_DifferentCount()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new HashSet<int>
        {
            3,
            2,
            1,
            0,
        };

        Assert.That(a.SetEquals(b), Is.False);
    }

    [Test]
    public void IsSuperset()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new OrderedListSet<int>
        {
            2,
            3,
        };

        Assert.That(a.ToImmutable().IsSupersetOf(b.ToImmutable()), Is.True);
        Assert.That(b.ToImmutable().IsSupersetOf(a.ToImmutable()), Is.False);
    }

    [Test]
    public void Overlaps_True()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new OrderedListSet<int>
        {
            2,
            3,
        };

        Assert.That(a.Overlaps(b), Is.True);
        Assert.That(b.Overlaps(a), Is.True);
    }

    [Test]
    public void Overlaps_False()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new OrderedListSet<int>
        {
            4,
            5,
        };

        Assert.That(a.Overlaps(b), Is.False);
        Assert.That(b.Overlaps(a), Is.False);
    }

    [Test]
    public void Overlaps_False_Empty()
    {
        var a = new OrderedListSet<int>
        {
            1,
            2,
            3,
        };

        var b = new OrderedListSet<int>();

        Assert.That(a.Overlaps(b), Is.False);
        Assert.That(b.Overlaps(a), Is.False);
    }
}
