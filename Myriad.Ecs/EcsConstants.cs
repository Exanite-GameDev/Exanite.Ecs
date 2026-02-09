using Exanite.Myriad.Ecs.Collections;

namespace Exanite.Myriad.Ecs;

internal static class EcsConstants
{
    /// <summary>
    /// Defines the power of 2 base capacity of the <see cref="SegmentedList{T}"/>.
    /// </summary>
    internal const int SegmentedListBaseCapacityPowerOf2 = 10;

    /// <summary>
    /// The initial entity capacity of an archetype.
    /// </summary>
    internal const int ArchetypeInitialCapacity = 1024;

    /// <summary>
    /// Number of local entity IDs to keep per command buffer for the purpose of avoiding thread contention.
    /// </summary>
    internal const int CommandBufferLocalIdCount = 32;
}
