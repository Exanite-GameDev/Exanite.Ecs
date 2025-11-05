namespace Exanite.Myriad.Ecs.Components;

public interface IComponentSelfAdded
{
    /// <summary>
    /// Called when the component is added to an entity.
    /// </summary>
    public void OnAdded();
}
