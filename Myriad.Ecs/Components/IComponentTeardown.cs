namespace Exanite.Myriad.Ecs.Components;

public interface IComponentTeardown
{
    /// <summary>
    /// Called when the component is removed from an entity.
    /// </summary>
    public void Teardown();
}
