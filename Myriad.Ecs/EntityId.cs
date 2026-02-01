using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Exanite.Myriad.Ecs;

/// <summary>
/// The ID of an <see cref="Entity"/> (not carrying a reference to a <see cref="EcsWorld"/>)
/// </summary>
[DebuggerDisplay("{Index}v{Version}")]
internal readonly record struct EntityId : IComparable<EntityId>
{
    /// <summary>
    /// The <see cref="Entity"/> of an entity, may be re-used very quickly once an <see cref="Entity"/> is destroyed.
    /// </summary>
    public readonly int Index;

    /// <summary>
    /// The version number of this ID, may also be re-used but only after the full 32 bit counter has been overflowed for this specific ID.
    /// </summary>
    public readonly uint Version;

    internal EntityId(int index, uint version)
    {
        Index = index;
        Version = version;
    }

    /// <summary>
    /// Create a new <see cref="Entity"/> struct that represents this Entity
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Entity ToEntity(EcsWorld world)
    {
        return new Entity(this, world);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return $"#{Index} v{Version}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(EntityId other)
    {
        var order = Index.CompareTo(other.Index);
        if (order != 0)
        {
            return order;
        }

        return Version.CompareTo(other.Version);
    }
}
