namespace Exanite.Myriad.Ecs.Components;

public interface IComponentSelfReference<T> where T : IComponent
{
    /// <summary>
    /// A reference to this component when attached to an entity.
    /// </summary>
    /// <remarks>
    /// This will be set before <see cref="IComponentSetup"/>'s <see cref="IComponentSetup.Setup()"/> is called.
    /// </remarks>
    public ComponentRef<T> Self { get; set; }
}
