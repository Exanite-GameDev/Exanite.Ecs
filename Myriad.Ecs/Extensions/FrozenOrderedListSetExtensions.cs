using Myriad.Ecs.Collections;
using Myriad.Ecs.IDs;

namespace Myriad.Ecs.Extensions;

internal static class FrozenOrderedListSetExtensions
{
    public static ComponentBloomFilter ToBloomFilter(this FrozenOrderedListSet<ComponentID> set)
    {
        var filter = new ComponentBloomFilter();
        foreach (var item in set)
            filter.Add(item);
        return filter;
    }
}
