using System.Collections.Generic;
using Exanite.Core.Pooling;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Events;
using Exanite.Myriad.Ecs.Worlds;
using Exanite.Myriad.Ecs.Worlds.Archetypes;

namespace Exanite.Myriad.Ecs.CommandBuffers;

/// <summary>
/// Buffers up modifications to entities and replays them all at once.
/// Events for an entity are reported once all modifications to the entity are completed.
/// <para/>
/// Warning: The command buffer is allowed to reorder and merge operations together.
/// <br/>
/// Example 1: Setting the same component twice is the same as setting it once, with the last taking priority.
/// <br/>
/// Example 2: Destroying an entity after making modifications to it is the same as making no modifications to it before destroying it.
/// </summary>
/// <remarks>
/// The command buffer batches commands to avoid expensive operations
/// where adding/removing multiple components causes the entity to be
/// copied between multiple archetypes.
/// <para/>
/// Fully ordered events without dropping of intermediate were considered,
/// but the performance cost is likely too high.
/// <para/>
/// If fully ordered events are required, it's possible to submit commands
/// in smaller batches. This has similar cost to replaying commands without
/// merging.
/// </remarks>
public sealed class EcsCommandBuffer
{
    /// <summary>
    /// The <see cref="World"/> this <see cref="EcsCommandBuffer"/> is modifying.
    /// </summary>
    public EcsWorld World { get; }

    public bool HasBufferedOperations { get; private set; }
    public bool IsExecuting { get; private set; }

    /// <summary>
    /// New entities to be created.
    /// </summary>
    private readonly List<Entity> newEntities = [];

    /// <summary>
    /// Contains component values to be set onto entities.
    /// </summary>
    private readonly ComponentSetterCollection setters = new();

    /// <summary>
    /// All entity modifications to be applied.
    /// </summary>
    private readonly Dictionary<Entity, BufferedEntityModification> entityModifications = [];

    /// <summary>
    /// Entities to be destroyed.
    /// </summary>
    private readonly List<Entity> destroys = [];

    /// <summary>
    /// Archetypes to be destroyed.
    /// </summary>
    private readonly List<Archetype> archetypeDestroys = [];

    /// <summary>
    /// Stores an entity's component set after structural changes.
    /// <para/>
    /// This is used by <see cref="ApplyStructuralChanges"/> to figure out which archetype to move an entity to when components are added/removed.
    /// </summary>
    /// <remarks>
    /// Stores temporary data. Clear before use.
    /// </remarks>
    private readonly OrderedListSet<ComponentId> tempComponentsAfterMove = [];

    /// <summary>
    /// Create a new <see cref="EcsCommandBuffer"/> for the given <see cref="World"/>.
    /// </summary>
    internal EcsCommandBuffer(EcsWorld world)
    {
        World = world;
    }

    /// <summary>
    /// Create a new <see cref="Entity"/> in the world.
    /// </summary>
    public BufferedEntity Create()
    {
        EnsureIsExternallyMutable();
        HasBufferedOperations = true;

        // Acquire an ID
        // TODO: This can cause lock contention. Optimize by acquiring a batch of IDs.
        World.Entities.AcquireId(out var entityId);
        var entity = entityId.ToEntity(World);
        newEntities.Add(entity);

        return new BufferedEntity(entity, this);
    }

    /// <summary>
    /// Create a <see cref="BufferedEntity"/> using the specified entity.
    /// </summary>
    public BufferedEntity Use(Entity entity)
    {
        return new BufferedEntity(entity, this);
    }

    /// <summary>
    /// Add or overwrite a component attached to an entity.
    /// </summary>
    public BufferedEntity Set<T>(Entity entity, T value) where T : IComponent
    {
        EnsureIsExternallyMutable();
        HasBufferedOperations = true;

        var modification = GetBufferedModification(entity, true, false);

        // Create a setter and store it in the list (recycling the old one, if it's there)
        var id = ComponentId.Get<T>();
        if (modification.Sets!.TryGetValue(id, out var existing))
        {
            setters.Overwrite(existing, value);
        }
        else
        {
            var index = setters.Add(value);
            modification.Sets!.Add(id, index);
        }

        // Remove it from the "remove" set. In case it was previously removed
        modification.Removes?.Remove(id);

        return new BufferedEntity(entity, this);
    }

    /// <summary>
    /// Remove a component attached to an entity.
    /// </summary>
    public BufferedEntity Remove<T>(Entity entity) where T : IComponent
    {
        EnsureIsExternallyMutable();
        HasBufferedOperations = true;

        var modification = GetBufferedModification(entity, false, true);

        // Add a remover to the list
        var id = ComponentId.Get<T>();
        modification.Removes!.Add(id);

        // Remove it from the setters, if it's there
        modification.Sets?.Remove(id);

        return new BufferedEntity(entity, this);
    }

    /// <summary>
    /// Destroy an entity.
    /// </summary>
    public EcsCommandBuffer Destroy(Entity entity)
    {
        EnsureIsExternallyMutable();
        HasBufferedOperations = true;

        destroys.Add(entity);

        return this;
    }

    /// <summary>
    /// Bulk destroy entities.
    /// </summary>
    public EcsCommandBuffer Destroy(List<Entity> entities)
    {
        EnsureIsExternallyMutable();
        HasBufferedOperations = true;

        destroys.AddRange(entities);

        return this;
    }

    /// <summary>
    /// Bulk destroy all entities which match the given query.
    /// </summary>
    public EcsCommandBuffer Destroy(IArchetypeView query)
    {
        EnsureIsExternallyMutable();

        foreach (var archetype in query.Archetypes)
        {
            GuardUtility.IsTrue(archetype.World == World, "Cannot use archetype from one world with a command buffer for another world");

            archetypeDestroys.Add(archetype);
            HasBufferedOperations = true;
        }

        return this;
    }

    /// <summary>
    /// Apply all buffered operations to the <see cref="World"/>. The returned resolver is valid until the next time <see cref="Execute"/> is called.
    /// </summary>
    public void Execute()
    {
        EnsureIsExternallyMutable();

        if (!HasBufferedOperations)
        {
            return;
        }

        IsExecuting = true;
        {
            using var _ = World.AcquireCommandBuffer(out var recursiveCommandBuffer);

            // Create buffered entities.
            CreateEntities(recursiveCommandBuffer);

            // Destroy entities, this must occur before structural changes because it may trigger new structural changes
            // by adding a new phantom component.
            DestroyEntities(recursiveCommandBuffer);

            // Structural changes (add/remove components)
            // This also includes setting components
            ApplyStructuralChanges(recursiveCommandBuffer);

            // Clear all temporary state
            newEntities.Clear();
            setters.Clear(false);
            entityModifications.Clear();

            HasBufferedOperations = false;

            recursiveCommandBuffer.Execute();
        }
        IsExecuting = false;
    }

    /// <summary>
    /// Clear this <see cref="EcsCommandBuffer"/>.
    /// </summary>
    public void Clear()
    {
        if (!HasBufferedOperations)
        {
            return;
        }

        EnsureIsExternallyMutable();

        foreach (var newEntity in newEntities)
        {
            World.Entities.ReleaseId(newEntity.EntityId);
        }
        newEntities.Clear();

        setters.Clear(true);

        foreach (var (_, data) in entityModifications)
        {
            if (data.Removes != null)
            {
                data.Removes.Clear();
                SimplePool.Release(data.Removes);
            }

            if (data.Sets != null)
            {
                data.Sets.Clear();
                SimplePool.Release(data.Sets);
            }
        }
        entityModifications.Clear();

        destroys.Clear();
        archetypeDestroys.Clear();

        HasBufferedOperations = false;
    }

    private void CreateEntities(EcsCommandBuffer recursiveCommandBuffer)
    {
        var archetype = World.GetOrCreateArchetype(ImmutableOrderedListSet<ComponentId>.Empty.AsComponentIdSet());
        foreach (var newEntity in newEntities)
        {
            archetype.CreateEntity(recursiveCommandBuffer, newEntity.EntityId);
        }
    }

    private void DestroyEntities(EcsCommandBuffer recursiveCommandBuffer)
    {
        foreach (var archetype in archetypeDestroys)
        {
            DestroyArchetypeEntities(recursiveCommandBuffer, archetype);
        }
        archetypeDestroys.Clear();

        foreach (var entity in destroys)
        {
            // Destroy entity
            DestroyEntity(recursiveCommandBuffer, entity);

            // Return objects to pools
            if (entityModifications.Remove(entity, out var mod))
            {
                if (mod.Sets != null)
                {
                    mod.Sets.Clear();
                    SimplePool.Release(mod.Sets);
                }

                if (mod.Removes != null)
                {
                    mod.Removes.Clear();
                    SimplePool.Release(mod.Removes);
                }
            }
        }
        destroys.Clear();
    }

    private void ApplyStructuralChanges(EcsCommandBuffer recursiveCommandBuffer)
    {
        if (entityModifications.Count > 0)
        {
            // Calculate the new archetype for the entity
            foreach (var (entity, modification) in entityModifications)
            {
                if (!entity.IsAlive)
                {
                    continue;
                }

                var archetypeBeforeMove = World.Entities.GetArchetype(entity.EntityId);
                var componentsBeforeMove = archetypeBeforeMove.Components;

                // Initialize componentsAfterMove with componentsBeforeMove
                var componentsAfterMove = tempComponentsAfterMove;
                componentsAfterMove.Clear();
                componentsAfterMove.UnionWith(componentsBeforeMove);

                // Check if a move is required
                var moveRequired = false;
                var hash = archetypeBeforeMove.Hash;
                {
                    // Component adds/sets
                    if (modification.Sets != null)
                    {
                        foreach (var id in modification.Sets.Keys)
                        {
                            if (componentsAfterMove.Add(id))
                            {
                                hash = hash.Toggle(id);
                                moveRequired = true;
                            }
                        }
                    }

                    // Component removes
                    if (modification.Removes != null)
                    {
                        foreach (var remove in modification.Removes)
                        {
                            if (componentsAfterMove.Remove(remove))
                            {
                                hash = hash.Toggle(remove);
                                moveRequired = true;
                            }
                        }

                        // Recycle remove set
                        modification.Removes.Clear();
                        SimplePool.Release(modification.Removes);
                    }
                }

                // Get the location for the entity, moving it to a new archetype first if necessary
                EntityLocation location;
                if (moveRequired)
                {
                    // Raise component removed events
                    foreach (var componentId in componentsBeforeMove)
                    {
                        if (!componentsAfterMove.Contains(componentId))
                        {
                            archetypeBeforeMove.Lookup.ComponentEventDispatcherByComponentId[componentId.Value].OnComponentRemoved(recursiveCommandBuffer, entity);
                        }
                    }

                    // Get the new archetype we're moving to
                    var dstArchetype = World.GetOrCreateArchetype(componentsAfterMove.AsComponentIdSet(), hash);

                    // Migrate the entity across
                    location = World.MigrateEntity(entity.EntityId, dstArchetype);
                }
                else
                {
                    location = World.Entities.GetLocation(entity.EntityId);
                }

                if (modification.Sets != null)
                {
                    // Run all setters
                    foreach (var setter in modification.Sets.Values)
                    {
                        setters.Write(setter, location);
                    }

                    // Raise component added/modified events
                    foreach (var setter in modification.Sets.Values)
                    {
                        var eventDispatcher = location.Chunk.Lookup.ComponentEventDispatcherByComponentId[setter.ComponentId.Value];
                        if (componentsBeforeMove.Contains(setter.ComponentId))
                        {
                            eventDispatcher.OnComponentModified(recursiveCommandBuffer, entity);
                        }
                        else
                        {
                            eventDispatcher.OnComponentAdded(recursiveCommandBuffer, entity);
                        }
                    }
                }

                // Recycle setters
                if (modification.Sets != null)
                {
                    modification.Sets.Clear();
                    SimplePool.Release(modification.Sets);
                }
            }
        }
    }

    private BufferedEntityModification GetBufferedModification(Entity entity, bool ensureSet, bool ensureRemove)
    {
        if (!entityModifications.TryGetValue(entity, out var modification))
        {
            modification = new BufferedEntityModification(null, null);
            entityModifications[entity] = modification;
        }

        var needsUpdate = false;
        if (modification.Sets == null && ensureSet)
        {
            modification.Sets = SimplePool<Dictionary<ComponentId, ComponentSetterCollection.SetterId>>.Acquire();
            modification.Sets.Clear();

            needsUpdate = true;
        }

        if (modification.Removes == null && ensureRemove)
        {
            modification.Removes = SimplePool<OrderedListSet<ComponentId>>.Acquire();
            modification.Removes.Clear();

            needsUpdate = true;
        }

        if (needsUpdate)
        {
            entityModifications[entity] = modification;
        }

        return modification;
    }

    private void DestroyEntity(EcsCommandBuffer recursiveCommandBuffer, Entity entity)
    {
        // Ignore destroyed entities
        if (!entity.IsAlive)
        {
            return;
        }

        // Get the location for this entity
        ref var location = ref World.Entities.GetLocation(entity.EntityId);

        // Check this is still a valid entity reference. Early exit if the entity
        // is already dead.
        if (location.Version != entity.Version)
        {
            return;
        }

        // Raise component removed events
        foreach (var componentId in entity.ComponentIds)
        {
            var eventDispatcher = location.Chunk.Lookup.ComponentEventDispatcherByComponentId[componentId.Value];
            eventDispatcher.OnComponentRemoved(recursiveCommandBuffer, entity);
        }

        // Raise entity destroyed event
        World.EventBus.Raise(new EntityDestroyedEvent(recursiveCommandBuffer, entity));

        // Notify archetype this entity is dead
        location.Chunk.Archetype.RemoveEntity(location);

        // Release ID
        World.Entities.ReleaseId(entity.EntityId);
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

    /// <summary>
    /// Used to check if it is safe for external calls to modify the command buffer.
    /// </summary>
    private void EnsureIsExternallyMutable()
    {
        GuardUtility.IsFalse(World.IsDisposed, "World has been disposed");
        GuardUtility.IsFalse(IsExecuting, "Command buffer is currently executing");
    }

    private record struct BufferedEntityModification(Dictionary<ComponentId, ComponentSetterCollection.SetterId>? Sets, OrderedListSet<ComponentId>? Removes);
}
