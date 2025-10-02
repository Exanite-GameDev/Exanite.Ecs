using System;
using System.Collections.Generic;
using System.Threading;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Utilities;
using Exanite.Myriad.Ecs.Worlds.Archetypes;
using Exanite.Myriad.Ecs.Worlds.Chunks;

namespace Exanite.Myriad.Ecs.Queries;

/// <summary>
/// Describes a query for entities, bound to a world.
/// </summary>
public sealed class QueryDescription
{
    /// <summary>
    /// Cached result from the last time <see cref="GetArchetypeMatchResult"/> was called.
    /// </summary>
    private ArchetypeMatchResult? result;
    private readonly ReaderWriterLockSlim resultLock = new();
    private readonly OrderedListSet<ComponentId> temporarySet = [];

    private readonly ComponentBloomFilter includeBloom;
    private readonly ComponentBloomFilter excludeBloom;

    /// <summary>
    /// The <see cref="EcsWorld"/> that this query is for.
    /// </summary>
    public EcsWorld World { get; }

    /// <summary>
    /// The components which must be present on an entity for it to match this query.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> Include { get; }

    /// <summary>
    /// The components which must not be present on an entity for it to match this query.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> Exclude { get; }

    /// <summary>
    /// At least one of these components must be present on an entity for it to match this query.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> AtLeastOne { get; }

    /// <summary>
    /// Exactly one of these components must be present on an entity for it to match this query.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> ExactlyOne { get; }

    /// <summary>
    /// Not all of these components must be present on an entity for it to match this query.
    /// </summary>
    public ImmutableOrderedListSet<ComponentId> NotAll { get; }

    /// <summary>
    /// Describes a query for entities, bound to a world.
    /// </summary>
    internal QueryDescription(
        EcsWorld world,
        ImmutableOrderedListSet<ComponentId> include,
        ImmutableOrderedListSet<ComponentId> exclude,
        ImmutableOrderedListSet<ComponentId> atLeastOne,
        ImmutableOrderedListSet<ComponentId> exactlyOne,
        ImmutableOrderedListSet<ComponentId> NotAll)
    {
        World = world;

        Include = include;
        Exclude = exclude;
        AtLeastOne = atLeastOne;
        ExactlyOne = exactlyOne;
        NotAll = NotAll;

        includeBloom = include.ToBloomFilter();
        excludeBloom = exclude.ToBloomFilter();
    }

    /// <summary>
    /// Create a query builder which describes this query.
    /// </summary>
    public QueryBuilder ToBuilder()
    {
        var builder = new QueryBuilder();

        foreach (var id in Include)
        {
            builder.Include(id);
        }

        foreach (var id in Exclude)
        {
            builder.Exclude(id);
        }

        foreach (var id in AtLeastOne)
        {
            builder.AtLeastOne(id);
        }

        foreach (var id in ExactlyOne)
        {
            builder.ExactlyOne(id);
        }

        return builder;
    }

    #region Is In Query

    /// <summary>
    /// Check if the specified component is part of the Include filter.
    /// </summary>
    public bool IsIncluded<T>() where T : IComponent
    {
        return IsIncluded(ComponentId.Get<T>());
    }

    /// <summary>
    /// Check if the specified component is part of the Include filter.
    /// </summary>
    public bool IsIncluded(Type type)
    {
        return IsIncluded(ComponentId.Get(type));
    }

    /// <summary>
    /// Check if the specified component is part of the Include filter.
    /// </summary>
    public bool IsIncluded(ComponentId id)
    {
        return Include.Contains(id);
    }

    /// <summary>
    /// Check if the specified component is part of the Exclude filter.
    /// </summary>
    public bool IsExcluded<T>() where T : IComponent
    {
        return IsExcluded(ComponentId.Get<T>());
    }

    /// <summary>
    /// Check if the specified component is part of the Exclude filter.
    /// </summary>
    public bool IsExcluded(Type type)
    {
        return IsExcluded(ComponentId.Get(type));
    }

    /// <summary>
    /// Check if the specified component is part of the Exclude filter.
    /// </summary>
    public bool IsExcluded(ComponentId id)
    {
        return Exclude.Contains(id);
    }

    /// <summary>
    /// Check if the specified component is part of the AtLeastOne filter.
    /// </summary>
    public bool IsAtLeastOne<T>() where T : IComponent
    {
        return IsAtLeastOne(ComponentId.Get<T>());
    }

    /// <summary>
    /// Check if the specified component is part of the AtLeastOne filter.
    /// </summary>
    public bool IsAtLeastOne(Type type)
    {
        return IsAtLeastOne(ComponentId.Get(type));
    }

    /// <summary>
    /// Check if the specified component is part of the AtLeastOne filter.
    /// </summary>
    public bool IsAtLeastOne(ComponentId id)
    {
        return AtLeastOne.Contains(id);
    }

    /// <summary>
    /// Check if the specified component is part of the ExactlyOne filter.
    /// </summary>
    public bool IsExactlyOne<T>() where T : IComponent
    {
        return IsExactlyOne(ComponentId.Get<T>());
    }

    /// <summary>
    /// Check if the specified component is part of the ExactlyOne filter.
    /// </summary>
    public bool IsExactlyOne(Type type)
    {
        return IsExactlyOne(ComponentId.Get(type));
    }

    /// <summary>
    /// Check if the specified component is part of the ExactlyOne filter.
    /// </summary>
    public bool IsExactlyOne(ComponentId id)
    {
        return ExactlyOne.Contains(id);
    }

    /// <summary>
    /// Check if the specified component is part of the NotAll filter.
    /// </summary>
    public bool IsNotAll<T>() where T : IComponent
    {
        return IsNotAll(ComponentId.Get<T>());
    }

    /// <summary>
    /// Check if the specified component is part of the NotAll filter.
    /// </summary>
    public bool IsNotAll(Type type)
    {
        return IsNotAll(ComponentId.Get(type));
    }

    /// <summary>
    /// Check if the specified component is part of the NotAll filter.
    /// </summary>
    public bool IsNotAll(ComponentId id)
    {
        return NotAll.Contains(id);
    }

    #endregion

    #region Archetype Matching

    /// <summary>
    /// Get all archetypes which match this query.
    /// </summary>
    public ReadOnlySpan<Archetype> GetArchetypes()
    {
        return GetArchetypeMatchResult().Archetypes.AsSpan();
    }

    /// <summary>
    /// Get all archetypes which match this query.
    /// </summary>
    public ImmutableOrderedListSet<ArchetypeMatch> GetArchetypeMatches()
    {
        return GetArchetypeMatchResult().ArchetypesMatches;
    }

    /// <summary>
    /// Checks if an entity matches this query.
    /// </summary>
    public bool IsMatch(Entity entity)
    {
        if (!entity.IsAlive)
        {
            return false;
        }

        var matchResult = GetArchetypeMatchResult();
        return matchResult.ArchetypeSet.Contains(entity.World.GetArchetype(entity.EntityId));
    }

    /// <summary>
    /// Checks if a chunk matches this query.
    /// </summary>
    public bool IsMatch(Chunk chunk)
    {
        var matchResult = GetArchetypeMatchResult();
        return matchResult.ArchetypeSet.Contains(chunk.Archetype);
    }

    /// <summary>
    /// Checks if an archetype matches this query.
    /// </summary>
    public bool IsMatch(Archetype archetype)
    {
        var matchResult = GetArchetypeMatchResult();
        return matchResult.ArchetypeSet.Contains(archetype);
    }

    private ArchetypeMatchResult GetArchetypeMatchResult()
    {
        // Quickly check if we already have a non-stale result
        resultLock.EnterReadLock();
        try
        {
            if (result != null && !result.Value.IsStale(World))
            {
                return result.Value;
            }
        }
        finally
        {
            resultLock.ExitReadLock();
        }

        // We don't have a valid cached result, calculate it now
        resultLock.EnterWriteLock();
        try
        {
            // If this query has never been evaluated before do it now
            if (result == null)
            {
                // Check every archetype
                var matches = new List<ArchetypeMatch>();
                foreach (var archetype in World.Archetypes)
                {
                    if (TryMatch(archetype, out var match))
                    {
                        matches.Add(match);
                    }
                }

                // Store result for next time
                result = new ArchetypeMatchResult(World.Archetypes.Length, ImmutableOrderedListSet<ArchetypeMatch>.Create(matches));

                // Return matches
                return result.Value;
            }

            // If the number of archetypes has changed since last time regenerate the cache
            if (result.Value.IsStale(World))
            {
                // Lazily allocated set of new archetype matches
                var newMatches = default(OrderedListSet<ArchetypeMatch>?);

                // Check every new archetype
                for (var i = result.Value.ArchetypeWatermark; i < World.Archetypes.Length; i++)
                {
                    if (!TryMatch(World.Archetypes[i], out var match))
                    {
                        continue;
                    }

                    // Initialize new matches now that we know we need it
                    newMatches ??= new OrderedListSet<ArchetypeMatch>(result.Value.ArchetypesMatches);

                    // Add the match
                    newMatches.Add(match);
                }

                if (newMatches == null)
                {
                    // Copy is null, meaning nothing new was found, just use the old result with the new watermark
                    result = new ArchetypeMatchResult(World.Archetypes.Length, result.Value.ArchetypesMatches);
                }
                else
                {
                    // Create a new match result
                    result = new ArchetypeMatchResult(World.Archetypes.Length, ImmutableOrderedListSet<ArchetypeMatch>.Create(newMatches));
                }
            }

            return result.Value;
        }
        finally
        {
            resultLock.ExitWriteLock();
        }
    }

    private bool TryMatch(Archetype archetype, out ArchetypeMatch match)
    {
        match = default;

        // Apply the Include filter
        // Quick bloom filter test if the included components intersects with the archetype.
        // If this returns false there is definitely no overlap at all and we can early exit.
        if (Include.Count > 0 && !archetype.ComponentsBloomFilter.MaybeIntersects(in includeBloom))
        {
            return false;
        }

        // Do the full set check for included components
        if (!archetype.Components.IsSupersetOf(Include))
        {
            return false;
        }

        // Apply the Exclude filter
        // If this is false it means there is definitely _not_ an intersection, which means we can skip
        // the inner check.
        if (Exclude.Count > 0 && excludeBloom.MaybeIntersects(in archetype.ComponentsBloomFilter))
        {
            if (archetype.Components.Overlaps(Exclude))
            {
                return false;
            }
        }

        // Apply the NotAll filter
        if (NotAll.Count > 0 && archetype.Components.IsSupersetOf(NotAll))
        {
            return false;
        }

        // Use the temp hashset to do this
        var set = temporarySet;
        set.Clear();

        // Apply the ExactlyOne filter
        var exactlyOne = default(ComponentId?);
        if (ExactlyOne.Count > 0)
        {
            set.Clear();
            set.UnionWith(archetype.Components);
            set.IntersectWith(ExactlyOne);
            if (set.Count != 1)
            {
                set.Clear();
                return false;
            }

            exactlyOne = set.Single();
            set.Clear();
        }

        // Apply the AtLeastOne filter
        if (AtLeastOne.Count > 0)
        {
            set.Clear();
            set.UnionWith(archetype.Components);
            set.IntersectWith(AtLeastOne);
            if (set.Count == 0)
            {
                set.Clear();
                return false;
            }
        }
        else
        {
            set.Clear();
            set = null;
        }

        var atLeastOne = set?.ToImmutable();
        match = new ArchetypeMatch(archetype, atLeastOne, exactlyOne);

        return true;
    }

    private readonly struct ArchetypeMatchResult
    {
        /// <summary>
        /// The archetypes matching this query.
        /// </summary>
        public ImmutableOrderedListSet<ArchetypeMatch> ArchetypesMatches { get; }

        /// <summary>
        /// The archetypes matching this query.
        /// </summary>
        public List<Archetype> Archetypes { get; }

        /// <summary>
        /// The archetypes matching this query.
        /// </summary>
        public HashSet<Archetype> ArchetypeSet { get; }

        /// <summary>
        /// The number of archetypes in the world when this cache was created. Used for caching purposes.
        /// </summary>
        public int ArchetypeWatermark { get; }

        public ArchetypeMatchResult(int watermark, ImmutableOrderedListSet<ArchetypeMatch> archetypesMatches)
        {
            ArchetypesMatches = archetypesMatches;
            ArchetypeWatermark = watermark;

            Archetypes = new List<Archetype>(archetypesMatches.Count);
            foreach (var match in archetypesMatches)
            {
                Archetypes.Add(match.Archetype);
            }

            ArchetypeSet = [..Archetypes];
        }

        public bool IsStale(EcsWorld world)
        {
            return ArchetypeWatermark < world.ArchetypesCount;
        }
    }

    /// <summary>
    /// An archetype which matches a query.
    /// </summary>
    /// <param name="Archetype">The archetype.</param>
    /// <param name="AtLeastOne">All of the "at least one" components present (if there are any in this query).</param>
    /// <param name="ExactlyOne">The "exactly one" component present (if there is one in this query).</param>
    public readonly record struct ArchetypeMatch(Archetype Archetype, ImmutableOrderedListSet<ComponentId>? AtLeastOne, ComponentId? ExactlyOne) : IComparable<ArchetypeMatch>
    {
        /// <inheritdoc/>
        public int CompareTo(ArchetypeMatch other)
        {
            return Archetype.Hash.CompareTo(other.Archetype.Hash);
        }
    }

    #endregion

    #region LINQ

    /// <summary>
    /// Count how many entities match this query.
    /// </summary>
    public int Count()
    {
        var count = 0;
        foreach (var archetype in GetArchetypes())
        {
            count += archetype.EntityCount;
        }

        return count;
    }

    /// <summary>
    /// Check if this query matches any entities.
    /// </summary>
    public bool Any()
    {
        foreach (var archetype in GetArchetypes())
        {
            if (archetype.EntityCount > 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Get the first entity this query matches. Returns default if none.
    /// </summary>
    public Entity FirstOrDefault()
    {
        foreach (var archetype in GetArchetypes())
        {
            if (archetype.EntityCount == 0)
            {
                continue;
            }

            foreach (var chunk in archetype.Chunks)
            {
                if (chunk.EntityCount > 0)
                {
                    return chunk.Entities[0];
                }
            }
        }

        return default;
    }

    /// <summary>
    /// Get the first entity this query matches. Throws if none.
    /// </summary>
    public Entity First()
    {
        var entity = FirstOrDefault();
        GuardUtility.IsTrue(entity.IsAlive, "QueryDescription.First() found no matching entities");
        return entity;
    }

    /// <summary>
    /// Get the single entity this query matches. Returns default if none.
    /// </summary>
    public Entity SingleOrDefault()
    {
        Entity result = default;
        foreach (var archetype in GetArchetypes())
        {
            if (archetype.EntityCount == 0)
            {
                continue;
            }

            foreach (var chunk in archetype.Chunks)
            {
                if (chunk.EntityCount == 0)
                {
                    continue;
                }

                GuardUtility.IsFalse(chunk.EntityCount > 1 || result.IsAlive, "QueryDescription.SingleOrDefault() found more than one matching entity");

                result = chunk.Entities[0];
            }
        }

        return result;
    }

    /// <summary>
    /// Get the single entity this query matches. Throws if none.
    /// </summary>
    public Entity Single()
    {
        var entity = SingleOrDefault();
        GuardUtility.IsTrue(entity.IsAlive, "QueryDescription.SingleOrDefault() found no matching entities");
        return entity;
    }

    /// <summary>
    /// Get a random entity this query matches. Returns default if none.
    /// </summary>
    public Entity RandomOrDefault(Random random)
    {
        // Get total entity count
        var count = Count();
        if (count == 0)
        {
            return default;
        }

        // Choose the index of the entity
        var choice = random.Next(0, count);

        // Find that entity
        foreach (var archetype in GetArchetypes())
        {
            // Check if it's within this archetype, if not move to the next archetype
            if (choice - archetype.EntityCount >= 0)
            {
                choice -= archetype.EntityCount;
            }
            else
            {
                // Step through chunks
                foreach (var chunk in archetype.Chunks)
                {
                    // Check if it's within this chunk, if not move to next chunk
                    if (choice - chunk.EntityCount >= 0)
                    {
                        choice -= chunk.EntityCount;
                    }
                    else
                    {
                        return chunk.Entities[choice];
                    }
                }
            }
        }

        // This shouldn't happen
        return default;
    }

    /// <summary>
    /// Get a random entity this query matches. Throws if none.
    /// </summary>
    public Entity Random(Random random)
    {
        var entity = RandomOrDefault(random);
        GuardUtility.IsTrue(entity.IsAlive, "QueryDescription.RandomOrDefault() found no matching entities");
        return entity;
    }

    #endregion
}
