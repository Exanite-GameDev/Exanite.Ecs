using Myriad.Ecs.Collections;
using Myriad.Ecs.Components;

namespace Myriad.Ecs.Utilities;

internal static class CollectionUtility
{
    public static ComponentBloomFilter ToBloomFilter(this ImmutableOrderedListSet<ComponentId> set)
    {
        var filter = new ComponentBloomFilter();
        foreach (var item in set)
        {
            filter.Add(item);
        }

        return filter;
    }
}
