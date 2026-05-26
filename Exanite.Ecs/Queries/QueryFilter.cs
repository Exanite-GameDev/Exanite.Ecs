using System;
using System.Collections.Generic;
using System.Text;
using Exanite.Core.Pooling;
using Exanite.Core.Utilities;
using Exanite.Ecs.Collections;
using Exanite.Ecs.Components;

namespace Exanite.Ecs.Queries;

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
    public IReadOnlyList<TypeId> AtLeastOneFilter => atLeastOneFilter.Items;
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
    /// Whether this query checks against any interface components.
    /// </summary>
    public bool HasComponents => includeFilter.HasComponents
        || excludeFilter.HasComponents
        || atLeastOneFilter.HasComponents
        || exactlyOneFilter.HasComponents
        || notAllFilter.HasComponents;

    /// <summary>
    /// Whether this query checks against any interface components.
    /// </summary>
    public bool HasInterfaces => includeFilter.HasInterfaces
        || excludeFilter.HasInterfaces
        || atLeastOneFilter.HasInterfaces
        || exactlyOneFilter.HasInterfaces
        || notAllFilter.HasInterfaces;

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
    /// Get or create a cached <see cref="QueryView"/> from the current state of this filter.
    /// </summary>
    /// <remarks>
    /// Calling this multiple times with equivalent filters will return the same view.
    /// </remarks>
    public QueryView Build(EcsWorld world)
    {
        var key = new QueryCacheKey(
            includeFilter.ToReadOnlySet(),
            excludeFilter.ToReadOnlySet(),
            atLeastOneFilter.ToReadOnlySet(),
            exactlyOneFilter.ToReadOnlySet(),
            notAllFilter.ToReadOnlySet());

        return world.QueryViewCache.GetOrAdd(key, CreateQueryView, world);

        static QueryView CreateQueryView(QueryCacheKey key, EcsWorld world)
        {
            return new QueryView(
                world,
                key.IncludeFilter,
                key.ExcludeFilter,
                key.AtLeastOneFilter,
                key.ExactlyOneFilter,
                key.NotAllFilter);
        }
    }

    /// <summary>
    /// The given component must exist for an entity to be matched by this query.
    /// </summary>
    public QueryFilter Include<T>() where T : IEcsType
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
    public QueryFilter Exclude<T>() where T : IEcsType
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
    public QueryFilter AtLeastOne<T>() where T : IEcsType
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
    public QueryFilter ExactlyOne<T>() where T : IEcsType
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
    public QueryFilter NotAll<T>() where T : IEcsType
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

    public override string ToString()
    {
        return ToString(true, true);
    }

    public string ToString(bool includeComponents, bool includeInterfaces)
    {
        using (StringBuilderPool.Acquire(out var builder))
        {
            builder.Append('[');

            var isFirst = true;
            AppendFilter(builder, "Include", includeFilter, ref isFirst, includeComponents, includeInterfaces);
            AppendFilter(builder, "Exclude", excludeFilter, ref isFirst, includeComponents, includeInterfaces);
            AppendFilter(builder, "AtLeastOne", atLeastOneFilter, ref isFirst, includeComponents, includeInterfaces);
            AppendFilter(builder, "ExactlyOne", exactlyOneFilter, ref isFirst, includeComponents, includeInterfaces);
            AppendFilter(builder, "NotAll", notAllFilter, ref isFirst, includeComponents, includeInterfaces);

            builder.Append(']');

            return builder.ToString();
        }
    }

    private static void AppendFilter(StringBuilder builder, string name, TypeIdSet filter, ref bool isFirst, bool includeComponents, bool includeInterfaces)
    {
        if ((!includeComponents || !filter.HasComponents) && (!includeInterfaces || !filter.HasInterfaces))
        {
            return;
        }

        if (!isFirst)
        {
            builder.Append(", ");
        }
        isFirst = false;

        builder.Append(name);
        builder.Append('<');

        var isFirstInFilter = true;
        foreach (var typeId in filter.Items)
        {
            if (typeId.IsComponent && !includeComponents)
            {
                continue;
            }

            if (typeId.IsInterface && !includeInterfaces)
            {
                continue;
            }

            if (!isFirstInFilter)
            {
                builder.Append(", ");
            }
            isFirstInFilter = false;

            builder.Append(TypeUtility.FormatConciseName(typeId.Type));
        }

        builder.Append('>');
    }

    private class TypeIdSet
    {
        public readonly OrderedListSet<TypeId> Items = [];
        public bool HasComponents;
        public bool HasInterfaces;

        private IReadOnlyOrderedListSet<TypeId>? immutableSet;

        public IReadOnlyOrderedListSet<TypeId> ToReadOnlySet()
        {
            if (immutableSet != null)
            {
                return immutableSet;
            }

            immutableSet = Items.Count == 0 ? OrderedListSet<TypeId>.Empty : Items.MakeNewImmutable();
            return immutableSet;
        }

        public void Add<T>() where T : IEcsType
        {
            Add(TypeId.Get<T>());
        }

        public void Add(Type type)
        {
            Add(TypeId.Get(type));
        }

        public void Add(TypeId id)
        {
            if (Items.Add(id))
            {
                HasComponents |= id.IsComponent;
                HasInterfaces |= id.IsInterface;
                immutableSet = null;
            }
        }
    }
}
