using Microsoft.VisualStudio.TestTools.UnitTesting;
using Myriad.Ecs.CommandBuffers;
using Myriad.Ecs.Worlds;

namespace Myriad.Ecs.Tests.Queries
{
    [TestClass]
    public class MapReduceQueryTests
    {
        [TestMethod]
        public void MapReduceNone()
        {
            var world = new World();

            var result = world.ExecuteMapReduce<MapGetInteger, Reduce.I32.Mul, int, ComponentInt32>(new MapGetInteger(), new Reduce.I32.Mul(), 2);

            Assert.AreEqual(2, result);
        }

        [TestMethod]
        public void MapReduceOne()
        {
            var world = new World();

            var cmd = new EcsCommandBuffer(world);
            cmd.Create().Set(new ComponentInt32(7));
            cmd.Playback().Dispose();

            var result = world.ExecuteMapReduce<MapGetInteger, Reduce.I32.Mul, int, ComponentInt32>(new MapGetInteger(), new Reduce.I32.Mul(), 2);

            Assert.AreEqual(14, result);
        }

        [TestMethod]
        public void MapReduceManyAdd()
        {
            var world = new World();

            var cmd = new EcsCommandBuffer(world);
            var sum = 0;
            for (var i = 0; i < 9999; i++)
            {
                sum += i;
                cmd.Create().Set(new ComponentInt32(i));
            }
            cmd.Playback().Dispose();

            var result = world.ExecuteMapReduce<MapGetInteger, Reduce.I32.Add, int, ComponentInt32>(0);

            Assert.AreEqual(sum, result);
        }

        [TestMethod]
        public void MapReduceManyMul()
        {
            var world = new World();

            var cmd = new EcsCommandBuffer(world);
            var prod = 0;
            for (var i = 1; i < 9999; i++)
            {
                prod *= i;
                cmd.Create().Set(new ComponentInt32(i));
            }
            cmd.Playback().Dispose();

            var result = world.ExecuteMapReduce<MapGetInteger, Reduce.I32.Mul, int, ComponentInt32>(0);

            Assert.AreEqual(prod, result);
        }

        [TestMethod]
        public void MapReduceManyMinMax()
        {
            var world = new World();

            var cmd = new EcsCommandBuffer(world);
            for (var i = 0; i < 9999; i++)
            {
                cmd.Create().Set(new ComponentInt32(i));
            }
            cmd.Playback().Dispose();


            Assert.AreEqual(0, world.ExecuteMapReduce<MapGetInteger, Reduce.I32.Min, int, ComponentInt32>(0));
            Assert.AreEqual(9998, world.ExecuteMapReduce<MapGetInteger, Reduce.I32.Max, int, ComponentInt32>(0));
        }

        [TestMethod]
        public void MapReduceManyBitwise()
        {
            var world = new World();

            var cmd = new EcsCommandBuffer(world);
            var rng = new Random(346534);

            var xor = 0;
            var and = ~0;
            var or = 0;

            for (var i = 0; i < 9999; i++)
            {
                var v = rng.Next();

                xor ^= v;
                and &= v;
                or |= v;

                cmd.Create().Set(new ComponentInt32(v));
            }
            cmd.Playback().Dispose();


            Assert.AreEqual(xor, world.ExecuteMapReduce<MapGetInteger, Reduce.I32.Xor, int, ComponentInt32>( 0));
            Assert.AreEqual(and, world.ExecuteMapReduce<MapGetInteger, Reduce.I32.And, int, ComponentInt32>(~0));
            Assert.AreEqual( or, world.ExecuteMapReduce<MapGetInteger, Reduce.I32. Or, int, ComponentInt32>( 0));
        }

        private struct MapGetInteger
            : IQueryMap<int, ComponentInt32>
        {
            public readonly int Execute(Worlds.Entity e, ref ComponentInt32 t0)
            {
                return t0.Value;
            }
        }
    }
}
