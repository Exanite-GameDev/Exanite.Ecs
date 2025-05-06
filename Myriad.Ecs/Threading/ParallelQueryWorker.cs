using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Myriad.Ecs.Queries;

namespace Myriad.Ecs.Threading;

internal class ParallelQueryWorker<TWork>
    : IThreadPoolWorkItem
    where TWork : struct, IWorkItem
{
    private ParallelQueryWorker<TWork>?[]? siblings;
    private CountdownEvent? counter;

    private readonly ConcurrentQueue<TWork> work = new();
    private readonly List<Exception> exceptions = [];

    public ManualResetEventSlim FinishEvent { get; } = new ManualResetEventSlim(false);

    public void Configure(ParallelQueryWorker<TWork>?[] siblings, CountdownEvent counter)
    {
        this.siblings = siblings;
        this.counter = counter;
        exceptions.Clear();
        FinishEvent.Reset();
    }

    public void Clear(ref List<Exception>? exceptions)
    {
        if (this.exceptions.Count > 0)
        {
            exceptions ??= [];
            exceptions.AddRange(this.exceptions);
            this.exceptions.Clear();
        }

        counter = null;
        siblings = null;
        work.Clear();
        FinishEvent.Reset();
    }

    public void Execute()
    {
        try
        {
            var counter = this.counter;
            var siblings = this.siblings;
            Debug.Assert(counter != null && siblings != null, "Cannot execute work - worker not configured");

            // Seed an RNG with the index of this worker in the siblings array
            var rng = new ValueRandom(Array.IndexOf(siblings, this));

            while (!counter.IsSet)
            {
                // Process the entire local queue
                while (this.work.TryDequeue(out var work))
                    DoWorkItem(counter, ref work);

                // Do a few rounds of trying to steal work off siblings.
                // Break out of the loop if there is any local work to do, or if the counter
                // is set (indicating there is no more work available anywhere).
                for (var i = 0; i < siblings.Length && work.IsEmpty && !counter.IsSet; i++)
                {
                    // Choose a random sibling. This prevents all workers starting from the first
                    // worker every time, which would cause unnecessary contention and bias the system
                    // to drain those queues first.
                    var idx = Math.Abs(rng.Next()) % siblings.Length;
                    var sibling = siblings[idx];

                    // Steal work
                    if (sibling == null || !sibling.Steal(out var work))
                    {
                        continue;
                    }

                    DoWorkItem(counter, ref work);
                }
            }
        }
        finally
        {
            FinishEvent.Set();
        }
    }

    private void DoWorkItem(CountdownEvent counter, ref TWork work)
    {
        counter.Signal();

        try
        {
            work.Execute();
        }
        catch (Exception ex)
        {
            exceptions.Add(ex);
        }
    }

    public void Enqueue(TWork work)
    {
        this.work.Enqueue(work);
    }

    public bool Steal(out TWork result)
    {
        return work.TryDequeue(out result);
    }
}

internal interface IWorkItem
{
    void Execute();
}
