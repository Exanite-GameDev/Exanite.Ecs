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
                    commandBuffer.Destroy(entity);
                }
            }
        }
        commandBuffer.Destroy(allEntitiesQuery);

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
        commandBuffer.Destroy(allEntitiesQuery);

        commandBuffer.Execute().Dispose();
        Assert.AreEqual(entityAddCount, handler.EntityRemovedCount);
    }

    private class WorldEventHandler :
        IEventHandler<EntityAddedEvent>,
        IEventHandler<EntityRemovedEvent>,
        IEventHandler<ComponentAdded<Component0>>,
        IEventHandler<ComponentModified<Component0>>,
        IEventHandler<ComponentRemoved<Component0>>,
        IEventHandler<ComponentDestroyed<Component0>>
    {
        public int EntityAddedCount { get; private set; }
        public int EntityRemovedCount { get; private set; }

        public int ComponentAddedCount { get; private set; }
        public int ComponentModifiedCount { get; private set; }
        public int ComponentRemovedCount { get; private set; }
        public int ComponentDestroyedCount { get; private set; }

        public WorldEventHandler RegisterAll(World world)
        {
            world.EventBus.Register<EntityAddedEvent>(this);
            world.EventBus.Register<EntityRemovedEvent>(this);
            world.EventBus.Register<ComponentAdded<Component0>>(this);
            world.EventBus.Register<ComponentModified<Component0>>(this);
            world.EventBus.Register<ComponentRemoved<Component0>>(this);
            world.EventBus.Register<ComponentDestroyed<Component0>>(this);

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

        public void OnEvent(ComponentAdded<Component0> e)
        {
            ComponentAddedCount++;
        }

        public void OnEvent(ComponentModified<Component0> e)
        {
            ComponentModifiedCount++;
        }

        public void OnEvent(ComponentRemoved<Component0> e)
        {
            ComponentRemovedCount++;
        }

        public void OnEvent(ComponentDestroyed<Component0> e)
        {
            ComponentDestroyedCount++;
        }
    }
}
