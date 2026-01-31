using System;
using Exanite.Myriad.Ecs.CommandBuffers;

namespace Exanite.Myriad.Ecs.Tests;

public static class TestHelpers
{
    public static EcsCommandBuffer SetupRandomEntities(EcsWorld world, uint uniqueComponents = 7, int count = 1_000_000)
    {
        uniqueComponents = Math.Clamp(uniqueComponents, 0, 7);

        var b = world.AcquireCommandBuffer();
        var r = new Random(123);
        for (var i = 0; i < count; i++)
        {
            var eb = b.Create();

            for (var j = 0; j < 5; j++)
            {
                switch (r.Next(0, checked((int)uniqueComponents)))
                {
                    case 0: eb.Set(new EcsByte(0)); break;
                    case 1: eb.Set(new EcsInt16(0)); break;
                    case 2: eb.Set(new EcsFloat(0)); break;
                    case 3: eb.Set(new EcsInt32(0)); break;
                    case 4: eb.Set(new EcsInt64(0)); break;
                    case 5: eb.Set(new Ecs0()); break;
                    case 6: eb.Set(new Ecs1()); break;
                }
            }
        }

        return b;
    }
}
