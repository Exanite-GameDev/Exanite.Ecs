using Myriad.Ecs.Collections;
using Myriad.Ecs.ComponentIds;

namespace Myriad.Ecs.Tests.Collections;

[TestClass]
public class ComponentBloomFilterTests
{
    [TestMethod]
    public void EmptyNotIntersect()
    {
        var a = new ComponentBloomFilter();
        var b = new ComponentBloomFilter();

        Assert.IsFalse(a.MaybeIntersects(ref b));
    }

    [TestMethod]
    public void DisjointNotIntersect()
    {
        var a = new ComponentBloomFilter();
        a.Add(ComponentId<Component0>.Id);
        a.Add(ComponentId<Component1>.Id);
        a.Add(ComponentId<Component2>.Id);
        a.Add(ComponentId<Component3>.Id);

        var b = new ComponentBloomFilter();
        b.Add(ComponentId<Component8>.Id);
        b.Add(ComponentId<Component9>.Id);
        b.Add(ComponentId<Component10>.Id);
        b.Add(ComponentId<Component11>.Id);

        var i = a.MaybeIntersects(ref b);
        Assert.IsFalse(i);

        // Note that this test _can_ fail, since the bloom filter is probabalistic and errs on the side of caution.
    }

    [TestMethod]
    public void IntersectingIntersect()
    {
        var a = new ComponentBloomFilter();
        a.Add(ComponentId<Component0>.Id);
        a.Add(ComponentId<Component1>.Id);
        a.Add(ComponentId<Component4>.Id);

        var b = new ComponentBloomFilter();
        b.Add(ComponentId<Component2>.Id);
        b.Add(ComponentId<Component3>.Id);
        b.Add(ComponentId<Component4>.Id);

        Assert.IsTrue(a.MaybeIntersects(ref b));
    }

    [TestMethod]
    public void UnionIntersects()
    {
        var a = new ComponentBloomFilter();
        a.Add(ComponentId<Component0>.Id);
        a.Add(ComponentId<Component1>.Id);
        a.Add(ComponentId<Component2>.Id);

        var b = new ComponentBloomFilter();
        b.Add(ComponentId<Component3>.Id);
        b.Add(ComponentId<Component4>.Id);
        b.Add(ComponentId<Component5>.Id);

        var c = new ComponentBloomFilter();
        c.Add(ComponentId<Component0>.Id);

        Assert.IsFalse(a.MaybeIntersects(ref b));
        Assert.IsFalse(b.MaybeIntersects(ref c));
        Assert.IsTrue(a.MaybeIntersects(ref c));

        var d = new ComponentBloomFilter();
        d.Union(ref b);
        d.Union(ref c);

        Assert.IsTrue(a.MaybeIntersects(ref d));
    }
}
