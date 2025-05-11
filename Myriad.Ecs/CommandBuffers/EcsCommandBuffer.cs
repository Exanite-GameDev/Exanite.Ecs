using System;
using System.Buffers;
using System.Collections.Generic;
using Exanite.Core.Pooling;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Queries;
using Exanite.Myriad.Ecs.Worlds;
using Exanite.Myriad.Ecs.Worlds.Archetypes;

namespace Exanite.Myriad.Ecs.CommandBuffers;

/// <summary>
/// Buffers up modifications to entities and replays them all at once.
/// </summary>
public sealed partial class EcsCommandBuffer
{
    internal uint Version { get; private set; }

    /// <summary>
    /// The <see cref="World"/> this <see cref="EcsCommandBuffer"/> is modifying.
    /// </summary>
    public World World { get; }

    public bool HasBufferedOperations { get; private set; }

    /// <summary>
    /// Collection of all components to be set onto entities.
    /// </summary>
    private readonly ComponentSetterCollection setters = new();

    private readonly List<BufferedEntityData> bufferedEntities = [];

    private readonly Dictionary<Entity, EntityModificationData> entityModifications = [];

    private readonly List<Entity> destroys = [];
    private readonly List<QueryDescription> queryDestroys = [];

    /// <summary>
    /// Stores an entity's component set after structural changes.
    /// <para/>
    /// This is used by <see cref="ApplyStructuralChanges"/> to figure out which archetype to move an entity to when components are added/removed.
    /// </summary>
    /// <remarks>
    /// Stores temporary data. Clear before use.
    /// </remarks>
    private readonly OrderedListSet<ComponentId> tempComponentsAfterMove = [];

    private EcsCommandBufferResolver nextResolver;

    /// <summary>
    /// Create a new <see cref="EcsCommandBuffer"/> for the given <see cref="World"/>.
    /// </summary>
    public EcsCommandBuffer(World world)
    {
        World = world;

        nextResolver = SimplePool<EcsCommandBufferResolver>.Acquire();
        nextResolver.Configure(this);
    }

    #region Clear

    /// <summary>
    /// Clear this <see cref="EcsCommandBuffer"/>.
    /// </summary>
    public void Clear()
    {
        if (!HasBufferedOperations)
        {
            return;
        }

        setters.Clear();

        foreach (var bufferedEntity in bufferedEntities)
        {
            var entitySetters = bufferedEntity.Setters;
            entitySetters.Clear();
            SimplePool.Release(entitySetters);
        }
        bufferedEntities.Clear();

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

        archetypeEdges.Clear();

        destroys.Clear();
        queryDestroys.Clear();

        HasBufferedOperations = false;

        nextResolver.Dispose();

        // Update version and get new resolver
        unchecked { Version++; }
        nextResolver = SimplePool<EcsCommandBufferResolver>.Acquire();
        nextResolver.Configure(this);
    }

    #endregion

    #region Playback

    /// <summary>
    /// Apply all buffered operations to the <see cref="World"/>.
    /// </summary>
    public EcsCommandBufferResolver Execute()
    {
        // Use this resolver for this execution
        var resolver = nextResolver;
        if (!HasBufferedOperations)
        {
            // Update version and get new resolver
            unchecked { Version++; }
            nextResolver = SimplePool<EcsCommandBufferResolver>.Acquire();
            nextResolver.Configure(this);

            return resolver;
        }

        // Create buffered entities.
        CreateBufferedEntities(resolver);

        // Destroy entities, this must occur before structural changes because it may trigger new structural changes
        // by adding a new phantom component.
        DestroyEntities();

        // Structural changes (add/remove components)
        ApplyStructuralChanges();

        // Clear all temporary state
        setters.Clear();
        entityModifications.Clear();
        archetypeEdges.Clear();

        HasBufferedOperations = false;

        // Update version and get new resolver
        unchecked { Version++; }
        nextResolver = SimplePool<EcsCommandBufferResolver>.Acquire();
        nextResolver.Configure(this);

        // Return the resolver
        return resolver;
    }

    private void DestroyEntities()
    {
        foreach (var query in queryDestroys)
        {
            foreach (var archetype in query.GetArchetypes())
            {
                if (archetype.EntityCount == 0)
                {
                    continue;
                }

                World.DestroyImmediate(archetype);
            }
        }
        queryDestroys.Clear();

        foreach (var entity in destroys)
        {
            // Skip destroyed entities
            if (!entity.Exists())
            {
                continue;
            }

            // Destroy entity
            World.DestroyImmediate(entity.EntityId);

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

    private void ApplyStructuralChanges()
    {
        if (entityModifications.Count > 0)
        {
            // Calculate the new archetype for the entity
            foreach (var (entity, modification) in entityModifications)
            {
                var archetypeBeforeMove = World.GetArchetype(entity.EntityId);

                // Initialize componentsAfterMove with the components the entity currently has
                var componentsAfterMove = tempComponentsAfterMove;
                componentsAfterMove.Clear();
                componentsAfterMove.UnionWith(archetypeBeforeMove.Components);

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
                EntityStorageLocation location;
                if (moveRequired)
                {
                    // Raise component removed events
                    foreach (var componentId in archetypeBeforeMove.Components)
                    {
                        if (!componentsAfterMove.Contains(componentId))
                        {
                            archetypeBeforeMove.ComponentEventDispatcherByComponentId[componentId.Value].RaiseComponentRemoved(World, entity);
                        }
                    }

                    // Get the new archetype we're moving to
                    var dstArchetype = World.GetOrCreateArchetype(componentsAfterMove, hash);

                    // Migrate the entity across
                    location = World.MigrateEntity(entity.EntityId, dstArchetype);
                }
                else
                {
                    location = World.GetEntityStorageLocation(entity.EntityId);
                }

                // Run all setters
                if (modification.Sets != null)
                {
                    foreach (var set in modification.Sets.Values)
                    {
                        setters.Write(set, location);
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

    private void CreateBufferedEntities(EcsCommandBufferResolver resolver)
    {
        // Keep a map from archetype key -> archetype.
        // This means we only need to calculate it once per archetype key.
        var archetypeLookup = ArrayPool<Archetype>.Shared.Rent(archetypeEdges.Count + 1);
        Array.Clear(archetypeLookup, 0, archetypeLookup.Length);
        try
        {
            foreach (var bufferedData in bufferedEntities)
            {
                var components = bufferedData.Setters;

                var archetype = GetArchetype(bufferedData, archetypeLookup);

                var location = archetype.CreateEntity();

                // Store the new ID in the resolver so it can be retrieved later
                resolver.Lookup.Add(bufferedData.Id, location.Entity);

                // Write the components into the entity
                foreach (var setter in components.Values)
                {
                    // Write component
                    setters.Write(setter, location);

                    // Raise component added events
                    var eventDispatcher = archetype.ComponentEventDispatcherByComponentId[setter.Id.Value];
                    eventDispatcher.RaiseComponentAdded(World, location.Entity.ToEntity(World));
                }

                // Recycle
                components.Clear();
                SimplePool.Release(components);
            }

            bufferedEntities.Clear();
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
        var set = SimplePool<Dictionary<ComponentId, ComponentSetterCollection.SetterId>>.Acquire();
        set.Clear();

        // Store this entity in the collection of entities
        // Put it in aggregate node 0 (i.e. no components)
        var id = (uint)bufferedEntities.Count;
        bufferedEntities.Add(new BufferedEntityData(id, set));

        return new BufferedEntity(id, this, nextResolver);
    }

    /// <summary>
    /// Add or overwrite a component attached to an entity.
    /// </summary>
    public EcsCommandBuffer Set<T>(Entity entity, T value) where T : IComponent
    {
        HasBufferedOperations = true;

        SetInternal(entity, value);

        return this;
    }

    /// <summary>
    /// Remove a component attached to an entity.
    /// </summary>
    public EcsCommandBuffer Remove<T>(Entity entity) where T : IComponent
    {
        HasBufferedOperations = true;

        var mod = GetModificationData(entity, false, true);

        // Add a remover to the list
        var id = ComponentId.Get<T>();
        mod.Removes!.Add(id);

        // Remove it from the setters, if it's there
        mod.Sets?.Remove(id);

        return this;
    }

    /// <summary>
    /// Destroy an entity.
    /// </summary>
    public EcsCommandBuffer Destroy(Entity entity)
    {
        HasBufferedOperations = true;

        destroys.Add(entity);

        return this;
    }

    /// <summary>
    /// Bulk destroy entities.
    /// </summary>
    public EcsCommandBuffer Destroy(List<Entity> entities)
    {
        HasBufferedOperations = true;

        destroys.AddRange(entities);

        return this;
    }

    /// <summary>
    /// Bulk destroy all entities which match the given query.
    /// </summary>
    public EcsCommandBuffer Destroy(QueryDescription entities)
    {
        if (entities.World != World)
        {
            throw new ArgumentException("Cannot use QueryDescription from one World with CommandBuffer for another World");
        }

        HasBufferedOperations = true;

        queryDestroys.Add(entities);

        return this;
    }

    internal void SetBuffered<T>(uint id, T value) where T : IComponent
    {
        AssertUtility.IsTrue(id < bufferedEntities.Count, "Unknown entity ID in SetBuffered");

        var bufferedEntity = bufferedEntities[(int)id];
        var entitySetters = bufferedEntity.Setters;

        var key = ComponentId.Get<T>();

        if (entitySetters.TryGetValue(key, out var existing))
        {
            setters.Overwrite(existing, value);
        }
        else
        {
            // Add to global collection of setters
            var setterIndex = setters.Add(value);

            // Store the index in the per-entity collection
            entitySetters.Add(key, setterIndex);

            // Update node id. Skip it if it's in node -1, once an entity is
            // marked as node -1 it's been opted out of aggregation.
            if (bufferedEntity.ArchetypeKey != -1)
            {
                bufferedEntity.ArchetypeKey = GetArchetypeKey(bufferedEntity.ArchetypeKey, key);
                bufferedEntities[(int)id] = bufferedEntity;
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

        // Remove it from the "remove" set. In case it was previously removed
        mod.Removes?.Remove(id);
    }

    private EntityModificationData GetModificationData(Entity entity, bool ensureSet, bool ensureRemove)
    {
        // Add it if it's missing
        if (!entityModifications.TryGetValue(entity, out var existing))
        {
            var mod = new EntityModificationData(
                ensureSet ? SimplePool<Dictionary<ComponentId, ComponentSetterCollection.SetterId>>.Acquire() : null,
                ensureRemove ? SimplePool<OrderedListSet<ComponentId>>.Acquire() : null
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
                mod.Sets = SimplePool<Dictionary<ComponentId, ComponentSetterCollection.SetterId>>.Acquire();
                overwrite = true;
            }

            if (mod.Removes == null && ensureRemove)
            {
                mod.Removes = SimplePool<OrderedListSet<ComponentId>>.Acquire();
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
    /// Data about a new entity being created.
    /// </summary>
    private record struct BufferedEntityData
    {
        /// <summary>
        /// ID of this buffered entity, used in resolver to get actual entity.
        /// </summary>
        public uint Id { get; }

        /// <summary>
        /// All setters to be run on this entity.
        /// </summary>
        public Dictionary<ComponentId, ComponentSetterCollection.SetterId> Setters { get; }

        /// <summary>
        /// The "Node ID" of this entity, all buffered entities with the same node ID are in the same archetype (except -1).
        /// </summary>
        public int ArchetypeKey { get; set; }

        /// <summary>
        /// Data about a new entity being created.
        /// </summary>
        /// <param name="id">ID of this buffered entity, used in resolver to get actual entity.</param>
        /// <param name="setters">All setters to be run on this entity.</param>
        public BufferedEntityData(uint id, Dictionary<ComponentId, ComponentSetterCollection.SetterId> setters)
        {
            Id = id;
            Setters = setters;
        }
    }

    private record struct EntityModificationData(Dictionary<ComponentId, ComponentSetterCollection.SetterId>? Sets, OrderedListSet<ComponentId>? Removes);
}
