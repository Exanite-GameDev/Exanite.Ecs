using System;
using Exanite.Core.Utilities;

namespace Exanite.Myriad.Ecs;

public static class QueryUtilities
{
    /// <summary>
    /// Count how many entities match this query.
    /// </summary>
    public static int Count(this IArchetypeCollection collection)
    {
        var count = 0;
        foreach (var archetype in collection.Archetypes)
        {
            count += archetype.EntityCount;
        }

        return count;
    }

    /// <summary>
    /// Check if this query matches any entities.
    /// </summary>
    public static bool Any(this IArchetypeCollection collection)
    {
        foreach (var archetype in collection.Archetypes)
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
    public static Entity FirstOrDefault(this IArchetypeCollection collection)
    {
        foreach (var archetype in collection.Archetypes)
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
    public static Entity First(this IArchetypeCollection collection)
    {
        var entity = collection.FirstOrDefault();
        GuardUtility.IsTrue(entity.IsAlive, "QueryView.First() found no matching entities");
        return entity;
    }

    /// <summary>
    /// Get the single entity this query matches. Returns default if none.
    /// </summary>
    public static Entity SingleOrDefault(this IArchetypeCollection collection)
    {
        Entity entity = default;
        foreach (var archetype in collection.Archetypes)
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

                GuardUtility.IsFalse(chunk.EntityCount > 1 || entity.IsAlive, "QueryView.SingleOrDefault() found more than one matching entity");

                entity = chunk.Entities[0];
            }
        }

        return entity;
    }

    /// <summary>
    /// Get the single entity this query matches. Throws if none.
    /// </summary>
    public static Entity Single(this IArchetypeCollection collection)
    {
        var entity = collection.SingleOrDefault();
        GuardUtility.IsTrue(entity.IsAlive, "QueryView.SingleOrDefault() found no matching entities");
        return entity;
    }

    /// <summary>
    /// Get a random entity this query matches. Returns default if none.
    /// </summary>
    public static Entity RandomOrDefault(this IArchetypeCollection collection, Random random)
    {
        // Get total entity count
        var count = collection.Count();
        if (count == 0)
        {
            return default;
        }

        // Choose the index of the entity
        var choice = random.Next(0, count);

        // Find that entity
        foreach (var archetype in collection.Archetypes)
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
    public static Entity Random(this IArchetypeCollection collection, Random random)
    {
        var entity = collection.RandomOrDefault(random);
        GuardUtility.IsTrue(entity.IsAlive, "QueryView.RandomOrDefault() found no matching entities");
        return entity;
    }
}
