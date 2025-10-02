using System;
using System.Linq;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Exanite.Myriad.Ecs.Tests;

[TestClass]
public class QueryBuilderTests
{
    [TestMethod]
    public void CreateEmpty()
    {
        Assert.IsNotNull(new QueryBuilder());
    }

    [TestMethod]
    public void CreateInclude()
    {
        var q = new QueryBuilder()
           .Include<ComponentFloat>()
           .Include<ComponentFloat>()
           .Include(typeof(ComponentFloat));

        Assert.IsTrue(q.IsIncluded<ComponentFloat>());
        Assert.IsTrue(q.IsIncluded(typeof(ComponentFloat)));
        Assert.IsTrue(q.IsIncluded(ComponentId.Get<ComponentFloat>()));
        CollectionAssert.Contains(q.Included.ToArray(), ComponentId.Get<ComponentFloat>());

        Assert.IsFalse(q.IsIncluded<ComponentInt32>());
        Assert.IsFalse(q.IsIncluded(typeof(ComponentInt32)));
        Assert.IsFalse(q.IsIncluded(ComponentId.Get<ComponentInt32>()));
        CollectionAssert.DoesNotContain(q.Included.ToArray(), ComponentId.Get<ComponentInt32>());

        Assert.IsFalse(q.IsExcluded<ComponentInt32>());
        Assert.IsFalse(q.IsIncluded(typeof(ComponentInt32)));
        Assert.IsFalse(q.IsIncluded(ComponentId.Get<ComponentInt32>()));

        Assert.IsFalse(q.IsExcluded<ComponentFloat>());
        Assert.IsFalse(q.IsExcluded(typeof(ComponentFloat)));
        Assert.IsFalse(q.IsExcluded(ComponentId.Get<ComponentFloat>()));
    }

    [TestMethod]
    public void IncludeDuplicateThrows()
    {
        var q = new QueryBuilder()
           .Include<ComponentFloat>()
           .Include<ComponentInt16>()
           .Include<ComponentInt32>()
           .Include<ComponentFloat>();

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            q.Exclude<ComponentFloat>();
        });

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            q.Exclude(typeof(ComponentFloat));
        });

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            q.ExactlyOne<ComponentFloat>();
        });

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            q.ExactlyOne(typeof(ComponentFloat));
        });

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            q.AtLeastOne<ComponentFloat>();
        });

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            q.AtLeastOne(typeof(ComponentFloat));
        });
    }

    [TestMethod]
    public void CreateExclude()
    {
        var q = new QueryBuilder()
           .Exclude<ComponentFloat>();

        Assert.IsTrue(q.IsExcluded<ComponentFloat>());
        CollectionAssert.Contains(q.Excluded.ToArray(), ComponentId.Get<ComponentFloat>());

        Assert.IsFalse(q.IsExcluded<ComponentInt32>());
        CollectionAssert.DoesNotContain(q.Excluded.ToArray(), ComponentId.Get<ComponentInt32>());

        Assert.IsFalse(q.IsIncluded<ComponentInt32>());
        Assert.IsFalse(q.IsIncluded<ComponentFloat>());
    }

    [TestMethod]
    public void ExcludeDuplicateThrows()
    {
        var q = new QueryBuilder()
           .Exclude(typeof(ComponentFloat))
           .Exclude<ComponentInt16>()
           .Exclude<ComponentInt32>()
           .Exclude<ComponentFloat>();

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            q.Include<ComponentFloat>();
        });

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            q.ExactlyOne<ComponentFloat>();
        });

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            q.AtLeastOne<ComponentFloat>();
        });
    }

    [TestMethod]
    public void CreateAtLeastOne()
    {
        var q = new QueryBuilder()
           .AtLeastOne<ComponentFloat>()
           .AtLeastOne(typeof(ComponentFloat));

        Assert.IsTrue(q.IsAtLeastOne<ComponentFloat>());
        Assert.IsTrue(q.IsAtLeastOne(typeof(ComponentFloat)));
        Assert.IsTrue(q.IsAtLeastOne(ComponentId.Get<ComponentFloat>()));
        CollectionAssert.Contains(q.AtLeastOnes.ToArray(), ComponentId.Get<ComponentFloat>());

        Assert.IsFalse(q.IsAtLeastOne<ComponentInt32>());
        Assert.IsFalse(q.IsAtLeastOne(typeof(ComponentInt32)));
        Assert.IsFalse(q.IsAtLeastOne(ComponentId.Get<ComponentInt32>()));
        CollectionAssert.DoesNotContain(q.AtLeastOnes.ToArray(), ComponentId.Get<ComponentInt32>());

        Assert.IsFalse(q.IsIncluded<ComponentInt32>());
        Assert.IsFalse(q.IsIncluded(typeof(ComponentInt32)));
        Assert.IsFalse(q.IsIncluded<ComponentFloat>());
        Assert.IsFalse(q.IsIncluded(typeof(ComponentFloat)));
    }

    [TestMethod]
    public void AtLeastOneDuplicateThrows()
    {
        var q = new QueryBuilder()
               .AtLeastOne<ComponentFloat>()
               .AtLeastOne<ComponentInt16>()
               .AtLeastOne<ComponentInt32>()
               .AtLeastOne<ComponentFloat>();

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            q.Include<ComponentFloat>();
        });

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            q.ExactlyOne<ComponentFloat>();
        });

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            q.Exclude<ComponentFloat>();
        });
    }

    [TestMethod]
    public void CreateExactyOneOf()
    {
        var q = new QueryBuilder()
           .ExactlyOne<ComponentFloat>()
           .ExactlyOne(typeof(ComponentFloat));

        Assert.IsTrue(q.IsExactlyOne<ComponentFloat>());
        Assert.IsTrue(q.IsExactlyOne(typeof(ComponentFloat)));
        Assert.IsTrue(q.IsExactlyOne(ComponentId.Get<ComponentFloat>()));
        CollectionAssert.Contains(q.ExactlyOnes.ToArray(), ComponentId.Get<ComponentFloat>());

        Assert.IsFalse(q.IsExactlyOne<ComponentInt32>());
        Assert.IsFalse(q.IsExactlyOne(typeof(ComponentInt32)));
        Assert.IsFalse(q.IsExactlyOne(ComponentId.Get<ComponentInt32>()));
        CollectionAssert.DoesNotContain(q.ExactlyOnes.ToArray(), ComponentId.Get<ComponentInt32>());

        Assert.IsFalse(q.IsIncluded<ComponentInt32>());
        Assert.IsFalse(q.IsIncluded(typeof(ComponentInt32)));
        Assert.IsFalse(q.IsIncluded<ComponentFloat>());
        Assert.IsFalse(q.IsIncluded(typeof(ComponentFloat)));
    }

    [TestMethod]
    public void ExactyOneOfDuplicateThrows()
    {
        var q = new QueryBuilder()
               .ExactlyOne<ComponentFloat>()
               .ExactlyOne<ComponentInt16>()
               .ExactlyOne<ComponentInt32>()
               .ExactlyOne<ComponentFloat>();

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            q.Include<ComponentFloat>();
        });

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            q.AtLeastOne<ComponentFloat>();
        });

        Assert.ThrowsException<InvalidOperationException>(() =>
        {
            q.Exclude<ComponentFloat>();
        });
    }

    [TestMethod]
    public void ConvertQueryToBuilder()
    {
        var world = new EcsWorld();

        var query = new QueryBuilder()
             .Include<ComponentInt16>()
             .Exclude<ComponentInt32>()
             .ExactlyOne<ComponentFloat>()
             .AtLeastOne<ComponentInt64>()
             .Build(world);

        var builder = query.ToBuilder();

        Assert.IsTrue(builder.IsIncluded<ComponentInt16>());
        Assert.IsFalse(builder.IsIncluded<ComponentInt32>());
        Assert.IsFalse(builder.IsIncluded<ComponentFloat>());
        Assert.IsFalse(builder.IsIncluded<ComponentInt64>());

        Assert.IsFalse(builder.IsExcluded<ComponentInt16>());
        Assert.IsTrue(builder.IsExcluded<ComponentInt32>());
        Assert.IsFalse(builder.IsExcluded<ComponentFloat>());
        Assert.IsFalse(builder.IsExcluded<ComponentInt64>());

        Assert.IsFalse(builder.IsExactlyOne<ComponentInt16>());
        Assert.IsFalse(builder.IsExactlyOne<ComponentInt32>());
        Assert.IsTrue(builder.IsExactlyOne<ComponentFloat>());
        Assert.IsFalse(builder.IsExactlyOne<ComponentInt64>());

        Assert.IsFalse(builder.IsAtLeastOne<ComponentInt16>());
        Assert.IsFalse(builder.IsAtLeastOne<ComponentInt32>());
        Assert.IsFalse(builder.IsAtLeastOne<ComponentFloat>());
        Assert.IsTrue(builder.IsAtLeastOne<ComponentInt64>());
    }

    [TestMethod]
    public void BuildTwiceSharesParts()
    {
        var world = new EcsWorld();

        var builder = new QueryBuilder()
            .Include<ComponentInt16>()
            .Exclude<ComponentInt32>()
            .ExactlyOne<ComponentFloat>()
            .AtLeastOne<ComponentInt64>();

        var q1 = builder.Build(world);
        var q2 = builder.Build(world);

        Assert.AreSame(q1.Include, q2.Include);
        Assert.AreSame(q1.Exclude, q2.Exclude);
        Assert.AreSame(q1.ExactlyOne, q2.ExactlyOne);
        Assert.AreSame(q1.AtLeastOne, q2.AtLeastOne);
    }
}
