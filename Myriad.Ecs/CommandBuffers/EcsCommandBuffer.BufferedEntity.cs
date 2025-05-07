using System;

namespace Exanite.Myriad.Ecs.CommandBuffers;

public sealed partial class EcsCommandBuffer
{
    /// <summary>
    /// An entity that has been created in a command buffer, but not yet created. Can be used to add components.
    /// </summary>
    public readonly record struct BufferedEntity
    {
        private readonly uint id;
        private readonly uint version;

        internal readonly EcsCommandBuffer Buffer;
        private readonly Resolver resolver;

        /// <summary>
        /// Get the <see cref="CommandBuffer"/> which this <see cref="BufferedEntity"/> is from.
        /// </summary>
        public EcsCommandBuffer CommandBuffer
        {
            get
            {
                CheckIsMutable();
                return Buffer;
            }
        }

        internal BufferedEntity(uint id, EcsCommandBuffer buffer, Resolver resolver)
        {
            this.id = id;
            Buffer = buffer;
            this.resolver = resolver;

            version = buffer.version;
        }

        private void CheckIsMutable()
        {
            if (version != Buffer.version)
            {
                throw new InvalidOperationException("Cannot use `BufferedEntity` after `CommandBuffer` has been played");
            }
        }

        /// <summary>
        /// Add a component to this entity
        /// </summary>
        /// <typeparam name="T">The type of component to add</typeparam>
        /// <param name="value">The value of the component to add</param>
        /// <returns>this buffered entity</returns>
        public BufferedEntity Set<T>(T value)
            where T : IComponent
        {
            CheckIsMutable();

            Buffer.SetBuffered(id, value);
            return this;
        }

        /// <summary>
        /// Resolve this buffered Entity into the real Entity that was constructed
        /// </summary>
        public Entity Resolve()
        {
            if (resolver.Parent == null)
            {
                throw new ObjectDisposedException("Resolver has already been disposed");
            }

            if (resolver.Parent != Buffer)
            {
                throw new InvalidOperationException("Cannot use a resolver from one CommandBuffer with BufferedEntity from another");
            }

            if (resolver.Version != version)
            {
                throw new ObjectDisposedException("Resolver has already been disposed");
            }

            return resolver.Lookup[id].ToEntity(resolver.World);
        }
    }
}
