using Exanite.Core.Events;
using Exanite.Myriad.Ecs.Events;
using Exanite.Myriad.Ecs.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Exanite.Myriad.Ecs.Tests;

[TestClass]
public class EventTests
{
    [TestMethod]
    public void CreatingEntities_RaisesEvents()
    {
        var world = new World();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireEventBuffer();

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.EntityAddedCount);
    }

    // TODO: Add test cases for destroying entities with phantom components

    [TestMethod]
    public void DestroyingEntities_UsingEntities_RaisesEvents()
    {
        var world = new World();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireEventBuffer();

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.EntityAddedCount);

        // Destroy entities
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.GetArchetypes())
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities.Span)
                {
                    commandBuffer.Delete(entity);
                }
            }
        }
        commandBuffer.Delete(allEntitiesQuery);

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.EntityRemovedCount);
    }

    [TestMethod]
    public void DestroyingEntities_UsingQuery_RaisesEvents()
    {
        var world = new World();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireEventBuffer();

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.EntityAddedCount);

        // Destroy entities
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
