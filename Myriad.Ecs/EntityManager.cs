using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs;

internal struct EntityManager
{
    private readonly Lock sync = new();
    private SegmentedList<EntityLocation> entities = new();

    /// <summary>
    /// Tracks released IDs so that they can be reused.
    /// </summary>
    /// <remarks>
    /// The ID version should be incremented when re-acquired.
    /// </remarks>
    private readonly List<EntityId> releasedIds = [];
    private int nextId = 1;

    public EntityManager() {}

    /// <summary>
    /// Gets the location for the specified entity.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref EntityLocation GetLocation(int entityIndex)
    {
        return ref entities[entityIndex];
    }

    /// <inheritdoc cref="GetLocation(int)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref EntityLocation GetLocation(EntityId entityId)
    {
        ref var location = ref entities[entityId.Index];
        GuardUtility.IsTrue(location.Version == entityId.Version, "Entity is not alive");
        return ref location;
    }

    /// <summary>
    /// Tries to get the location for the specified entity.
    /// </summary>
    internal bool TryGetLocation(EntityId entity, out Ref<EntityLocation> location)
    {
        location = new Ref<EntityLocation>(ref entities[entity.Index]);
        if (location.Value.Version != entity.Version)
        {
            location = default;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Acquires a new <see cref="EntityId"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref EntityLocation AcquireId(out EntityId entityId)
    {
        using var _ = sync.EnterScope();

        if (releasedIds.Count > 0)
        {
            var previousId = releasedIds[^1];
            releasedIds.RemoveAt(releasedIds.Count - 1);

            var version = previousId.Version + 1;
            if (version == 0)
            {
                // Ensure ID is never 0, even if it overflows and wraps around
                version += 1;
            }

            entityId = new EntityId(previousId.Index, version);
        }
        else
        {
            // Allocate a new ID. This must not overflow!
            entityId = new EntityId(checked(nextId++), 1);
            entities.EnsureCapacity(entityId.Index);
        }

        // Update the version
        ref var location = ref GetLocation(entityId.Index);
        location.Version = entityId.Version;

        return ref location;
    }

    /// <summary>
    /// Bulk acquires IDs.
    /// See <see cref="AcquireId"/>.
    /// </summary>
    /// <remarks>
    /// IDs are acquired in forward order.
    /// The ID at index 0 should be used first.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AcquireIds(Span<EntityId> entityIds)
    {
        using var _ = sync.EnterScope();

        // Reuse as many ids as possible
        var reuseCount = int.Min(releasedIds.Count, entityIds.Length);
        var reuseStartIndex = releasedIds.Count - reuseCount;
        releasedIds.AsSpan().Slice(reuseStartIndex, reuseCount).CopyTo(entityIds);
        releasedIds.RemoveRange(reuseStartIndex, reuseCount);

        for (var i = 0; i < reuseCount; i++)
        {
            ref var entityId = ref entityIds[i];

            var version = entityId.Version + 1;
            if (version == 0)
            {
                // Ensure ID is never 0, even if it overflows and wraps around
                version += 1;
            }

            entityId = new EntityId(entityId.Index, version);
        }

        // Allocate new ids for the rest
        var firstNewId = nextId;
        var newCount = int.Max(0, entityIds.Length - reuseCount);

        nextId = checked(firstNewId + newCount);
        entities.EnsureCapacity(firstNewId + newCount);
        for (var i = 0; i < newCount; i++)
        {
            // Allocate a new ID. This must not overflow!
            ref var entityId = ref entityIds[reuseCount + i];
            entityId = new EntityId(firstNewId + i, 1);
        }

        // Update versions
        foreach (var entityId in entityIds)
        {
            // Update the version
            ref var location = ref GetLocation(entityId.Index);
            location.Version = entityId.Version;
        }
    }

    /// <summary>
    /// Releases a used <see cref="EntityId"/>.
    /// This will increment the version.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReleaseId(EntityId entityId)
    {
        using var _ = sync.EnterScope();

        ref var location = ref GetLocation(entityId);

        // Invalidate the handle
        location.Version++;
        location.Archetype = null!;

        // Store this ID for re-use later
        releasedIds.Add(entityId);
    }

    /// <summary>
    /// Bulk releases used IDs.
    /// See <see cref="ReleaseId"/>.
    /// </summary>
    /// <remarks>
    /// IDs are released in reverse order.
    /// The ID at index 0 will be released last.
    /// This is to ensure that reacquiring will lead to the ID at index 0 being first again.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReleaseIds(ReadOnlySpan<EntityId> entityIds)
    {
        using var _ = sync.EnterScope();
        for (var i = entityIds.Length - 1; i >= 0; i--)
        {
            ReleaseId(entityIds[i]);
        }
    }

    /// <summary>
    /// Bulk releases used IDs.
    /// See <see cref="ReleaseId"/>.
    /// </summary>
    /// <remarks>
    /// IDs are released in reverse order.
    /// The ID at index 0 will be released last.
    /// This is to ensure that reacquiring will lead to the ID at index 0 being first again.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReleaseIds(ReadOnlySpan<Entity> entityIds)
    {
        using var _ = sync.EnterScope();
        for (var i = entityIds.Length - 1; i >= 0; i--)
        {
            ReleaseId(entityIds[i].EntityId);
        }
    }

    /// <summary>
    /// Releases a used <see cref="EntityId"/>.
    /// This will not increment the version.
    /// Make sure the location corresponding to the ID has never been modified.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReleaseUnusedId(EntityId entityId)
    {
        using var _ = sync.EnterScope();

        // Store this ID for re-use later
        releasedIds.Add(entityId);
    }

    /// <summary>
    /// Bulk releases unused IDs.
    /// See <see cref="ReleaseUnusedId"/>.
    /// </summary>
    /// <remarks>
    /// IDs are released in reverse order.
    /// The ID at index 0 will be released last.
    /// This is to ensure that reacquiring will lead to the ID at index 0 being first again.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ReleaseUnusedIds(ReadOnlySpan<EntityId> entityIds)
    {
        using var _ = sync.EnterScope();
        for (var i = entityIds.Length - 1; i >= 0; i--)
        {
            ReleaseUnusedId(entityIds[i]);
        }
    }
}
