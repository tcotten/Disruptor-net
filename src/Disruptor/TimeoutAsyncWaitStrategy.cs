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
public sealed class TimeoutAsyncWaitStrategy : IAsyncWaitStrategy
{
    private readonly List<TaskCompletionSource<bool>> _taskCompletionSources = new();
    private readonly object _gate = new();
    private readonly int _timeoutMilliseconds;
    private bool _hasSyncWaiter;

    public TimeoutAsyncWaitStrategy(TimeSpan timeout)
    {
        var totalMilliseconds = (long)timeout.TotalMilliseconds;
        if (totalMilliseconds is < 0 or > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout));
        }

        _timeoutMilliseconds = (int)totalMilliseconds;
    }

    public bool IsBlockingStrategy => true;

    public SequenceWaitResult WaitFor(long sequence, Sequence cursor, ISequence dependentSequence, CancellationToken cancellationToken)
    {
        var timeout = _timeoutMilliseconds;
        if (cursor.Value < sequence)
        {
            lock (_gate)
            {
                _hasSyncWaiter = true;
                while (cursor.Value < sequence)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var waitSucceeded = Monitor.Wait(_gate, timeout);
                    if (!waitSucceeded)
                    {
                        return SequenceWaitResult.Timeout;
                    }
                }
            }
        }

        var aggressiveSpinWait = new AggressiveSpinWait();
        long availableSequence;
        while ((availableSequence = dependentSequence.Value) < sequence)
        {
            cancellationToken.ThrowIfCancellationRequested();
            aggressiveSpinWait.SpinOnce();
        }

        return availableSequence;
    }

    public void SignalAllWhenBlocking()
    {
        lock (_gate)
        {
            if (_hasSyncWaiter)
            {
                Monitor.PulseAll(_gate);
            }

            foreach (var taskCompletionSource in _taskCompletionSources)
            {
                taskCompletionSource.TrySetResult(true);
            }
            _taskCompletionSources.Clear();
        }
    }

    public async ValueTask<SequenceWaitResult> WaitForAsync(long sequence, Sequence cursor, ISequence dependentSequence, AsyncWaitState asyncWaitState)
    {
        while (cursor.Value < sequence)
        {
            var waitSucceeded = await WaitForAsyncImpl(sequence, cursor, asyncWaitState).ConfigureAwait(false);
            if (!waitSucceeded)
            {
                return SequenceWaitResult.Timeout;
            }
        }

        var aggressiveSpinWait = new AggressiveSpinWait();
        long availableSequence;
        while ((availableSequence = dependentSequence.Value) < sequence)
        {
            asyncWaitState.ThrowIfCancellationRequested();
            aggressiveSpinWait.SpinOnce();
        }

        return availableSequence;
    }

    private async Task<bool> WaitForAsyncImpl(long sequence, Sequence cursor, AsyncWaitState asyncWaitState)
    {
        TaskCompletionSource<bool> taskCompletionSource;

        lock (_gate)
        {
            if (cursor.Value >= sequence)
            {
                return true;
            }

            asyncWaitState.ThrowIfCancellationRequested();

            taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _taskCompletionSources.Add(taskCompletionSource);
        }

        // Using cancellationToken in the await is not required because SignalAllWhenBlocking is always invoked by
        // the sequencer barrier after cancellation.

        await Task.WhenAny(taskCompletionSource.Task, Task.Delay(_timeoutMilliseconds)).ConfigureAwait(false);

        return taskCompletionSource.Task.IsCompleted;
    }
}
