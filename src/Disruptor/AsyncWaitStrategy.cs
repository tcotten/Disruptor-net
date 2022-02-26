using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Disruptor;

/// <summary>
/// Blocking wait strategy that uses <c>Monitor.Wait</c> for event processors waiting on a barrier.
///
/// Can be configured to generate timeouts. Activating timeouts is only useful if your event handler
/// handles timeouts (<see cref="IAsyncBatchEventHandler{T}.OnTimeout"/>).
/// </summary>
/// <remarks>
/// This strategy can be used when throughput and low-latency are not as important as CPU resources.
/// </remarks>
public sealed class AsyncWaitStrategy : IAsyncWaitStrategy
{
    private readonly List<AsyncWaitState> _asyncWaitStates = new();
    private readonly object _gate = new();
    private bool _hasSyncWaiter;

    public bool IsBlockingStrategy => true;

    public SequenceWaitResult WaitFor(long sequence, Sequence cursor, ISequence dependentSequence, CancellationToken cancellationToken)
    {
        if (cursor.Value < sequence)
        {
            lock (_gate)
            {
                _hasSyncWaiter = true;
                while (cursor.Value < sequence)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var waitSucceeded = Monitor.Wait(_gate);
                    if (!waitSucceeded)
                    {
                        return SequenceWaitResult.Timeout;
                    }
                }
            }
        }

        return dependentSequence.AggressiveSpinWaitFor(sequence, cancellationToken);
    }

    public void SignalAllWhenBlocking()
    {
        lock (_gate)
        {
            if (_hasSyncWaiter)
            {
                Monitor.PulseAll(_gate);
            }

            foreach (var asyncWaitState in _asyncWaitStates)
            {
                asyncWaitState.Signal();
            }
            _asyncWaitStates.Clear();
        }
    }

    public ValueTask<SequenceWaitResult> WaitForAsync(long sequence, Sequence cursor, ISequence dependentSequence, AsyncWaitState asyncWaitState)
    {
        if (cursor.Value < sequence)
        {
            lock (_gate)
            {
                if (cursor.Value < sequence)
                {
                    asyncWaitState.ThrowIfCancellationRequested();

                    // Using cancellationToken in the task is not required because SignalAllWhenBlocking is always invoked by
                    // the sequencer barrier after cancellation.

                    _asyncWaitStates.Add(asyncWaitState);

                    return asyncWaitState.Wait(sequence, dependentSequence);
                }
            }
        }

        var availableSequence = asyncWaitState.GetAvailableSequence(sequence, dependentSequence);

        return new ValueTask<SequenceWaitResult>(availableSequence);
    }
}
