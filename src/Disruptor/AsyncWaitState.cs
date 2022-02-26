using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Disruptor;

/// <summary>
/// State .
/// Used to store per-caller synchronization primitives.
/// </summary>
public class AsyncWaitState
{
    private readonly ValueTaskSource _valueTaskSource;
    private readonly CancellationToken _cancellationToken;
    private ManualResetValueTaskSourceCore<bool> _valueTaskSourceCore;
    private long _sequence;
    private ISequence? _dependentSequence;

    public AsyncWaitState(CancellationToken cancellationToken)
    {
        _valueTaskSource = new(this);
        _cancellationToken = cancellationToken;
        _valueTaskSourceCore = new() { RunContinuationsAsynchronously = true };
    }

    public CancellationToken CancellationToken => _cancellationToken;

    public void ThrowIfCancellationRequested()
    {
        CancellationToken.ThrowIfCancellationRequested();
    }

    public void Signal()
    {
        _valueTaskSource.SetResult();
    }

    public ValueTask<SequenceWaitResult> Wait(long sequence, ISequence dependentSequence)
    {
        _valueTaskSourceCore.Reset();
        _sequence = sequence;
        _dependentSequence = dependentSequence;

        return new ValueTask<SequenceWaitResult>(_valueTaskSource, _valueTaskSourceCore.Version);
    }

    public virtual SequenceWaitResult GetAvailableSequence(long sequence, ISequence dependentSequence)
    {
        return dependentSequence.AggressiveSpinWaitFor(sequence, _cancellationToken);
    }

    private SequenceWaitResult GetResult(short token)
    {
        _valueTaskSourceCore.GetResult(token);

        return GetAvailableSequence(_sequence, _dependentSequence!);
    }

    private ValueTaskSourceStatus GetStatus(short token)
    {
        return _valueTaskSourceCore.GetStatus(token);
    }

    private void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
    {
        _valueTaskSourceCore.OnCompleted(continuation, state, token, flags);
    }

    private void SetResult()
    {
        _valueTaskSourceCore.SetResult(true);
    }

    private class ValueTaskSource : IValueTaskSource<SequenceWaitResult>
    {
        private readonly AsyncWaitState _asyncWaitState;

        public ValueTaskSource(AsyncWaitState asyncWaitState)
        {
            _asyncWaitState = asyncWaitState;
        }

        public SequenceWaitResult GetResult(short token)
        {
            return _asyncWaitState.GetResult(token);
        }

        public ValueTaskSourceStatus GetStatus(short token)
        {
            return _asyncWaitState.GetStatus(token);
        }

        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            _asyncWaitState.OnCompleted(continuation, state, token, flags);
        }

        public void SetResult()
        {
            _asyncWaitState.SetResult();
        }
    }
}
