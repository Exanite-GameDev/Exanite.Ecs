using System.Threading;

namespace Myriad.Ecs.Locks;

internal class RwLock<T>(T value)
    where T : class
{
    private readonly ReaderWriterLockSlim @lock = new(LockRecursionPolicy.SupportsRecursion);

    public ReadLockHandle EnterReadLock()
    {
        @lock.EnterReadLock();
        return new ReadLockHandle(@lock, value);
    }

    public WriteLockHandle EnterWriteLock()
    {
        @lock.EnterWriteLock();
        return new WriteLockHandle(@lock, value);
    }

    public readonly ref struct ReadLockHandle
    {
        private readonly ReaderWriterLockSlim @lock;
        public readonly T Value;

        internal ReadLockHandle(ReaderWriterLockSlim @lock, T value)
        {
            this.@lock = @lock;
            Value = value;
        }

        public void Dispose()
        {
            @lock.ExitReadLock();
        }
    }

    public readonly ref struct WriteLockHandle
    {
        private readonly ReaderWriterLockSlim @lock;
        public readonly T Value;

        internal WriteLockHandle(ReaderWriterLockSlim @lock, T value)
        {
            this.@lock = @lock;
            Value = value;
        }

        public void Dispose()
        {
            @lock.ExitWriteLock();
        }
    }
}
