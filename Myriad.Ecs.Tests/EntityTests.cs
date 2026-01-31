using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs.Components;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class EntityTests
{
    [Fact]
    public void DefaultEntityIsNotAlive()
    {
        Assert.False(default(Entity).IsAlive);
        Assert.False(default(Entity).IsAlive);
    }

    [Fact]
    public void CompareDefaultEntity()
    {
        Assert.Equal(0, default(Entity).CompareTo(default));
    }

    [Fact]
    public void CompareEntityWithSelf()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        var bufferedEntity = commandBuffer.Create();
        commandBuffer.Execute();
        var entity = bufferedEntity.Resolve();

        Assert.Equal(0, entity.CompareTo(entity));
    }

    [Fact]
    public void CompareEntityWithAnother()
    {
        var world = new EcsWorld();
        var b = world.AcquireCommandBuffer();

        var eb1 = b.Create();
        var eb2 = b.Create();

        b.Execute();
        var entity1 = eb1.Resolve();
        var entity2 = eb2.Resolve();

        var c1 = entity1.CompareTo(entity2);
        var c2 = entity2.CompareTo(entity1);

        Assert.NotEqual(c1, c2);
        Assert.NotEqual(0, c1);
        Assert.NotEqual(0, c2);

        Assert.NotEqual(entity1.ToString(), entity2.ToString());
    }

    [Fact]
    public void GetComponent()
    {
        var w = new EcsWorld();
        var b = w.AcquireCommandBuffer();

        var e = b.Create()
            .Set(new EcsInt16(7));

        b.Execute();
        var entity = e.Resolve();

        ref var c = ref entity.GetComponent<EcsInt16>();
        Assert.Equal(7, c.Value);
    }

    [Fact]
    public void GetComponents()
    {
        var w = new EcsWorld();
        var b = w.AcquireCommandBuffer();

        var e = b.Create().Set(new EcsInt16(7));

        b.Execute();
        var entity = e.Resolve();

        Assert.Equal(1, entity.ComponentIds.Count);
        Assert.True(entity.ComponentIds.Contains(ComponentId.Get<EcsInt16>()));
    }

    [Fact]
    public void GetComponentDead()
    {
        var w = new EcsWorld();
        var b = w.AcquireCommandBuffer();

        var e = b.Create().Set(new EcsInt16(7));

        b.Execute();
        var entity = e.Resolve();

        b.Destroy(entity);
        b.Execute();

        Assert.Throws<GuardException>(() =>
        {
            _ = entity.ComponentIds.Count;
        });
    }

    [Fact]
    public void GetBoxedComponents()
    {
        var w = new EcsWorld();
        var b = w.AcquireCommandBuffer();

        var e = b.Create().Set(new EcsInt16(7));

        b.Execute();
        var entity = e.Resolve();

        Assert.Equal(1, entity.BoxedComponents.Length);
        Assert.Equal(new EcsInt16(7), (EcsInt16)entity.BoxedComponents[0]);
    }

    [Fact]
    public void GetBoxedComponent()
    {
        var w = new EcsWorld();
        var b = w.AcquireCommandBuffer();

        var e = b.Create()
            .Set(new EcsInt16(7));

        b.Execute();
        var entity = e.Resolve();

        var c = (EcsInt16)entity.GetBoxedComponent(ComponentId.Get<EcsInt16>())!;
        Assert.Equal(7, c.Value);

        Assert.Null(entity.GetBoxedComponent(ComponentId.Get<EcsInt32>()));

        b.Destroy(entity);
        b.Execute();

        Assert.Null(entity.GetBoxedComponent(ComponentId.Get<EcsInt16>()));
        Assert.Null(entity.GetBoxedComponent(ComponentId.Get<EcsInt32>()));
    }
}
