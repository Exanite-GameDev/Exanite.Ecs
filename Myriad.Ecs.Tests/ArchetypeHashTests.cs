using System;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Worlds.Archetypes;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class ArchetypeHashTests
{
    [Fact]
    public void ArchetypeHashEqual()
    {
        var hash1 = new ArchetypeHash()
            .Toggle(ComponentId.Get<ComponentFloat>());
        Console.WriteLine(hash1);

        var hash2 = new ArchetypeHash()
            .Toggle(ComponentId.Get<ComponentFloat>());
        Console.WriteLine(hash1);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ArchetypeHashNotEqual()
    {
        var hash1 = new ArchetypeHash();
        hash1 = hash1.Toggle(ComponentId.Get<ComponentInt16>());
        Console.WriteLine(hash1);

        var hash2 = new ArchetypeHash();
        hash2 = hash2.Toggle(ComponentId.Get<ComponentFloat>());
        Console.WriteLine(hash2);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
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
        Assert.NotEqual(hash1, hash2);

        // Remove the extra item
        hash2 = hash2.Toggle(ComponentId.Get<ComponentInt32>());
        Assert.Equal(hash1, hash2);
    }
}
