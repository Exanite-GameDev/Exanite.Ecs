using Exanite.Core.Utilities;

namespace Exanite.Myriad.Ecs.CommandBuffers;

/// <summary>
/// An entity that has been created in a command buffer, but not yet created. Can be used to add components.
/// </summary>
public readonly record struct BufferedEntity
{
    private readonly uint id;
    private readonly uint version;

    private readonly EcsCommandBufferResolver resolver;

    /// <summary>
    /// Get the <see cref="EcsCommandBuffer"/> which this <see cref="BufferedEntity"/> is from.
    /// </summary>
    public EcsCommandBuffer CommandBuffer { get; }

    internal BufferedEntity(uint id, EcsCommandBufferResolver resolver)
    {
        this.id = id;
        this.resolver = resolver;

        CommandBuffer = resolver.CommandBuffer;
        version = resolver.Version;
    }

    /// <summary>
    /// Add or overwrite a component attached to this entity.
    /// </summary>
    /// <typeparam name="T">The type of component to add.</typeparam>
    /// <param name="value">The value of the component to add.</param>
    /// <returns>This buffered entity.</returns>
    public BufferedEntity Set<T>(T value) where T : IComponent
    {
        unchecked
        {
            GuardUtility.IsTrue(resolver.Version == version && CommandBuffer.Version == version - 1, "Cannot modify buffered entity after command buffer has been executed");
        }

        CommandBuffer.SetBuffered(id, value);
        return this;
    }

    /// <summary>
    /// Resolve this <see cref="BufferedEntity"/> into the real <see cref="Entity"/> that was constructed.
    /// </summary>
    public Entity Resolve()
    {
        GuardUtility.IsTrue(resolver.Version == version && CommandBuffer.Version == version, "Buffered entity is no longer valid and cannot be resolved");

        return resolver.EntityIdsByBufferedEntityId[id].ToEntity(resolver.World);
    }
}
