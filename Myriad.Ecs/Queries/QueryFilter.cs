using System;
using System.Collections.Generic;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Queries;

/// <summary>
/// Build a new <see cref="QueryView"/> object
/// </summary>
public sealed class QueryFilter
{
    /// <summary>
    /// An Entity must include all of these components to be matched by this query.
    /// </summary>
    public IReadOnlyList<TypeId> IncludeFilter => includeFilter.Items;
    private readonly TypeIdSet includeFilter;

    /// <summary>
    /// Entities with these components will not be matched by this query.
    /// </summary>
    public IReadOnlyList<TypeId> ExcludeFilter => excludeFilter.Items;
    private readonly TypeIdSet excludeFilter;

    /// <summary>
    /// At least one of all these components must be on an Entity for it to be matched by this query.
    /// </summary>
    public IReadOnlyList<TypeId> AtLeastOneFiler => atLeastOneFilter.Items;
    private readonly TypeIdSet atLeastOneFilter;

    /// <summary>
    /// Exactly one of all these components must be on an Entity for it to be matched by this query.
    /// </summary>
    public IReadOnlyList<TypeId> ExactlyOneFilter => exactlyOneFilter.Items;
    private readonly TypeIdSet exactlyOneFilter;

    /// <summary>
    /// Not all of these components must be on an Entity for it to be matched by this query.
    /// </summary>
    public IReadOnlyList<TypeId> NotAllFilter => notAllFilter.Items;
    private readonly TypeIdSet notAllFilter;

    /// <summary>
    /// Create a new <see cref="QueryFilter"/>
    /// </summary>
    public QueryFilter()
    {
        includeFilter = new TypeIdSet();
        excludeFilter = new TypeIdSet();
        atLeastOneFilter = new TypeIdSet();
        exactlyOneFilter = new TypeIdSet();
        notAllFilter = new TypeIdSet();
    }

    /// <summary>
    /// Build a <see cref="QueryView"/> from the current state of this filter.
    /// </summary>
    public QueryView Build(EcsWorld world)
    {
        var key = new QueryCacheKey(
            includeFilter.ToImmutableSet(),
            excludeFilter.ToImmutableSet(),
            atLeastOneFilter.ToImmutableSet(),
            exactlyOneFilter.ToImmutableSet(),
            notAllFilter.ToImmutableSet());

        using (world.QueryViewCacheLock.EnterScope())
        {
            if (!world.QueryViewCache.TryGetValue(key, out var query))
            {
                world.QueryViewCache[key] = query = new QueryView(
                    world,
                    includeFilter.ToImmutableSet(),
                    excludeFilter.ToImmutableSet(),
                    atLeastOneFilter.ToImmutableSet(),
                    exactlyOneFilter.ToImmutableSet(),
                    notAllFilter.ToImmutableSet()
                );
            }

            return query;
        }
    }

    /// <summary>
    /// The given component must exist for an entity to be matched by this query.
    /// </summary>
    public QueryFilter Include<T>() where T : IComponent
    {
        includeFilter.Add<T>();
        return this;
    }

    /// <summary>
    /// The given component must exist for an entity to be matched by this query.
    /// </summary>
    public QueryFilter Include(Type type)
    {
        includeFilter.Add(type);
        return this;
    }

    /// <summary>
    /// The given component must exist for an entity to be matched by this query.
    /// </summary>
    public QueryFilter Include(TypeId id)
    {
        includeFilter.Add(id);
        return this;
    }

    /// <summary>
    /// The given component must not exist for an entity to be matched by this query.
    /// </summary>
    public QueryFilter Exclude<T>() where T : IComponent
    {
        excludeFilter.Add<T>();
        return this;
    }

    /// <summary>
    /// The given component must not exist for an entity to be matched by this query.
    /// </summary>
    public QueryFilter Exclude(Type type)
    {
        excludeFilter.Add(type);
        return this;
    }

    /// <summary>
    /// The given component must not exist for an entity to be matched by this query.
    /// </summary>
    public QueryFilter Exclude(TypeId id)
    {
        excludeFilter.Add(id);
        return this;
    }

    /// <summary>
    /// At least one of all components specified as AtLeastOne must exist for an entity to be matched by this query.
    /// </summary>
    public QueryFilter AtLeastOne<T>() where T : IComponent
    {
        atLeastOneFilter.Add<T>();
        return this;
    }

    /// <summary>
    /// At least one of all components specified as AtLeastOne must exist for an entity to be matched by this query.
    /// </summary>
    public QueryFilter AtLeastOne(Type type)
    {
        atLeastOneFilter.Add(type);
        return this;
    }

    /// <summary>
    /// At least one of all components specified as AtLeastOne must exist for an entity to be matched by this query.
    /// </summary>
    public QueryFilter AtLeastOne(TypeId id)
    {
        atLeastOneFilter.Add(id);
        return this;
    }

    /// <summary>
    /// Exactly one of all components specified as ExactlyOne must exist for an entity to be matched by this query.
    /// </summary>
    public QueryFilter ExactlyOne<T>() where T : IComponent
    {
        exactlyOneFilter.Add<T>();
        return this;
    }

    /// <summary>
    /// Exactly one of all components specified as ExactlyOne must exist for an entity to be matched by this query.
    /// </summary>
    public QueryFilter ExactlyOne(Type type)
    {
        exactlyOneFilter.Add(type);
        return this;
    }

    /// <summary>
    /// Exactly one of all components specified as ExactlyOne must exist for an entity to be matched by this query.
    /// </summary>
    public QueryFilter ExactlyOne(TypeId id)
    {
        exactlyOneFilter.Add(id);
        return this;
    }

    /// <summary>
    /// Not all of the components specified as NotAll must exist for an entity to be matched by this query.
    /// </summary>
    public QueryFilter NotAll<T>() where T : IComponent
    {
        notAllFilter.Add<T>();
        return this;
    }

    /// <summary>
    /// Not all of the components specified as NotAll must exist for an entity to be matched by this query.
    /// </summary>
    public QueryFilter NotAll(Type type)
    {
        notAllFilter.Add(type);
        return this;
    }

    /// <summary>
    /// Not all of the components specified as NotAll must exist for an entity to be matched by this query.
    /// </summary>
    public QueryFilter NotAll(TypeId id)
    {
        notAllFilter.Add(id);
        return this;
    }

    private class TypeIdSet
    {
        public readonly OrderedListSet<TypeId> Items = [];

        private ImmutableOrderedListSet<TypeId>? immutableSet;

        public ImmutableOrderedListSet<TypeId> ToImmutableSet()
        {
            if (immutableSet != null)
            {
                return immutableSet;
            }

            immutableSet = Items.Count == 0 ? ImmutableOrderedListSet<TypeId>.Empty : ImmutableOrderedListSet<TypeId>.Create(Items);
            return immutableSet;
        }

        public void Add(TypeId id)
        {
            if (Items.Add(id))
            {
                immutableSet = null;
            }
        }

        public void Add(Type type)
        {
            Add(TypeId.Get(type));
        }

        public void Add<T>() where T : IComponent
        {
            Add(TypeId.Get<T>());
        }
    }
}
