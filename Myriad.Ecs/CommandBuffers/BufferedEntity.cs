using System;

namespace Exanite.Myriad.Ecs.CommandBuffers;

/// <summary>
/// An entity that is being processed by a command buffer.
/// Mainly used for fluent method chaining.
/// </summary>
public readonly record struct BufferedEntity : IEquatable<Entity>
{
    public readonly Entity Entity;
    public readonly EcsCommandBuffer CommandBuffer;

    internal BufferedEntity(Entity entity, EcsCommandBuffer commandBuffer)
    {
        Entity = entity;
        CommandBuffer = commandBuffer;
    }

    public static implicit operator Entity(BufferedEntity value)
    {
        return value.Entity;
    }

    public static bool operator ==(BufferedEntity a, Entity b) => a.Entity == b;
    public static bool operator !=(BufferedEntity a, Entity b) => a.Entity != b;

    public static bool operator ==(Entity a, BufferedEntity b) => a == b.Entity;
    public static bool operator !=(Entity a, BufferedEntity b) => a != b.Entity;

    /// <inheritdoc cref="EcsCommandBuffer.Set"/>
    public BufferedEntity Set<T>(T value) where T : IComponent
    {
        return CommandBuffer.Set(Entity, value);
    }

    /// <inheritdoc cref="EcsCommandBuffer.Remove"/>
    public BufferedEntity Remove<T>() where T : IComponent
    {
        return CommandBuffer.Remove<T>(Entity);
    }

    public bool Equals(Entity other)
    {
        return Entity.Equals(other);
    }

    public override int GetHashCode()
    {
        return Entity.GetHashCode();
    }
}
