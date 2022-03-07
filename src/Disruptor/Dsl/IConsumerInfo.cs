using System.Threading.Tasks;
using Disruptor.Processing;

namespace Disruptor.Dsl;

internal interface IConsumerInfo
{
    ISequence[] Sequences { get; }

    ISequenceBarrier? Barrier { get; }

    bool IsEndOfChain { get; }

    void Start(TaskScheduler taskScheduler);

    void Halt();

    void MarkAsUsedInBarrier();

    bool IsRunning { get; }
}