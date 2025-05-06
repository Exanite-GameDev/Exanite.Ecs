using Microsoft.VisualStudio.TestTools.UnitTesting;
using Myriad.Ecs.Components;

namespace Myriad.Ecs.Tests;

[TestClass]
public class ComponentRegistryTests
{
    [TestMethod]
    public void CannotAssignNonComponent()
    {
        Assert.ThrowsException<ArgumentException>(() =>
        {
            ComponentId.Get(typeof(int));
        });
    }

    [TestMethod]
    public void AssignsDistinctIds()
    {
        var ids = new[]
        {
            ComponentId.Get<ComponentInt32>(),
            ComponentId.Get<ComponentInt64>(),
            ComponentId.Get(typeof(ComponentInt16)),
            ComponentId.Get(typeof(ComponentFloat)),
        };

        Assert.AreEqual(4, ids.Distinct().Count());

        Assert.AreEqual(typeof(ComponentInt16), ComponentId.Get<ComponentInt16>().Type);
    }

    [TestMethod]
    public void DoesNotReassign()
    {
        var id = ComponentId.Get<ComponentInt32>();
        var id2 = ComponentId.Get<ComponentInt32>();

        Assert.AreEqual(id, id2);
    }

    [TestMethod]
    public void ThrowsForUnknownId()
    {
        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            var t = default(ComponentId).Type;
        });
    }

    [TestMethod]
    public void PhantomEntityHasPhantomFlag()
    {
        Assert.IsTrue(ComponentId.Get(typeof(TestPhantom0)).IsPhantomComponent);
        Assert.IsTrue(ComponentId.Get(typeof(TestPhantom1)).IsPhantomComponent);
        Assert.IsTrue(ComponentId.Get(typeof(TestPhantom2)).IsPhantomComponent);
        Assert.IsFalse(ComponentId.Get(typeof(Component1)).IsPhantomComponent);
    }
}
