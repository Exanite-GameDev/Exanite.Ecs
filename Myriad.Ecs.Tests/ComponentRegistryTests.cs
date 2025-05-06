using Myriad.Ecs.ComponentIds;

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
            ComponentId<ComponentInt32>.Id,
            ComponentId<ComponentInt64>.Id,
            ComponentId.Get(typeof(ComponentInt16)),
            ComponentId.Get(typeof(ComponentFloat)),
        };

        Assert.AreEqual(4, ids.Distinct().Count());

        Assert.AreEqual(typeof(ComponentInt16), ComponentId<ComponentInt16>.Id.Type);
    }

    [TestMethod]
    public void DoesNotReassign()
    {
        var id = ComponentId<ComponentInt32>.Id;
        var id2 = ComponentId<ComponentInt32>.Id;

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
