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

        public readonly ComponentSetterCollection Setters = new();

        public CommandState() {}

        public void Clear(EcsWorld world)
        {
            EntitiesToDestroy.Clear();
            ArchetypesToDestroy.Clear();
            EntityStates.Clear();
            Actions.Clear();

            Setters.Clear();
        }
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
        /// <summary>
        /// Component ID of the component being overwritten.
        /// </summary>
        internal readonly ComponentId ComponentId;

        /// <summary>
        /// Index of the value in the values list.
        /// </summary>
        internal readonly int Index;

        internal SetterId(ComponentId componentId, int index)
        {
            ComponentId = componentId;
            Index = index;
        }
    }

    /// <summary>
    /// Contains values of components to be set onto entities.
    /// Involves no boxing.
    /// </summary>
    private class ComponentSetterCollection
    {
        private readonly Dictionary<ComponentId, IComponentList> components = [];

        /// <summary>
        /// Add a new component value to the collection.
        /// </summary>
        public SetterId Create<T>(T value) where T : IComponent
        {
            var id = ComponentId.Get<T>();

            if (!components.TryGetValue(id, out var list))
            {
                list = SimplePool<ComponentList<T>>.Acquire();
                components.Add(id, list);
            }

            var index = ((ComponentList<T>)list).Create(value);
            return new SetterId(id, index);
        }

        /// <summary>
        /// Replace an existing component value.
        /// </summary>
        public void Replace<T>(T value, SetterId existing) where T : IComponent
        {
            ((ComponentList<T>)components[existing.ComponentId]).Replace(value, existing.Index);;
        }

        /// <summary>
        /// Output the stored component value to the specified entity location.
        /// </summary>
        public void Write(SetterId id, EntityLocation location)
        {
            var list = components[id.ComponentId];
            list.Write(id.Index, location);
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
