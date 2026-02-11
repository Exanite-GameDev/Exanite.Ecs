using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Queries;

internal record struct QueryCacheKey(
    ImmutableOrderedListSet<TypeId> IncludeFilter,
    ImmutableOrderedListSet<TypeId> ExcludeFilter,
    ImmutableOrderedListSet<TypeId> AtLeastOneFilter,
    ImmutableOrderedListSet<TypeId> ExactlyOneFilter,
    ImmutableOrderedListSet<TypeId> NotAllFilter);
