namespace Exanite.Myriad.Ecs;

/// <summary>
/// <para>
/// A phantom component acts like a normal component until the entity is destroyed. At
/// that point instead of being destroyed the entity will automatically have a
/// <see cref="ComponentPhantom"/> component added.
/// </para>
/// <para>
/// If an entity with a <see cref="ComponentPhantom"/> entity is destroyed, it will
/// actually be destroyed. It will also automatically be destroyed if it has no more
/// <see cref="IComponentPhantom"/> components attached.
/// </para>
/// </summary>
public interface IComponentPhantom : IComponent;
