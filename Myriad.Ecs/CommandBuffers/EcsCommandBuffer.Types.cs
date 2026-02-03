using System;
using System.Collections.Generic;
using Exanite.Core.Pooling;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.CommandBuffers;

public partial class EcsCommandBuffer
{
    private readonly struct CommandState
    {
        public readonly List<EntityId> EntitiesToDestroy = [];
        public readonly List<Archetype> ArchetypesToDestroy = [];
        public readonly Dictionary<EntityId, EntityState> EntityStates = [];
        public readonly List<Action> Actions = [];

        public readonly PrefabEntityToNewEntityLookup Lookup = new();
        public readonly ComponentSetterCollection Setters = new();

        public CommandState() {}

        public void Clear()
        {
            EntitiesToDestroy.Clear();
            ArchetypesToDestroy.Clear();
            EntityStates.Clear();
            Actions.Clear();

            Lookup.Clear();
            Setters.Clear();
        }
    }

    private class PrefabEntityToNewEntityLookup : IEntityLookup
    {
        private readonly Dictionary<LookupKey, Entity> primaries = [];
        private readonly Dictionary<Entity, Entity> secondaries = [];

        private EntityId currentEntity;

        public void Add(Entity prefab, Entity newEntity)
        {
            primaries.Add(new LookupKey(prefab, newEntity.EntityId), newEntity);
            secondaries.Add(prefab, newEntity);
        }

        public void SetCurrentEntity(EntityId entity)
        {
            currentEntity = entity;
        }

        public EcsRef<T> Get<T>(EcsRef<T> reference, EntityLookupPolicy policy = EntityLookupPolicy.PreserveIfNotExist) where T : IComponent
        {
            return new EcsRef<T>(Get(reference.Entity, policy));
        }

        public Entity Get(Entity entity, EntityLookupPolicy policy = EntityLookupPolicy.PreserveIfNotExist)
        {
            if (primaries.TryGetValue(new LookupKey(entity, currentEntity), out var newEntity))
            {
                return newEntity;
            }

            if (secondaries.TryGetValue(entity, out newEntity))
            {
                return newEntity;
            }

            return EntityLookupUtility.HandleLookupPolicy(entity, policy);
        }

        public void Clear()
        {
            primaries.Clear();
            secondaries.Clear();
        }

        private record struct LookupKey(Entity Prefab, EntityId CurrentEntity);
    }

    private struct EntityState
    {
        public bool NeedsCreation;
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

    private readonly struct SetterId
    {
        private readonly int index;

        /// <summary>
        /// Component ID of the component being overwritten.
        /// </summary>
        public readonly ComponentId ComponentId;

        /// <summary>
        /// If <see cref="IsPrefab"/> is false,
        /// then this is the index of the value in the corresponding components list.
        /// </summary>
        public int ValueIndex => index;

        /// <summary>
        /// If <see cref="IsPrefab"/> is true,
        /// then this is the index of the source entity in the prefabs list.
        /// </summary>
        public int PrefabIndex => ~index;

        /// <summary>
        /// Whether this setter is valid or not.
        /// </summary>
        public bool IsValid => ComponentId.Value != 0;

        /// <summary>
        /// Whether the setter reads from a prefab or not.
        /// </summary>
        public bool IsPrefab => index < 0;

        private SetterId(ComponentId componentId, int index)
        {
            ComponentId = componentId;
            this.index = index;
        }

        public static SetterId FromValueIndex(ComponentId componentId, int index)
        {
            return new SetterId(componentId, index);
        }

        public static SetterId FromPrefabIndex(ComponentId componentId, int index)
        {
            return new SetterId(componentId, ~index);
        }
    }

    /// <summary>
    /// Contains values of components to be set onto entities.
    /// Involves no boxing.
    /// </summary>
    private class ComponentSetterCollection
    {
        private readonly Dictionary<ComponentId, IComponentList> components = [];
        private readonly List<Entity> prefabs = [];
        private readonly Dictionary<Entity, int> prefabLookup = [];

        /// <summary>
        /// Creates a setter using a component value.
        /// </summary>
        public void CreateFromValue<T>(T value, ref SetterId setterId) where T : IComponent
        {
            if (setterId.IsValid && !setterId.IsPrefab)
            {
                ((ComponentList<T>)components[setterId.ComponentId]).Replace(value, setterId.ValueIndex);

                return;
            }

            var componentId = ComponentId.Get<T>();
            if (!components.TryGetValue(componentId, out var list))
            {
                components[componentId] = list = SimplePool<ComponentList<T>>.Acquire();
            }

            var index = ((ComponentList<T>)list).Create(value);
            setterId = SetterId.FromValueIndex(componentId, index);
        }

        /// <summary>
        /// Creates a setter using a component stored by a prefab entity.
        /// </summary>
        public void CreateFromPrefab(Entity prefab, ComponentId componentId, ref SetterId setterId)
        {
            var prefabIndex = prefabLookup.GetValueOrDefault(prefab, -1);
            if (prefabIndex == -1)
            {
                prefabIndex = prefabs.Count;
                prefabs.Add(prefab);
                prefabLookup[prefab] = prefabIndex;
            }

            setterId = SetterId.FromPrefabIndex(componentId, prefabIndex);
        }

        /// <summary>
        /// Output the stored component value to the specified entity location.
        /// </summary>
        public void Write(SetterId setterId, EntityLocation dstLocation)
        {
            if (setterId.IsPrefab)
            {
                var srcEntity = prefabs[setterId.PrefabIndex];
                ref var srcLocation = ref srcEntity.World.Entities.GetLocation(srcEntity.EntityId);

                var srcComponents = srcLocation.Chunk.GetComponentArray(setterId.ComponentId);
                var dstComponents = dstLocation.Chunk.GetComponentArray(setterId.ComponentId);

                Array.Copy(srcComponents, srcLocation.IndexInChunk, dstComponents, dstLocation.IndexInChunk, 1);
            }
            else
            {
                var list = components[setterId.ComponentId];
                list.Write(setterId.ValueIndex, dstLocation);
            }
        }

        /// <summary>
        /// Clear all component values stored in this collection.
        /// </summary>
        public void Clear()
        {
            foreach (var (_, value) in components)
            {
                value.Recycle();
            }

            components.Clear();
        }

        private interface IComponentList
        {
            public void Write(int index, EntityLocation location);
            public void Recycle();
        }

        private class ComponentList<T> : IComponentList where T : IComponent
        {
            private readonly List<T> values = [];

            public int Create(T value)
            {
                values.Add(value);
                return values.Count - 1;
            }

            public void Replace(T value, int index)
            {
                values[index] = value;
            }

            public void Recycle()
            {
                values.Clear();
                SimplePool.Release(this);
            }

            public void Write(int index, EntityLocation location)
            {
                location.Chunk.Get<T>(location.IndexInChunk) = values[index];
            }
        }
    }
}
