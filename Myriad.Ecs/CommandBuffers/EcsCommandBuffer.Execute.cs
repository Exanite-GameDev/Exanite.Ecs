using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Exanite.Core.Pooling;
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

    private void ExecuteInternal()
    {
        using var _ = World.AcquireCommandBuffer(out var recursiveCommandBuffer);
        try
        {
            DestroyEntities(recursiveCommandBuffer, state.ArchetypesToDestroy, state.EntitiesToDestroy);
            CreateAndApplyStructuralChanges(recursiveCommandBuffer, state.EntityStates);

            foreach (var action in state.Actions)
            {
                action.Invoke();
            }
        }
        finally
        {
            // Release unused local IDs
            World.Entities.ReleaseUnusedIds(localIdPool.AsSpan()[nextLocalIdPoolIndex..]);
            nextLocalIdPoolIndex = localIdPool.Length;

            // Clear commands
            state.Clear(World, true);

            HasBufferedOperations = false;
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
        if (archetype.Entities.Length == 0)
        {
            return;
        }

        // Mark entities as dead and send events
        foreach (var componentId in archetype.Components)
        {
            var dispatcher = archetype.Info.ComponentDispatcherByComponentId[componentId.Value];
            dispatcher.OnComponentRemoved(recursiveCommandBuffer, archetype, 0, archetype.Entities.Length);
        }

        // Raise entity destroyed events
        foreach (var entity in archetype.Entities)
        {
            World.EventBus.Raise(new EntityDestroyedEvent(recursiveCommandBuffer, entity));
        }

        // Release IDs
        World.Entities.ReleaseIds(archetype.Storage.EntityColumn.AsSpan(0, archetype.EntityCount));

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
        var archetype = location.Archetype;
        foreach (var componentId in archetype.Components)
        {
            var dispatcher = archetype.Info.ComponentDispatcherByComponentId[componentId.Value];
            dispatcher.OnComponentRemoved(recursiveCommandBuffer, entity);
        }

        // Raise entity destroyed event
        World.EventBus.Raise(new EntityDestroyedEvent(recursiveCommandBuffer, entity));

        // Notify archetype this entity is dead
        location.Archetype.RemoveEntity(location);

        // Release ID
        World.Entities.ReleaseId(entityId);
    }

    private void CreateAndApplyStructuralChanges(EcsCommandBuffer recursiveCommandBuffer, Dictionary<EntityId, EntityState> entityStates)
    {
        // Sort entities by target archetype
        // TODO: Take advantage of the fact that the entity ids are likely already partially sorted by archetype ID due to the way that queries iterate
        var creates = DictionaryPool<Archetype, List<(EntityId EntityId, Dictionary<ComponentId, SetterId>? Sets)>>.Acquire();
        var moves = DictionaryPool<Archetype, List<EntityId>>.Acquire();
        var unmoved = ListPool<EntityId>.Acquire();
        try
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

                if (!entityState.NeedsCreation)
                {
                    // Add existing components to set
                    var archetype = location.Archetype;

                    archetypeHash = archetype.Info.Hash;
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
                if (entityState.NeedsCreation)
                {
                    var dstArchetype = World.GetOrCreateArchetype(componentIdSet.AsComponentIdSet(), archetypeHash);
                    AddToDictionary(creates, dstArchetype, (entityId, entityState.Sets));
                    continue;
                }

                // Case 2: Entity moved
                if (setChanged)
                {
                    var dstArchetype = World.GetOrCreateArchetype(componentIdSet.AsComponentIdSet(), archetypeHash);
                    AddToDictionary(moves, dstArchetype, entityId);
                    continue;
                }

                // Case 3: Entity unmoved
                unmoved.Add(entityId);
            }

            // Handle creates
            foreach (var (dstArchetype, entityCreates) in creates)
            {
                dstArchetype.EnsureCapacity(dstArchetype.EntityCount + entityCreates.Count);
                foreach (var entityCreate in entityCreates)
                {
                    // Create the entity
                    ref var location = ref World.Entities.GetLocation(entityCreate.EntityId.Index);
                    dstArchetype.AddEntity(entityCreate.EntityId, ref location);

                    // TODO: This is inefficient. Rework how writes are done
                    // Write component values
                    WriteComponentValues(entityCreate.Sets, location);

                    // Raise component copied events
                    if (entityCreate.Sets != null)
                    {
                        foreach (var (componentId, setterId) in entityCreate.Sets)
                        {
                            // Did not already have the component, so we raise copied if needed, then added
                            if (setterId.IsPrefab)
                            {
                                var dispatcher = dstArchetype.Info.ComponentDispatcherByComponentId[componentId.Value];
                                state.Lookup.SetContext(entityCreate.EntityId, setterId.PrefabGroupKey);
                                dispatcher.OnComponentCopied(recursiveCommandBuffer, entityCreate.EntityId.ToEntity(World), state.Lookup);
                            }
                        }
                    }
                }

                // Raise component added events
                foreach (var componentId in dstArchetype.Components)
                {
                    var dispatcher = dstArchetype.Info.ComponentDispatcherByComponentId[componentId.Value];
                    dispatcher.OnComponentAdded(recursiveCommandBuffer, dstArchetype, dstArchetype.EntityCount - entityCreates.Count, entityCreates.Count);
                }

                // Raise entity created events
                foreach (var entityCreate in entityCreates)
                {
                    World.EventBus.Raise(new EntityCreatedEvent(recursiveCommandBuffer, entityCreate.EntityId.ToEntity(World)));
                }
            }

            // Handle moves
            {
                   // // Raise component removed events
                    // if (entityState.Removes != null)
                    // {
                    //     foreach (var componentId in entityState.Removes)
                    //     {
                    //         if (srcArchetype.Components.Contains(componentId))
                    //         {
                    //             var dispatcher = srcArchetype.Info.ComponentDispatcherByComponentId[componentId.Value];
                    //             dispatcher.OnComponentRemoved(recursiveCommandBuffer, entity);
                    //         }
                    //     }
                    // }
                    //
                    // // Migrate the entity
                    // srcArchetype.MigrateEntity(entityId, dstArchetype, ref location);
                    //
                    // // Write component values
                    // WriteComponentValues(entityState, location);
                    //
                    // // Raise component copied/added/modified events
                    // if (entityState.Sets != null)
                    // {
                    //     foreach (var (componentId, setterId) in entityState.Sets)
                    //     {
                    //         var dispatcher = dstArchetype.Info.ComponentDispatcherByComponentId[componentId.Value];
                    //         if (srcArchetype.Components.Contains(componentId))
                    //         {
                    //             // Already had the component, so we raise copied if needed, then modified
                    //             if (setterId.IsPrefab)
                    //             {
                    //                 state.Lookup.SetContext(entityId, setterId.PrefabGroupKey);
                    //                 dispatcher.OnComponentCopied(recursiveCommandBuffer, entity, state.Lookup);
                    //             }
                    //
                    //             dispatcher.OnComponentModified(recursiveCommandBuffer, entity);
                    //         }
                    //         else
                    //         {
                    //             // Did not already have the component, so we raise copied if needed, then added
                    //             if (setterId.IsPrefab)
                    //             {
                    //                 state.Lookup.SetContext(entityId, setterId.PrefabGroupKey);
                    //                 dispatcher.OnComponentCopied(recursiveCommandBuffer, entity, state.Lookup);
                    //             }
                    //
                    //             dispatcher.OnComponentAdded(recursiveCommandBuffer, entity);
                    //         }
                    //     }
                    // }
            }

            // Handle unmoved
            {
                // {
                //
                //     // Write component values
                //     WriteComponentValues(entityState, location);
                //
                //     // Raise component copied/modified events
                //     if (entityState.Sets != null)
                //     {
                //         var dstArchetype = location.Archetype;
                //         foreach (var (componentId, setterId) in entityState.Sets)
                //         {
                //             // Already had the component, so we raise copied if needed, then modified
                //             var dispatcher = dstArchetype.Info.ComponentDispatcherByComponentId[componentId.Value];
                //             if (setterId.IsPrefab)
                //             {
                //                 state.Lookup.SetContext(entityId, setterId.PrefabGroupKey);
                //                 dispatcher.OnComponentCopied(recursiveCommandBuffer, entity, state.Lookup);
                //             }
                //
                //             dispatcher.OnComponentModified(recursiveCommandBuffer, entity);
                //         }
                //     }
                // }
            }
        }
        finally
        {
            // Release pooled collections
            DictionaryPool<Archetype, List<(EntityId EntityId, Dictionary<ComponentId, SetterId>? Sets)>>.Release(creates);
            DictionaryPool<Archetype, List<EntityId>>.Release(moves);
            ListPool<EntityId>.Release(unmoved);
        }

        return;

        void AddToDictionary<T>(Dictionary<Archetype, List<T>> collection, Archetype dstArchetype, T value)
        {
            ref var entityIds = ref CollectionsMarshal.GetValueRefOrAddDefault(collection, dstArchetype, out _);
            entityIds ??= ListPool<T>.Acquire();
            entityIds.Add(value);
        }
    }

    private void WriteComponentValues(Dictionary<ComponentId, SetterId>? sets, EntityLocation location)
    {
        if (sets != null)
        {
            foreach (var setter in sets.Values)
            {
                state.Setters.Write(setter, location);
            }
        }
    }
}
