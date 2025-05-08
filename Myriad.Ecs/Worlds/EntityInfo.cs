using Exanite.Myriad.Ecs.Worlds.Archetypes;
using Exanite.Myriad.Ecs.Worlds.Chunks;

namespace Exanite.Myriad.Ecs.Worlds;

internal struct StorageLocation
{
    /// <summary>
    /// The current version of this entity
    /// </summary>
    public uint Version;

    /// <summary>
    /// The chunk in the archetype which contains this entity
    /// </summary>
    public Chunk Chunk;

    /// <summary>
    /// The row in the chunk which contains this entity
    /// </summary>
    public int RowIndex;

    public readonly EntityStorageLocation GetEntityStorageLocation(EntityId entity)
    {
        return new EntityStorageLocation(entity, RowIndex, Chunk);
    }
}
