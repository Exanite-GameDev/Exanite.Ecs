using System;
using Exanite.Ecs.Components;
using Exanite.Ecs.Worlds;

namespace Exanite.Ecs.Collections;

internal static class ComponentIdSetUtility
{
    public static ArchetypeHash ToArchetypeHash(this ReadOnlySpan<ComponentId> componentIds)
    {
        var hash = new ArchetypeHash();
        foreach (var componentId in componentIds)
        {
            hash = hash.Toggle(componentId);
        }

        return hash;
    }

    public static ComponentBloomFilter ToBloomFilter(this ReadOnlySpan<TypeId> typeIds)
    {
        var filter = new ComponentBloomFilter();
        foreach (var typeId in typeIds)
        {
            filter.Add(typeId);
        }

        return filter;
    }
}
