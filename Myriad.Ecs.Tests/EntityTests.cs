using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Components;
using NUnit.Framework;

namespace Exanite.Myriad.Ecs.Tests;

[TestFixture]
public class EntityTests
{
    [Test]
    public void DefaultEntityIsNotAlive()
    {
        Assert.That(default(Entity).IsAlive, Is.False);
        Assert.That(default(Entity).IsAlive, Is.False);
    }

    [Test]
    public void CompareDefaultEntity()
    {
        Assert.That(default(Entity).CompareTo(default), Is.EqualTo(0));
    }

    [Test]
    public void CompareEntityWithSelf()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        var bufferedEntity = commandBuffer.Create();
        commandBuffer.Execute();
        var entity = bufferedEntity.Resolve();

        Assert.That(entity.CompareTo(entity), Is.EqualTo(0));
    }

    [Test]
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

        Assert.That(c2, Is.Not.EqualTo(c1));
        Assert.That(c1, Is.Not.EqualTo(0));
        Assert.That(c2, Is.Not.EqualTo(0));

        Assert.That(entity2.ToString(), Is.Not.EqualTo(entity1.ToString()));
    }

    [Test]
    public void GetComponent()
    {
        var w = new EcsWorld();
        var b = w.AcquireCommandBuffer();

        var e = b.Create()
                 .Set(new ComponentInt16(7));

        b.Execute();
        var entity = e.Resolve();

        ref var c = ref entity.GetComponent<ComponentInt16>();
        Assert.That(c.Value, Is.EqualTo(7));
    }

    [Test]
    public void GetComponents()
    {
        var w = new EcsWorld();
        var b = w.AcquireCommandBuffer();

        var e = b.Create().Set(new ComponentInt16(7));

        b.Execute();
        var entity = e.Resolve();

        Assert.That(entity.ComponentIds.Count, Is.EqualTo(1));
        Assert.That(entity.ComponentIds.Contains(ComponentId.Get<ComponentInt16>()), Is.True);
    }

    [Test]
    public void GetComponentDead()
    {
        var w = new EcsWorld();
        var b = w.AcquireCommandBuffer();

        var e = b.Create().Set(new ComponentInt16(7));

        b.Execute();
        var entity = e.Resolve();

        b.Destroy(entity);
        b.Execute();

        Assert.Throws<GuardException>(() =>
        {
            _ = entity.ComponentIds.Count;
        });
    }

    [Test]
    public void GetBoxedComponents()
    {
        var w = new EcsWorld();
        var b = w.AcquireCommandBuffer();

        var e = b.Create().Set(new ComponentInt16(7));

        b.Execute();
        var entity = e.Resolve();

        Assert.That(entity.BoxedComponents.Length, Is.EqualTo(1));
        Assert.That((ComponentInt16)entity.BoxedComponents[0], Is.EqualTo(new ComponentInt16(7)));
    }

    [Test]
    public void GetBoxedComponent()
    {
        var w = new EcsWorld();
        var b = w.AcquireCommandBuffer();

        var e = b.Create()
                 .Set(new ComponentInt16(7));

        b.Execute();
        var entity = e.Resolve();

        var c = (ComponentInt16)entity.GetBoxedComponent(ComponentId.Get<ComponentInt16>())!;
        Assert.That(c.Value, Is.EqualTo(7));

        Assert.That(entity.GetBoxedComponent(ComponentId.Get<ComponentInt32>()), Is.Null);

        b.Destroy(entity);
        b.Execute();

        Assert.That(entity.GetBoxedComponent(ComponentId.Get<ComponentInt16>()), Is.Null);
        Assert.That(entity.GetBoxedComponent(ComponentId.Get<ComponentInt32>()), Is.Null);
    }
}
