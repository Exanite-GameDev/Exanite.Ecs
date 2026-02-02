using System;
using System.Collections.Generic;
using Exanite.Core.Pooling;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.CommandBuffers;

public partial class EcsCommandBuffer
{
    public readonly struct SetterId
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
    /// Contains collections of components to be set onto entities.
    /// Involves no boxing.
    /// </summary>
    internal class ComponentSetterCollection
    {
        private readonly Dictionary<ComponentId, IComponentList> components = [];

        /// <summary>
        /// Add a new component value to the collection.
        /// </summary>
        public SetterId Add<T>(T value) where T : IComponent
        {
            var id = ComponentId.Get<T>();

            if (!components.TryGetValue(id, out var list))
            {
                list = SimplePool<ComponentList<T>>.Acquire();
                components.Add(id, list);
            }

            var index = ((ComponentList<T>)list).Add(value);
            return new SetterId(id, index);
        }

        /// <summary>
        /// Overwrite an existing component value in the collection.
        /// </summary>
        public void Overwrite<T>(SetterId index, T value) where T : IComponent
        {
            var id = ComponentId.Get<T>();
            ((ComponentList<T>)components[id]).Overwrite(index, value);
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
        /// <param name="disposeComponents">
        /// Should the components be disposed?
        /// <para/>
        /// Recommendation is to set this to true if the components were not "moved" elsewhere.
        /// For example, if the components were never written using <see cref="Write"/>.
        /// </param>
        public void Clear(bool disposeComponents)
        {
            foreach (var (_, value) in components)
            {
                value.Recycle(disposeComponents);
            }

            components.Clear();
        }

        private interface IComponentList
        {
            public void Write(int index, EntityLocation location);
            public void Recycle(bool disposeComponents);
        }

        private class ComponentList<T> : IComponentList where T : IComponent
        {
            private readonly List<T> values = [];

            public int Add(T value)
            {
                values.Add(value);
                return values.Count - 1;
            }

            public void Overwrite(SetterId index, T value)
            {
                if (value is IDisposable)
                {
                    ((IDisposable)values[index.Index]).Dispose();
                }

                values[index.Index] = value;
            }

            public void Recycle(bool disposeComponents)
            {
                if (disposeComponents && values.Count > 0 && values[0] is IDisposable)
                {
                    foreach (var component in values)
                    {
                        ((IDisposable)component).Dispose();
                    }
                }

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
