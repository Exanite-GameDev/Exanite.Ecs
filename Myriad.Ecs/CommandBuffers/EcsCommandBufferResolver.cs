using System.Collections.Generic;

namespace Exanite.Myriad.Ecs.CommandBuffers;

/// <summary>
/// Used to resolve <see cref="Entity"/>s from <see cref="BufferedEntity"/>s.
/// Can also be used to resolve the list of entities created in the last command buffer execution.
/// </summary>
public sealed class EcsCommandBufferResolver
{
    internal uint Version { get; private set; }

    internal readonly EcsCommandBuffer CommandBuffer;
    internal readonly SortedList<uint, EntityId> EntityIdsByBufferedEntityId = [];

    /// <summary>
    /// The <see cref="World"/> this resolver is for.
    /// </summary>
    public EcsWorld World => CommandBuffer.World;

    /// <summary>
    /// Get the number of entities in this <see cref="EcsCommandBufferResolver"/>
    /// </summary>
    public int Count => EntityIdsByBufferedEntityId.Count;

    /// <summary>
    /// Get the nth entity in this <see cref="EcsCommandBufferResolver"/>. Entities are in an arbitrary order.
    /// </summary>
    public Entity this[int index] => EntityIdsByBufferedEntityId.Values[index].ToEntity(World);

    public EcsCommandBufferResolver(EcsCommandBuffer commandBuffer)
    {
        this.CommandBuffer = commandBuffer;
    }

    internal void Reset()
    {
        EntityIdsByBufferedEntityId.Clear();

        unchecked
        {
            Version = CommandBuffer.Version + 1;
        }
    }
}
