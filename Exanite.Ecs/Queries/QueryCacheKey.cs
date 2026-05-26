using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Queries;

internal record struct QueryCacheKey(
    IReadOnlyOrderedListSet<TypeId> IncludeFilter,
    IReadOnlyOrderedListSet<TypeId> ExcludeFilter,
    IReadOnlyOrderedListSet<TypeId> AtLeastOneFilter,
    IReadOnlyOrderedListSet<TypeId> ExactlyOneFilter,
    IReadOnlyOrderedListSet<TypeId> NotAllFilter);
