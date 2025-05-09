using Exanite.Core.Events;
using Exanite.Myriad.Ecs.Events;
using Exanite.Myriad.Ecs.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Exanite.Myriad.Ecs.Tests;

[TestClass]
public class EventTests
{
    [TestMethod]
    public void CreatingAndDestroyingEntity_RaisesEvents()
    {
        var world = new World();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireEventBuffer();

        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.EntityAddedCount);

        var allEntitiesQuery = new QueryBuilder().Build(world);
        commandBuffer.Delete(allEntitiesQuery);

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.EntityRemovedCount);
    }

    private class WorldEventHandler :
        IEventHandler<EntityAddedEvent>,
        IEventHandler<EntityRemovedEvent>,
        IEventHandler<ComponentAdded<ComponentTest>>,
        IEventHandler<ComponentModified<ComponentTest>>,
        IEventHandler<ComponentRemoved<ComponentTest>>,
        IEventHandler<ComponentDestroyed<ComponentTest>>
    {
        public int EntityAddedCount { get; set; }
        public int EntityRemovedCount { get; set; }

        public int ComponentAddedCount { get; set; }
        public int ComponentModifiedCount { get; set; }
        public int ComponentRemovedCount { get; set; }
        public int ComponentDestroyedCount { get; set; }

        public WorldEventHandler RegisterAll(World world)
        {
            world.EventBus.Register<EntityAddedEvent>(this);
            world.EventBus.Register<EntityRemovedEvent>(this);
            world.EventBus.Register<ComponentAdded<ComponentTest>>(this);
            world.EventBus.Register<ComponentModified<ComponentTest>>(this);
            world.EventBus.Register<ComponentRemoved<ComponentTest>>(this);
            world.EventBus.Register<ComponentDestroyed<ComponentTest>>(this);

            return this;
        }

        public void OnEvent(EntityAddedEvent e)
        {
            EntityAddedCount++;
        }

        public void OnEvent(EntityRemovedEvent e)
        {
            EntityRemovedCount++;
        }

        public void OnEvent(ComponentAdded<ComponentTest> e)
        {
            ComponentAddedCount++;
        }

        public void OnEvent(ComponentModified<ComponentTest> e)
        {
            ComponentModifiedCount++;
        }

        public void OnEvent(ComponentRemoved<ComponentTest> e)
        {
            ComponentRemovedCount++;
        }

        public void OnEvent(ComponentDestroyed<ComponentTest> e)
        {
            ComponentDestroyedCount++;
        }
    }

    private struct ComponentTest : IComponent;
}
