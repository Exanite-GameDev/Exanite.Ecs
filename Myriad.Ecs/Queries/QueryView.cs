using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Exanite.Core.Pooling;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs.Queries;

/// <summary>
/// Contains the set of matched archetypes based on a filter described by <see cref="QueryFilter"/>.
/// </summary>
public sealed class QueryView : IArchetypeView
{
    private MatchResult result = new(0, ImmutableOrderedListSet<ArchetypeMatch>.Empty);

    private readonly ComponentBloomFilter includeBloom;
    private readonly ComponentBloomFilter excludeBloom;

    /// <summary>
    /// The <see cref="EcsWorld"/> that this query is for.
    /// </summary>
    public EcsWorld World { get; }

    /// <summary>
    /// The components which must be present on an entity for it to match this query.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> IncludeFilter { get; }

    /// <summary>
    /// The components which must not be present on an entity for it to match this query.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> ExcludeFilter { get; }

    /// <summary>
    /// At least one of these components must be present on an entity for it to match this query.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> AtLeastOneFilter { get; }

    /// <summary>
    /// Exactly one of these components must be present on an entity for it to match this query.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> ExactlyOneFilter { get; }

    /// <summary>
    /// Not all of these components must be present on an entity for it to match this query.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> NotAllFilter { get; }

    /// <summary>
    /// Describes a query for entities, bound to a world.
    /// </summary>
    internal QueryView(
        EcsWorld world,
        ImmutableOrderedListSet<ComponentId> includeFilter,
        ImmutableOrderedListSet<ComponentId> excludeFilter,
        ImmutableOrderedListSet<ComponentId> atLeastOneFilter,
        ImmutableOrderedListSet<ComponentId> exactlyOneFilter,
        ImmutableOrderedListSet<ComponentId> notAllFilter)
    {
        World = world;

        IncludeFilter = includeFilter;
        ExcludeFilter = excludeFilter;
        AtLeastOneFilter = atLeastOneFilter;
        ExactlyOneFilter = exactlyOneFilter;
        NotAllFilter = notAllFilter;

        includeBloom = includeFilter.ToBloomFilter();
        excludeBloom = excludeFilter.ToBloomFilter();
    }

    /// <summary>
    /// Create a <see cref="QueryFilter"/> with the same filter as this query.
    /// </summary>
    public QueryFilter ToFilter()
    {
        var filter = new QueryFilter();

        foreach (var id in IncludeFilter)
        {
            filter.Include(id);
        }

        foreach (var id in ExcludeFilter)
        {
            filter.Exclude(id);
        }

        foreach (var id in AtLeastOneFilter)
        {
            filter.AtLeastOne(id);
        }

        foreach (var id in ExactlyOneFilter)
        {
            filter.ExactlyOne(id);
        }

        foreach (var id in NotAllFilter)
        {
            filter.NotAll(id);
        }

        return filter;
    }

    /// <summary>
    /// The archetypes matched by this query.
    /// </summary>
    public ReadOnlySpan<Archetype> Archetypes => GetMatchResult().Archetypes.AsSpan();

    /// <inheritdoc cref="Archetypes"/>
    public IReadOnlyList<Archetype> ArchetypesList => GetMatchResult().Archetypes;

    /// <summary>
    /// Checks if an entity matches this query.
    /// </summary>
    public bool IsMatch(Entity entity)
    {
        if (!entity.IsAlive)
        {
            return false;
        }

        var matchResult = GetMatchResult();
        return matchResult.ArchetypeSet.Contains(entity.World.Entities.GetLocation(entity.EntityId).Archetype);
    }

    /// <summary>
    /// Checks if an archetype matches this query.
    /// </summary>
    public bool IsMatch(Archetype archetype)
    {
        var matchResult = GetMatchResult();
        return matchResult.ArchetypeSet.Contains(archetype);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MatchResult GetMatchResult()
    {
        // Quickly check if we already have a non-stale result
        var currentResult = Volatile.Read(in result);
        if (currentResult.Version == Volatile.Read(in World.Version))
        {
            return currentResult;
        }

        return GetMatchResultCold(this, currentResult);
    }

    /// <summary>
    /// This assumes the current result is stale.
    /// </summary>
    /// <remarks>
    /// <see cref="view"/> is passed in manually to avoid accessing instance state on accident.
    /// </remarks>
    private static MatchResult GetMatchResultCold(QueryView view, MatchResult oldResult)
    {
        // Lazily allocated set of new archetype matches
        var newMatches = default(OrderedListSet<ArchetypeMatch>?);

        // Check every new archetype
        var archetypes = view.World.Archetypes;
        {
            // Acquire temporary set from pool
            // No need to clear on returning since component id does not have managed references
            using var _ = SimplePool<OrderedListSet<ComponentId>>.Acquire(out var temporarySet);
            for (var i = oldResult.Version; i < archetypes.Length; i++)
            {
                if (!view.TryMatch(archetypes[i], temporarySet, out var match))
                {
                    continue;
                }

                // Initialize new matches now that we know we need it
                newMatches ??= new OrderedListSet<ArchetypeMatch>(oldResult.ArchetypesMatches);

                // Add the match
                newMatches.Add(match);
            }
        }

        MatchResult localResult;
        if (newMatches == null)
        {
            // Copy is null, meaning nothing new was found, just use the old result with the new watermark
            localResult = new MatchResult(archetypes.Length, oldResult.ArchetypesMatches);
        }
        else
        {
            // Create a new match result
            localResult = new MatchResult(archetypes.Length, ImmutableOrderedListSet<ArchetypeMatch>.Create(newMatches));
        }

        var exchangeResult = Interlocked.CompareExchange(ref view.result, localResult, oldResult);
        if (exchangeResult != oldResult)
        {
            // Someone updated it before us
            // Recycle our collections immediately since no other thread has access yet
            ListPool<Archetype>.Release(localResult.Archetypes);
            HashSetPool<Archetype>.Release(localResult.ArchetypeSet);

            // Read again to get the absolute latest data
            return Volatile.Read(in view.result);
        }

        // Successfully replaced
        // Defer the recycling of old collections to the world
        // The world will recycle them at the next sync point
        view.World.Recycle(oldResult.Archetypes);
        view.World.Recycle(oldResult.ArchetypeSet);

        return localResult;
    }

    private bool TryMatch(Archetype archetype, OrderedListSet<ComponentId> temporarySet, out ArchetypeMatch match)
    {
        match = default;

        // Apply the Include filter
        // Quick bloom filter test if the included components intersects with the archetype.
        // If this returns false there is definitely no overlap at all and we can early exit.
        if (IncludeFilter.Count > 0 && !archetype.Info.BloomFilter.MaybeIntersects(in includeBloom))
        {
            return false;
        }

        // Do the full set check for included components
        if (!archetype.Components.IsSupersetOf(IncludeFilter))
        {
            return false;
        }

        // Apply the Exclude filter
        // If this is false it means there is definitely _not_ an intersection, which means we can skip
        // the inner check.
        if (ExcludeFilter.Count > 0 && excludeBloom.MaybeIntersects(in archetype.Info.BloomFilter))
        {
            if (archetype.Components.Overlaps(ExcludeFilter))
            {
                return false;
            }
        }

        // Apply the ExactlyOne filter
        if (ExactlyOneFilter.Count > 0)
        {
            temporarySet.Clear();
            temporarySet.UnionWith(archetype.Components);
            temporarySet.IntersectWith(ExactlyOneFilter);
            if (temporarySet.Count != 1)
            {
                temporarySet.Clear();
                return false;
            }
        }

        // Apply the AtLeastOne filter
        if (AtLeastOneFilter.Count > 0)
        {
            temporarySet.Clear();
            temporarySet.UnionWith(archetype.Components);
            temporarySet.IntersectWith(AtLeastOneFilter);
            if (temporarySet.Count == 0)
            {
                temporarySet.Clear();
                return false;
            }
        }

        // Apply the NotAll filter
        if (NotAllFilter.Count > 0 && archetype.Components.IsSupersetOf(NotAllFilter))
        {
            return false;
        }

        temporarySet.Clear();
        match = new ArchetypeMatch(archetype);

        return true;
    }

    private class MatchResult
    {
        /// <summary>
        /// The archetypes matching this query.
        /// </summary>
        public readonly List<Archetype> Archetypes;

        /// <summary>
        /// The archetypes matching this query.
        /// </summary>
        public readonly HashSet<Archetype> ArchetypeSet;

        /// <summary>
        /// The archetypes matching this query.
        /// </summary>
        public readonly ImmutableOrderedListSet<ArchetypeMatch> ArchetypesMatches;

        /// <summary>
        /// The number of archetypes in the world when this cache was created. Used for caching purposes.
        /// </summary>
        public readonly int Version;

        public MatchResult(int version, ImmutableOrderedListSet<ArchetypeMatch> archetypesMatches)
        {
            ArchetypesMatches = archetypesMatches;
            Version = version;

            Archetypes = ListPool<Archetype>.Acquire();
            Archetypes.EnsureCapacity(archetypesMatches.Count);
            foreach (var match in archetypesMatches)
            {
                Archetypes.Add(match.Archetype);
            }

            ArchetypeSet = HashSetPool<Archetype>.Acquire();
            ArchetypeSet.EnsureCapacity(ArchetypesMatches.Count);
            ArchetypeSet.UnionWith(Archetypes);
        }
    }

    /// <summary>
    /// An archetype that matched a query.
    /// </summary>
    private readonly record struct ArchetypeMatch(Archetype Archetype) : IComparable<ArchetypeMatch>
    {
        /// <inheritdoc/>
        public int CompareTo(ArchetypeMatch other)
        {
            return Archetype.Info.Hash.CompareTo(other.Archetype.Info.Hash);
        }
    }
}
