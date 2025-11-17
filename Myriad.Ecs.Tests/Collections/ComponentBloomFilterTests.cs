using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests.Collections;

public class ComponentBloomFilterTests
{
    [Fact]
    public void EmptyNotIntersect()
    {
        var a = new ComponentBloomFilter();
        var b = new ComponentBloomFilter();

        Assert.False(a.MaybeIntersects(ref b));
    }

    [Fact]
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
        Assert.False(i);

        // Note that this test _can_ fail, since the bloom filter is probabilistic and errors on the side of caution.
    }

    [Fact]
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

        Assert.True(a.MaybeIntersects(ref b));
    }

    [Fact]
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

        Assert.False(a.MaybeIntersects(ref b));
        Assert.False(b.MaybeIntersects(ref c));
        Assert.True(a.MaybeIntersects(ref c));

        var d = new ComponentBloomFilter();
        d.Union(ref b);
        d.Union(ref c);

        Assert.True(a.MaybeIntersects(ref d));
    }
}
