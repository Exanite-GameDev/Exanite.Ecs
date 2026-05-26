namespace Exanite.Myriad.Ecs.Tests;

public interface IEcsInterface0 : IEcsInterface;

public interface IEcsInterface1 : IEcsInterface;

public interface IEcsInterface2 : IEcsInterface;

public record struct EcsByte(byte Value) : IEcsComponent;

public record struct EcsInt16(short Value) : IEcsComponent;

public record struct EcsFloat(float Value) : IEcsComponent;

public record struct EcsInt32(int Value) : IEcsComponent;

public record struct EcsInt64(long Value) : IEcsComponent;

public record struct Ecs0 : IEcsComponent;

public record struct Ecs1 : IEcsComponent;

public record struct Ecs2 : IEcsComponent;

public record struct Ecs3 : IEcsComponent;

public record struct Ecs4 : IEcsComponent;

public record struct Ecs5 : IEcsComponent;

public record struct Ecs6 : IEcsComponent;

public record struct Ecs7 : IEcsComponent;

public record struct Ecs8 : IEcsComponent;

public record struct Ecs9 : IEcsComponent;

public record struct Ecs10 : IEcsComponent;

public record struct Ecs11 : IEcsComponent;

public record struct Ecs12 : IEcsComponent;

public record struct Ecs13 : IEcsComponent;

public record struct Ecs14 : IEcsComponent;

public record struct Ecs15 : IEcsComponent;

public record struct Ecs16 : IEcsComponent;

public record struct Ecs17 : IEcsComponent;

public record struct EcsGeneric<T> : IEcsComponent
{
    public T Value;
}
