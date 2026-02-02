namespace Exanite.Myriad.Ecs.CommandBuffers;

public partial class EcsCommandBuffer
{
    private void ExecuteInternal()
    {
        using var _ = World.AcquireCommandBuffer(out var recursiveCommandBuffer);

        // TODO

        // Release unused local IDs
        World.Entities.ReleaseUnusedIds(localIdPool);

        // Clear commands
        State.Clear(World, false);

        recursiveCommandBuffer.Execute();
    }
}
