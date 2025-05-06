using Myriad.Ecs.ComponentIds;
using Myriad.Ecs.Worlds.Archetypes;

namespace Myriad.Ecs.Tests;

[TestClass]
public class ArchetypeHashTests
{
    [TestMethod]
    public void ArchetypeHashEqual()
    {
        var hash1 = new ArchetypeHash()
           .Toggle(ComponentId<ComponentFloat>.Id);
        Console.WriteLine(hash1);

        var hash2 = new ArchetypeHash()
           .Toggle(ComponentId<ComponentFloat>.Id);
        Console.WriteLine(hash1);

        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public void ArchetypeHashNotEqual()
    {
        var hash1 = new ArchetypeHash();
        hash1 = hash1.Toggle(ComponentId<ComponentInt16>.Id);
        Console.WriteLine(hash1);

        var hash2 = new ArchetypeHash();
        hash2 = hash2.Toggle(ComponentId<ComponentFloat>.Id);
        Console.WriteLine(hash2);

        Assert.AreNotEqual(hash1, hash2);
    }

    [TestMethod]
    public void ArchetypeHashRemoveComponents()
    {
        var hash1 = new ArchetypeHash()
           .Toggle(ComponentId<ComponentInt16>.Id)
            .Toggle(ComponentId<ComponentFloat>.Id);
        Console.WriteLine(hash1);

        // Create the same hash again, with one extra item
        var hash2 = new ArchetypeHash()
           .Toggle(ComponentId<ComponentInt16>.Id)
           .Toggle(ComponentId<ComponentFloat>.Id)
           .Toggle(ComponentId<ComponentInt32>.Id);
        Console.WriteLine(hash2);
        Assert.AreNotEqual(hash1, hash2);

        // Remove the extra item
        hash2 = hash2.Toggle(ComponentId<ComponentInt32>.Id);
        Assert.AreEqual(hash1, hash2);
    }
}
