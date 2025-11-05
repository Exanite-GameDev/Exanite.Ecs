namespace Exanite.Myriad.Ecs.Components;

public interface IComponentSetup
{
    /// <summary>
    /// Called when the component is added to an entity.
    /// </summary>
    public void Setup();
}