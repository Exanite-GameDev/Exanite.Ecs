namespace Exanite.Myriad.Ecs.CommandBuffers;

public partial class EcsCommandBuffer
{
    // /// <summary>
    // /// Stores an entity's component set after structural changes.
    // /// <para/>
    // /// This is used by <see cref="ApplyStructuralChanges"/> to figure out which archetype to move an entity to when components are added/removed.
    // /// </summary>
    // /// <remarks>
    // /// Stores temporary data. Clear before use.
    // /// </remarks>
    // private readonly OrderedListSet<ComponentId> tempComponentsAfterMove = [];


    // /// <summary>
    // /// Apply all buffered operations to the <see cref="World"/>.
    // /// </summary>
    // public void Execute()
    // {
    //     EnsureIsExternallyMutable();
    //
    //     if (!HasBufferedOperations)
    //     {
    //         return;
    //     }
    //
    //     IsExecuting = true;
    //     {
    //         using var _ = World.AcquireCommandBuffer(out var recursiveCommandBuffer);
    //
    //         // Create buffered entities.
    //         CreateEntities(recursiveCommandBuffer);
    //
    //         // Destroy entities, this must occur before structural changes because it may trigger new structural changes
    //         // by adding a new phantom component.
    //         DestroyEntities(recursiveCommandBuffer);
    //
    //         // Structural changes (add/remove components)
    //         // This also includes setting components
    //         ApplyStructuralChanges(recursiveCommandBuffer);
    //
    //         // Release unused local IDs
    //         World.Entities.ReleaseUnusedIds(localIdPool);
    //
    //         // Clear all temporary state
    //         newEntities.Clear();
    //         setters.Clear(false);
    //         entityModifications.Clear();
    //
    //         HasBufferedOperations = false;
    //
    //         recursiveCommandBuffer.Execute();
    //     }
    //     IsExecuting = false;
    // }

    // private void CreateEntities(EcsCommandBuffer recursiveCommandBuffer)
    // {
    //     var archetype = World.GetOrCreateArchetype(ImmutableOrderedListSet<ComponentId>.Empty.AsComponentIdSet());
    //     foreach (var newEntity in newEntities)
    //     {
    //         archetype.CreateEntity(recursiveCommandBuffer, newEntity);
    //     }
    // }
    //
    // private void DestroyEntities(EcsCommandBuffer recursiveCommandBuffer)
    // {
    //     foreach (var archetype in archetypeDestroys)
    //     {
    //         DestroyArchetypeEntities(recursiveCommandBuffer, archetype);
    //     }
    //     archetypeDestroys.Clear();
    //
    //     foreach (var entity in destroys)
    //     {
    //         // Destroy entity
    //         DestroyEntity(recursiveCommandBuffer, entity);
    //
    //         // Return objects to pools
    //         if (entityModifications.Remove(entity, out var mod))
    //         {
    //             if (mod.Sets != null)
    //             {
    //                 mod.Sets.Clear();
    //                 SimplePool.Release(mod.Sets);
    //             }
    //
    //             if (mod.Removes != null)
    //             {
    //                 mod.Removes.Clear();
    //                 SimplePool.Release(mod.Removes);
    //             }
    //         }
    //     }
    //     destroys.Clear();
    // }
    //
    // private void ApplyStructuralChanges(EcsCommandBuffer recursiveCommandBuffer)
    // {
    //     if (entityModifications.Count > 0)
    //     {
    //         // Calculate the new archetype for the entity
    //         foreach (var (entity, modification) in entityModifications)
    //         {
    //             if (!entity.IsAlive)
    //             {
    //                 continue;
    //             }
    //
    //             var archetypeBeforeMove = World.Entities.GetArchetype(entity.EntityId);
    //             var componentsBeforeMove = archetypeBeforeMove.Components;
    //
    //             // Initialize componentsAfterMove with componentsBeforeMove
    //             var componentsAfterMove = tempComponentsAfterMove;
    //             componentsAfterMove.Clear();
    //             componentsAfterMove.UnionWith(componentsBeforeMove);
    //
    //             // Check if a move is required
    //             var moveRequired = false;
    //             var hash = archetypeBeforeMove.Hash;
    //             {
    //                 // Component adds/sets
    //                 if (modification.Sets != null)
    //                 {
    //                     foreach (var id in modification.Sets.Keys)
    //                     {
    //                         if (componentsAfterMove.Add(id))
    //                         {
    //                             hash = hash.Toggle(id);
    //                             moveRequired = true;
    //                         }
    //                     }
    //                 }
    //
    //                 // Component removes
    //                 if (modification.Removes != null)
    //                 {
    //                     foreach (var remove in modification.Removes)
    //                     {
    //                         if (componentsAfterMove.Remove(remove))
    //                         {
    //                             hash = hash.Toggle(remove);
    //                             moveRequired = true;
    //                         }
    //                     }
    //
    //                     // Recycle remove set
    //                     modification.Removes.Clear();
    //                     SimplePool.Release(modification.Removes);
    //                 }
    //             }
    //
    //             // Get the location for the entity, moving it to a new archetype first if necessary
    //             EntityLocation location;
    //             if (moveRequired)
    //             {
    //                 // Raise component removed events
    //                 foreach (var componentId in componentsBeforeMove)
    //                 {
    //                     if (!componentsAfterMove.Contains(componentId))
    //                     {
    //                         archetypeBeforeMove.Lookup.ComponentEventDispatcherByComponentId[componentId.Value].OnComponentRemoved(recursiveCommandBuffer, entity);
    //                     }
    //                 }
    //
    //                 // Get the new archetype we're moving to
    //                 var dstArchetype = World.GetOrCreateArchetype(componentsAfterMove.AsComponentIdSet(), hash);
    //
    //                 // Migrate the entity across
    //                 location = World.MigrateEntity(entity.EntityId, dstArchetype);
    //             }
    //             else
    //             {
    //                 location = World.Entities.GetLocation(entity.EntityId);
    //             }
    //
    //             if (modification.Sets != null)
    //             {
    //                 // Run all setters
    //                 foreach (var setter in modification.Sets.Values)
    //                 {
    //                     setters.Write(setter, location);
    //                 }
    //
    //                 // Raise component added/modified events
    //                 foreach (var setter in modification.Sets.Values)
    //                 {
    //                     var eventDispatcher = location.Chunk.Lookup.ComponentEventDispatcherByComponentId[setter.ComponentId.Value];
    //                     if (componentsBeforeMove.Contains(setter.ComponentId))
    //                     {
    //                         eventDispatcher.OnComponentModified(recursiveCommandBuffer, entity);
    //                     }
    //                     else
    //                     {
    //                         eventDispatcher.OnComponentAdded(recursiveCommandBuffer, entity);
    //                     }
    //                 }
    //             }
    //
    //             // Recycle setters
    //             if (modification.Sets != null)
    //             {
    //                 modification.Sets.Clear();
    //                 SimplePool.Release(modification.Sets);
    //             }
    //         }
    //     }
    // }
    //
    // private void DestroyEntity(EcsCommandBuffer recursiveCommandBuffer, Entity entity)
    // {
    //     // Ignore destroyed entities
    //     if (!entity.IsAlive)
    //     {
    //         return;
    //     }
    //
    //     // Get the location for this entity
    //     ref var location = ref World.Entities.GetLocation(entity.EntityId);
    //
    //     // Check this is still a valid entity reference. Early exit if the entity
    //     // is already dead.
    //     if (location.Version != entity.Version)
    //     {
    //         return;
    //     }
    //
    //     // Raise component removed events
    //     foreach (var componentId in entity.ComponentIds)
    //     {
    //         var eventDispatcher = location.Chunk.Lookup.ComponentEventDispatcherByComponentId[componentId.Value];
    //         eventDispatcher.OnComponentRemoved(recursiveCommandBuffer, entity);
    //     }
    //
    //     // Raise entity destroyed event
    //     World.EventBus.Raise(new EntityDestroyedEvent(recursiveCommandBuffer, entity));
    //
    //     // Notify archetype this entity is dead
    //     location.Chunk.Archetype.RemoveEntity(location);
    //
    //     // Release ID
    //     World.Entities.ReleaseId(entity.EntityId);
    // }
    //
    // private void DestroyArchetypeEntities(EcsCommandBuffer recursiveCommandBuffer, Archetype archetype)
    // {
    //     if (archetype.EntityCount == 0)
    //     {
    //         return;
    //     }
    //
    //     // Mark entities as dead and send events
    //     foreach (var chunk in archetype.Chunks)
    //     {
    //         foreach (var entity in chunk.Entities)
    //         {
    //             // Raise component removed events
    //             foreach (var componentId in entity.ComponentIds)
    //             {
    //                 var eventDispatcher = archetype.Lookup.ComponentEventDispatcherByComponentId[componentId.Value];
    //                 eventDispatcher.OnComponentRemoved(recursiveCommandBuffer, entity);
    //             }
    //
    //             // Raise entity destroyed event
    //             World.EventBus.Raise(new EntityDestroyedEvent(recursiveCommandBuffer, entity));
    //
    //             // Release ID
    //             World.Entities.ReleaseId(entity.EntityId);
    //         }
    //     }
    //
    //     // Clear the archetype
    //     archetype.Clear();
    // }

}
