using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Exanite.Core.Pooling;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
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
/// Example 2: Destroying an entity and making modifications to it is the same as simply destroying it.
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
public sealed partial class EcsCommandBuffer
{
    /// <summary>
    /// The <see cref="World"/> this <see cref="EcsCommandBuffer"/> is modifying.
    /// </summary>
    public EcsWorld World { get; }

    public bool HasBufferedOperations { get; private set; }
    public bool IsExecuting { get; private set; }

    /// <summary>
    /// A pool of local IDs.
    /// These are bulk acquired to avoid thread contention.
    /// </summary>
    private readonly List<EntityId> localIdPool = new();

    /// <summary>
    /// New entities to be created.
    /// </summary>
    private readonly List<EntityId> newEntities = [];

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

        if (localIdPool.Count == 0)
        {
            // Bulk acquire IDs if none available locally
            // This is to avoid thread contention
            World.Entities.AcquireIds(localIdPool, EcsConstants.CommandBufferLocalIdCount);
        }

        // Acquire an ID
        var entityId = localIdPool[^1];
        localIdPool.RemoveAt(localIdPool.Count - 1);

        // Save it as a new entity
        newEntities.Add(entityId);

        return new BufferedEntity(entityId.ToEntity(World), this);
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
        EnsureIsFromCurrentWorld(entity);
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
        EnsureIsFromCurrentWorld(entity);
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
        EnsureIsFromCurrentWorld(entity);
        HasBufferedOperations = true;

        destroys.Add(entity);

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
            EnsureIsFromCurrentWorld(archetype);
        }

        archetypeDestroys.AddRange(query.Archetypes);
        HasBufferedOperations = true;

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

            // Release unused local IDs
            World.Entities.ReleaseUnusedIds(localIdPool);

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

        // Release used entity IDs
        // Do not reuse these without releasing since external callers already have access to them
        foreach (var newEntity in newEntities)
        {
            World.Entities.ReleaseId(newEntity);
        }
        newEntities.Clear();

        // Release unused local IDs
        World.Entities.ReleaseUnusedIds(localIdPool);

        // Clear rest of internal state
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
