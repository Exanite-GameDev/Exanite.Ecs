using System;
using System.Linq;
using Exanite.Myriad.Ecs.Components;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class ComponentRegistryTests
{
    [Fact]
    public void CannotAssignNonComponent()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            ComponentId.Get(typeof(int));
        });
    }

    [Fact]
    public void AssignsDistinctIds()
    {
        var ids = new[]
        {
            ComponentId.Get<ComponentInt32>(),
            ComponentId.Get<ComponentInt64>(),
            ComponentId.Get(typeof(ComponentInt16)),
            ComponentId.Get(typeof(ComponentFloat)),
        };

        Assert.Equal(4, ids.Distinct().Count());

        Assert.Equal(typeof(ComponentInt16), ComponentId.Get<ComponentInt16>().Type);
    }

    [Fact]
    public void DoesNotReassign()
    {
        var id = ComponentId.Get<ComponentInt32>();
        var id2 = ComponentId.Get<ComponentInt32>();

        Assert.Equal(id, id2);
    }

    [Fact]
    public void ThrowsForUnknownId()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            _ = default(ComponentId).Type;
        });
    }
}
