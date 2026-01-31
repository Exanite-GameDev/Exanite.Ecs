namespace Exanite.Myriad.Ecs.Components;

public interface IComponentSelfReference<T> where T : IComponent
{
    /// <summary>
    /// A reference to this component when attached to an entity.
    /// </summary>
    /// <remarks>
    /// This will be set before <see cref="IComponentSelfAdded"/>'s <see cref="IComponentSelfAdded.OnAdded"/> or <see cref="IComponentSelfCopied.OnCopied"/> is called.
    /// </remarks>
    public EcsRef<T> Self { get; set; }
}
