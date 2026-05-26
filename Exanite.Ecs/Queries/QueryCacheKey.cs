using Exanite.Ecs.Collections;
using Exanite.Ecs.Components;

namespace Exanite.Ecs.Queries;

internal record struct QueryCacheKey(
    IReadOnlyOrderedListSet<TypeId> IncludeFilter,
    IReadOnlyOrderedListSet<TypeId> ExcludeFilter,
    IReadOnlyOrderedListSet<TypeId> AtLeastOneFilter,
    IReadOnlyOrderedListSet<TypeId> ExactlyOneFilter,
    IReadOnlyOrderedListSet<TypeId> NotAllFilter);
