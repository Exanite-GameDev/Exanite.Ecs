using System.Numerics;
using Myriad.Ecs.Command;
using Myriad.Ecs.Components;

namespace Myriad.Ecs.Tests;

public record struct ComponentByte(byte Value) : IComponent;
public record struct ComponentInt16(short Value) : IComponent;
public record struct ComponentFloat(float Value) : IComponent;
public record struct ComponentInt32(int Value) : IComponent;
public record struct ComponentInt64(long Value) : IComponent;
public record struct ComponentObject(object Value) : IComponent;
public record struct ComponentVector3(Vector3 Value) : IComponent;

public record struct Component0 : IComponent;
public record struct Component1 : IComponent;
public record struct Component2 : IComponent;
public record struct Component3 : IComponent;
public record struct Component4 : IComponent;
public record struct Component5 : IComponent;
public record struct Component6 : IComponent;
public record struct Component7 : IComponent;
public record struct Component8 : IComponent;
public record struct Component9 : IComponent;
public record struct Component10 : IComponent;
public record struct Component11 : IComponent;
public record struct Component12 : IComponent;
public record struct Component13 : IComponent;
public record struct Component14 : IComponent;
public record struct Component15 : IComponent;
public record struct Component16 : IComponent;
public record struct Component17 : IComponent;

public record struct TestPhantom0 : IComponentPhantom;
public record struct TestPhantom1 : IComponentPhantom;
public record struct TestPhantom2 : IComponentPhantom;

public record struct PhantomNotifier : IPhantomNotifierComponent
{
    public required List<EntityId> CalledWith;

    public void OnBecomePhantom(EntityId self)
    {
        CalledWith.Add(self);
    }
}

public record struct Relational1(Entity Target) : IEntityRelationComponent;
public record struct Relational2(Entity Target, int X) : IEntityRelationComponent;
public record struct Relational3(Entity Target, float Y) : IEntityRelationComponent;

public class BoxedInt
{
    public string? Id;
    public int Value;
}

public readonly record struct TestDisposable
    : IDisposableComponent
{
    private readonly BoxedInt box;

    public TestDisposable(BoxedInt box)
    {
        this.box = box;
    }

    public void Dispose(ref LazyCommandBuffer lazy)
    {
        box.Value++;
    }
}

public record struct TestDisposableParent
    : IDisposableComponent, IEntityRelationComponent
{
    public Entity Target { get; set; }

    public void Dispose(ref LazyCommandBuffer lazy)
    {
        lazy.CommandBuffer.Delete(Target);
    }
}

public readonly record struct TestDisposablePhantom
    : IDisposableComponent, IComponentPhantom
{
    private readonly BoxedInt box;

    public TestDisposablePhantom(BoxedInt box)
    {
        this.box = box;
    }

    public void Dispose(ref LazyCommandBuffer laz)
    {
        box.Value++;
    }
}
