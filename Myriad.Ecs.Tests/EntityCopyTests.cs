using System.Linq;
using Exanite.Myriad.Ecs.Components;
using Exanite.Myriad.Ecs.Queries;
using Xunit;

namespace Exanite.Myriad.Ecs.Tests;

public class EntityCopyTests
{
    [Fact]
    public void CopyFrom_CopiesComponentData()
    {
        var floatValue = 1f;
        var byteValue = (byte)2;
        var int32Value = 3;

        var srcWorld = new EcsWorld();
        using (srcWorld.AcquireCommandBuffer(out var commandBuffer))
        {
            commandBuffer.Create()
                .Set(new EcsFloat(floatValue))
                .Set(new EcsByte(byteValue))
                .Set(new EcsInt32(int32Value));

            commandBuffer.Execute();
        }

        var dstWorld = new EcsWorld();
        using (dstWorld.AcquireCommandBuffer(out var commandBuffer))
        {
            commandBuffer.Create()
                .CopyFrom(srcWorld.Single());

            commandBuffer.Execute();
        }

        // Check component values
        Assert.Equal(floatValue, GetSingle<EcsFloat>(dstWorld).Value);
        Assert.Equal(byteValue, GetSingle<EcsByte>(dstWorld).Value);
        Assert.Equal(int32Value, GetSingle<EcsInt32>(dstWorld).Value);
    }

    [Fact]
    public void CopyFrom_UpdatesSelfReference()
    {
        var srcWorld = new EcsWorld();
        using (srcWorld.AcquireCommandBuffer(out var commandBuffer))
        {
            commandBuffer.Create()
                .Set(new EcsSelfReference());

            commandBuffer.Execute();
        }

        var dstWorld = new EcsWorld();
        using (dstWorld.AcquireCommandBuffer(out var commandBuffer))
        {
            commandBuffer.Create()
                .CopyFrom(srcWorld.Single());

            commandBuffer.Execute();
        }

        Assert.Equal(srcWorld, GetSingle<EcsSelfReference>(srcWorld).Self.Entity.World);
        Assert.Equal(dstWorld, GetSingle<EcsSelfReference>(dstWorld).Self.Entity.World);
    }

    [Fact]
    public void CopyFrom_UpdatesOtherReferences()
    {
        var srcWorld = new EcsWorld();
        using (srcWorld.AcquireCommandBuffer(out var commandBuffer))
        {
            commandBuffer.Create()
                .Set(new EcsCopied());

            commandBuffer.Execute();
        }

        var dstWorld = new EcsWorld();
        using (dstWorld.AcquireCommandBuffer(out var commandBuffer))
        {
            commandBuffer.Create()
                .CopyFrom(srcWorld.Single());

            commandBuffer.Execute();
        }

        Assert.Equal(srcWorld, GetSingle<EcsCopied>(srcWorld).Self.Entity.World);
        Assert.Equal(dstWorld, GetSingle<EcsCopied>(dstWorld).Self.Entity.World);

        Assert.Equal(srcWorld, GetSingle<EcsCopied>(srcWorld).Self2.Entity.World);
        Assert.Equal(dstWorld, GetSingle<EcsCopied>(dstWorld).Self2.Entity.World);

        Assert.Equal(srcWorld, GetSingle<EcsCopied>(srcWorld).Self3.World);
        Assert.Equal(dstWorld, GetSingle<EcsCopied>(dstWorld).Self3.World);
    }

    private T GetSingle<T>(EcsWorld world) where T : IComponent
    {
        return new QueryFilter().Include<T>().Build(world).First().GetComponent<T>();
    }

    private struct EcsSelfReference : IComponent, IComponentSelfReference<EcsSelfReference>
    {
        public EcsRef<EcsSelfReference> Self { get; set; }
    }

    private struct EcsCopied : IComponent, IComponentSelfReference<EcsCopied>, IComponentAdded, IComponentCopied
    {
        public EcsRef<EcsCopied> Self { get; set; }

        // Self automatically gets updated
        // These allow for testing OnCopied's functionality separately
        public EcsRef<EcsCopied> Self2 { get; set; }
        public Entity Self3 { get; set; }

        public void OnAdded()
        {
            Self2 = Self;
            Self3 = Self.Entity;
        }

        public void OnCopied(EcsWorld newWorld, IEntityLookup lookup)
        {
            Self2 = lookup.Get(Self2);
            Self3 = lookup.Get(Self3);
        }
    }
}
