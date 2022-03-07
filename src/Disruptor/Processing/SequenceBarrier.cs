using System.Runtime.CompilerServices;
using System.Threading;
using Disruptor.Util;
using JetBrains.Annotations;

namespace Disruptor.Processing;

/// <summary>
/// <see cref="ISequenceBarrier"/> handed out for gating <see cref="IEventProcessor"/> on a cursor sequence and optional dependent <see cref="IEventProcessor"/>s,
/// using the given WaitStrategy.
/// </summary>
public class SequenceBarrier : ISequenceBarrier
{
    private readonly ISequencer _sequencer;
    private readonly IWaitStrategy _waitStrategy;
    private readonly ISequence _dependentSequence;
    private readonly Sequence _cursorSequence;
    private readonly bool _isCursorPublished;
    private volatile CancellationTokenSource _cancellationTokenSource;

    [UsedImplicitly]
    public SequenceBarrier(ISequencer sequencer, IWaitStrategy waitStrategy, Sequence cursorSequence, ISequence[] dependentSequences)
    {
        _sequencer = sequencer;
        _waitStrategy = waitStrategy;
        _cursorSequence = cursorSequence;
        _dependentSequence = SequenceGroups.CreateReadOnlySequence(cursorSequence, dependentSequences);
        _cancellationTokenSource = new CancellationTokenSource();
        _isCursorPublished = sequencer.IsCursorPublished;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | Constants.AggressiveOptimization)]
    public SequenceWaitResult WaitFor(long sequence)
    {
        _cancellationTokenSource.Token.ThrowIfCancellationRequested();

        var availableSequence = _dependentSequence.Value;
        if (availableSequence >= sequence)
            return _isCursorPublished ? availableSequence : _sequencer.GetHighestPublishedSequence(sequence, availableSequence);

        return WaitForImpl(sequence);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private SequenceWaitResult WaitForImpl(long sequence)
    {
        var result = _waitStrategy.WaitFor(sequence, _cursorSequence, _dependentSequence, _cancellationTokenSource.Token);

        if (result.UnsafeAvailableSequence < sequence || _isCursorPublished)
            return result;

        return _sequencer.GetHighestPublishedSequence(sequence, result.UnsafeAvailableSequence);
    }

    public long Cursor => _dependentSequence.Value;

    public CancellationToken CancellationToken
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _cancellationTokenSource.Token;
    }

    public void ResetProcessing()
    {
        // Not disposing the previous value should be fine because the CancellationTokenSource instance
        // has no finalizer and no unmanaged resources to release.

        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void CancelProcessing()
    {
        _cancellationTokenSource.Cancel();
        _waitStrategy.SignalAllWhenBlocking();
    }
}
