namespace Exanite.Myriad.Ecs.Components;

public interface IComponentSelfReference<T> where T : IComponent
{
    public ComponentRef<T> Self { get; set; }
}
