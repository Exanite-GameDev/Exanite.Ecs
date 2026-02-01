namespace Exanite.Myriad.Ecs.CommandBuffers;

/// <summary>
/// An entity that is being processed by a command buffer.
/// Mainly used for fluent method chaining.
/// </summary>
public readonly record struct BufferedEntity
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
}
