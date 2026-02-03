using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Exanite.Core.Pooling;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Events;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.CommandBuffers;

public partial class EcsCommandBuffer
{
    /// <remarks>
    /// Stores temporary data. Clear before use.
    /// </remarks>
    private readonly OrderedListSet<ComponentId> tempComponentSet = [];

    private struct EntityState
    {
        public bool IsCreated;
        public Dictionary<ComponentId, SetterId>? Sets;
        public OrderedListSet<ComponentId>? Removes;

        public Dictionary<ComponentId, SetterId> GetOrAcquireSets()
        {
            if (Sets == null)
            {
                Sets = SimplePool<Dictionary<ComponentId, SetterId>>.Acquire();
                Sets.Clear();
            }

            return Sets;
        }

        public OrderedListSet<ComponentId> GetOrAcquireRemoves()
        {
            if (Removes == null)
            {
                Removes = SimplePool<OrderedListSet<ComponentId>>.Acquire();
                Removes.Clear();
            }

            return Removes;
        }

        public void Release()
        {
            if (Sets != null)
            {
                SimplePool<Dictionary<ComponentId, SetterId>>.Release(Sets);
            }

            if (Removes != null)
            {
                SimplePool<OrderedListSet<ComponentId>>.Release(Removes);
            }
        }
    }

    private void ExecuteInternal()
    {
        using var _ = World.AcquireCommandBuffer(out var recursiveCommandBuffer);
        using (ListPool<EntityId>.Acquire(out var entitiesToDestroy))
        using (ListPool<Archetype>.Acquire(out var archetypesToDestroy))
        using (DictionaryPool<EntityId, EntityState>.Acquire(out var entityStates))
        using (ListPool<Action>.Acquire(out var actions))
        {
            // Preprocess
            foreach (var command in state.Commands)
            {
                switch (command.Type)
                {
                    case CommandType.CreateEntity:
                    {
                        var typedCommand = state.CreateEntityCommands[command.Index];
                        ref var entityState = ref GetEntityState(typedCommand.EntityId);
                        entityState.IsCreated = true;

                        break;
                    }
                    case CommandType.DestroyEntity:
                    {
                        var typedCommand = state.DestroyEntityCommands[command.Index];
                        ref var entityState = ref GetEntityState(typedCommand.EntityId);
                        entityState.IsCreated = false;
                        entitiesToDestroy.Add(typedCommand.EntityId);

                        break;
                    }
                    case CommandType.DestroyArchetype:
                    {
                        var typedCommand = state.DestroyArchetypeCommands[command.Index];
                        archetypesToDestroy.Add(typedCommand.Archetype);

                        break;
                    }
                    case CommandType.SetComponent:
                    {
                        var typedCommand = state.SetComponentCommands[command.Index];
                        ref var entityState = ref GetEntityState(typedCommand.EntityId);

                        var sets = entityState.GetOrAcquireSets();
                        sets[typedCommand.SetterId.ComponentId] = typedCommand.SetterId;

                        // Prevent the remove, if it exists
                        entityState.Removes?.Remove(typedCommand.SetterId.ComponentId);

                        break;
                    }
                    case CommandType.RemoveComponent:
                    {
                        var typedCommand = state.RemoveComponentCommands[command.Index];
                        ref var entityState = ref GetEntityState(typedCommand.EntityId);

                        var removes = entityState.GetOrAcquireRemoves();
                        removes.Add(typedCommand.ComponentId);

                        // Prevent the set, if it exists
                        entityState.Sets?.Remove(typedCommand.ComponentId);

                        break;
                    }
                    case CommandType.DeferredAction:
                    {
                        var typedCommand = state.DeferredActionCommands[command.Index];
                        actions.Add(typedCommand.Action);

                        break;
                    }
                    default: throw ExceptionUtility.NotSupportedEnumValue(command.Type);
                }
            }

            // Execute
            {
                DestroyEntities(recursiveCommandBuffer, archetypesToDestroy, entitiesToDestroy);

                CreateAndApplyStructuralChanges(recursiveCommandBuffer, entityStates);

                foreach (var action in actions)
                {
                    action.Invoke();
                }
            }

            // TODO: Add try-finally for cleanup
            // Release unused local IDs
            World.Entities.ReleaseUnusedIds(localIdPool);

            // Clear commands
            state.Clear(World);

            // Release entity state collections
            foreach (var entityState in entityStates.Values)
            {
                entityState.Release();
            }

            ref EntityState GetEntityState(EntityId entityId)
            {
                return ref CollectionsMarshal.GetValueRefOrAddDefault(entityStates, entityId, out var _);
            }
        }

        // Run any newly enqueued commands
        recursiveCommandBuffer.Execute();
    }

    private void DestroyEntities(EcsCommandBuffer recursiveCommandBuffer, List<Archetype> archetypes, List<EntityId> entities)
    {
        foreach (var archetype in archetypes)
        {
            DestroyArchetypeEntities(recursiveCommandBuffer, archetype);
        }

        foreach (var entity in entities)
        {
            DestroyEntity(recursiveCommandBuffer, entity);
        }
    }

    private void DestroyArchetypeEntities(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype)
    {
        if (archetype.EntityCount == 0)
        {
            return;
        }

        // Mark entities as dead and send events
        foreach (var chunk in archetype.Chunks)
        {
            foreach (var entity in chunk.Entities)
            {
                // Raise component removed events
                foreach (var componentId in entity.ComponentIds)
                {
                    var eventDispatcher = archetype.Lookup.ComponentEventDispatcherByComponentId[componentId.Value];
                    eventDispatcher.OnComponentRemoved(recursiveCommandBuffer, entity);
                }

                // Raise entity destroyed event
                World.EventBus.Raise(new EntityDestroyedEvent(recursiveCommandBuffer, entity));

                // Release ID
                World.Entities.ReleaseId(entity.EntityId);
            }
        }

        // Clear the archetype
        archetype.Clear();
    }

    private void DestroyEntity(EcsCommandBuffer recursiveCommandBuffer, EntityId entityId)
    {
        ref var location = ref World.Entities.GetLocation(entityId.Index);
        if (location.Version != entityId.Version)
        {
            // Ignore destroyed entities
            return;
        }

        // Raise component removed events
        var entity = entityId.ToEntity(World);
        var archetype = location.Chunk.Archetype;
        foreach (var componentId in archetype.Components)
        {
            var eventDispatcher = archetype.Lookup.ComponentEventDispatcherByComponentId[componentId.Value];
            eventDispatcher.OnComponentRemoved(recursiveCommandBuffer, entity);
        }

        // Raise entity destroyed event
        World.EventBus.Raise(new EntityDestroyedEvent(recursiveCommandBuffer, entity));

        // Notify archetype this entity is dead
        location.Chunk.Archetype.RemoveEntity(location);

        // Release ID
        World.Entities.ReleaseId(entityId);
    }

    private void CreateAndApplyStructuralChanges(EcsCommandBuffer recursiveCommandBuffer, Dictionary<EntityId, EntityState> entityStates)
    {
        foreach (var (entityId, entityState) in entityStates)
        {
            ref var location = ref World.Entities.GetLocation(entityId.Index);
            if (location.Version != entityId.Version)
            {
                // Ignore destroyed entities
                continue;
            }

            var archetypeHash = new ArchetypeHash();
            var componentIdSet = tempComponentSet;
            componentIdSet.Clear();

            if (!entityState.IsCreated)
            {
                // Add existing components to set
                var archetype = location.Chunk.Archetype;

                archetypeHash = archetype.Hash;
                componentIdSet.UnionWith(archetype.Components);
            }

            // Consider component changes
            var setChanged = false;
            {
                // Component sets
                if (entityState.Sets != null)
                {
                    foreach (var componentId in entityState.Sets.Keys)
                    {
                        if (componentIdSet.Add(componentId))
                        {
                            archetypeHash = archetypeHash.Toggle(componentId);
                            setChanged = true;
                        }
                    }
                }

                // Component removes
                if (entityState.Removes != null)
                {
                    foreach (var componentId in entityState.Removes)
                    {
                        if (componentIdSet.Remove(componentId))
                        {
                            archetypeHash = archetypeHash.Toggle(componentId);
                            setChanged = true;
                        }
                    }
                }
            }

            // Case 1: Entity created
            var entity = entityId.ToEntity(World);
            if (entityState.IsCreated)
            {
                // Create the entity
                var dstArchetype = World.GetOrCreateArchetype(componentIdSet.AsComponentIdSet(), archetypeHash);
                dstArchetype.AddEntity(entityId, ref location);

                // Write component values
                if (entityState.Sets != null)
                {
                    foreach (var setter in entityState.Sets.Values)
                    {
                        state.Setters.Write(setter, location);
                    }
                }

                // Raise component added events
                if (entityState.Sets != null)
                {
                    foreach (var componentId in entityState.Sets.Keys)
                    {
                        var eventDispatcher = dstArchetype.Lookup.ComponentEventDispatcherByComponentId[componentId.Value];
                        eventDispatcher.OnComponentAdded(recursiveCommandBuffer, entity);
                    }
                }

                // Raise entity created event
                World.EventBus.Raise(new EntityCreatedEvent(recursiveCommandBuffer, entity));

                continue;
            }

            // Case 2: Entity moved
            if (setChanged)
            {
                var srcArchetype = location.Chunk.Archetype;
                var dstArchetype = World.GetOrCreateArchetype(componentIdSet.AsComponentIdSet(), archetypeHash);

                // Raise component removed events
                if (entityState.Removes != null)
                {
                    foreach (var componentId in entityState.Removes)
                    {
                        var eventDispatcher = srcArchetype.Lookup.ComponentEventDispatcherByComponentId[componentId.Value];
                        eventDispatcher.OnComponentRemoved(recursiveCommandBuffer, entity);
                    }
                }

                // Migrate the entity
                srcArchetype.MigrateEntity(entityId, dstArchetype, ref location);

                // Write component values
                WriteComponentValues(entityState, location);

                // Raise component added/modified events
                if (entityState.Sets != null)
                {
                    foreach (var componentId in entityState.Sets.Keys)
                    {
                        var eventDispatcher = dstArchetype.Lookup.ComponentEventDispatcherByComponentId[componentId.Value];
                        if (srcArchetype.Components.Contains(componentId))
                        {
                            eventDispatcher.OnComponentModified(recursiveCommandBuffer, entity);
                        }
                        else
                        {
                            eventDispatcher.OnComponentAdded(recursiveCommandBuffer, entity);
                        }
                    }
                }

                continue;
            }

            // Case 3: Entity unmoved
            {
                // Write component values
                WriteComponentValues(entityState, location);

                // Raise component modified events
                if (entityState.Sets != null)
                {
                    var dstArchetype = location.Chunk.Archetype;
                    foreach (var componentId in entityState.Sets.Keys)
                    {
                        var eventDispatcher = dstArchetype.Lookup.ComponentEventDispatcherByComponentId[componentId.Value];
                        eventDispatcher.OnComponentModified(recursiveCommandBuffer, entity);
                    }
                }
            }
        }
    }

    private void WriteComponentValues(EntityState entityState, EntityLocation location)
    {
        if (entityState.Sets != null)
        {
            foreach (var setter in entityState.Sets.Values)
            {
                state.Setters.Write(setter, location);
            }
        }
    }
}
