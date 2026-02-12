namespace Exanite.Myriad.Ecs;

/// <summary>
/// Marker interface for storage-backed components.
/// These components must be implemented as structs and are used to attach data to each entity.
/// </summary>
public interface IComponent : IEcsType;
