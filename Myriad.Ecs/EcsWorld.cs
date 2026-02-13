using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Exanite.Core.Events;
using Exanite.Core.Pooling;
using Exanite.Core.Runtime;
using Exanite.Core.Utilities;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Events;
using Exanite.Myriad.Ecs.Queries;
using Exanite.Myriad.Ecs.Worlds;

namespace Exanite.Myriad.Ecs;

/// <summary>
/// A world contains all entities.
/// </summary>
public sealed class EcsWorld : IArchetypeView, ITrackedDisposable
{
    private static readonly Lock IdLock = new();
    private static int NextWorldId = 1;

    internal EntityManager Entities = new();

    private readonly List<Archetype> archetypes = [];
    private readonly Dictionary<ArchetypeHash, List<Archetype>> archetypesByHash = [];

    /// <summary>
    /// Must be read using <see cref="Volatile"/>.
    /// </summary>
    /// <remarks>
    /// Guaranteed to be the same as the archetype count,
    /// however, since this is a field, it can be read using <see cref="Volatile"/>.
    /// </remarks>
    internal int Version;

    private readonly List<InterfaceResolverRegistration> interfaceResolvers = new();

    internal readonly Lock QueryViewCacheLock = new();
    internal readonly Dictionary<QueryCacheKey, QueryView> QueryViewCache = new();
    private readonly QueryView allEntitiesQuery;

    private readonly Pool<EcsCommandBuffer> commandBufferPool;
    private readonly HashSet<EcsCommandBuffer> activeCommandBuffers = new();

    private readonly Lock recycleLock = new();
    private readonly List<List<Archetype>> archetypeListsToRecycle = new();

    public bool IsDisposing { get; private set; }
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// The unique identifier for this world.
    /// </summary>
    /// <remarks>
    /// If too many worlds have been created,
    /// this can overflow and lead to id collisions,
    /// but this is mainly for debugging and
    /// you have to create a lot of worlds to overflow.
    /// </remarks>
    public readonly int Id;

    /// <summary>
    /// The archetypes stored by this world.
    /// </summary>
    /// <remarks>
    /// This can include empty archetypes.
    /// </remarks>
    public ReadOnlySpan<Archetype> Archetypes => archetypes.AsSpan();

    /// <inheritdoc cref="Archetypes"/>
    public IReadOnlyList<Archetype> ArchetypesList => archetypes;

    /// <summary>
    /// The interface resolvers registered for this world.
    /// </summary>
    /// <remarks>
    /// This does not represent the actual processing order of the resolvers.
    /// The resolvers are topologically sorted first by the interfaces they depend on.
    /// </remarks>
    public IReadOnlyList<InterfaceResolverRegistration> InterfaceResolvers => interfaceResolvers;

    private readonly List<InterfaceResolverRegistration> sortedInterfaceResolvers = new();

    public readonly EventBus EventBus = new();

    public EcsWorld()
    {
        using (IdLock.EnterScope())
        {
            Id = NextWorldId++;
        }

        allEntitiesQuery = new QueryFilter().Build(this);

        commandBufferPool = new Pool<EcsCommandBuffer>(
            create: () => new EcsCommandBuffer(this),
            onAcquire: commandBuffer =>
            {
                activeCommandBuffers.Add(commandBuffer);
            },
            onRelease: commandBuffer =>
            {
                commandBuffer.Clear();
                activeCommandBuffers.Remove(commandBuffer);
            });
    }

    public Pool<EcsCommandBuffer>.Handle AcquireCommandBuffer(out EcsCommandBuffer value)
    {
        return commandBufferPool.Acquire(out value);
    }

    public EcsCommandBuffer AcquireCommandBuffer()
    {
        return commandBufferPool.Acquire();
    }

    public void ReleaseCommandBuffer(EcsCommandBuffer value)
    {
        commandBufferPool.Release(value);
    }

    /// <summary>
    /// Copies all entities and their components to the destination world.
    /// Data in the target world will be overwritten.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEntityLookup CopyTo(EcsWorld dstWorld)
    {
        return CopyTo(dstWorld, allEntitiesQuery);
    }

    /// <summary>
    /// Copies all entities and their components to the destination world for the specified archetypes.
    /// Data in the target world will be overwritten.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEntityLookup CopyTo(EcsWorld dstWorld, IArchetypeView view)
    {
        dstWorld.Clear();
        return AddTo(dstWorld, view);
    }

    /// <summary>
    /// Copies all entities and their components to the destination world.
    /// Data in the target world will be kept.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEntityLookup AddTo(EcsWorld dstWorld)
    {
        return AddTo(dstWorld, allEntitiesQuery);
    }

    /// <summary>
    /// Copies all entities and their components to the destination world.
    /// Data in the target world will be kept.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEntityLookup AddTo(EcsWorld dstWorld, IArchetypeView view)
    {
        OnSyncPoint();

        using var _ = AcquireCommandBuffer(out var commandBuffer);
        using var __ = ListPool<Archetype>.Acquire(out var dstArchetypes);

        var lookup = new EntityLookup();
        var srcArchetypes = view.Archetypes;

        foreach (var srcArchetype in srcArchetypes)
        {
            if (srcArchetype.Entities.Length == 0)
            {
                continue;
            }

            var dstArchetype = dstWorld.GetOrCreateArchetype(srcArchetype.Components, srcArchetype.Info.Hash);
            dstArchetype.AddFrom(srcArchetype, lookup);
            dstArchetypes.Add(dstArchetype);
        }

        for (var archetypeI = 0; archetypeI < srcArchetypes.Length; archetypeI++)
        {
            var srcArchetype = srcArchetypes[archetypeI];
            var dstArchetype = dstArchetypes[archetypeI];

            // Raise component copied/added events
            var dstStart = dstArchetype.Entities.Length - srcArchetype.Entities.Length;
            var dstComponentIdByColumnIndex = dstArchetype.Info.ComponentIdByColumnIndex;
            foreach (var componentId in dstComponentIdByColumnIndex)
            {
                var dispatcher = dstArchetype.Info.ComponentDispatcherByComponentId[componentId.Value];
                dispatcher.OnComponentCopied(commandBuffer, dstArchetype, dstStart, srcArchetype.Entities.Length, lookup);
                dispatcher.OnComponentAdded(commandBuffer, dstArchetype, dstStart, srcArchetype.Entities.Length);
            }

            // Raise entity created events
            for (var entityI = 0; entityI < srcArchetype.Entities.Length; entityI++)
            {
                dstWorld.EventBus.Raise(new EntityCreatedEvent(commandBuffer, dstArchetype.Entities[dstStart + entityI]));
            }
        }

        commandBuffer.Execute();

        return lookup;
    }

    /// <summary>
    /// Add an interface that will be resolved for all archetypes that match the specified filter.
    /// This can be used to implement polymorphic behavior that is decided on the shape of an entity (ie, the archetype).
    /// Interfaces can replace patterns such as switch statements, vtables, and derived data.
    /// <para/>
    /// Resolvers can filter by both physical components and interface components.
    /// <para/>
    /// Resolvers are evaluated after topologically sorting the resolvers
    /// by the interface components the resolver filters by (explicit dependency)
    /// and the resolvers that provide the same interface (implicit dependency).
    /// <para/>
    /// Resolvers never depend on themselves, meaning that they can filter by the interface component they themselves provide.
    /// In this case, the resolver filter will only see interface components that exist at time of filtering.
    /// <para/>
    /// For resolvers that only filter by physical components, the sorted order will exactly match the registration order.
    /// For resolvers that filter by interface components, the sorted order will ensure that all resolver filters are evaluated consistently.
    /// Cycles are not allowed.
    /// <para/>
    /// Later resolvers are able to override interfaces provided by earlier resolvers, according to the sorted evaluation order.
    /// </summary>
    /// <remarks>
    /// Modifying resolvers will lead to all existing archetypes being updated and existing queries invalidated.
    /// To avoid this, consider registering all resolvers before any archetypes are created.
    /// </remarks>
    /// <param name="filter">The archetypes that this interface component will be resolved for.</param>
    /// <param name="factory">Get or create the concrete implementation of the interface component for the specified archetype.</param>
    public void RegisterInterfaceResolver<T>(QueryFilter filter, InterfaceResolverFactory<T> factory) where T : class, IInterfaceComponent
    {
        var registration = new InterfaceResolverRegistration(InterfaceId.Get<T>(), filter, (previous, components) =>
        {
            return factory.Invoke((T?)previous, components);
        });

        RegisterInterfaceResolver(registration);
    }

    /// <inheritdoc cref="RegisterInterfaceResolver{T}"/>
    public void RegisterInterfaceResolver(InterfaceResolverRegistration registration)
    {
        interfaceResolvers.Add(registration);
        OnResolversModified();
    }

    /// <summary>
    /// Bulk registers a set of interface resolvers to avoid repeated invalidations of archetype and query data.
    /// Also see <see cref="RegisterInterfaceResolver{T}"/>.
    /// </summary>
    public void RegisterInterfaceResolvers(ReadOnlySpan<InterfaceResolverRegistration> registrations)
    {
        interfaceResolvers.AddRange(registrations);
        OnResolversModified();
    }

    /// <summary>
    /// Replaces all interface resolvers in bulk to avoid repeated invalidations of archetype and query data.
    /// Also see <see cref="RegisterInterfaceResolver{T}"/>.
    /// </summary>
    public void SetInterfaceResolvers(ReadOnlySpan<InterfaceResolverRegistration> registrations)
    {
        interfaceResolvers.Clear();
        interfaceResolvers.AddRange(registrations);
        OnResolversModified();
    }

    /// <summary>
    /// Clears the resolvers list, updates all archetypes, and invalidates existing queries.
    /// Also see <see cref="RegisterInterfaceResolver{T}"/>.
    /// </summary>
    public void ClearInterfaceResolvers()
    {
        interfaceResolvers.Clear();
        OnResolversModified();
    }

    /// <summary>
    /// Clears the world by destroying all entities.
    /// </summary>
    public void Clear()
    {
        OnSyncPoint();

        using var _ = AcquireCommandBuffer(out var commandBuffer);
        commandBuffer.Destroy(allEntitiesQuery);
        commandBuffer.Execute();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposing = true;

        // Destroy all entities
        Clear();

        // Clear all active command buffers
        foreach (var activeCommandBuffer in activeCommandBuffers)
        {
            activeCommandBuffer.Clear();
        }
        activeCommandBuffers.Clear();

        // Clear event handlers
        EventBus.Dispose();

        // Run the sync point again to ensure everything has been recycled
        OnSyncPoint();

        IsDisposing = false;
        IsDisposed = true;

        GuardUtility.IsTrue(allEntitiesQuery.Count() == 0, "Expected entity count to be 0 after world disposal");
    }

    internal void Recycle(List<Archetype> value)
    {
        using var _ = recycleLock.EnterScope();
        archetypeListsToRecycle.Add(value);
    }

    /// <summary>
    /// Call when a sync point is reached to clean up internal data.
    /// </summary>
    internal void OnSyncPoint()
    {
        using var _ = recycleLock.EnterScope();

        foreach (var value in archetypeListsToRecycle)
        {
            ListPool<Archetype>.Release(value);
        }
        archetypeListsToRecycle.Clear();
    }

    /// <summary>
    /// Call when resolvers are modified to invalidate archetype infos and queries.
    /// </summary>
    internal void OnResolversModified()
    {
        // This will get recomputed on the next access
        // Notably, this means that the sort only occurs if there is at least one archetype in Release mode
        sortedInterfaceResolvers.Clear();

#if DEBUG
        // Eagerly recalculate resolvers in Debug mode to catch cycles as soon as they happen
        GetSortedResolvers();
#endif

        foreach (var archetype in archetypes)
        {
            archetype.UpdateInterfaceComponentResolutions();
        }

        foreach (var queryView in QueryViewCache.Values)
        {
            queryView.Invalidate();
        }
    }

    internal ReadOnlySpan<InterfaceResolverRegistration> GetSortedResolvers()
    {
        var count = interfaceResolvers.Count;
        if (count == sortedInterfaceResolvers.Count)
        {
            // Already sorted
            return sortedInterfaceResolvers.AsSpan();
        }

        // Tracks the number of dependencies each resolver is waiting for
        // Index by resolver index
        var rawDependencyCountByResolver = ArrayPool<int>.Shared.Rent(count);
        var dependencyCountByResolver = rawDependencyCountByResolver.AsSpan(0, count);
        dependencyCountByResolver.Clear();

        // Tracks the resolvers to notify once the resolver at that index is done
        // Index by resolver index
        var rawDependentsByResolver = ArrayPool<List<int>?>.Shared.Rent(count);
        var dependentsByResolver = rawDependentsByResolver.AsSpan(0, count);
        dependentsByResolver.Clear();

        // Tracks the resolvers that provide the specified interface
        // Index by interface index
        var providersByInterface = ListPool<List<int>?>.Acquire();

        try
        {
            // Identify which resolvers produce each interface
            for (var i = 0; i < interfaceResolvers.Count; i++)
            {
                var registration = interfaceResolvers[i];
                var interfaceIndex = ~registration.Id.Value;
                CollectionsMarshal.SetCount(providersByInterface, int.Max(providersByInterface.Count, interfaceIndex + 1));

                ref var providers = ref providersByInterface.AsSpan()[interfaceIndex];
                providers ??= ListPool<int>.Acquire();

                if (providers.Count > 0)
                {
                    // There is an existing provider
                    // Add it as an implicit dependency
                    var providerIndex = providers[^1];

                    dependencyCountByResolver[i]++;

                    ref var dependants = ref dependentsByResolver[providerIndex];
                    dependants ??= ListPool<int>.Acquire();
                    dependants.Add(i);
                }

                providers.Add(i);
            }

            // Handle explicit dependencies by looking at the filters for each resolver
            for (var i = 0; i < interfaceResolvers.Count; i++)
            {
                var registration = interfaceResolvers[i];
                var filter = registration.Filter;
                if (!filter.HasInterfaces)
                {
                    continue;
                }

                // Gather dependencies
                using var _ = SimplePool<OrderedListSet<InterfaceId>>.Acquire(out var dependencies);
                AddDependencies(filter.IncludeFilter, dependencies);
                AddDependencies(filter.ExcludeFilter, dependencies);
                AddDependencies(filter.AtLeastOneFilter, dependencies);
                AddDependencies(filter.ExactlyOneFilter, dependencies);
                AddDependencies(filter.NotAllFilter, dependencies);

                // Add to tracking structures
                foreach (var interfaceId in dependencies)
                {
                    // Add all resolvers that provide this interface as a dependency
                    var interfaceIndex = ~interfaceId.Value;
                    var providers = (uint)interfaceIndex < providersByInterface.Count ? providersByInterface[interfaceIndex] : null;
                    if (providers != null)
                    {
                        foreach (var providerIndex in providers)
                        {
                            // Don't depend on self
                            if (providerIndex == i)
                            {
                                continue;
                            }

                            // Don't depend on future providers that provide the current interface
                            // These are handled as implicit dependencies
                            if (providerIndex > i && registration.Id == interfaceId)
                            {
                                continue;
                            }

                            // Add as explicit dependency
                            dependencyCountByResolver[i]++;

                            ref var dependants = ref dependentsByResolver[providerIndex];
                            dependants ??= ListPool<int>.Acquire();
                            dependants.Add(i);
                        }
                    }
                }
            }

            // Add resolvers with 0 dependencies to ready queue
            using var __ = SimplePool<Queue<int>>.Acquire(out var ready);
            for (var i = 0; i < dependencyCountByResolver.Length; i++)
            {
                var dependencyCount = dependencyCountByResolver[i];
                if (dependencyCount == 0)
                {
                    ready.Enqueue(i);
                }
            }

            // Process resolvers that are ready one by one
            while (ready.TryDequeue(out var resolverIndex))
            {
                sortedInterfaceResolvers.Add(interfaceResolvers[resolverIndex]);

                // Notify dependents
                var dependents = dependentsByResolver[resolverIndex];
                if (dependents != null)
                {
                    foreach (var dependentIndex in dependents)
                    {
                        dependencyCountByResolver[dependentIndex]--;
                        if (dependencyCountByResolver[dependentIndex] == 0)
                        {
                            // Resolver has no more pending dependencies and is now ready
                            ready.Enqueue(dependentIndex);
                        }
                    }
                }
            }

            // If we didn't output some resolvers, then we have a cycle
            if (sortedInterfaceResolvers.Count != interfaceResolvers.Count)
            {
                // Identify which resolvers participate in the cycle
                // Don't care about allocations here since this is a cold path
                var resolversInCycle = new List<InterfaceResolverRegistration>();
                for (var i = 0; i < dependencyCountByResolver.Length; i++)
                {
                    var dependencyCount = dependencyCountByResolver[i];
                    if (dependencyCount != 0)
                    {
                        resolversInCycle.Add(interfaceResolvers[i]);
                    }
                }

                GuardUtility.Throw($"Cycle detected when sorting interface resolvers. Relevant resolvers:"
                    + $"\n    {string.Join("\n    ", resolversInCycle.Select(r => r.ToString(false, true)))}");
            }
        }
        finally
        {
            // Release pooled collections
            ArrayPool<int>.Shared.Return(rawDependencyCountByResolver);

            foreach (var list in dependentsByResolver)
            {
                if (list != null)
                {
                    ListPool<int>.Release(list);
                }
            }
            dependentsByResolver.Clear();
            ArrayPool<List<int>?>.Shared.Return(rawDependentsByResolver);

            foreach (var list in providersByInterface)
            {
                if (list != null)
                {
                    ListPool<int>.Release(list);
                }
            }
            ListPool<List<int>?>.Release(providersByInterface);
        }

        return sortedInterfaceResolvers.AsSpan();

        static void AddDependencies(IReadOnlyList<TypeId> filter, OrderedListSet<InterfaceId> results)
        {
            foreach (var typeId in filter)
            {
                if (typeId.IsInterface)
                {
                    results.Add((InterfaceId)typeId);
                }
            }
        }
    }

    /// <summary>
    /// Find an archetype with the given set of components, using a precomputed archetype hash.
    /// </summary>
    internal Archetype GetOrCreateArchetype(IReadOnlyOrderedListSet<ComponentId> components, ArchetypeHash hash)
    {
        // Get list of all archetypes with this hash
        if (!archetypesByHash.TryGetValue(hash, out var candidates))
        {
            candidates = [];
            archetypesByHash.Add(hash, candidates);
        }

        // Check if any of the candidates are the one we need
        foreach (var archetype in candidates)
        {
            if (components.SetEquals(archetype.Components))
            {
                return archetype;
            }
        }

        // Didn't find one, create the new archetype
        var newArchetype = new Archetype(archetypes.Count + 1, this, components.MakeNewImmutable());

        // Add it to the relevant lists
        archetypes.Add(newArchetype);
        candidates.Add(newArchetype);

        // Increment version
        Interlocked.Increment(ref Version);

        return newArchetype;
    }

    /// <summary>
    /// Find an archetype with the given set of components.
    /// </summary>
    internal Archetype GetOrCreateArchetype(OrderedListSet<ComponentId> components)
    {
        return GetOrCreateArchetype(components, components.Items.ToArchetypeHash());
    }
}
