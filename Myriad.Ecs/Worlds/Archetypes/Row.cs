using Exanite.Myriad.Ecs.Worlds.Chunks;

namespace Exanite.Myriad.Ecs.Worlds.Archetypes;

internal readonly record struct Row
{
    public EntityId Entity { get; }
    public int RowIndex { get; }
    public Chunk Chunk { get; }

    internal Row(EntityId entity, int rowIndex, Chunk chunk)
    {
        Entity = entity;
        RowIndex = rowIndex;
        Chunk = chunk;
    }

    public ref T GetMutable<T>()
        where T : IComponent
    {
        return ref Chunk.Get<T>(Entity, RowIndex);
    }
}
