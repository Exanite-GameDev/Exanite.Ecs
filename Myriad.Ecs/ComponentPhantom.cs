namespace Exanite.Myriad.Ecs;

/// <summary>
/// <para>
/// Indicates that the entity this is attached to is a "phantom". Phantom entities
/// are automatically excluded from queries and must be specifically requested.
/// </para>
/// <para>An entity will automatically become a phantom if it is destroyed, but still has
/// <see cref="IComponentPhantom"/> components attached.
/// </para>
/// <para>
/// If an entity with a <see cref="ComponentPhantom"/> component is destroyed, it will
/// actually be destroyed. It will automatically be destroyed if it has no more
/// <see cref="IComponentPhantom"/> components attached.
/// </para>
/// </summary>
public struct ComponentPhantom : IComponent;
