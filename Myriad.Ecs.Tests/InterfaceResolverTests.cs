using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs.Queries;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class InterfaceResolverTests
{
    [Fact]
    public void Resolves_ForMatchedArchetype()
    {
        using var world = new EcsWorld();
        world.RegisterInterfaceResolver<IEcsDamageable>(
            new QueryFilter().Include<EcsHealth>(),
            (_, _) => new DefaultDamageable());

        using var _ = world.AcquireCommandBuffer(out var commandBuffer);
        var entity = commandBuffer.Create()
            .Set(new EcsHealth(10))
            .Entity;

        commandBuffer.Execute();

        Assert.True(entity.TryResolve<IEcsDamageable>(out var damageable));
        Assert.NotNull(damageable);

        ref var health = ref entity.Get<EcsHealth>();
        damageable.Damage(ref health, 4);
        Assert.Equal(6, health.Health);
    }

    [Fact]
    public void DoesNotResolve_ForUnmatchedArchetype()
    {
        using var world = new EcsWorld();
        world.RegisterInterfaceResolver<IEcsDamageable>(
            new QueryFilter().Include<EcsHealth>(),
            (_, _) => new DefaultDamageable());

        using var _ = world.AcquireCommandBuffer(out var commandBuffer);
        var entity = commandBuffer.Create()
            .Set(new EcsInt32(10))
            .Entity;

        commandBuffer.Execute();

        Assert.False(entity.TryResolve<IEcsDamageable>(out var damageable));
        Assert.Null(damageable);
    }

    [Fact]
    public void Resolve_Throws_ForUnmatchedArchetype()
    {
        using var world = new EcsWorld();
        world.RegisterInterfaceResolver<IEcsDamageable>(
            new QueryFilter().Include<EcsHealth>(),
            (_, _) => new DefaultDamageable());

        using var __ = world.AcquireCommandBuffer(out var commandBuffer);
        var entity = commandBuffer.Create()
            .Set(new EcsInt32(10))
            .Entity;

        commandBuffer.Execute();

        Assert.Throws<GuardException>(() =>
        {
            _ = entity.Resolve<IEcsDamageable>();
        });
    }

    [Fact]
    public void CanBeCleared()
    {
        using var world = new EcsWorld();
        world.RegisterInterfaceResolver<IEcsDamageable>(
            new QueryFilter().Include<EcsHealth>(),
            (_, _) => new DefaultDamageable());

        using var _ = world.AcquireCommandBuffer(out var commandBuffer);
        var entity = commandBuffer.Create()
            .Set(new EcsHealth(10))
            .Entity;

        commandBuffer.Execute();

        Assert.True(entity.TryResolve<IEcsDamageable>(out var damageable));
        Assert.NotNull(damageable);

        world.ClearInterfaceResolvers();

        Assert.False(entity.TryResolve(out damageable));
        Assert.Null(damageable);
    }

    [Fact]
    public void CanBeReplacedWithNull()
    {
        using var world = new EcsWorld();
        world.RegisterInterfaceResolver<IEcsDamageable>(
            new QueryFilter().Include<EcsHealth>(),
            (_, _) => new DefaultDamageable());

        using var _ = world.AcquireCommandBuffer(out var commandBuffer);
        var entity = commandBuffer.Create()
            .Set(new EcsHealth(10))
            .Entity;

        commandBuffer.Execute();

        Assert.True(entity.TryResolve<IEcsDamageable>(out var damageable));
        Assert.NotNull(damageable);

        world.RegisterInterfaceResolver<IEcsDamageable>(
            new QueryFilter().Include<EcsHealth>(),
            (_, _) => null);

        Assert.False(entity.TryResolve(out damageable));
        Assert.Null(damageable);
    }

    [Fact]
    public void CanBeWrapped()
    {
        using var world = new EcsWorld();
        world.RegisterInterfaceResolver<IEcsDamageable>(
            new QueryFilter().Include<EcsHealth>(),
            (_, _) => new DefaultDamageable());

        using var _ = world.AcquireCommandBuffer(out var commandBuffer);
        var entity = commandBuffer.Create()
            .Set(new EcsHealth(10))
            .Entity;

        commandBuffer.Execute();

        Assert.True(entity.TryResolve<IEcsDamageable>(out var damageable));
        Assert.NotNull(damageable);
        Assert.IsType<DefaultDamageable>(damageable);

        // Should deal 4 damage, leaving 6 health left
        ref var health = ref entity.Get<EcsHealth>();
        damageable.Damage(ref health, 4);
        Assert.Equal(6, health.Health);

        world.RegisterInterfaceResolver<IEcsDamageable>(
            new QueryFilter().Include<EcsHealth>(),
            (previous, _) => previous != null ? new ShieldedDamageable(previous) : null);

        Assert.True(entity.TryResolve(out damageable));
        Assert.NotNull(damageable);
        Assert.IsType<ShieldedDamageable>(damageable);

        // Should deal 2 damage, leaving 4 health left
        damageable.Damage(ref health, 4);
        Assert.Equal(4, health.Health);
    }

    [Fact]
    public void CanBeQueried()
    {
        using var world = new EcsWorld();
        world.RegisterInterfaceResolver<IEcsDamageable>(
            new QueryFilter().Include<EcsHealth>(),
            (_, _) => new DefaultDamageable());

        using var _ = world.AcquireCommandBuffer(out var commandBuffer);

        commandBuffer.Create().Set(new EcsHealth(10));
        commandBuffer.Create();
        commandBuffer.Execute();

        var view = new QueryFilter()
            .Include<EcsHealth>()
            .Include<IEcsDamageable>()
            .Build(world);

        Assert.Equal(1, view.Count());
    }

    [Fact]
    public void ModificationsUpdateQuery()
    {
        using var world = new EcsWorld();
        world.RegisterInterfaceResolver<IEcsDamageable>(
            new QueryFilter().Include<EcsHealth>(),
            (_, _) => new DefaultDamageable());

        using var _ = world.AcquireCommandBuffer(out var commandBuffer);

        commandBuffer.Create().Set(new EcsHealth(10));
        commandBuffer.Create();
        commandBuffer.Execute();

        var view = new QueryFilter()
            .Include<IEcsDamageable>()
            .Build(world);
        Assert.Equal(1, view.Count());

        world.ClearInterfaceResolvers();
        Assert.Equal(0, view.Count());

        // Allow matching any
        world.RegisterInterfaceResolver<IEcsDamageable>(
            new QueryFilter(),
            (_, _) => new DefaultDamageable());
        Assert.Equal(2, view.Count());
    }

    [Fact]
    public void Resolvers_CanFilterBy_InterfaceComponents()
    {
        using var world = new EcsWorld();
        world.RegisterInterfaceResolver<IEcsDamageable>(
            new QueryFilter().Include<EcsHealth>(),
            (_, _) => new DefaultDamageable());

        using var _ = world.AcquireCommandBuffer(out var commandBuffer);
        var entity = commandBuffer.Create().Set(new EcsHealth(10)).Entity;
        commandBuffer.Execute();

        Assert.True(entity.TryResolve<IEcsDamageable>(out var damageable));
        Assert.NotNull(damageable);
        Assert.IsType<DefaultDamageable>(damageable);

        // Register another resolver that filters for the first
        // This looks like a circular dependency, but is allowed
        // because resolvers do not depend on themselves
        world.RegisterInterfaceResolver<IEcsDamageable>(
            new QueryFilter().Include<IEcsDamageable>(),
            (previous, _) => new ShieldedDamageable(previous!));

        Assert.True(entity.TryResolve(out damageable));
        Assert.NotNull(damageable);
        Assert.IsType<ShieldedDamageable>(damageable);
    }

    [Fact]
    public void CircularDependency_Throws()
    {
        using var world = new EcsWorld();

        // Ensure at least 1 archetype exists to force the resolver sorting to occur every time a new resolver is added
        using var _ = world.AcquireCommandBuffer(out var commandBuffer);
        commandBuffer.Create();
        commandBuffer.Execute();

        world.RegisterInterfaceResolver<IEcsInterface0>(
            new QueryFilter().Include<IEcsInterface1>(),
            (_, _) => null);

        Assert.Throws<GuardException>(() =>
        {
            world.RegisterInterfaceResolver<IEcsInterface1>(
                new QueryFilter().Include<IEcsInterface0>(),
                (_, _) => null);
        });
    }

    private struct EcsHealth : IComponent
    {
        public int Health;

        public EcsHealth(int health)
        {
            Health = health;
        }
    }

    private class DefaultDamageable : IEcsDamageable
    {
        public void Damage(ref EcsHealth health, int amount)
        {
            health.Health -= amount;
        }
    }

    private class ShieldedDamageable : IEcsDamageable
    {
        private readonly IEcsDamageable inner;

        public ShieldedDamageable(IEcsDamageable inner)
        {
            this.inner = inner;
        }

        public void Damage(ref EcsHealth health, int amount)
        {
            inner.Damage(ref health, amount / 2);
        }
    }

    private interface IEcsDamageable : IInterfaceComponent
    {
        void Damage(ref EcsHealth health, int amount);
    }
}
