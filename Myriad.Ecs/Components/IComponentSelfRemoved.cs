namespace Exanite.Myriad.Ecs.Components;

public interface IComponentSelfRemoved
{
    /// <summary>
    /// Called when the component is removed from an entity.
    /// </summary>
    public void OnRemoved();
}
