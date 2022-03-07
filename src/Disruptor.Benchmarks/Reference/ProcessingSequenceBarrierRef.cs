using System.Runtime.CompilerServices;
using System.Threading;
using Disruptor.Util;
using JetBrains.Annotations;

namespace Disruptor.Benchmarks.Reference;

/// <summary>
/// <see cref="ISequenceBarrierRef"/> handed out for gating <see cref="IEventProcessor"/> on a cursor sequence and optional dependent <see cref="IEventProcessor"/>s,
///  using the given WaitStrategy.
/// </summary>
public class ProcessingSequenceBarrierRef : ISequenceBarrierRef
{
    // ReSharper disable FieldCanBeMadeReadOnly.Local (performance: the runtime type will be a struct)
    private readonly ISequencer _sequencer;
    private readonly IWaitStrategy _waitStrategy;
    // ReSharper restore FieldCanBeMadeReadOnly.Local

    private readonly ISequence _dependentSequence;
    private readonly Sequence _cursorSequence;
    private volatile CancellationTokenSource _cancellationTokenSource;

    [UsedImplicitly]
    public ProcessingSequenceBarrierRef(ISequencer sequencer, IWaitStrategy waitStrategy, Sequence cursorSequence, ISequence[] dependentSequences)
    {
        _sequencer = sequencer;
        _waitStrategy = waitStrategy;
        _cursorSequence = cursorSequence;
        _dependentSequence = SequenceGroups.CreateReadOnlySequence(cursorSequence, dependentSequences);
        _cancellationTokenSource = new CancellationTokenSource();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | Constants.AggressiveOptimization)]
    public SequenceWaitResult WaitFor(long sequence)
    {
        var cancellationToken = _cancellationTokenSource.Token;
        cancellationToken.ThrowIfCancellationRequested();

        var result = _waitStrategy.WaitFor(sequence, _cursorSequence, _dependentSequence, cancellationToken);

        if (result.UnsafeAvailableSequence < sequence)
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
