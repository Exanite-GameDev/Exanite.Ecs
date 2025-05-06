using BenchmarkDotNet.Attributes;
using Myriad.Ecs.Benchmarks.Components;
using Myriad.Ecs.Command;
using Myriad.Ecs.Worlds;

namespace Myriad.Ecs.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class EntityCreateBenchmark
{
    private const int COUNT = 1_000_000;

    private World _world = null!;

    [GlobalSetup]
    public void Setup()
    {
        _world = new WorldBuilder().Build();
    }

    private static void AddEntity(EcsCommandBuffer buffer, Random random)
    {
        var entity = buffer.Create();

        for (var i = 0; i < 5; i++)
        {
            switch (random.Next(0, 5))
            {
                case 0: entity.Set(new ComponentByte((byte)i)); break;
                case 1: entity.Set(new ComponentInt16((short)i)); break;
                case 2: entity.Set(new ComponentFloat(i)); break;
                case 3: entity.Set(new ComponentInt32(i)); break;
                case 4: entity.Set(new ComponentInt64(i)); break;
            }
        }
    }

    [Benchmark]
    public void CreateBuffered()
    {
        var rng = new Random(1);
        var buffer = new EcsCommandBuffer(_world);

        for (var i = 0; i < COUNT; i++)
            AddEntity(buffer, rng);

        using var resolver = buffer.Playback();
    }

    [Benchmark]
    public void CreateUnbuffered()
    {
        var rng = new Random(1);
        var buffer = new EcsCommandBuffer(_world);

        for (var i = 0; i < COUNT; i++)
        {
            AddEntity(buffer, rng);
            using var resolver = buffer.Playback();
        }
    }
}
