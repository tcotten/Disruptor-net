using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Disruptor.Util;
using JetBrains.Annotations;

namespace Disruptor.Processing;

/// <summary>
/// Coordination barrier for asynchronous event processors. <see cref="SequenceBarrier"/>
/// </summary>
public class AsyncSequenceBarrier : ISequenceBarrier
{
    private readonly IAsyncWaitStrategy _waitStrategy;
    private readonly ISequencer _sequencer;
    private readonly ISequence _dependentSequence;
    private readonly Sequence _cursorSequence;
    private readonly bool _isCursorPublished;
    private volatile CancellationTokenSource _cancellationTokenSource;

    [UsedImplicitly]
    public AsyncSequenceBarrier(ISequencer sequencer, IAsyncWaitStrategy waitStrategy, Sequence cursorSequence, ISequence[] dependentSequences)
    {
        _sequencer = sequencer;
        _waitStrategy = waitStrategy;
        _cursorSequence = cursorSequence;
        _dependentSequence = SequenceGroups.CreateReadOnlySequence(cursorSequence, dependentSequences);
        _cancellationTokenSource = new CancellationTokenSource();
        _isCursorPublished = _sequencer.IsCursorPublished;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | Constants.AggressiveOptimization)]
    public ValueTask<SequenceWaitResult> WaitForAsync(long sequence)
    {
        _cancellationTokenSource.Token.ThrowIfCancellationRequested();

        var availableSequence = _dependentSequence.Value;
        if (availableSequence >= sequence)
            return new ValueTask<SequenceWaitResult>(_isCursorPublished ? availableSequence : _sequencer.GetHighestPublishedSequence(sequence, availableSequence));

        return WaitForAsyncImpl(sequence);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private async ValueTask<SequenceWaitResult> WaitForAsyncImpl(long sequence)
    {
        var result = await _waitStrategy.WaitForAsync(sequence, _cursorSequence, _dependentSequence, _cancellationTokenSource.Token).ConfigureAwait(false);

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
