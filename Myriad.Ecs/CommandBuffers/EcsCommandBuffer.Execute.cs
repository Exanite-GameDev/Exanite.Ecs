using System;
using System.Collections.Generic;
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
    private readonly Comparison<EntityModification> sortModifications;

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
        // Use a flat list to gather and sort modifications into batches
        using var _ = ListPool<EntityModification>.Acquire(out var modifications);
        // Gather modifications and calculate src/dst archetypes
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
                modifications.Add(new EntityModification(entityId, 0, dstArchetype.Id, entityState.Sets));
                continue;
            }

            // Case 2: Entity moved
            if (setChanged)
            {
                var dstArchetype = World.GetOrCreateArchetype(componentIdSet.AsComponentIdSet(), archetypeHash);
                modifications.Add(new EntityModification(entityId, location.Archetype.Id, dstArchetype.Id, entityState.Sets));
                continue;
            }

            // Case 3: Entity unmoved
            {
                var archetypeId = location.Archetype.Id;
                modifications.Add(new EntityModification(entityId, archetypeId, archetypeId, entityState.Sets));
            }
        }

        if (modifications.Count == 0)
        {
            return;
        }

        // Sort into batches
        modifications.Sort(sortModifications);

        // Split into batches
        var leftIndex = 0;
        var srcArchetypeId = modifications[0].SrcArchetypeId;
        var dstArchetypeId = modifications[0].DstArchetypeId;
        for (var rightIndex = 1; rightIndex <= modifications.Count; rightIndex++)
        {
            if (rightIndex == modifications.Count
                || srcArchetypeId != modifications[rightIndex].SrcArchetypeId
                || dstArchetypeId != modifications[rightIndex].DstArchetypeId)
            {
                // New batch detected
                var batch = modifications.AsSpan().Slice(leftIndex, rightIndex - leftIndex);
                if (srcArchetypeId == 0)
                {
                    // Create
                    var dstArchetype = World.Archetypes[dstArchetypeId - 1];

                    dstArchetype.EnsureCapacity(dstArchetype.EntityCount + batch.Length);
                    foreach (var modification in batch)
                    {
                        // Create the entity
                        ref var location = ref World.Entities.GetLocation(modification.EntityId.Index);
                        dstArchetype.AddEntity(modification.EntityId, ref location);

                        // TODO: This is inefficient due to repeated dst component column lookup. Rework how writes are done
                        // Write component values
                        WriteComponentValues(modification.Sets, location);

                        // Raise component copied events
                        if (modification.Sets != null)
                        {
                            foreach (var (componentId, setterId) in modification.Sets)
                            {
                                // Did not already have the component, so we raise copied if needed, then added
                                if (setterId.IsPrefab)
                                {
                                    var dispatcher = dstArchetype.Info.ComponentDispatcherByComponentId[componentId.Value];
                                    state.Lookup.SetContext(modification.EntityId, setterId.PrefabGroupKey);
                                    dispatcher.OnComponentCopied(recursiveCommandBuffer, modification.EntityId.ToEntity(World), state.Lookup);
                                }
                            }
                        }
                    }

                    // Raise component added events
                    foreach (var componentId in dstArchetype.Components)
                    {
                        var dispatcher = dstArchetype.Info.ComponentDispatcherByComponentId[componentId.Value];
                        dispatcher.OnComponentAdded(recursiveCommandBuffer, dstArchetype, dstArchetype.EntityCount - batch.Length, batch.Length);
                    }

                    // Raise entity created events
                    foreach (var modification in batch)
                    {
                        World.EventBus.Raise(new EntityCreatedEvent(recursiveCommandBuffer, modification.EntityId.ToEntity(World)));
                    }
                }
                else if (srcArchetypeId != dstArchetypeId)
                {
                    // Move
                    var dstArchetype = World.Archetypes[dstArchetypeId - 1];
                    var srcArchetype = World.Archetypes[srcArchetypeId - 1];

                    // Raise component removed events
                    // Removes vary by source/destination archetype pair
                    {
                        var srcComponents = srcArchetype.Components;
                        var dstComponents = dstArchetype.Components;

                        var srcIndex = 0;
                        var dstIndex = 0;
                        while (srcIndex < srcComponents.Count)
                        {
                            var srcComponentId = srcComponents[srcIndex];
                            if (dstIndex >= dstComponents.Count || srcComponentId.Value < dstComponents[dstIndex].Value)
                            {
                                // Component was removed
                                var dispatcher = srcArchetype.Info.ComponentDispatcherByComponentId[srcComponentId.Value];
                                foreach (var modification in batch)
                                {
                                    // Raise the event per entity since we can't guarantee the source entities are contiguous
                                    dispatcher.OnComponentRemoved(recursiveCommandBuffer, modification.EntityId.ToEntity(World));
                                }

                                srcIndex++;
                            }
                            else if (srcComponentId.Value == dstComponents[dstIndex].Value)
                            {
                                // Component exists in both
                                srcIndex++;
                                dstIndex++;
                            }
                            else
                            {
                                // Component was added
                                dstIndex++;
                            }
                        }
                    }

                    // Migrate the entities
                    dstArchetype.EnsureCapacity(dstArchetype.EntityCount + batch.Length);
                    foreach (var modification in batch)
                    {
                        // Migrate the entity
                        ref var location = ref World.Entities.GetLocation(modification.EntityId.Index);
                        srcArchetype.MigrateEntity(modification.EntityId, dstArchetype, ref location);
                    }

                    // Write component values
                    foreach (var modification in batch)
                    {
                        // TODO: This is inefficient due to repeated dst component column lookup. Rework how writes are done
                        ref var location = ref World.Entities.GetLocation(modification.EntityId.Index);
                        WriteComponentValues(modification.Sets, location);
                    }

                    // Raise component copied events
                    foreach (var modification in batch)
                    {
                        if (modification.Sets != null)
                        {
                            foreach (var (componentId, setterId) in modification.Sets)
                            {
                                // Did not already have the component, so we raise copied if needed, then added
                                if (setterId.IsPrefab)
                                {
                                    var dispatcher = dstArchetype.Info.ComponentDispatcherByComponentId[componentId.Value];
                                    state.Lookup.SetContext(modification.EntityId, setterId.PrefabGroupKey);
                                    dispatcher.OnComponentCopied(recursiveCommandBuffer, modification.EntityId.ToEntity(World), state.Lookup);
                                }
                            }
                        }
                    }

                    // Raise component added events
                    // Adds vary by source/destination archetype pair
                    {
                        var srcComponents = srcArchetype.Components;
                        var dstComponents = dstArchetype.Components;

                        var srcIndex = 0;
                        var dstIndex = 0;
                        while (dstIndex < dstComponents.Count)
                        {
                            var dstComponentId = dstComponents[dstIndex];
                            if (srcIndex >= srcComponents.Count || dstComponentId.Value < srcComponents[srcIndex].Value)
                            {
                                // Component was added
                                var dispatcher = dstArchetype.Info.ComponentDispatcherByComponentId[dstComponentId.Value];
                                dispatcher.OnComponentAdded(recursiveCommandBuffer, dstArchetype, dstArchetype.EntityCount - batch.Length, batch.Length);

                                dstIndex++;
                            }
                            else if (dstComponentId.Value == srcComponents[srcIndex].Value)
                            {
                                // Component exists in both
                                srcIndex++;
                                dstIndex++;
                            }
                            else
                            {
                                // Component was removed
                                srcIndex++;
                            }
                        }
                    }

                    // Raise component modified events
                    // Modifications vary per entity
                    //
                    // This is handled separately from the above loop
                    // because the number of sets is usually much lower than the number of matched components
                    foreach (var modification in batch)
                    {
                        if (modification.Sets != null)
                        {
                            foreach (var componentId in modification.Sets.Keys)
                            {
                                if (srcArchetype.Components.Contains(componentId))
                                {
                                    var dispatcher = dstArchetype.Info.ComponentDispatcherByComponentId[componentId.Value];
                                    dispatcher.OnComponentModified(recursiveCommandBuffer, modification.EntityId.ToEntity(World));
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Unmoved
                    var dstArchetype = World.Archetypes[dstArchetypeId - 1];

                    foreach (var modification in batch)
                    {
                        // Write component values
                        ref var location = ref World.Entities.GetLocation(modification.EntityId.Index);
                        WriteComponentValues(modification.Sets, location);

                        // Raise component copied/modified events
                        if (modification.Sets != null)
                        {
                            var entity = modification.EntityId.ToEntity(World);
                            foreach (var (componentId, setterId) in modification.Sets)
                            {
                                // Already had the component, so we raise copied if needed, then modified
                                var dispatcher = dstArchetype.Info.ComponentDispatcherByComponentId[componentId.Value];
                                if (setterId.IsPrefab)
                                {
                                    state.Lookup.SetContext(modification.EntityId, setterId.PrefabGroupKey);
                                    dispatcher.OnComponentCopied(recursiveCommandBuffer, entity, state.Lookup);
                                }

                                dispatcher.OnComponentModified(recursiveCommandBuffer, entity);
                            }
                        }
                    }
                }

                // Update batch information
                if (rightIndex != modifications.Count)
                {
                    leftIndex = rightIndex;
                    srcArchetypeId = modifications[rightIndex].SrcArchetypeId;
                    dstArchetypeId = modifications[rightIndex].DstArchetypeId;
                }
            }
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
