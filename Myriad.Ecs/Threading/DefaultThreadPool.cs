using System;
using System.Threading;

namespace Myriad.Ecs.Threading;

/// <summary>
/// Use the dotnet <see cref="ThreadPool"/>
/// </summary>
public class DefaultThreadPool
    : IThreadPool
{
    /// <inheritdoc/>
    public int Threads { get; } = Math.Max(4, Math.Min(64, Environment.ProcessorCount) - 3);

    /// <inheritdoc/>
    public void QueueUserWorkItem(IThreadPoolWorkItem callback)
    {
        ThreadPool.UnsafeQueueUserWorkItem(callback, false);
    }
}
