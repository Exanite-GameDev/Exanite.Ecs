using Exanite.Core.Utilities;

namespace Exanite.Myriad.Ecs.CommandBuffers;

public partial class EcsCommandBuffer
{
    private void ExecuteInternal()
    {
        using var _ = World.AcquireCommandBuffer(out var recursiveCommandBuffer);

        // TODO: Decide if I want to coalesce sets followed by removes
        // Same with creates followed by destroys
        // I probably do, since prefabs can have repeated sets and removes.

        foreach (var command in State.Commands)
        {
            switch (command.Type)
            {
                case CommandType.CreateEntity:
                {
                    break;
                }
                case CommandType.DestroyEntity:
                {
                    break;
                }
                case CommandType.DestroyArchetypeView:
                {
                    break;
                }
                case CommandType.SetComponent:
                {
                    break;
                }
                case CommandType.RemoveComponent:
                {
                    break;
                }
                case CommandType.DeferredAction:
                {
                    break;
                }
                default: throw ExceptionUtility.NotSupportedEnumValue(command.Type);
            }
        }

        // Release unused local IDs
        World.Entities.ReleaseUnusedIds(localIdPool);

        // Clear commands
        State.Clear(World, false);

        recursiveCommandBuffer.Execute();
    }
}
