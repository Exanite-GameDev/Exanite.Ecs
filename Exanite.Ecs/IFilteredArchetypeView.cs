namespace Exanite.Myriad.Ecs;

/// <summary>
/// Represents a high-performance filtered collection of archetypes.
/// </summary>
/// <remarks>
/// Use this interface when it is expected that the matched entities share a specific set of component types.
/// This is notably useful in queries because queries usually iterate over a specific set of component types.
/// </remarks>
public interface IFilteredArchetypeView : IArchetypeView;
