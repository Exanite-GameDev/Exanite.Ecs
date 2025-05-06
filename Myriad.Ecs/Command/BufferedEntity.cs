using System;

namespace Myriad.Ecs.Command;

public sealed partial class EcsCommandBuffer
{
    /// <summary>
    /// An entity that has been created in a command buffer, but not yet created. Can be used to add components.
    /// </summary>
    public readonly record struct BufferedEntity
    {
        private readonly uint _id;
        private readonly uint _version;

        internal readonly EcsCommandBuffer _buffer;
        private readonly Resolver _resolver;

        /// <summary>
        /// Get the <see cref="CommandBuffer"/> which this <see cref="BufferedEntity"/> is from.
        /// </summary>
        public EcsCommandBuffer CommandBuffer
        {
            get
            {
                CheckIsMutable();
                return _buffer;
            }
        }

        internal BufferedEntity(uint id, EcsCommandBuffer buffer, Resolver resolver)
        {
            _id = id;
            _buffer = buffer;
            _resolver = resolver;

            _version = buffer._version;
        }

        private void CheckIsMutable()
        {
            if (_version != _buffer._version)
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

            _buffer.SetBuffered(_id, value);
            return this;
        }

        /// <summary>
        /// Resolve this buffered Entity into the real Entity that was constructed
        /// </summary>
        /// <returns></returns>
        public Entity Resolve()
        {
            if (_resolver.Parent == null)
            {
                throw new ObjectDisposedException("Resolver has already been disposed");
            }

            if (_resolver.Parent != _buffer)
            {
                throw new InvalidOperationException("Cannot use a resolver from one CommandBuffer with BufferedEntity from another");
            }

            if (_resolver.Version != _version)
            {
                throw new ObjectDisposedException("Resolver has already been disposed");
            }

            return _resolver.Lookup[_id].ToEntity(_resolver.World);
        }
    }
}
