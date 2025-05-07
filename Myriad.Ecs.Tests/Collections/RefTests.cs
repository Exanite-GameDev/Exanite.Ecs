using Exanite.Myriad.Ecs.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Exanite.Myriad.Ecs.Tests.Collections;

[TestClass]
public class RefTests
{
    [TestMethod]
    public void RefItem()
    {
        var i = 1;
        var r = new Ref<int>(ref i);

        r.Value++;

        Assert.AreEqual(i, r.Value);
        Assert.AreEqual(2, i);
    }

    [TestMethod]
    public void RefItemCast()
    {
        var i = 1;
        var r = new Ref<int>(ref i);
        r.Value++;

        var j = r.Value;

        Assert.AreEqual(i, j);
        Assert.AreEqual(2, j);
    }
}
