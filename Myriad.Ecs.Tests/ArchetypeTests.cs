using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Queries;
using NUnit.Framework;

namespace Exanite.Myriad.Ecs.Tests;

[TestFixture]
public class ArchetypeTests
{
    [Test]
    public void EnumerateManyEntities()
    {
        var w = new EcsWorld();

        var cb = new EcsCommandBuffer(w);
        for (var i = 0; i < 10000; i++)
        {
            var e = cb.Create();
            e.Set(new ComponentInt32(i)).Set(new ComponentInt64(i));
            if (i % 7 == 0)
            {
                e.Set(new ComponentFloat(i));
            }
        }
        cb.Execute();

        foreach (var archetype in w.Archetypes)
        {
            var entityCount = 0;
            foreach (var chunk in archetype.Chunks)
            {
                entityCount += chunk.EntityCount;
            }

            Assert.That(entityCount, Is.EqualTo(archetype.EntityCount));
        }

        var query = new QueryBuilder().Include<ComponentInt32>().Include<ComponentInt64>().Build(w);
        Assert.That(query.Count(), Is.EqualTo(10000));

        foreach (var archetypeMatch in query.GetArchetypes())
        {
            foreach (var chunk in archetypeMatch.Chunks)
            {
                var a = chunk.GetSpan<ComponentInt32>();
                var b = chunk.GetSpan<ComponentInt64>();

                for (var i = 0; i < chunk.EntityCount; i++)
                {
                    Assert.That((int)b[i].Value, Is.EqualTo(a[i].Value));
                }
            }
        }
    }
}
