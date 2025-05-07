using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using Myriad.Ecs.Allocations;
using Myriad.Ecs.Collections;
using Myriad.Ecs.Components;
using Myriad.Ecs.Queries;
using Myriad.Ecs.Worlds.Archetypes;

namespace Myriad.Ecs.CommandBuffers;

/// <summary>
/// Buffers up modifications to entities and replays them all at once.
/// </summary>
public sealed partial class EcsCommandBuffer
{
    private uint version;

    /// <summary>
    /// The <see cref="World"/> this <see cref="EcsCommandBuffer"/> is modifying
    /// </summary>
    public World World { get; }

    public bool HasBufferedOperations { get; private set; }

    /// <summary>
    /// Collection of all components to be set onto entities
    /// </summary>
    private readonly ComponentSetterCollection setters = new();

    private readonly List<BufferedEntityData> bufferedSets = [];

    private readonly Dictionary<Entity, EntityModificationData> entityModifications = [];
    private readonly List<Entity> deletes = [];
    private readonly List<QueryDescription> archetypeDeletes = [];
    private readonly OrderedListSet<Entity> maybeAddingPhantomComponent = [];

    private readonly OrderedListSet<ComponentId> tempComponentIdSet = [];

    private Resolver nextResolver;

    /// <summary>
    /// Create a new <see cref="EcsCommandBuffer"/> for the given <see cref="World"/>
    /// </summary>
    public EcsCommandBuffer(World world)
    {
        World = world;

        nextResolver = Pool<Resolver>.Get();
        nextResolver.Configure(this);
    }

    #region Clear

    /// <summary>
    /// Clear this <see cref="EcsCommandBuffer"/>
    /// </summary>
    public void Clear()
    {
        // We can't actually make any changes, but we do still need the lazy buffer
        var lazy = new LazyCommandBuffer(World);

        setters.ClearAndDispose(ref lazy);

        foreach (var bufferedEntity in bufferedSets)
        {
            var setters = bufferedEntity.Setters;
            setters.Clear();
            Pool.Return(setters);
        }
        bufferedSets.Clear();

        foreach (var (_, data) in entityModifications)
        {
            if (data.Removes != null)
            {
                data.Removes.Clear();
                Pool.Return(data.Removes);
            }

            if (data.Sets != null)
            {
                data.Sets.Clear();
                Pool.Return(data.Sets);
            }
        }
        entityModifications.Clear();

        archetypeEdges.Clear();

        deletes.Clear();
        archetypeDeletes.Clear();
        maybeAddingPhantomComponent.Clear();
        tempComponentIdSet.Clear();

        HasBufferedOperations = false;

        unchecked { version++; }
        nextResolver.Dispose();
        nextResolver = Pool<Resolver>.Get();
        nextResolver.Configure(this);

        if (lazy.TryGetBuffer(out var cmd))
        {
            cmd.Clear();
        }
    }

    #endregion

    #region Playback

    /// <summary>
    /// Apply all of the operations in this buffer to the <see cref="World"/>
    /// </summary>
    public Resolver Playback()
    {
        // Use this resolver for this playback
        var resolver = nextResolver;

        // Create buffered entities.
        CreateBufferedEntities(resolver);

        // Lazy command buffer accumulates any changes caused by applying this command buffer
        var lazy = new LazyCommandBuffer(World);

        // Delete entities, this must occur before structural changes because it may trigger new structural changes
        // by adding a new phantom component.
        DeleteEntities(ref lazy);

        // Structural changes (add/remove components)
        ApplyStructuralChanges(ref lazy);

        // Clear all temporary state
        maybeAddingPhantomComponent.Clear();
        setters.Clear();
        entityModifications.Clear();
        tempComponentIdSet.Clear();
        archetypeEdges.Clear();

        HasBufferedOperations = false;

        // Update the version of this buffer, invalidating all buffered entities for further modification
        unchecked { version++; }

        // Apply any changes caused by these changes
        if (lazy.TryGetBuffer(out var lazyBuffer))
        {
            lazyBuffer.Playback().Dispose();
            World.ReturnCommandBuffer(lazyBuffer);
        }

        // Create a resolver ready to use in the future
        nextResolver = Pool<Resolver>.Get();
        nextResolver.Configure(this);

        // Return the resolver
        return resolver;
    }

    private void DeleteEntities(ref LazyCommandBuffer lazy)
    {
        foreach (var query in archetypeDeletes)
        {
            foreach (var archetype in query.GetArchetypes())
            {
                if (archetype.EntityCount == 0)
                {
                    continue;
                }

                World.DeleteImmediate(archetype, ref lazy);
            }
        }
        archetypeDeletes.Clear();

        foreach (var delete in deletes)
        {
            // Skip deleted entities
            if (!delete.Exists())
            {
                continue;
            }

            var archetype = World.GetArchetype(delete.EntityId);
            if (archetype is { IsPhantom: false, HasPhantomComponents: true } || IsAddingPhantomComponent(delete))
            {
                // It has phantom components and isn't yet a phantom. Add a Phantom component.
                SetInternal(delete, new ComponentPhantom());
            }
            else
            {
                World.DeleteImmediate(delete.EntityId, ref lazy);

                // Return objects to pools
                if (entityModifications.Remove(delete, out var mod))
                {
                    if (mod.Sets != null)
                    {
                        mod.Sets.Clear();
                        Pool.Return(mod.Sets);
                    }

                    if (mod.Removes != null)
                    {
                        mod.Removes.Clear();
                        Pool.Return(mod.Removes);
                    }
                }
            }
        }

        deletes.Clear();

        return;

        // Check if this entity should not be deleted, because a phantom component is being added
        bool IsAddingPhantomComponent(Entity entity)
        {
            if (maybeAddingPhantomComponent.Contains(entity) && entityModifications.TryGetValue(entity, out var mod) && mod.Sets != null)
            {
                foreach (var key in mod.Sets.Keys)
                {
                    if (key.IsPhantomComponent)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    private void ApplyStructuralChanges(ref LazyCommandBuffer lazy)
    {
        if (entityModifications.Count > 0)
        {
            // Calculate the new archetype for the entity
            foreach (var (entity, mod) in entityModifications)
            {
                var currentArchetype = World.GetArchetype(entity.EntityId);

                // Set all of the current archetype components
                tempComponentIdSet.Clear();
                tempComponentIdSet.UnionWith(currentArchetype.Components);
                var moveRequired = false;

                // Calculate the hash and component set of the new archetype
                var hash = currentArchetype.Hash;
                if (mod.Sets != null)
                {
                    foreach (var id in mod.Sets.Keys)
                    {
                        if (tempComponentIdSet.Add(id))
                        {
                            hash = hash.Toggle(id);
                            moveRequired = true;
                        }
                    }
                }
                if (mod.Removes != null)
                {
                    foreach (var remove in mod.Removes)
                    {
                        if (tempComponentIdSet.Remove(remove))
                        {
                            hash = hash.Toggle(remove);
                            moveRequired = true;
                        }
                    }

                    // Recycle remove set
                    mod.Removes.Clear();
                    Pool.Return(mod.Removes);
                }

                // Check if the entity will have any phantom components after this change
                var destHasPhantomComponents = tempComponentIdSet.Any(static a => a.IsPhantomComponent);

                // Entity must be auto deleted if, after the change, it will be a `Phantom` but not have any phantom components
                var autodelete = tempComponentIdSet.Contains(ComponentId.Get<ComponentPhantom>()) && !destHasPhantomComponents;
                if (autodelete)
                {
                    World.DeleteImmediate(entity.EntityId, ref lazy);
                }
                else
                {
                    // Get a row handle for the entity, moving it to a new archetype first if necessary
                    Row row;
                    if (moveRequired)
                    {
                        // Get the new archetype we're moving to
                        var newArchetype = World.GetOrCreateArchetype(tempComponentIdSet, hash);

                        // Migrate the entity across
                        row = World.MigrateEntity(entity.EntityId, newArchetype, ref lazy);
                    }
                    else
                    {
                        row = World.GetRow(entity.EntityId);
                    }

                    // Run all setters
                    if (mod.Sets != null)
                    {
                        foreach (var set in mod.Sets.Values)
                        {
                            setters.Write(set, row);
                        }
                    }
                }

                // Recycle setters
                if (mod.Sets != null)
                {
                    mod.Sets.Clear();
                    Pool.Return(mod.Sets);
                }
            }
        }
    }

    private void CreateBufferedEntities(Resolver resolver)
    {
        tempComponentIdSet.Clear();

        // Keep a map from archetype key -> archetype. This means we only need to calculate it once
        // per archetype key.
        var archetypeLookup = ArrayPool<Archetype>.Shared.Rent(archetypeEdges.Count + 1);
        Array.Clear(archetypeLookup, 0, archetypeLookup.Length);
        try
        {
            foreach (var bufferedData in bufferedSets)
            {
                var components = bufferedData.Setters;

                var archetype = GetArchetype(bufferedData, archetypeLookup);

                var slot = archetype.CreateEntity();

                // Store the new ID in the resolver so it can be retrieved later
                resolver.Lookup.Add(bufferedData.Id, slot.Entity);

                // Write the components into the entity
                foreach (var setter in components.Values)
                {
                    setters.Write(setter, slot);
                }

                // Recycle
                components.Clear();
                Pool.Return(components);
            }

            bufferedSets.Clear();
            tempComponentIdSet.Clear();
        }
        finally
        {
            ArrayPool<Archetype>.Shared.Return(archetypeLookup);
        }
    }

    private Archetype GetArchetype(BufferedEntityData entityData, Archetype?[] archetypeLookup)
    {
        // Check the cache
        if (entityData.ArchetypeKey >= 0)
        {
            var a = archetypeLookup[entityData.ArchetypeKey];
            if (a != null)
            {
                return a;
            }
        }

        // Get the archetype
        var archetype = World.GetOrCreateArchetype(entityData.Setters);

        // If the node ID is positive, cache it
        if (entityData.ArchetypeKey >= 0)
        {
            archetypeLookup[entityData.ArchetypeKey] = archetype;
        }

        return archetype;
    }

    #endregion

    /// <summary>
    /// Create a new <see cref="Entity"/> in the world.
    /// </summary>
    public BufferedEntity Create()
    {
        HasBufferedOperations = true;

        // Get a set to hold all of the component setters
        var set = Pool<Dictionary<ComponentId, ComponentSetterCollection.SetterId>>.Get();
        set.Clear();

        // Store this entity in the collection of entities
        // Put it in aggregate node 0 (i.e. no components)
        var id = (uint)bufferedSets.Count;
        bufferedSets.Add(new BufferedEntityData(id, set));

        return new BufferedEntity(id, this, nextResolver);
    }

    /// <summary>
    /// Add or overwrite a component attached to an entity
    /// </summary>
    public void Set<T>(Entity entity, T value) where T : IComponent
    {
        HasBufferedOperations = true;

        if (typeof(T) == typeof(ComponentPhantom))
        {
            throw new InvalidOperationException("Cannot manually attach `Phantom` component to an entity");
        }

        SetInternal(entity, value);
    }

    /// <summary>
    /// Remove a component attached to an entity
    /// </summary>
    public void Remove<T>(Entity entity) where T : IComponent
    {
        HasBufferedOperations = true;

        if (typeof(T) == typeof(ComponentPhantom))
        {
            throw new InvalidOperationException("Cannot remove `Phantom` component from an entity");
        }

        var mod = GetModificationData(entity, false, true);

        // Add a remover to the list
        var id = ComponentId.Get<T>();
        mod.Removes!.Add(id);

        // Remove it from the setters, if it's there
        mod.Sets?.Remove(id);
    }

    /// <summary>
    /// Delete an entity
    /// </summary>
    public void Delete(Entity entity)
    {
        HasBufferedOperations = true;

        deletes.Add(entity);
    }

    /// <summary>
    /// Bulk delete entities
    /// </summary>
    public void Delete(List<Entity> entities)
    {
        HasBufferedOperations = true;

        deletes.AddRange(entities);
    }

    /// <summary>
    /// Bulk delete all entities which match the given query
    /// </summary>
    public void Delete(QueryDescription entities)
    {
        if (entities.World != World)
        {
            throw new ArgumentException("Cannot use QueryDescription from one World with CommandBuffer for another World");
        }

        HasBufferedOperations = true;

        archetypeDeletes.Add(entities);
    }

    private void SetBuffered<T>(uint id, T value) where T : IComponent
    {
        Debug.Assert(id < bufferedSets.Count, "Unknown entity ID in SetBuffered");

        if (typeof(T) == typeof(ComponentPhantom))
        {
            throw new InvalidOperationException("Cannot manually attach `Phantom` component to an entity");
        }

        var bufferedData = bufferedSets[(int)id];
        var setters = bufferedData.Setters;

        var key = ComponentId.Get<T>();

        if (setters.TryGetValue(key, out var existing))
        {
            this.setters.Overwrite(existing, value);
        }
        else
        {
            // Add to global collection of setters
            var setterIndex = this.setters.Add(value);

            // Store the index in the per-entity collection
            setters.Add(key, setterIndex);

            // Update node id. Skip it if it's in node -1, once an entity is
            // marked as node -1 it's been opted out of aggregation.
            if (bufferedData.ArchetypeKey != -1)
            {
                bufferedData.ArchetypeKey = GetArchetypeKey(bufferedData.ArchetypeKey, key);
                bufferedSets[(int)id] = bufferedData;
            }
        }
    }

    private void SetInternal<T>(Entity entity, T value) where T : IComponent
    {
        var mod = GetModificationData(entity, true, false);

        // Create a setter and store it in the list (recycling the old one, if it's there)
        var id = ComponentId.Get<T>();
        if (mod.Sets!.TryGetValue(id, out var existing))
        {
            setters.Overwrite(existing, value);
        }
        else
        {
            var index = setters.Add(value);
            mod.Sets!.Add(id, index);
        }

        // Check if this is a phantom component being added
        if (id.IsPhantomComponent)
        {
            maybeAddingPhantomComponent.Add(entity);
        }

        // Remove it from the "remove" set. In case it was previously removed
        mod.Removes?.Remove(id);
    }

    private EntityModificationData GetModificationData(Entity entity, bool ensureSet, bool ensureRemove)
    {
        // Add it if it's missing
        if (!entityModifications.TryGetValue(entity, out var existing))
        {
            var mod = new EntityModificationData(
                ensureSet ? Pool<Dictionary<ComponentId, ComponentSetterCollection.SetterId>>.Get() : null,
                ensureRemove ? Pool<OrderedListSet<ComponentId>>.Get() : null
            );
            mod.Sets?.Clear();
            mod.Removes?.Clear();

            entityModifications.Add(entity, mod);

            return mod;
        }
        else
        {
            // Found it, now modify it (if necessary)
            var mod = existing;

            var overwrite = false;
            if (mod.Sets == null && ensureSet)
            {
                mod.Sets = Pool<Dictionary<ComponentId, ComponentSetterCollection.SetterId>>.Get();
                overwrite = true;
            }

            if (mod.Removes == null && ensureRemove)
            {
                mod.Removes = Pool<OrderedListSet<ComponentId>>.Get();
                overwrite = true;
            }

            if (overwrite)
            {
                entityModifications[entity] = mod;
            }

            return mod;
        }
    }

    /// <summary>
    /// Data about a new entity being created
    /// </summary>
    private record struct BufferedEntityData
    {
        /// <summary>ID of this buffered entity, used in resolver to get actual entity</summary>
        public uint Id { get; }

        /// <summary>All setters to be run on this entity</summary>
        public Dictionary<ComponentId, ComponentSetterCollection.SetterId> Setters { get; }

        /// <summary>The "Node ID" of this entity, all buffered entities with the same node ID are in the same archetype (except -1)</summary>
        public int ArchetypeKey { get; set; }

        /// <summary>
        /// Data about a new entity being created
        /// </summary>
        /// <param name="id">ID of this buffered entity, used in resolver to get actual entity</param>
        /// <param name="setters">All setters to be run on this entity</param>
        public BufferedEntityData(uint id, Dictionary<ComponentId, ComponentSetterCollection.SetterId> setters)
        {
            Id = id;
            Setters = setters;
        }
    }

    private record struct EntityModificationData(Dictionary<ComponentId, ComponentSetterCollection.SetterId>? Sets, OrderedListSet<ComponentId>? Removes);
}
