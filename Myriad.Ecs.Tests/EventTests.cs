using System;
using Exanite.Core.Events;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Events;
using Exanite.Myriad.Ecs.Queries;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class EventTests
{
    [Fact]
    public void CreateEntity_RaisesEntityCreatedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.EntityCreatedCount);
    }

    [Fact]
    public void DestroyEntity_UsingEntities_RaisesEntityDestroyedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute();

        // Destroy entities
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.Archetypes)
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer.Destroy(entity);
                }
            }
        }
        commandBuffer.Destroy(allEntitiesQuery);

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.EntityDestroyedCount);
    }

    [Fact]
    public void DestroyEntity_UsingQuery_RaisesEntityDestroyedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute();

        // Destroy entities
        var allEntitiesQuery = new QueryBuilder().Build(world);
        commandBuffer.Destroy(allEntitiesQuery);

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.EntityDestroyedCount);
    }

    [Fact]
    public void DestroyEntity_UsingEntities_RaisesComponentRemovedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create().Set(new Component0());
        }

        commandBuffer.Execute();

        // Destroy entities
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.Archetypes)
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer.Destroy(entity);
                }
            }
        }
        commandBuffer.Destroy(allEntitiesQuery);

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentRemovedCount);
    }

    [Fact]
    public void DestroyEntity_UsingQuery_RaisesComponentRemovedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create().Set(new Component0());
        }

        commandBuffer.Execute();

        // Destroy entities
        var allEntitiesQuery = new QueryBuilder().Build(world);
        commandBuffer.Destroy(allEntitiesQuery);

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentRemovedCount);
    }

    [Fact]
    public void RemoveComponent_RaisesComponentRemovedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create().Set(new Component0());
        }

        commandBuffer.Execute();

        // Remove components
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.Archetypes)
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer.Remove<Component0>(entity);
                }
            }
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentRemovedCount);
    }

    [Fact]
    public void SetComponent_Once_OnBufferedEntity_RaisesComponentAddedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create().Set(new Component0());
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentAddedCount);
    }

    [Fact]
    public void SetComponent_Once_OnWorldEntity_RaisesComponentAddedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute();

        // Set components
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.Archetypes)
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer.Set(entity, new Component0());
                }
            }
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentAddedCount);
    }

    [Fact]
    public void SetComponent_Twice_InDifferentCommandBuffers_OnBufferedEntity_RaisesComponentAddedAndComponentModifiedEvents()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create().Set(new Component0());
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentAddedCount);

        // Set components
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.Archetypes)
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer.Set(entity, new Component0());
                }
            }
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentModifiedCount);
    }

    [Fact]
    public void SetComponent_Twice_InDifferentCommandBuffers_OnWorldEntity_RaisesComponentAddedAndComponentModifiedEvents()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute();

        // Set components
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.Archetypes)
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer.Set(entity, new Component0());
                }
            }
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentAddedCount);

        // Set components again
        foreach (var archetype in allEntitiesQuery.Archetypes)
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer.Set(entity, new Component0());
                }
            }
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentModifiedCount);
    }

    [Fact]
    public void SetComponent_Twice_InSameCommandBuffer_OnBufferedEntity_OnlyRaisesComponentAddedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create()
                .Set(new Component0())
                .Set(new Component0());
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentAddedCount);
        Assert.Equal(0, handler.ComponentModifiedCount);
    }

    [Fact]
    public void SetComponent_Twice_InSameCommandBuffer_OnWorldEntity_OnlyRaisesComponentAddedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute();

        // Set components
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.Archetypes)
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer
                        .Set(entity, new Component0())
                        .Set(entity, new Component0());
                }
            }
        }

        commandBuffer.Execute();
        Assert.Equal(entityAddCount, handler.ComponentAddedCount);
        Assert.Equal(0, handler.ComponentModifiedCount);
    }

    [Fact]
    public void DestroyEntity_AfterModifyingEntity_InSameCommandBuffer_OnlyRaisesEntityDestroyedEvent()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute();

        // Modify entity and destroy entity
        var allEntitiesQuery = new QueryBuilder().Build(world);
        foreach (var archetype in allEntitiesQuery.Archetypes)
        {
            foreach (var chunk in archetype.Chunks)
            {
                foreach (var entity in chunk.Entities)
                {
                    commandBuffer.Set(entity, new Component0());
                    commandBuffer.Set(entity, new Component0());

                    commandBuffer.Destroy(entity);
                }
            }
        }

        commandBuffer.Execute();
        Assert.Equal(0, handler.ComponentAddedCount);
        Assert.Equal(0, handler.ComponentModifiedCount);
        Assert.Equal(entityAddCount, handler.EntityDestroyedCount);
    }

    [Fact]
    public void DisposeWorld_DestroysAllEntities_And_RaisesComponentRemovedAndEntityDestroyedEvents()
    {
        var world = new EcsWorld();
        var handler = new WorldEventHandler().RegisterAll(world);
        var commandBuffer = world.AcquireCommandBuffer();

        world.EventBus.RegisterForwardAllTo(new EventLogger());

        // Create entities
        var entityAddCount = 10;
        for (var i = 0; i < entityAddCount; i++)
        {
            commandBuffer.Create()
                .Set(new Component0());
        }

        commandBuffer.Execute();

        // Dispose world
        world.Dispose();

        Assert.Equal(entityAddCount, handler.EntityDestroyedCount);
        Assert.Equal(entityAddCount, handler.ComponentRemovedCount);
    }

    [Fact]
    public void CommandBufferExecute_AfterDisposingWorld_ThrowsException()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        world.Dispose();
        Assert.Throws<GuardException>(() => commandBuffer.Execute());
    }

    [Fact]
    public void ComponentDisposable_IsDisposed_WhenDestroyed()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        var disposeCount = 0;
        var bufferedEntity = commandBuffer.Create()
            .Set(new ComponentDisposable()
            {
                DisposeAction = () => disposeCount++,
            });

        commandBuffer.Execute();
        Assert.Equal(0, disposeCount);

        var entity = bufferedEntity.Resolve();
        commandBuffer.Destroy(entity);

        commandBuffer.Execute();
        Assert.Equal(1, disposeCount);
    }

    [Fact]
    public void ComponentDisposable_IsDisposed_WhenOverwritten_InCommandBuffer()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        var disposeCount = 0;
        var bufferedEntity = commandBuffer.Create()
            .Set(new ComponentDisposable()
            {
                DisposeAction = () => disposeCount++,
            });
        Assert.Equal(0, disposeCount);

        bufferedEntity.Set(new ComponentDisposable()
        {
            DisposeAction = () => disposeCount++,
        });
        Assert.Equal(1, disposeCount);
    }

    [Fact]
    public void ComponentDisposable_IsDisposed_WhenCommandBuffer_IsCleared()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        var disposeCount = 0;
        commandBuffer.Create()
            .Set(new ComponentDisposable()
            {
                DisposeAction = () => disposeCount++,
            });
        Assert.Equal(0, disposeCount);

        commandBuffer.Clear();
        Assert.Equal(1, disposeCount);
    }

    [Fact]
    public void DisposingWorld_ClearsCommandBuffer()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        var disposeCount = 0;
        commandBuffer.Create()
            .Set(new ComponentDisposable()
            {
                DisposeAction = () => disposeCount++,
            });
        Assert.Equal(0, disposeCount);

        world.Dispose();
        Assert.False(commandBuffer.HasBufferedOperations);
    }

    private struct ComponentDisposable : IComponent, IDisposable
    {
        public required Action DisposeAction;

        public void Dispose()
        {
            DisposeAction.Invoke();
        }
    }

    private class WorldEventHandler :
        IEventHandler<EntityCreatedEvent>,
        IEventHandler<EntityDestroyedEvent>,
        IEventHandler<ComponentAdded<Component0>>,
        IEventHandler<ComponentModified<Component0>>,
        IEventHandler<ComponentRemoved<Component0>>
    {
        public int EntityCreatedCount { get; private set; }
        public int EntityDestroyedCount { get; private set; }

        public int ComponentAddedCount { get; private set; }
        public int ComponentModifiedCount { get; private set; }
        public int ComponentRemovedCount { get; private set; }

        public WorldEventHandler RegisterAll(EcsWorld world)
        {
            world.EventBus.Register<EntityCreatedEvent>(this);
            world.EventBus.Register<EntityDestroyedEvent>(this);
            world.EventBus.Register<ComponentAdded<Component0>>(this);
            world.EventBus.Register<ComponentModified<Component0>>(this);
            world.EventBus.Register<ComponentRemoved<Component0>>(this);

            return this;
        }

        public void OnEvent(EntityCreatedEvent e)
        {
            EntityCreatedCount++;
        }

        public void OnEvent(EntityDestroyedEvent e)
        {
            EntityDestroyedCount++;
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
    }

    private class EventLogger : IAllEventHandler
    {
        void IAllEventHandler.OnEvent<T>(T e)
        {
            typeof(T).Dump("Event type");
        }
    }
}
