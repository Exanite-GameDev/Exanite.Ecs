using System;
using System.Collections.Generic;
using System.Linq;
using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs.Components;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class TypeRegistryTests
{
    [Fact]
    public void ComponentId_CannotUseOpenGeneric()
    {
        Assert.Throws<GuardException>(() =>
        {
            ComponentId.Get(typeof(EcsGeneric<>));
        });
    }

    [Fact]
    public void ComponentId_CanUseComponent()
    {
        ComponentId.Get(typeof(Ecs0));
    }

    [Fact]
    public void ComponentId_CannotUseNonComponent()
    {
        Assert.Throws<GuardException>(() =>
        {
            ComponentId.Get(typeof(int));
        });

        Assert.Throws<GuardException>(() =>
        {
            ComponentId.Get(typeof(List<>));
        });

        Assert.Throws<GuardException>(() =>
        {
            ComponentId.Get(typeof(List<int>));
        });

        Assert.Throws<GuardException>(() =>
        {
            ComponentId.Get(typeof(IEcsInterface0));
        });
    }

    [Fact]
    public void InterfaceId_CanUseInterfaceComponent()
    {
        InterfaceId.Get(typeof(IEcsInterface0));
    }

    [Fact]
    public void InterfaceId_CannotUseNonInterfaceComponent()
    {
        Assert.Throws<GuardException>(() =>
        {
            InterfaceId.Get(typeof(Ecs0));
        });

        Assert.Throws<GuardException>(() =>
        {
            InterfaceId.Get(typeof(int));
        });

        Assert.Throws<GuardException>(() =>
        {
            InterfaceId.Get(typeof(IDisposable));
        });
    }

    [Fact]
    public void TypeId_CanUseComponentOrInterfaceComponent()
    {
        TypeId.Get(typeof(IEcsInterface0));
        TypeId.Get(typeof(Ecs0));
    }

    [Fact]
    public void TypeId_CannotUseNonEcsType()
    {
        Assert.Throws<GuardException>(() =>
        {
            TypeId.Get(typeof(int));
        });

        Assert.Throws<GuardException>(() =>
        {
            TypeId.Get(typeof(IDisposable));
        });
    }

    [Fact]
    public void ComponentId_DoesNotReassign()
    {
        var id1 = ComponentId.Get<Ecs0>();
        var id2 = ComponentId.Get<Ecs0>();

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void InterfaceId_DoesNotReassign()
    {
        var id1 = InterfaceId.Get<IEcsInterface0>();
        var id2 = InterfaceId.Get<IEcsInterface0>();

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void TypeId_Equals_ComponentId()
    {
        var id1 = TypeId.Get<Ecs0>();
        var id2 = ComponentId.Get<Ecs0>();

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void TypeId_Equals_InterfaceId()
    {
        var id1 = TypeId.Get<IEcsInterface0>();
        var id2 = InterfaceId.Get<IEcsInterface0>();

        Assert.Equal(id1, id2);
    }

    [Fact]
    public void AssignsDistinctIds()
    {
        var ids = new TypeId[]
        {
            ComponentId.Get<EcsInt32>(),
            ComponentId.Get<EcsInt64>(),
            ComponentId.Get(typeof(EcsInt16)),
            ComponentId.Get(typeof(EcsFloat)),

            InterfaceId.Get<IEcsInterface0>(),
            InterfaceId.Get(typeof(IEcsInterface1)),
        };

        Assert.Equal(6, ids.Distinct().Count());
    }

    [Fact]
    public void CanResolveType()
    {
        Assert.Equal(typeof(EcsInt16), ComponentId.Get<EcsInt16>().Type);
        Assert.Equal(typeof(IEcsInterface0), InterfaceId.Get<IEcsInterface0>().Type);

        Assert.Equal(typeof(EcsInt16), TypeId.Get<EcsInt16>().Type);
        Assert.Equal(typeof(IEcsInterface0), TypeId.Get<IEcsInterface0>().Type);
    }

    [Fact]
    public void ThrowsForZeroInitializedId()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = default(ComponentId).Type;
        });

        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            _ = default(InterfaceId).Type;
        });

        Assert.Throws<GuardException>(() =>
        {
            _ = default(TypeId).Type;
        });
    }
}
