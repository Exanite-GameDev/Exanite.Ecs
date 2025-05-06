using System.Collections.Generic;
using System.Diagnostics;
using Myriad.Ecs.Allocations;
using Myriad.Ecs.ComponentIds;
using Myriad.Ecs.Worlds.Archetypes;

namespace Myriad.Ecs.Command;

/// <summary>
/// Collection of components of all different types, keyed by ComponentId, involves no boxing
/// </summary>
internal class ComponentSetterCollection
{
    private readonly Dictionary<ComponentId, IComponentList> components = [];

    public void Clear()
    {
        foreach (var (_, value) in components)
            value.Recycle();
        components.Clear();
    }

    public void ClearAndDispose(ref LazyCommandBuffer buffer)
    {
        Clear();
    }

    public SetterId Add<T>(T value)
        where T : IComponent
    {
        var id = ComponentId.Get<T>();

        if (!components.TryGetValue(id, out var list))
        {
            list = Pool<GenericComponentList<T>>.Get();
            list.Clear();
            components.Add(id, list);
        }

        var idx = ((GenericComponentList<T>)list).Add(value);
        return new SetterId(id, idx);
    }

    public void Overwrite<T>(SetterId index, T value) where T : IComponent
    {
        var id = ComponentId.Get<T>();
        ((GenericComponentList<T>)components[id]).Overwrite(index, value);
    }

    public void Write(SetterId id, Row row)
    {
        var list = components[id.Id];
        list.Write(id.Index, row);
    }

    public readonly struct SetterId
    {
        /// <summary>
        /// Component ID of the component being overwritten
        /// </summary>
        internal readonly ComponentId Id;

        /// <summary>
        /// Index of the setter in the setters list
        /// </summary>
        internal readonly int Index;

        internal SetterId(ComponentId id, int idx)
        {
            Id = id;
            Index = idx;
        }
    }

    #region component list
    private interface IComponentList
    {
        public void Clear();

        void Recycle();

        void Write(int index, Row dest);
    }

    [DebuggerDisplay("Count = {values.Count}")]
    private class GenericComponentList<T> : IComponentList where T : IComponent
    {
        private readonly List<T> values = [];

        public void Clear()
        {
            values.Clear();
        }

        public int Add(T value)
        {
            values.Add(value);
            return values.Count - 1;
        }

        public void Overwrite(SetterId index, T value)
        {
            values[index.Index] = value;
        }

        public void Recycle()
        {
            Pool.Return(this);
        }

        public void Write(int index, Row dest)
        {
            dest.GetMutable<T>() = values[index];
        }
    }
    #endregion
}
