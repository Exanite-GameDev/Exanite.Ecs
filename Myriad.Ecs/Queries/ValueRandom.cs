using System;
using Myriad.Ecs.xxHash;
using System.Runtime.InteropServices;

namespace Myriad.Ecs.Queries;

internal struct ValueRandom
{
    private int seed;

    public ValueRandom(int seed)
    {
        this.seed = seed;
    }

    public int Next()
    {
        Span<int> seed = stackalloc int[] { this.seed };

        // Hash the state, to generate 64 bits
        var byteSpan = MemoryMarshal.Cast<int, byte>(seed);
        var hash = xxHash64.ComputeHash(byteSpan, seed: 568456);

        // Take the low bits and the high bits as two 32 bit values
        var low = (uint)(hash & 0xFFFF_FFFF);
        var hi = (uint)((hash >> 32) & 0xFFFF_FFFF);

        // Next state is the high bits
        this.seed = unchecked((int)hi);

        // Result is the low bits
        return unchecked((int)low);
    }
}
