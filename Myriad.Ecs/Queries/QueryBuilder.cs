using System;
using System.Collections.Generic;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs.Queries;

/// <summary>
/// Build a new <see cref="QueryDescription"/> object
/// </summary>
public sealed class QueryBuilder
{
    /// <summary>
    /// An Entity must include all of these components to be matched by this query.
    /// </summary>
    public IReadOnlyList<ComponentId> Included => include.Items;
    private readonly ComponentSet include;

    /// <summary>
    /// Entities with these components will not be matched by this query.
    /// </summary>
    public IReadOnlyList<ComponentId> Excluded => exclude.Items;
    private readonly ComponentSet exclude;

    /// <summary>
    /// At least one of all these components must be on an Entity for it to be matched by this query.
    /// </summary>
    public IReadOnlyList<ComponentId> AtLeastOnes => atLeastOne.Items;
    private readonly ComponentSet atLeastOne;

    /// <summary>
    /// Exactly one of all these components must be on an Entity for it to be matched by this query.
    /// </summary>
    public IReadOnlyList<ComponentId> ExactlyOnes => exactlyOne.Items;
    private readonly ComponentSet exactlyOne;

    /// <summary>
    /// Not all of these components must be on an Entity for it to be matched by this query.
    /// </summary>
    public IReadOnlyList<ComponentId> NotAlls => notAll.Items;
    private readonly ComponentSet notAll;

    /// <summary>
    /// Create a new <see cref="QueryBuilder"/>
    /// </summary>
    public QueryBuilder()
    {
        include = new ComponentSet();
        exclude = new ComponentSet();
        atLeastOne = new ComponentSet();
        exactlyOne = new ComponentSet();
        notAll = new ComponentSet();
    }

    /// <summary>
    /// Build a <see cref="QueryDescription"/> from the current state of this builder.
    /// </summary>
    public QueryDescription Build(EcsWorld world)
    {
        return new QueryDescription(
            world,
            include.ToImmutableSet(),
            exclude.ToImmutableSet(),
            atLeastOne.ToImmutableSet(),
            exactlyOne.ToImmutableSet(),
            notAll.ToImmutableSet()
        );
    }

    /// <summary>
    /// The given component must exist for an entity to be matched by this query.
    /// </summary>
    public QueryBuilder Include<T>() where T : IComponent
    {
        include.Add<T>();
        return this;
    }

    /// <summary>
    /// The given component must exist for an entity to be matched by this query.
    /// </summary>
    public QueryBuilder Include(Type type)
    {
        include.Add(type);
        return this;
    }

    /// <summary>
    /// The given component must exist for an entity to be matched by this query.
    /// </summary>
    public QueryBuilder Include(ComponentId id)
    {
        include.Add(id);
        return this;
    }

    /// <summary>
    /// The given component must not exist for an entity to be matched by this query.
    /// </summary>
    public QueryBuilder Exclude<T>() where T : IComponent
    {
        exclude.Add<T>();
        return this;
    }

    /// <summary>
    /// The given component must not exist for an entity to be matched by this query.
    /// </summary>
    public QueryBuilder Exclude(Type type)
    {
        exclude.Add(type);
        return this;
    }

    /// <summary>
    /// The given component must not exist for an entity to be matched by this query.
    /// </summary>
    public QueryBuilder Exclude(ComponentId id)
    {
        exclude.Add(id);
        return this;
    }

    /// <summary>
    /// At least one of all components specified as AtLeastOne must exist for an entity to be matched by this query.
    /// </summary>
    public QueryBuilder AtLeastOne<T>() where T : IComponent
    {
        atLeastOne.Add<T>();
        return this;
    }

    /// <summary>
    /// At least one of all components specified as AtLeastOne must exist for an entity to be matched by this query.
    /// </summary>
    public QueryBuilder AtLeastOne(Type type)
    {
        atLeastOne.Add(type);
        return this;
    }

    /// <summary>
    /// At least one of all components specified as AtLeastOne must exist for an entity to be matched by this query.
    /// </summary>
    public QueryBuilder AtLeastOne(ComponentId id)
    {
        atLeastOne.Add(id);
        return this;
    }

    /// <summary>
    /// Exactly one of all components specified as ExactlyOne must exist for an entity to be matched by this query.
    /// </summary>
    public QueryBuilder ExactlyOne<T>() where T : IComponent
    {
        exactlyOne.Add<T>();
        return this;
    }

    /// <summary>
    /// Exactly one of all components specified as ExactlyOne must exist for an entity to be matched by this query.
    /// </summary>
    public QueryBuilder ExactlyOne(Type type)
    {
        exactlyOne.Add(type);
        return this;
    }

    /// <summary>
    /// Exactly one of all components specified as ExactlyOne must exist for an entity to be matched by this query.
    /// </summary>
    public QueryBuilder ExactlyOne(ComponentId id)
    {
        exactlyOne.Add(id);
        return this;
    }

    /// <summary>
    /// Not all of the components specified as NotAll must exist for an entity to be matched by this query.
    /// </summary>
    public QueryBuilder NotAll<T>() where T : IComponent
    {
        notAll.Add<T>();
        return this;
    }

    /// <summary>
    /// Not all of the components specified as NotAll must exist for an entity to be matched by this query.
    /// </summary>
    public QueryBuilder NotAll(Type type)
    {
        notAll.Add(type);
        return this;
    }

    /// <summary>
    /// Not all of the components specified as NotAll must exist for an entity to be matched by this query.
    /// </summary>
    public QueryBuilder NotAll(ComponentId id)
    {
        notAll.Add(id);
        return this;
    }

    private class ComponentSet
    {
        public readonly OrderedListSet<ComponentId> Items = [];

        private ImmutableOrderedListSet<ComponentId>? immutableSet;

        public ImmutableOrderedListSet<ComponentId> ToImmutableSet()
        {
            if (immutableSet != null)
            {
                return immutableSet;
            }

            immutableSet = Items.Count == 0 ? ImmutableOrderedListSet<ComponentId>.Empty : ImmutableOrderedListSet<ComponentId>.Create(Items);
            return immutableSet;
        }

        public void Add(ComponentId id)
        {
            if (Items.Add(id))
            {
                immutableSet = null;
            }
        }

        public void Add(Type type)
        {
            Add(ComponentId.Get(type));
        }

        public void Add<T>() where T : IComponent
        {
            Add(ComponentId.Get<T>());
        }
    }
}
