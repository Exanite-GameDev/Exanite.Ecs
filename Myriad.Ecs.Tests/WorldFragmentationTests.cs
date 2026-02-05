using Exanite.Myriad.Ecs.Queries;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class WorldFragmentationTests
{
    [Fact]
    public void AddTo_DoesNotFragmentChunks()
    {
        var srcWorld = new EcsWorld();
        using (srcWorld.AcquireCommandBuffer(out var commandBuffer))
        {
            commandBuffer.Create();
            commandBuffer.Execute();
        }

        var dstWorld = new EcsWorld();
        var addCount = 10;
        for (var i = 0; i < addCount; i++)
        {
            srcWorld.AddTo(dstWorld);
        }

        Assert.Equal(1, dstWorld.Archetypes.Length);
        Assert.Equal(1, dstWorld.Archetypes[0].Chunks.Length);

        Assert.Equal(addCount, dstWorld.Count());
        Assert.Equal(addCount, dstWorld.Archetypes[0].EntityCount);
        Assert.Equal(addCount, dstWorld.Archetypes[0].Chunks[0].Entities.Length);
    }

    [Fact]
    public void Clear_LeavesNoEmptyChunks()
    {
        var world = new EcsWorld();
        using (world.AcquireCommandBuffer(out var commandBuffer))
        {
            commandBuffer.Create();
            commandBuffer.Execute();
        }

        world.Clear();

        Assert.Equal(0, world.Count());
        Assert.Equal(1, world.Archetypes.Length);
        Assert.Equal(0, world.Archetypes[0].Chunks.Length);
    }

    [Fact]
    public void DestroyingAllEntities_LeavesNoEmptyChunks()
    {
        var world = new EcsWorld();
        using var _ = world.AcquireCommandBuffer(out var commandBuffer);
        commandBuffer.Create();
        commandBuffer.Execute();

        var allEntitiesQuery = new QueryFilter().Build(world);
        commandBuffer.Destroy(allEntitiesQuery);
        commandBuffer.Execute();

        Assert.Equal(0, world.Count());
        Assert.Equal(1, world.Archetypes.Length);
        Assert.Equal(0, world.Archetypes[0].Chunks.Length);
    }

    [Fact]
    public void CreatingMaxPlusOneEntities_ThenDestroyingFirst_DoesNotFragmentChunks()
    {
        var world = new EcsWorld();
        using var _ = world.AcquireCommandBuffer(out var commandBuffer);

        var first = commandBuffer.Create();
        for (var i = 0; i < EcsConstants.ChunkEntityCount; i++)
        {
            commandBuffer.Create();
        }

        commandBuffer.Execute();

        // Should have two chunks
        Assert.Equal(EcsConstants.ChunkEntityCount + 1, world.Count());
        Assert.Equal(1, world.Archetypes.Length);
        Assert.Equal(2, world.Archetypes[0].Chunks.Length);

        commandBuffer.Destroy(first);
        commandBuffer.Execute();

        // Should have one chunk
        Assert.Equal(EcsConstants.ChunkEntityCount, world.Count());
        Assert.Equal(1, world.Archetypes.Length);
        Assert.Equal(1, world.Archetypes[0].Chunks.Length);
    }
}
