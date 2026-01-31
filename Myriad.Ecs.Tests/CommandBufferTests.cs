using System;
using System.Collections.Generic;
using System.Linq;
using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Queries;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class EcsCommandBufferTests
{
    [Fact]
    public void CreateCommandBuffer()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();
        Assert.NotNull(commandBuffer);
    }

    [Fact]
    public void CreateEntity()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        var eb = commandBuffer.Create();
        Assert.Equal(commandBuffer, eb.CommandBuffer);

        commandBuffer.Execute();
        var entity = eb.Resolve();

        Assert.True(entity.IsAlive);
        Assert.Equal(1, world.ArchetypesList.Count);
        Assert.Equal(0, world.ArchetypesList.Single().Components.Count);
    }

    [Fact]
    public void CreateManyEntities()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        // Create lots of entities
        var buffered = new List<BufferedEntity>();
        for (var i = 0; i < 50000; i++)
        {
            buffered.Add(commandBuffer.Create().Set(new EcsInt32(i)));
        }

        // Execute buffer
        commandBuffer.Execute();

        // Resolve results
        var entities = new List<Entity>();
        foreach (var bufferedEntity in buffered)
        {
            entities.Add(bufferedEntity.Resolve());
        }

        for (var i = 0; i < entities.Count; i++)
        {
            var entity = entities[i];
            Assert.True(entity.IsAlive);
            Assert.Equal(1, world.ArchetypesList.Count);
            Assert.Equal(1, world.ArchetypesList.Single().Components.Count);
            Assert.Equal(i, entity.GetComponent<EcsInt32>().Value);
        }
    }

    [Fact]
    public void ChurnCreateDestroy()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        var rng = new Random(46576);

        // keep track ofevery single entity ever created
        var alive = new List<Entity>();
        var dead = new List<Entity>();

        // Do lots of rounds of creation and destruction
        for (var i = 0; i < 20; i++)
        {
            // Create lots of entities
            var buffered = new List<BufferedEntity>();
            for (var j = 0; j < 10000; j++)
            {
                var b = buffer.Create().Set(new EcsInt32(j));
                buffered.Add(b);

                for (var k = 0; k < 3; k++)
                {
                    switch (rng.Next(0, 6))
                    {
                        case 0: b.Set(new EcsByte((byte)i)); break;
                        case 1: b.Set(new EcsInt16((short)i)); break;
                        case 2: b.Set(new EcsFloat(i)); break;
                        case 3: b.Set(new EcsInt32(i)); break;
                        case 4: b.Set(new EcsInt64(i)); break;
                    }
                }
            }

            // Destroy some random entities
            for (var j = 0; j < 1000; j++)
            {
                if (alive.Count == 0)
                {
                    break;
                }

                var index = rng.Next(0, alive.Count);
                var ent = alive[index];
                Assert.True(ent.IsAlive);
                buffer.Destroy(ent);
                alive.RemoveAt(index);
                dead.Add(ent);
            }

            // Execute
            buffer.Execute();

            // Resolve results
            foreach (var b in buffered)
            {
                alive.Add(b.Resolve());
            }

            // Check all the entities
            foreach (var entity in alive)
            {
                Assert.True(entity.IsAlive);
            }

            foreach (var entity in dead)
            {
                Assert.True(!entity.IsAlive);
            }

            // Check archetypes
            Assert.Equal(alive.Count, world.ArchetypesList.Select(a => a.EntityCount).Sum());
        }
    }

    [Fact]
    public void ChurnStructural()
    {
        var world = new EcsWorld();
        var entities = new List<Entity>();

        var setupResolver = TestHelpers.SetupRandomEntities(world, 10_000).Execute();
        for (var i = 0; i < setupResolver.Count; i++)
        {
            entities.Add(setupResolver[i]);
        }

        var commandBuffer = world.AcquireCommandBuffer();
        var random = new Random(551514);
        for (var i = 0; i < 16; i++)
        {
            foreach (var entity in entities)
            {
                // Apply to 10% of entities
                if (random.NextSingle() > 0.1f)
                {
                    continue;
                }

                // Do some random ops
                var count = random.Next(1, 5);
                var update = random.NextSingle() > 0.5;
                for (var j = 0; j < count; j++)
                {
                    switch (random.Next(7))
                    {
                        case 0:
                            ChangeComponent<EcsByte>(entity, commandBuffer, update);
                            break;
                        case 1:
                            ChangeComponent<EcsInt16>(entity, commandBuffer, update);
                            break;
                        case 2:
                            ChangeComponent<EcsFloat>(entity, commandBuffer, update);
                            break;
                        case 3:
                            ChangeComponent<EcsInt32>(entity, commandBuffer, update);
                            break;
                        case 4:
                            ChangeComponent<EcsInt64>(entity, commandBuffer, update);
                            break;
                        case 5:
                            ChangeComponent<Ecs0>(entity, commandBuffer, update);
                            break;
                        case 6:
                            ChangeComponent<Ecs1>(entity, commandBuffer, update);
                            break;
                    }
                }
            }

            commandBuffer.Execute();
        }

        void ChangeComponent<T>(Entity e, EcsCommandBuffer b, bool update) where T : struct, IComponent
        {
            if (e.HasComponent<T>() && !update)
            {
                b.Remove<T>(e);
            }
            else
            {
                b.Set(e, default(T));
            }
        }
    }

    [Fact]
    public void ResolveBufferedEntity_AfterSecondExecute_Throws()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        var bufferedEntity = commandBuffer.Create();
        commandBuffer.Execute();

        commandBuffer.Create();
        commandBuffer.Execute();

        Assert.Throws<GuardException>(() =>
        {
            bufferedEntity.Resolve();
        });
    }

    [Fact]
    public void ModifyBufferedEntity_AfterExecute_Throws()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        // Create the entity
        var bufferedEntity = commandBuffer.Create();
        commandBuffer.Execute();

        // Try to modify the buffered entity
        Assert.Throws<GuardException>(() =>
        {
            bufferedEntity.Set(new EcsFloat(8));
        });
    }

    [Fact]
    public void CreateEntityAndSet()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        var bufferedEntity = buffer
            .Create()
            .Set(new EcsFloat(17));

        buffer.Execute();
        var entity = bufferedEntity.Resolve();

        Assert.True(entity.IsAlive);
        Assert.Equal(1, world.ArchetypesList.Count);
        Assert.Equal(1, world.ArchetypesList.Single().Components.Count);
        Assert.True(world.ArchetypesList.Single().Components.Contains(ComponentId.Get<EcsFloat>()));
    }

    [Fact]
    public void SetTwiceOnNewEntityWithOverwrite()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        var bufferedEntity = buffer.Create();

        bufferedEntity.Set(new EcsFloat(1));
        bufferedEntity.Set(new EcsFloat(2));
    }

    [Fact]
    public void DestroyEntity()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        var bufferedEntities = new[]
        {
            buffer.Create().Set(new EcsFloat(1)),
            buffer.Create().Set(new EcsFloat(2)),
            buffer.Create().Set(new EcsFloat(3)),
        };

        buffer.Execute();

        var entities = new[]
        {
            bufferedEntities[0].Resolve(),
            bufferedEntities[1].Resolve(),
            bufferedEntities[2].Resolve(),
        };

        foreach (var entity in entities)
        {
            Assert.True(entity.IsAlive);
        }

        buffer.Destroy(entities[1]);
        buffer.Execute();

        Assert.True(entities[0].IsAlive);
        Assert.False(entities[1].IsAlive);
        Assert.True(entities[2].IsAlive);

        Assert.Equal(1, entities[0].GetComponent<EcsFloat>().Value);
        Assert.Equal(3, entities[2].GetComponent<EcsFloat>().Value);
    }

    [Fact]
    public void DestroyEntityTwice()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        var buffered = buffer.Create().Set(new EcsFloat(1));
        buffer.Execute();
        var entity = buffered.Resolve();
        Assert.True(entity.IsAlive);

        buffer.Destroy(entity);
        buffer.Destroy(entity);
        buffer.Destroy(entity);
        buffer.Destroy(entity);
        buffer.Destroy(entity);
        buffer.Execute();

        Assert.False(entity.IsAlive);
    }

    [Fact]
    public void DestroyDeadEntity()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create an entity
        var buffered = buffer.Create().Set(new EcsFloat(1));
        buffer.Execute();

        var entity = buffered.Resolve();
        Assert.True(entity.IsAlive);

        // Setup deletion for that entity
        buffer.Destroy(entity);

        // Destroy that entity before playing back the first buffer
        var buffer2 = world.AcquireCommandBuffer();
        buffer2.Destroy(entity);
        buffer2.Execute();
        Assert.False(entity.IsAlive);

        // Now play the first buffer back
        buffer.Execute();

        Assert.False(entity.IsAlive);
    }

    [Fact]
    public void DestroyEntities()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        var buffered = new[]
        {
            buffer.Create().Set(new EcsFloat(1)),
            buffer.Create().Set(new EcsFloat(2)),
            buffer.Create().Set(new EcsFloat(3)),
        };

        buffer.Execute();

        var entities = new[]
        {
            buffered[0].Resolve(),
            buffered[1].Resolve(),
            buffered[2].Resolve(),
        };

        foreach (var entity in entities)
        {
            Assert.True(entity.IsAlive);
        }

        buffer.Destroy([entities[0], entities[1]]);
        buffer.Execute();

        Assert.False(entities[0].IsAlive);
        Assert.False(entities[1].IsAlive);
        Assert.True(entities[2].IsAlive);

        Assert.Equal(3, entities[2].GetComponent<EcsFloat>().Value);
    }

    [Fact]
    public void DestroyByQueryMixedWorlds()
    {
        var world1 = new EcsWorld();
        var world2 = new EcsWorld();

        var cmd = world1.AcquireCommandBuffer();

        var q = new QueryFilter().Include<Ecs0>().Build(world2);

        Assert.Throws<GuardException>(() =>
        {
            cmd.Destroy(q);
        });
    }

    [Fact]
    public void DestroyByQuery()
    {
        var world = new EcsWorld();

        TestHelpers.SetupRandomEntities(world, count: 100000).Execute();

        // Get the archetypes we're about to destroy
        var deleting = (from archetype in world.ArchetypesList
            where archetype.Components.Contains(ComponentId.Get<Ecs0>())
            where archetype.Components.Contains(ComponentId.Get<Ecs1>())
            where !archetype.Components.Contains(ComponentId.Get<EcsInt32>())
            select archetype).ToArray();

        // Get all other archetypes
        var others = (from archetype in world.ArchetypesList
            where !deleting.Contains(archetype)
            select (archetype, archetype.EntityCount)).ToArray();

        // Query to destroy
        var q = new QueryFilter().Include<Ecs0>().Include<Ecs1>().Exclude<EcsInt32>().Build(world);

        // Get an entity we're going to destroy
        var dead = q.FirstOrDefault();

        // Destroy it
        var buffer = world.AcquireCommandBuffer();
        buffer.Destroy(q);
        buffer.Execute();

        // Check the archetypes
        foreach (var archetype in deleting)
        {
            Assert.Equal(0, archetype.EntityCount);
        }

        foreach (var (archetype, count) in others)
        {
            Assert.Equal(count, archetype.EntityCount);
        }

        // Check it's dead
        Assert.False(dead.IsAlive);
    }

    [Fact]
    public void ModifyThenDestroy()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create entity
        var buffered = buffer.Create().Set(new EcsFloat(1));
        buffer.Execute();

        var entity = buffered.Resolve();
        Assert.True(entity.IsAlive);

        // Modify it _and_ destroy it
        buffer.Set(entity, new EcsInt64(7));
        buffer.Remove<EcsFloat>(entity);
        buffer.Destroy(entity);

        buffer.Execute();

        // Check it's dead
        Assert.False(entity.IsAlive);
        Assert.Equal(0, new QueryFilter().Include<EcsFloat>().Build(world).Count());
        Assert.Equal(0, new QueryFilter().Include<EcsInt16>().Build(world).Count());
        Assert.Equal(0, new QueryFilter().Include<EcsInt32>().Build(world).Count());
        Assert.Equal(0, new QueryFilter().Include<EcsInt64>().Build(world).Count());
        Assert.Equal(0, new QueryFilter().Include<EcsFloat>().Build(world).Count());
    }

    [Fact]
    public void RemoveFromEntity()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create an entity with 2 components
        var eb = buffer
            .Create()
            .Set(new EcsFloat(123))
            .Set(new EcsInt16(456));

        buffer.Execute();
        var entity = eb.Resolve();

        // Remove a component
        buffer.Remove<EcsInt16>(entity);
        buffer.Execute();

        Assert.Equal(123, entity.GetComponent<EcsFloat>().Value);
        Assert.False(entity.HasComponent<EcsInt16>());
    }

    [Fact]
    public void AddToEntity()
    {
        var world = new EcsWorld();
        var commandBuffer = world.AcquireCommandBuffer();

        // Create an entity with 2 components
        var bufferedEntity = commandBuffer
            .Create()
            .Set(new EcsFloat(123))
            .Set(new EcsInt16(456));

        commandBuffer.Execute();
        var entity = bufferedEntity.Resolve();

        // Add a third
        commandBuffer.Set(entity, new EcsInt32(789));
        commandBuffer.Execute();

        // Check they are all present
        Assert.True(entity.HasComponent<EcsFloat>());
        Assert.True(entity.HasComponent<EcsInt16>());
        Assert.True(entity.HasComponent<EcsInt32>());
    }

    [Fact]
    public void SetOnEntity()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create an entity with 2 components
        var eb = buffer
            .Create()
            .Set(new EcsFloat(123))
            .Set(new EcsInt16(456));

        buffer.Execute();
        var entity = eb.Resolve();

        // Overwrite one
        buffer.Set(entity, new EcsInt16(789));
        buffer.Execute();

        // Check the value has changed
        Assert.True(entity.HasComponent<EcsFloat>());
        Assert.True(entity.HasComponent<EcsInt16>());
        Assert.Equal(789, entity.GetComponent<EcsInt16>().Value);
    }

    [Fact]
    public void SetTwiceOnEntity()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create an entity with 2 components
        var eb = buffer
            .Create()
            .Set(new EcsFloat(123))
            .Set(new EcsInt16(456));

        buffer.Execute();
        var entity = eb.Resolve();

        // Overwrite one, twice
        buffer.Set(entity, new EcsInt16(789));
        buffer.Set(entity, new EcsInt16(987));
        buffer.Execute();

        // Check the value has changed to the latest value
        Assert.True(entity.HasComponent<EcsFloat>());
        Assert.True(entity.HasComponent<EcsInt16>());
        Assert.Equal(987, entity.GetComponent<EcsInt16>().Value);
    }

    [Fact]
    public void SetThenRemoveOnEntity()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create an entity with 2 components
        var eb = buffer
            .Create()
            .Set(new EcsFloat(123))
            .Set(new EcsInt16(456));

        buffer.Execute();
        var entity = eb.Resolve();

        // Overwrite one
        buffer.Set(entity, new EcsInt16(789));

        // Then remove it
        buffer.Remove<EcsInt16>(entity);

        buffer.Execute();

        // Check the value is gone
        Assert.True(entity.HasComponent<EcsFloat>());
        Assert.False(entity.HasComponent<EcsInt16>());
    }

    [Fact]
    public void RemoveInvalidComponent()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create an entity with 2 components
        var eb = buffer
            .Create()
            .Set(new EcsFloat(123))
            .Set(new EcsInt16(456));

        buffer.Execute();
        var entity = eb.Resolve();

        // Remove a component
        buffer.Remove<EcsInt32>(entity);

        buffer.Execute();

        // Check entity is unchanged
        Assert.True(entity.HasComponent<EcsFloat>());
        Assert.True(entity.HasComponent<EcsInt16>());
        Assert.False(entity.HasComponent<EcsInt32>());
    }

    [Fact]
    public void RemoveAndSetComponent()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create an entity with 2 components
        var eb = buffer
            .Create()
            .Set(new EcsFloat(123))
            .Set(new EcsInt16(456));

        buffer.Execute();
        var entity = eb.Resolve();

        // Remove a component
        buffer.Remove<EcsInt16>(entity);

        // Then set the same component!
        buffer.Set(entity, new EcsInt16(789));

        buffer.Execute();

        // Check entity structure is unchanged
        Assert.True(entity.HasComponent<EcsFloat>());
        Assert.True(entity.HasComponent<EcsInt16>());
        Assert.False(entity.HasComponent<EcsInt32>());

        // Check value is correct
        Assert.Equal(789, entity.GetComponent<EcsInt16>().Value);
    }

    [Fact]
    public void CreateManyArchetypes()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create entities in lots of different archetypes. The idea is to
        // create so many the entity runs out of aggregation buffers.

        var buffered = new List<BufferedEntity>();
        var rng = new Random(17);
        for (var i = 0; i < 1024; i++)
        {
            var eb = buffer.Create();
            buffered.Add(eb);

            for (var j = 0; j < 4; j++)
            {
                switch (rng.Next(18))
                {
                    case 0: eb.Set(new Ecs0()); break;
                    case 1: eb.Set(new Ecs1()); break;
                    case 2: eb.Set(new Ecs2()); break;
                    case 3: eb.Set(new Ecs3()); break;
                    case 4: eb.Set(new Ecs4()); break;
                    case 5: eb.Set(new Ecs5()); break;
                    case 6: eb.Set(new Ecs6()); break;
                    case 7: eb.Set(new Ecs7()); break;
                    case 8: eb.Set(new Ecs8()); break;
                    case 9: eb.Set(new Ecs9()); break;
                    case 10: eb.Set(new Ecs10()); break;
                    case 11: eb.Set(new Ecs11()); break;
                    case 12: eb.Set(new Ecs12()); break;
                    case 13: eb.Set(new Ecs13()); break;
                    case 14: eb.Set(new Ecs14()); break;
                    case 15: eb.Set(new Ecs15()); break;
                    case 16: eb.Set(new Ecs16()); break;
                    case 17: eb.Set(new Ecs17()); break;
                }
            }
        }

        var resolver = buffer.Execute();
        Assert.Equal(1024, resolver.Count);

        // Ensure this is identical to the loop above!
        rng = new Random(17);
        for (var i = 0; i < buffered.Count; i++)
        {
            var entity = buffered[i].Resolve();

            for (var j = 0; j < 4; j++)
            {
                switch (rng.Next(18))
                {
                    case 0: Assert.True(entity.HasComponent<Ecs0>()); break;
                    case 1: Assert.True(entity.HasComponent<Ecs1>()); break;
                    case 2: Assert.True(entity.HasComponent<Ecs2>()); break;
                    case 3: Assert.True(entity.HasComponent<Ecs3>()); break;
                    case 4: Assert.True(entity.HasComponent<Ecs4>()); break;
                    case 5: Assert.True(entity.HasComponent<Ecs5>()); break;
                    case 6: Assert.True(entity.HasComponent<Ecs6>()); break;
                    case 7: Assert.True(entity.HasComponent<Ecs7>()); break;
                    case 8: Assert.True(entity.HasComponent<Ecs8>()); break;
                    case 9: Assert.True(entity.HasComponent<Ecs9>()); break;
                    case 10: Assert.True(entity.HasComponent<Ecs10>()); break;
                    case 11: Assert.True(entity.HasComponent<Ecs11>()); break;
                    case 12: Assert.True(entity.HasComponent<Ecs12>()); break;
                    case 13: Assert.True(entity.HasComponent<Ecs13>()); break;
                    case 14: Assert.True(entity.HasComponent<Ecs14>()); break;
                    case 15: Assert.True(entity.HasComponent<Ecs15>()); break;
                    case 16: Assert.True(entity.HasComponent<Ecs16>()); break;
                    case 17: Assert.True(entity.HasComponent<Ecs17>()); break;
                }
            }
        }
    }

    [Fact]
    public void StructuralChanges()
    {
        var world = new EcsWorld();
        var buffer = world.AcquireCommandBuffer();

        // Create some entities
        buffer.Create().Set(new Ecs0()).Set(new Ecs1()).Set(new Ecs2());
        buffer.Create().Set(new Ecs0()).Set(new Ecs1()).Set(new Ecs2());
        buffer.Create().Set(new Ecs0()).Set(new Ecs1()).Set(new Ecs2());
        buffer.Create().Set(new Ecs0()).Set(new Ecs1()).Set(new Ecs2());
        var resolver = buffer.Execute();
        var entity0 = resolver[0];
        var entity1 = resolver[1];
        var entity2 = resolver[2];
        var entity3 = resolver[3];

        // Add component to 0
        buffer.Set(entity0, new Ecs3());

        // Remove component from 1
        buffer.Remove<Ecs0>(entity1);

        // Add and remove 2
        buffer.Remove<Ecs0>(entity2);
        buffer.Set(entity2, new Ecs4());

        // Do nothing to 3

        // Apply all that
        buffer.Execute();

        // Check 1 has everything expected
        Assert.Equal(4, entity0.ComponentIds.Count);
        entity0.GetComponent<Ecs0>();
        entity0.GetComponent<Ecs1>();
        entity0.GetComponent<Ecs2>();
        entity0.GetComponent<Ecs3>();

        // Check 2 has everything expected
        Assert.Equal(2, entity1.ComponentIds.Count);
        entity1.GetComponent<Ecs1>();
        entity1.GetComponent<Ecs2>();

        // Check 3 has everything expected
        Assert.Equal(3, entity2.ComponentIds.Count);
        entity2.GetComponent<Ecs1>();
        entity2.GetComponent<Ecs2>();
        entity2.GetComponent<Ecs4>();

        // Check other is unchanged
        Assert.Equal(3, entity3.ComponentIds.Count);
        entity3.GetComponent<Ecs0>();
        entity3.GetComponent<Ecs1>();
        entity3.GetComponent<Ecs2>();
    }

    [Fact]
    public void ClearSet()
    {
        var world = new EcsWorld();
        var cmd = world.AcquireCommandBuffer();

        // Create an entity
        var eb = cmd.Create().Set(new Ecs0());

        cmd.Execute();
        var e = eb.Resolve();

        // Set value, then clear buffer
        cmd.Set(e, new Ecs1());
        cmd.Clear();
        cmd.Execute();

        Assert.True(e.HasComponent<Ecs0>());
        Assert.False(e.HasComponent<Ecs1>());
    }

    [Fact]
    public void ClearBufferedSet()
    {
        var world = new EcsWorld();
        var cmd = world.AcquireCommandBuffer();

        // Create an entity
        var eb = cmd.Create().Set(new Ecs0());
        cmd.Execute();
        _ = eb.Resolve();

        // Create another entity then clear
        cmd.Create().Set(new Ecs0());
        cmd.Clear();
        cmd.Execute();

        Assert.Equal(1, new QueryFilter().Include<Ecs0>().Build(world).Count());
    }

    [Fact]
    public void ResolveClearedEntity()
    {
        var world = new EcsWorld();
        var cmd = world.AcquireCommandBuffer();

        // Create an entity
        var eb = cmd.Create().Set(new Ecs0());
        cmd.Clear();
        cmd.Execute();

        Assert.Throws<GuardException>(() =>
        {
            eb.Resolve();
        });
    }


    [Fact]
    public void ClearBufferRemove()
    {
        var world = new EcsWorld();
        var cmd = world.AcquireCommandBuffer();

        // Create an entity
        var eb = cmd.Create().Set(new Ecs0()).Set(new Ecs1());
        cmd.Execute();
        var e = eb.Resolve();

        // Remove value, then clear buffer
        cmd.Remove<Ecs1>(e);
        cmd.Clear();
        cmd.Execute();

        Assert.True(e.HasComponent<Ecs0>());
        Assert.True(e.HasComponent<Ecs1>());
    }

    [Fact]
    public void ClearBufferDestroy()
    {
        var world = new EcsWorld();
        var cmd = world.AcquireCommandBuffer();

        // Create an entity
        var eb = cmd.Create().Set(new Ecs0()).Set(new Ecs1());
        cmd.Execute();
        var e = eb.Resolve();

        // Destroy entity, then clear buffer
        cmd.Destroy(e);
        cmd.Clear();
        cmd.Execute();

        Assert.True(e.IsAlive);
        Assert.True(e.HasComponent<Ecs0>());
        Assert.True(e.HasComponent<Ecs1>());
    }

    [Fact]
    public void ClearBufferDestroyArchetype()
    {
        var world = new EcsWorld();
        var cmd = world.AcquireCommandBuffer();

        // Create an entity
        var eb = cmd.Create().Set(new Ecs0()).Set(new Ecs1());
        cmd.Execute();
        var e = eb.Resolve();

        // Destroy archetypes, then clear buffer
        cmd.Destroy(new QueryFilter().Include<Ecs0>().Build(world));
        cmd.Clear();
        cmd.Execute();

        Assert.True(e.IsAlive);
        Assert.True(e.HasComponent<Ecs0>());
        Assert.True(e.HasComponent<Ecs1>());
    }
}
