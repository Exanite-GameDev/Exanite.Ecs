using Myriad.Ecs.Collections;
using Myriad.Ecs.ComponentIds;

namespace Myriad.Ecs.Utilities;

internal static class FrozenOrderedListSetUtility
{
    public static ComponentBloomFilter ToBloomFilter(this FrozenOrderedListSet<ComponentId> set)
    {
        var filter = new ComponentBloomFilter();
        foreach (var item in set)
            filter.Add(item);
        return filter;
    }
}
