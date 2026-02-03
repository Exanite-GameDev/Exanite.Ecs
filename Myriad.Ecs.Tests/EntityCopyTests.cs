using System.Collections.Generic;
using Exanite.Core.Utilities;
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

    [Fact]
    public void CopyFrom_CreateHierarchy()
    {
        var srcWorld = new EcsWorld();
        using var _ = srcWorld.AcquireCommandBuffer(out var srcCommandBuffer);

        var srcChild1 = srcCommandBuffer.Create().Set(new EcsTransform()).Entity;
        var srcChild2 = srcCommandBuffer.Create().Set(new EcsTransform()).Entity;
        var srcRoot = srcCommandBuffer.Create().Set(new EcsTransform()
        {
            Children = [srcChild1.GetStorableComponentUnchecked<EcsTransform>(), srcChild2.GetStorableComponentUnchecked<EcsTransform>()],
        });

        srcCommandBuffer.Execute();

        var dstWorld = new EcsWorld();
        using var __ = dstWorld.AcquireCommandBuffer(out var dstCommandBuffer);

        // Order shouldn't matter
        var dstChild2 = dstCommandBuffer.Create().CopyFrom(srcChild2);
        var dstRoot = dstCommandBuffer.Create().CopyFrom(srcRoot).Entity;
        var dstChild1 = dstCommandBuffer.Create().CopyFrom(srcChild1);

        dstCommandBuffer.Execute();

        foreach (var entity in new QueryFilter().Build(dstWorld).EnumerateEntities())
        {
            Assert.True(entity.HasComponent<EcsTransform>());

            ref var transform = ref entity.GetComponent<EcsTransform>();
            Assert.Equal(dstWorld, transform.Self.Entity.World);

            foreach (var child in transform.Children)
            {
                Assert.Equal(dstWorld, child.Entity.World);
            }
        }

        Assert.Equal(2, dstRoot.GetComponent<EcsTransform>().Children.Count);
        Assert.Equal(dstChild1, dstRoot.GetComponent<EcsTransform>().Children[0].Entity);
        Assert.Equal(dstChild2, dstRoot.GetComponent<EcsTransform>().Children[1].Entity);
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

    private struct EcsTransform : IComponent, IComponentSelfReference<EcsTransform>, IComponentCopied
    {
        public EcsRef<EcsTransform> Self { get; set; }

        public List<EcsRef<EcsTransform>> Children = new();

        public EcsTransform() {}

        public void OnCopied(EcsWorld newWorld, IEntityLookup lookup)
        {
            foreach (ref var child in Children.AsSpan())
            {
                child = lookup.Get(child);
            }
        }
    }
}
