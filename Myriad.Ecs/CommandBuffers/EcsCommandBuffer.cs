using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.CommandBuffers;

/// <summary>
/// Buffers up modifications to entities and replays them all at once.
/// Events for an entity are reported once all modifications to the entity are completed.
/// <para/>
/// Warning: The command buffer is allowed to reorder and merge operations together.
/// <br/>
/// Example 1: Setting the same component twice is the same as setting it once, with the last taking priority.
/// <br/>
/// Example 2: Destroying an entity and making modifications to it is the same as simply destroying it.
/// </summary>
/// <remarks>
/// The command buffer batches commands to avoid expensive operations
/// where adding/removing multiple components causes the entity to be
/// copied between multiple archetypes.
/// <para/>
/// Fully ordered events without dropping of intermediate events was considered,
/// but the performance cost is likely too high.
/// <para/>
/// If fully ordered events are required, it's possible to submit commands
/// in smaller batches. This has similar cost to replaying commands without
/// merging.
/// </remarks>
public sealed partial class EcsCommandBuffer
{
    /// <summary>
    /// The <see cref="World"/> this <see cref="EcsCommandBuffer"/> is modifying.
    /// </summary>
    public EcsWorld World { get; }

    public bool HasBufferedOperations { get; private set; }
    public bool IsExecuting { get; private set; }

    /// <summary>
    /// Contains information about commands enqueued in the command buffer.
    /// </summary>
    private readonly CommandState state = new();

    /// <summary>
    /// A pool of local IDs.
    /// These are bulk acquired to avoid thread contention.
    /// </summary>
    private readonly List<EntityId> localIdPool = new();

    /// <summary>
    /// Create a new <see cref="EcsCommandBuffer"/> for the given <see cref="World"/>.
    /// </summary>
    internal EcsCommandBuffer(EcsWorld world)
    {
        World = world;
    }

    /// <summary>
    /// Create a <see cref="BufferedEntity"/> using the specified entity.
    /// This allows for fluent method chaining.
    /// </summary>
    public BufferedEntity Use(Entity entity)
    {
        return new BufferedEntity(entity, this);
    }

    /// <summary>
    /// Create a new <see cref="Entity"/> in the world.
    /// </summary>
    public BufferedEntity Create()
    {
        EnsureIsExternallyMutable();

        if (localIdPool.Count == 0)
        {
            // Bulk acquire IDs if none available locally
            // This is to avoid thread contention
            World.Entities.AcquireIds(localIdPool, EcsConstants.CommandBufferLocalIdCount);
        }

        // Acquire an ID
        var entityId = localIdPool[^1];
        localIdPool.RemoveAt(localIdPool.Count - 1);

        // Store the command
        ref var entityState = ref GetEntityState(entityId);
        entityState.NeedsCreation = true;

        HasBufferedOperations = true;

        return new BufferedEntity(entityId.ToEntity(World), this);
    }

    /// <summary>
    /// Copies all components from the target prefab onto the specified entity.
    /// </summary>
    /// <remarks>
    /// The component types to copy are read at the time of recording; however, the component values are read at time of execution.
    /// It is assumed that the prefab entity is not modified between now and when the command buffer is executed.
    /// </remarks>
    public BufferedEntity CopyFrom(Entity entity, Entity prefab)
    {
        EnsureIsExternallyMutable();

        // Store the commands
        ref var entityState = ref GetEntityState(entity.EntityId);
        var sets = entityState.GetOrAcquireSets();
        foreach (var componentId in prefab.ComponentIds)
        {
            ref var setterId = ref CollectionsMarshal.GetValueRefOrAddDefault(sets, componentId, out _);
            state.Setters.CreateFromPrefab(prefab, componentId, ref setterId);

            // Prevent the remove, if it exists
            entityState.Removes?.Remove(componentId);
        }

        // Store the prefab for lookup purposes
        // TODO: this doesn't work. prefabs to new entity relations are many to many. ahhhhhhhhh
        state.RawLookup[prefab] = entity;

        return new BufferedEntity(entity, this);
    }

    /// <summary>
    /// Add or overwrite a component attached to an entity.
    /// </summary>
    public BufferedEntity Set<T>(Entity entity, T value) where T : IComponent
    {
        EnsureIsExternallyMutable();
        EnsureIsFromCurrentWorld(entity);

        var componentId = ComponentId.Get<T>();

        // Store the command
        ref var entityState = ref GetEntityState(entity.EntityId);

        var sets = entityState.GetOrAcquireSets();
        ref var setterId = ref CollectionsMarshal.GetValueRefOrAddDefault(sets, componentId, out _);
        state.Setters.CreateFromValue(value, ref setterId);

        // Prevent the remove, if it exists
        entityState.Removes?.Remove(componentId);

        HasBufferedOperations = true;

        return new BufferedEntity(entity, this);
    }

    /// <summary>
    /// Remove a component attached to an entity.
    /// </summary>
    public BufferedEntity Remove<T>(Entity entity) where T : IComponent
    {
        EnsureIsExternallyMutable();
        EnsureIsFromCurrentWorld(entity);

        // Store the command
        var componentId = ComponentId.Get<T>();
        ref var entityState = ref GetEntityState(entity.EntityId);
        var removes = entityState.GetOrAcquireRemoves();
        removes.Add(componentId);

        // Prevent the set, if it exists
        entityState.Sets?.Remove(componentId);

        HasBufferedOperations = true;

        return new BufferedEntity(entity, this);
    }

    /// <summary>
    /// Destroy an entity.
    /// </summary>
    public EcsCommandBuffer Destroy(Entity entity)
    {
        EnsureIsExternallyMutable();
        EnsureIsFromCurrentWorld(entity);

        // Store the command
        ref var entityState = ref GetEntityState(entity.EntityId);
        entityState.NeedsCreation = false;
        state.EntitiesToDestroy.Add(entity.EntityId);

        HasBufferedOperations = true;

        return this;
    }

    /// <summary>
    /// Bulk destroy entities.
    /// </summary>
    public EcsCommandBuffer Destroy(ReadOnlySpan<Entity> entities)
    {
        EnsureIsExternallyMutable();
        foreach (var entity in entities)
        {
            EnsureIsFromCurrentWorld(entity);
        }

        // Store the commands
        foreach (var entity in entities)
        {
            ref var entityState = ref GetEntityState(entity.EntityId);
            entityState.NeedsCreation = false;
            state.EntitiesToDestroy.Add(entity.EntityId);
        }

        HasBufferedOperations = true;

        return this;
    }

    /// <summary>
    /// Bulk destroy all entities in archetypes stored by the view.
    /// </summary>
    /// <remarks>
    /// The archetypes to destroy are read at the time of recording.
    /// It is assumed that no new archetypes are added between now and when the command buffer is executed.
    /// </remarks>
    public EcsCommandBuffer Destroy(IArchetypeView view)
    {
        EnsureIsExternallyMutable();

        // Capture the archetypes span first
        var archetypes = view.Archetypes;
        foreach (var archetype in archetypes)
        {
            EnsureIsFromCurrentWorld(archetype);
        }

        // Store the commands
        foreach (var archetype in archetypes)
        {
            state.ArchetypesToDestroy.Add(archetype);
        }

        HasBufferedOperations = true;

        return this;
    }

    /// <summary>
    /// Defers the specified action until the command buffer is executed.
    /// </summary>
    public EcsCommandBuffer Defer(Action action)
    {
        EnsureIsExternallyMutable();

        // Store the command
        state.Actions.Add(action);

        HasBufferedOperations = true;

        return this;
    }

    /// <summary>
    /// Clear this <see cref="EcsCommandBuffer"/>.
    /// </summary>
    public void Clear()
    {
        EnsureIsExternallyMutable();
        if (!HasBufferedOperations)
        {
            return;
        }

        // Release used entity IDs
        // Do not reuse these without releasing since external callers already have access to them
        foreach (var (entityId, entityState) in state.EntityStates)
        {
            if (entityState.NeedsCreation)
            {
                World.Entities.ReleaseId(entityId);
            }
        }

        // Release unused local IDs
        World.Entities.ReleaseUnusedIds(localIdPool);

        // Clear commands
        state.Clear(World);

        HasBufferedOperations = false;
    }

    /// <summary>
    /// Apply all buffered operations to the <see cref="World"/>.
    /// </summary>
    public void Execute()
    {
        EnsureIsExternallyMutable();
        if (!HasBufferedOperations)
        {
            return;
        }

        IsExecuting = true;
        try
        {
            ExecuteInternal();
        }
        finally
        {
            IsExecuting = false;
        }
    }

    /// <summary>
    /// Used to check if it is safe for external calls to modify the command buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureIsExternallyMutable()
    {
        GuardUtility.IsFalse(World.IsDisposed, "World has been disposed");
        GuardUtility.IsFalse(IsExecuting, "Command buffer is currently executing");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureIsFromCurrentWorld(Entity entity)
    {
        GuardUtility.IsTrue(entity.World == World, "Entity must belong to the same world as the command buffer");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureIsFromCurrentWorld(Archetype archetype)
    {
        GuardUtility.IsTrue(archetype.World == World, "Archetype must belong to the same world as the command buffer");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref EntityState GetEntityState(EntityId entityId)
    {
        return ref CollectionsMarshal.GetValueRefOrAddDefault(state.EntityStates, entityId, out var _);
    }
}
