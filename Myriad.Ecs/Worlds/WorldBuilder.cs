using System;
using System.Collections.Generic;
using System.Linq;
using Myriad.Ecs.Collections;
using Myriad.Ecs.ComponentIds;
using Myriad.Ecs.Threading;

namespace Myriad.Ecs.Worlds;

/// <summary>
/// A builder to create a new <see cref="World"/>
/// </summary>
public sealed partial class WorldBuilder
{
    private readonly List<OrderedListSet<ComponentId>> _archetypes = [ ];
    private IThreadPool? _pool;

    private bool AddArchetype(HashSet<ComponentId> ids)
    {
        if (_archetypes.Any(a => a.SetEquals(ids)))
            return false;

        _archetypes.Add(new OrderedListSet<ComponentId>(ids));
        return true;
    }

    /// <summary>
    /// Declare a specific archetype that should be created ahead of time in this world. This
    /// can prevent expensive structural changes in the world later.
    /// </summary>
    public WorldBuilder WithArchetype(params Type[] types)
    {
        var set = new HashSet<ComponentId>(types.Length);

        foreach (var type in types)
            if (!set.Add(ComponentId.Get(type)))
                throw new ArgumentException($"Duplicate component type: {type.Name}");

        AddArchetype(set);

        return this;
    }

    /// <summary>
    /// Define the threadpool system used by this world
    /// </summary>
    /// <param name="pool"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public WorldBuilder WithThreadPool(IThreadPool pool)
    {
        if (_pool != null)
            throw new InvalidOperationException("Cannot call 'WithThreadPool' twice");
        _pool = pool;

        return this;
    }

    /// <summary>
    /// Create a new <see cref="World"/> using the configuration in this <see cref="WorldBuilder"/>.
    /// </summary>
    /// <returns></returns>
    public World Build()
    {
        var w = new World(
            _pool ?? new DefaultThreadPool()
        );

        foreach (var components in _archetypes)
            w.GetOrCreateArchetype(components);

        return w;
    }
}
