namespace Myriad.Ecs.Threading;

internal class CountdownEventContainer
{
    public CountdownEvent Event { get; } = new(0);
}
