using System;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds.Archetypes;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Exanite.Myriad.Ecs.Tests;

[TestFixture]
public class ArchetypeHashTests
{
    [Test]
    public void ArchetypeHashEqual()
    {
        var hash1 = new ArchetypeHash()
           .Toggle(ComponentId.Get<ComponentFloat>());
        Console.WriteLine(hash1);

        var hash2 = new ArchetypeHash()
           .Toggle(ComponentId.Get<ComponentFloat>());
        Console.WriteLine(hash1);

        Assert.That(hash2, Is.EqualTo(hash1));
    }

    [Test]
    public void ArchetypeHashNotEqual()
    {
        var hash1 = new ArchetypeHash();
        hash1 = hash1.Toggle(ComponentId.Get<ComponentInt16>());
        Console.WriteLine(hash1);

        var hash2 = new ArchetypeHash();
        hash2 = hash2.Toggle(ComponentId.Get<ComponentFloat>());
        Console.WriteLine(hash2);

        Assert.That(hash2, Is.Not.EqualTo(hash1));
    }

    [Test]
    public void ArchetypeHashRemoveComponents()
    {
        var hash1 = new ArchetypeHash()
           .Toggle(ComponentId.Get<ComponentInt16>())
            .Toggle(ComponentId.Get<ComponentFloat>());
        Console.WriteLine(hash1);

        // Create the same hash again, with one extra item
        var hash2 = new ArchetypeHash()
           .Toggle(ComponentId.Get<ComponentInt16>())
           .Toggle(ComponentId.Get<ComponentFloat>())
           .Toggle(ComponentId.Get<ComponentInt32>());
        Console.WriteLine(hash2);
        Assert.That(hash2, Is.Not.EqualTo(hash1));

        // Remove the extra item
        hash2 = hash2.Toggle(ComponentId.Get<ComponentInt32>());
        Assert.That(hash2, Is.EqualTo(hash1));
    }
}
