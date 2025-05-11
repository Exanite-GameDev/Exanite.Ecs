using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.Components;

namespace Exanite.Myriad.Ecs;

/// <summary>
/// The ID of an <see cref="Ecs.Entity"/> (not carrying a reference to a <see cref="World"/>)
/// </summary>
[DebuggerDisplay("{Id}v{Version}")]
internal readonly record struct EntityId : IComparable<EntityId>
{
    /// <summary>
    /// The <see cref="Ecs.Entity"/> of an entity, may be re-used very quickly once an <see cref="Ecs.Entity"/> is destroyed.
    /// </summary>
    public readonly int Id;

    /// <summary>
    /// The version number of this ID, may also be re-used but only after the full 32 bit counter has been overflowed for this specific ID.
    /// </summary>
    public readonly uint Version;

    internal EntityId(int id, uint version)
    {
        Id = id;
        Version = version;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return $"{Id} v{Version}";
    }

    /// <summary>
    /// Create a new <see cref="Ecs.Entity"/> struct that represents this Entity
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Entity ToEntity(World world)
    {
        return new Entity(this, world);
    }

    /// <summary>
    /// Check if this Entity still exists.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsAlive(World world)
    {
        return Id != 0 && world.GetVersion(Id) == Version;
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(EntityId other)
    {
        var idc = Id.CompareTo(other.Id);
        if (idc != 0)
        {
            return idc;
        }

        return Version.CompareTo(other.Version);
    }

    /// <summary>
    /// Get a unique 64 bit ID for this entity
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long UniqueId()
    {
        // Set the entity ID and version into the hi and lo 32 bits
        var u = new Union64
        {
            I0 = Id,
            U1 = Version
        };

        // Swap around some bytes (this is effectively an injective hash)
        Swap(ref u.B0, ref u.B1);
        Swap(ref u.B2, ref u.B3);
        Swap(ref u.B4, ref u.B5);
        Swap(ref u.B6, ref u.B7);
        unchecked
        {
            u.I0 *= 1297519;
            u.I1 *= 722479;
        }
        Swap(ref u.B4, ref u.B1);
        Swap(ref u.B7, ref u.B3);
        Swap(ref u.B0, ref u.B2);
        Swap(ref u.B6, ref u.B5);

        return u.Long;

        static void Swap(ref byte a, ref byte b)
        {
            (a, b) = (b, a);
        }
    }

    /// <summary>
    /// Get the set of components which this entity currently has
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableOrderedListSet<ComponentId> GetComponents(World world)
    {
        var location = world.GetStorageLocation(this);
        return location.Chunk.Archetype.Components;
    }

    /// <summary>
    /// Check if this entity has a component
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasComponent<T>(World world) where T : IComponent
    {
        return GetComponents(world).Contains(ComponentId.Get<T>());
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref T GetComponent<T>(World world) where T : IComponent
    {
        return ref GetComponentRef<T>(world).Value;
    }

    /// <summary>
    /// Get a reference to a component of the given type. If the entity
    /// does not have this component an exception will be thrown.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueRef<T> GetComponentRef<T>(World world) where T : IComponent
    {
        ref var entityInfo = ref world.GetStorageLocation(this);
        return entityInfo.Chunk.GetRef<T>(this, entityInfo.IndexInChunk);
    }

    /// <summary>
    /// Get a <b>boxed copy</b> of a component from this entity. Only use for debugging!
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object? GetBoxedComponent(World world, ComponentId id)
    {
        if (!IsAlive(world))
        {
            return null;
        }

        if (!GetComponents(world).Contains(id))
        {
            return null;
        }

        ref var entityInfo = ref world.GetStorageLocation(this);
        return entityInfo.Chunk.GetComponentArray(id).GetValue(entityInfo.IndexInChunk);
    }
}
