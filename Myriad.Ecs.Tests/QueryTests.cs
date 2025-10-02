using System;
using System.Collections.Generic;
using System.Linq;
using Exanite.Core.Runtime;
using Exanite.Myriad.Ecs.Collections;
using Exanite.Myriad.Ecs.CommandBuffers;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Queries;
using Exanite.Myriad.Ecs.Worlds.Archetypes;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Exanite.Myriad.Ecs.Tests;

[TestFixture]
public class QueryTests
{
    [Test]
    public void IncludeMatchNone()
    {
        var world = new EcsWorld();

        var q = new QueryBuilder()
           .Include<ComponentFloat>()
           .Build(world);

        var archetypes = q.GetArchetypes();
        Assert.That(archetypes.Length, Is.EqualTo(0));
    }

    [Test]
    public void IncludeMatchOne()
    {
        var w = new EcsWorld();
        w.GetOrCreateArchetype([ComponentId.Get<ComponentInt32>()]);
        w.GetOrCreateArchetype([ComponentId.Get<ComponentFloat>()]);

        var q = new QueryBuilder()
           .Include<ComponentFloat>()
           .Build(w);

        var a = q.GetArchetypes();
        Assert.That(a.Length, Is.EqualTo(1));
    }

    [Test]
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
        Assert.That(a.Length, Is.EqualTo(1));

        // Add an archetype to the world that the query should match
        var c1 = new OrderedListSet<ComponentId>(new HashSet<ComponentId> { ComponentId.Get<ComponentInt32>(), ComponentId.Get<ComponentFloat>() });
        w.GetOrCreateArchetype(c1, ArchetypeHash.Create(c1));

        // Check it now matches 2 archetypes
        var b = q.GetArchetypes();
        Assert.That(b.Length, Is.EqualTo(2));
        foreach (var archetype in b)
        {
            Assert.That(archetype.Components.Contains(ComponentId.Get<ComponentFloat>()), Is.True);
        }

        // Add an archetype to the world that the query should NOT match
        var c2 = new OrderedListSet<ComponentId>(new HashSet<ComponentId> { ComponentId.Get<ComponentInt32>(), ComponentId.Get<ComponentByte>() });
        w.GetOrCreateArchetype(c2, ArchetypeHash.Create(c2));

        // Check it now matches 2 archetypes
        var c = q.GetArchetypes();
        Assert.That(c.Length, Is.EqualTo(2));
        foreach (var archetype in c)
        {
            Assert.That(archetype.Components.Contains(ComponentId.Get<ComponentFloat>()), Is.True);
        }
    }

    [Test]
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
        Assert.That(archetypes.Count, Is.EqualTo(2));

        Assert.That(archetypes.All(x => x.Components.Contains(ComponentId.Get<ComponentFloat>())), Is.True);
    }

    [Test]
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
        Assert.That(archetypes.Length, Is.EqualTo(1));

        var archetype = archetypes[0];
        Assert.That(archetype.Components.Contains(ComponentId.Get<ComponentFloat>()), Is.True);
        Assert.That(archetype.Components.Contains(ComponentId.Get<ComponentInt32>()), Is.False);
    }

    [Test]
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
        Assert.That(archetypes.Length, Is.EqualTo(2));

        foreach (var archetype in archetypes)
        {
            Assert.That(archetype.Components.Count == 1, Is.True);
        }
    }

    [Test]
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
        Assert.That(archetypes.Length, Is.EqualTo(3));

        foreach (var archetype in archetypes)
        {
            Assert.That(archetype.Components.Contains(ComponentId.Get<ComponentInt32>())
                       || archetype.Components.Contains(ComponentId.Get<ComponentFloat>()), Is.True);
        }
    }

    [Test]
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
        Assert.That(archetypes.Length, Is.EqualTo(3));
    }

    [Test]
    public void First_ThrowsNoMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        Assert.Throws<GuardException>(() => q.First());
    }

    [Test]
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

        Assert.That(q.First(), Is.EqualTo(e));
    }

    [Test]
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

        Assert.That(new[] { e1, e2 }.Contains(q.First()), Is.True);
    }

    [Test]
    public void FirstOrDefault_NullNoMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        Assert.That(q.FirstOrDefault().IsAlive, Is.False);
    }

    [Test]
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

        Assert.That(q.FirstOrDefault(), Is.EqualTo(e));
    }

    [Test]
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

        Assert.That(new[] { e1, e2 }.Contains(q.FirstOrDefault()), Is.True);
    }

    [Test]
    public void SingleOrDefault_NullNoMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        Assert.That(q.SingleOrDefault().IsAlive, Is.False);
    }

    [Test]
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

        Assert.Throws<GuardException>(() => q.SingleOrDefault());
    }

    [Test]
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

        Assert.That(q.SingleOrDefault(), Is.EqualTo(e));
    }

    [Test]
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

        Assert.Throws<GuardException>(() => q.Single());
    }

    [Test]
    public void Single_ThrowsNoMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        Assert.Throws<GuardException>(() => q.Single());
    }

    [Test]
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

        Assert.That(q.Single(), Is.EqualTo(e));
    }

    [Test]
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

        Assert.That(q.Any(), Is.True);
    }

    [Test]
    public void Any_False()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        Assert.That(q.Any(), Is.False);
    }

    [Test]
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

        Assert.That(q.IsMatch(e), Is.True);
    }

    [Test]
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

        Assert.That(q.IsMatch(e), Is.False);
    }

    [Test]
    public void Random_NullNoMatch()
    {
        var w = new EcsWorld();

        var q = new QueryBuilder()
               .Include<Component0>()
               .Build(w);

        var rng = new Random(123);

        Assert.That(q.RandomOrDefault(rng).IsAlive, Is.False);
    }

    [Test]
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

        Assert.That(q.RandomOrDefault(r), Is.EqualTo(e));
    }

    [Test]
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
            Assert.That(entities.Contains(q.RandomOrDefault(r)), Is.True);
        }
    }
}
