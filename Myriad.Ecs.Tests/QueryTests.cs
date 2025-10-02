using System;
using System.Collections.Generic;
using System.Linq;
using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Queries;
using Exanite.Myriad.Ecs.Worlds.Archetypes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Exanite.Myriad.Ecs.Tests;

[TestClass]
public class QueryTests
{
    [TestMethod]
    public void IncludeMatchNone()
    {
        var world = new EcsWorld();

        var q = new QueryBuilder()
           .Include<ComponentFloat>()
           .Build(world);

        var archetypes = q.GetArchetypes();
        Assert.AreEqual(0, archetypes.Length);
    }

    [TestMethod]
    public void IncludeMatchOne()
    {
        var w = new EcsWorld();
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>()]);

        var q = new QueryBuilder()
           .Include<ComponentFloat>()
           .Build(w);

        var a = q.GetArchetypes();
        Assert.AreEqual(1, a.Length);
    }

    [TestMethod]
    public void IncludeMatchCaching()
    {
        var w = new EcsWorld();
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>()]);

        // Query that matches just one of the archetypes in the world
        var q = new QueryBuilder()
           .Include<ComponentFloat>()
           .Build(w);

        // Match once, check it matches one archetype
        var a = q.GetArchetypes();
        Assert.AreEqual(1, a.Length);

        // Add an archetype to the world that the query should match
        var c1 = new OrderedListSet<ComponentId>(new HashSet<ComponentId> { ComponentId.Get<ComponentInt32>(), ComponentId.Get<ComponentFloat>() });
        w.GetOrCreateArchetype(c1, ArchetypeHash.Create(c1));

        // Check it now matches 2 archetypes
        var b = q.GetArchetypes();
        Assert.AreEqual(2, b.Length);
        foreach (var archetype in b)
        {
            Assert.IsTrue(archetype.Components.Contains(ComponentId.Get<ComponentFloat>()));
        }

        // Add an archetype to the world that the query should NOT match
        var c2 = new OrderedListSet<ComponentId>(new HashSet<ComponentId> { ComponentId.Get<ComponentInt32>(), ComponentId.Get<ComponentByte>() });
        w.GetOrCreateArchetype(c2, ArchetypeHash.Create(c2));

        // Check it now matches 2 archetypes
        var c = q.GetArchetypes();
        Assert.AreEqual(2, c.Length);
        foreach (var archetype in c)
        {
            Assert.IsTrue(archetype.Components.Contains(ComponentId.Get<ComponentFloat>()));
        }
    }

    [TestMethod]
    public void IncludeMatchMultiple()
    {
        var w = new EcsWorld();
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>(), ComponentId.Get<ComponentInt32>()]);

        var q = new QueryBuilder()
           .Include<ComponentFloat>()
           .Build(w);

        var archetypes = q.GetArchetypesList();
        Assert.AreEqual(2, archetypes.Count);

        Assert.IsTrue(archetypes.All(x => x.Components.Contains(ComponentId.Get<ComponentFloat>())));
    }

    [TestMethod]
    public void IncludeExclude()
    {
        var w = new EcsWorld();
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>(), ComponentId.Get<ComponentInt32>()]);

        var q = new QueryBuilder()
            .Include<ComponentFloat>()
            .Exclude<ComponentInt32>()
            .Build(w);

        var archetypes = q.GetArchetypes();
        Assert.AreEqual(1, archetypes.Length);

        var archetype = archetypes[0];
        Assert.IsTrue(archetype.Components.Contains(ComponentId.Get<ComponentFloat>()));
        Assert.IsFalse(archetype.Components.Contains(ComponentId.Get<ComponentInt32>()));
    }

    [TestMethod]
    public void ExactlyOne()
    {
        var w = new EcsWorld();
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt16>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>(), ComponentId.Get<ComponentInt32>()]);

        var q = new QueryBuilder()
                .ExactlyOne<ComponentFloat>()
                .ExactlyOne<ComponentInt32>()
                .Build(w);

        var archetypes = q.GetArchetypes();
        Assert.AreEqual(2, archetypes.Length);

        foreach (var archetype in archetypes)
        {
            Assert.IsTrue(archetype.Components.Count == 1);
        }
    }

    [TestMethod]
    public void AtLeastOne()
    {
        var w = new EcsWorld();
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>(), ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt16>(), ComponentId.Get<ComponentInt64>()]);

        var q = new QueryBuilder()
            .AtLeastOne<ComponentFloat>()
            .AtLeastOne<ComponentInt32>()
            .Build(w);

        var archetypes = q.GetArchetypes();
        Assert.AreEqual(3, archetypes.Length);

        foreach (var archetype in archetypes)
        {
            Assert.IsTrue(archetype.Components.Contains(ComponentId.Get<ComponentInt32>())
                       || archetype.Components.Contains(ComponentId.Get<ComponentFloat>()));
        }
    }

    [TestMethod]
    public void NotAll()
    {
        var w = new EcsWorld();
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt16>(), ComponentId.Get<ComponentInt64>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>(), ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>(), ComponentId.Get<ComponentInt32>(), ComponentId.Get<ComponentInt64>()]);

        var q = new QueryBuilder()
            .NotAll<ComponentFloat>()
            .NotAll<ComponentInt32>()
            .Build(w);

        var archetypes = q.GetArchetypes();
        Assert.AreEqual(3, archetypes.Length);
    }

    [TestMethod]
    public void First_ThrowsNoMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        Assert.ThrowsException<GuardException>(() => q.First());
    }

    [TestMethod]
    public void First_MatchSingle()
    {
        var world = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(world);

        var c = new EcsCommandBuffer(world);
        var eb = c.Create().Set(new Component0());

        c.Execute();
        var e = eb.Resolve();

        Assert.AreEqual(e, q.First());
    }

    [TestMethod]
    public void First_MatchMultiple()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb1 = c.Create().Set(new Component0());
        var eb2 = c.Create().Set(new Component0());

        c.Execute();
        var e1 = eb1.Resolve();
        var e2 = eb2.Resolve();

        Assert.IsTrue(new[] { e1, e2 }.Contains(q.First()));
    }

    [TestMethod]
    public void FirstOrDefault_NullNoMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        Assert.IsFalse(q.FirstOrDefault().IsAlive);
    }

    [TestMethod]
    public void FirstOrDefault_MatchSingle()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb = c.Create().Set(new Component0());

        c.Execute();
        var e = eb.Resolve();

        Assert.AreEqual(e, q.FirstOrDefault());
    }

    [TestMethod]
    public void FirstOrDefault_MatchMultiple()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb1 = c.Create().Set(new Component0());
        var eb2 = c.Create().Set(new Component0());

        c.Execute();
        var e1 = eb1.Resolve();
        var e2 = eb2.Resolve();

        Assert.IsTrue(new[] { e1, e2 }.Contains(q.FirstOrDefault()));
    }

    [TestMethod]
    public void SingleOrDefault_NullNoMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        Assert.IsFalse(q.SingleOrDefault().IsAlive);
    }

    [TestMethod]
    public void SingleOrDefault_ThrowsMultipleMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        c.Create().Set(new Component0());
        c.Create().Set(new Component0());
        c.Execute();

        Assert.ThrowsException<GuardException>(() => q.SingleOrDefault());
    }

    [TestMethod]
    public void SingleOrDefault_MatchSingle()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb = c.Create().Set(new Component0());

        c.Execute();
        var e = eb.Resolve();

        Assert.AreEqual(e, q.SingleOrDefault());
    }

    [TestMethod]
    public void Single_ThrowsMultipleMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        c.Create().Set(new Component0());
        c.Create().Set(new Component0());
        c.Execute();

        Assert.ThrowsException<GuardException>(() => q.Single());
    }

    [TestMethod]
    public void Single_ThrowsNoMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        Assert.ThrowsException<GuardException>(() => q.Single());
    }

    [TestMethod]
    public void Single_MatchSingle()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb = c.Create().Set(new Component0());

        c.Execute();
        var e = eb.Resolve();

        Assert.AreEqual(e, q.Single());
    }

    [TestMethod]
    public void Any_True()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb = c.Create().Set(new Component0());

        c.Execute();
        _ = eb.Resolve();

        Assert.IsTrue(q.Any());
    }

    [TestMethod]
    public void Any_False()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        Assert.IsFalse(q.Any());
    }

    [TestMethod]
    public void Contains_True()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb = c.Create().Set(new Component0());

        c.Execute();
        var e = eb.Resolve();

        Assert.IsTrue(q.IsMatch(e));
    }

    [TestMethod]
    public void Contains_False()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb = c.Create();

        c.Execute();
        var e = eb.Resolve();

        Assert.IsFalse(q.IsMatch(e));
    }

    [TestMethod]
    public void Random_NullNoMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var rng = new Random(123);

        Assert.IsFalse(q.RandomOrDefault(rng).IsAlive);
    }

    [TestMethod]
    public void Random_MatchSingle()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        var eb = c.Create().Set(new Component0());

        c.Execute();
        var e = eb.Resolve();

        var r = new Random(123);

        Assert.AreEqual(e, q.RandomOrDefault(r));
    }

    [TestMethod]
    public void Random_MatchRandom()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<ComponentInt32>()
               .Build(w);

        var c = new EcsCommandBuffer(w);
        for (var i = 0; i < 10000; i++)
        {
            c.Create().Set(new ComponentInt32(i));
        }

        for (var i = 0; i < 10000; i++)
        {
            c.Create().Set(new ComponentInt32(i)).Set(new Component0());
        }

        for (var i = 0; i < 10000; i++)
        {
            c.Create().Set(new ComponentInt32(i)).Set(new Component1());
        }

        var resolver = c.Execute();
        var entities = new List<Entity>();
        for (var i = 0; i < resolver.Count; i++)
        {
            entities.Add(resolver[i]);
        }

        var r = new Random(123);

        for (var i = 0; i < 1000; i++)
        {
            Assert.IsTrue(entities.Contains(q.RandomOrDefault(r)));
        }
    }
}
