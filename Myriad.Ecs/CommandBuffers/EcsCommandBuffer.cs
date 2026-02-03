using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    public bool HasBufferedOperations => state.Commands.Count != 0;
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
        state.Commands.Add(new Command(CommandType.CreateEntity, state.CreateEntityCommands.Count));
        state.CreateEntityCommands.Add(new CreateEntityCommand(entityId));

        return new BufferedEntity(entityId.ToEntity(World), this);
    }

    /// <summary>
    /// Add or overwrite a component attached to an entity.
    /// </summary>
    public BufferedEntity Set<T>(Entity entity, T value) where T : IComponent
    {
        EnsureIsExternallyMutable();
        EnsureIsFromCurrentWorld(entity);

        // Create the setter
        var setterId = state.Setters.Create(value);

        // Store the command
        state.Commands.Add(new Command(CommandType.SetComponent, state.SetComponentCommands.Count));
        state.SetComponentCommands.Add(new SetComponentCommand(entity.EntityId, setterId));

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
        state.Commands.Add(new Command(CommandType.RemoveComponent, state.RemoveComponentCommands.Count));
        state.RemoveComponentCommands.Add(new RemoveComponentCommand(entity.EntityId, ComponentId.Get<T>()));

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
        state.Commands.Add(new Command(CommandType.DestroyEntity, state.DestroyEntityCommands.Count));
        state.DestroyEntityCommands.Add(new DestroyEntityCommand(entity.EntityId));

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
            state.Commands.Add(new Command(CommandType.DestroyEntity, state.DestroyEntityCommands.Count));
            state.DestroyEntityCommands.Add(new DestroyEntityCommand(entity.EntityId));
        }

        return this;
    }

    /// <summary>
    /// Bulk destroy all entities in archetypes stored by the view.
    /// </summary>
    /// <remarks>
    /// Note that the view is evaluated at time of execution.
    /// </remarks>
    public EcsCommandBuffer Destroy(IArchetypeView view)
    {
        EnsureIsExternallyMutable();

        // Store the command
        state.Commands.Add(new Command(CommandType.DestroyArchetypeView, state.DestroyArchetypeViewCommands.Count));
        state.DestroyArchetypeViewCommands.Add(new DestroyArchetypeViewCommand(view));

        return this;
    }

    /// <summary>
    /// Defers the specified action until the command buffer is executed.
    /// </summary>
    public EcsCommandBuffer Defer(Action action)
    {
        EnsureIsExternallyMutable();

        // Store the command
        state.Commands.Add(new Command(CommandType.DeferredAction, state.DeferredActionCommands.Count));
        state.DeferredActionCommands.Add(new DeferredActionCommand(action));

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

        // Release unused local IDs
        World.Entities.ReleaseUnusedIds(localIdPool);

        // Clear commands
        state.Clear(World);
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
}
