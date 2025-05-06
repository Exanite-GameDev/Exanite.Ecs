using System.Collections.Generic;
using Myriad.Ecs.Collections;
using Myriad.Ecs.Command;
using Myriad.Ecs.Components;
using Myriad.Ecs.ComponentIds;
using Myriad.Ecs.Worlds.Chunks;

namespace Myriad.Ecs.Worlds.Archetypes;

internal class ArchetypeComponentDisposal
{
    private readonly List<IDisposer> _disposers = [ ];

    public ArchetypeComponentDisposal(FrozenOrderedListSet<ComponentId> components)
    {
        // Get a disposer for each disposable component
        foreach (var component in components)
        {
            if (!component.IsDisposableComponent)
                continue;
            _disposers.Add(Disposer.Get(component));
        }
    }

    public void DisposeEntity(ref LazyCommandBuffer buffer, EntityInfo info)
    {
        DisposeEntity(ref buffer, info.Chunk, info.RowIndex);
    }

    public void DisposeEntity(ref LazyCommandBuffer buffer, Chunk chunk, int rowIndex)
    {
        foreach (var disposer in _disposers)
            disposer.Dispose(chunk, rowIndex, ref buffer);
    }

    /// <summary>
    /// Dispose components which are not in the destination archetype
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="info"></param>
    /// <param name="to"></param>
    public void DisposeRemoved(ref LazyCommandBuffer buffer, EntityInfo info, FrozenOrderedListSet<ComponentId> to)
    {
        foreach (var disposer in _disposers)
            if (!to.Contains(disposer.Component))
                disposer.Dispose(info.Chunk, info.RowIndex, ref buffer);
    }
}
