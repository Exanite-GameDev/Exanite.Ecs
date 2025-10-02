using System;
using System.Linq;
using Exanite.Myriad.Ecs.Components;
using NUnit.Framework;

namespace Exanite.Myriad.Ecs.Tests;

[TestFixture]
public class ComponentRegistryTests
{
    [Test]
    public void CannotAssignNonComponent()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            ComponentId.Get(typeof(int));
        });
    }

    [Test]
    public void AssignsDistinctIds()
    {
        var ids = new[]
        {
            ComponentId.Get<ComponentInt32>(),
            ComponentId.Get<ComponentInt64>(),
            ComponentId.Get(typeof(ComponentInt16)),
            ComponentId.Get(typeof(ComponentFloat)),
        };

        Assert.That(ids.Distinct().Count(), Is.EqualTo(4));

        Assert.That(ComponentId.Get<ComponentInt16>().Type, Is.EqualTo(typeof(ComponentInt16)));
    }

    [Test]
    public void DoesNotReassign()
    {
        var id = ComponentId.Get<ComponentInt32>();
        var id2 = ComponentId.Get<ComponentInt32>();

        Assert.That(id2, Is.EqualTo(id));
    }

    [Test]
    public void ThrowsForUnknownId()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = default(ComponentId).Type;
        });
    }
}
