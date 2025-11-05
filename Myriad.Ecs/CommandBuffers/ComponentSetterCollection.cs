using System;
using System.Collections.Generic;
using Exanite.Core.Pooling;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.CommandBuffers;

/// <summary>
/// Contains collections of components to be set onto entities.
/// Involves no boxing.
/// </summary>
internal class ComponentSetterCollection
{
    private readonly Dictionary<ComponentId, IComponentList> components = [];

    public void Clear(bool disposeComponents)
    {
        foreach (var (_, value) in components)
        {
            value.Recycle(disposeComponents);
        }

        components.Clear();
    }

    public SetterId Add<T>(T value) where T : IComponent
    {
        var id = ComponentId.Get<T>();

        if (!components.TryGetValue(id, out var list))
        {
            list = SimplePool<GenericComponentList<T>>.Acquire();
            components.Add(id, list);
        }

        var index = ((GenericComponentList<T>)list).Add(value);
        return new SetterId(id, index);
    }

    public void Overwrite<T>(SetterId index, T value) where T : IComponent
    {
        var id = ComponentId.Get<T>();
        ((GenericComponentList<T>)components[id]).Overwrite(index, value);
    }

    public void Write(SetterId id, EntityStorageLocation location)
    {
        var list = components[id.ComponentId];
        list.Write(id.Index, location);
    }

    public readonly struct SetterId
    {
        /// <summary>
        /// Component ID of the component being overwritten.
        /// </summary>
        internal readonly ComponentId ComponentId;

        /// <summary>
        /// Index of the setter in the setters list.
        /// </summary>
        internal readonly int Index;

        internal SetterId(ComponentId componentId, int index)
        {
            ComponentId = componentId;
            Index = index;
        }
    }

    private interface IComponentList
    {
        public void Write(int index, EntityStorageLocation location);
        public void Recycle(bool disposeComponents);
    }

    private class GenericComponentList<T> : IComponentList where T : IComponent
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

        public void Write(int index, EntityStorageLocation location)
        {
            location.GetMutable<T>() = values[index];
        }
    }
}
