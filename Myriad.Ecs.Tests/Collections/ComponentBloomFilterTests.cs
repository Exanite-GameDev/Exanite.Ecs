using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Exanite.Myriad.Ecs.Tests.Collections;

[TestFixture]
public class ComponentBloomFilterTests
{
    [Test]
    public void EmptyNotIntersect()
    {
        var a = new ComponentBloomFilter();
        var b = new ComponentBloomFilter();

        Assert.That(a.MaybeIntersects(ref b), Is.False);
    }

    [Test]
    public void DisjointNotIntersect()
    {
        var a = new ComponentBloomFilter();
        a.Add(ComponentId.Get<Component0>());
        a.Add(ComponentId.Get<Component1>());
        a.Add(ComponentId.Get<Component2>());
        a.Add(ComponentId.Get<Component3>());

        var b = new ComponentBloomFilter();
        b.Add(ComponentId.Get<Component8>());
        b.Add(ComponentId.Get<Component9>());
        b.Add(ComponentId.Get<Component10>());
        b.Add(ComponentId.Get<Component11>());

        var i = a.MaybeIntersects(ref b);
        Assert.That(i, Is.False);

        // Note that this test _can_ fail, since the bloom filter is probabilistic and errors on the side of caution.
    }

    [Test]
    public void IntersectingIntersect()
    {
        var a = new ComponentBloomFilter();
        a.Add(ComponentId.Get<Component0>());
        a.Add(ComponentId.Get<Component1>());
        a.Add(ComponentId.Get<Component4>());

        var b = new ComponentBloomFilter();
        b.Add(ComponentId.Get<Component2>());
        b.Add(ComponentId.Get<Component3>());
        b.Add(ComponentId.Get<Component4>());

        Assert.That(a.MaybeIntersects(ref b), Is.True);
    }

    [Test]
    public void UnionIntersects()
    {
        var a = new ComponentBloomFilter();
        a.Add(ComponentId.Get<Component0>());
        a.Add(ComponentId.Get<Component1>());
        a.Add(ComponentId.Get<Component2>());

        var b = new ComponentBloomFilter();
        b.Add(ComponentId.Get<Component3>());
        b.Add(ComponentId.Get<Component4>());
        b.Add(ComponentId.Get<Component5>());

        var c = new ComponentBloomFilter();
        c.Add(ComponentId.Get<Component0>());

        Assert.That(a.MaybeIntersects(ref b), Is.False);
        Assert.That(b.MaybeIntersects(ref c), Is.False);
        Assert.That(a.MaybeIntersects(ref c), Is.True);

        var d = new ComponentBloomFilter();
        d.Union(ref b);
        d.Union(ref c);

        Assert.That(a.MaybeIntersects(ref d), Is.True);
    }
}
