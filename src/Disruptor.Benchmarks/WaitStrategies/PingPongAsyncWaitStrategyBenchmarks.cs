using System;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Disruptor.Benchmarks.WaitStrategies;

[MemoryDiagnoser]
public class PingPongAsyncWaitStrategyBenchmarks : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly AsyncWaitStrategy _pingWaitStrategy = new();
    private readonly AsyncWaitStrategy _pongWaitStrategy = new();
    private readonly Sequence _pingCursor = new();
    private readonly Sequence _pongCursor = new();
    private readonly AsyncWaitState _pingAsyncWaitState;
    private readonly Task _pongTask;

    public PingPongAsyncWaitStrategyBenchmarks()
    {
        _pingAsyncWaitState = new AsyncWaitState(_cancellationTokenSource.Token);
        _pongTask = Task.Run(RunPong);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _pongTask.Wait();
    }

    private async Task RunPong()
    {
        var asyncWaitState = new AsyncWaitState(_cancellationTokenSource.Token);
        var sequence = -1L;

        try
        {
            while (true)
            {
                sequence++;

                await _pingWaitStrategy.WaitForAsync(sequence, _pingCursor, _pingCursor, asyncWaitState).ConfigureAwait(false);

                _pongCursor.SetValue(sequence);
                _pongWaitStrategy.SignalAllWhenBlocking();
            }
        }
        catch (OperationCanceledException e)
        {
            Console.WriteLine(e);
        }
    }

    public const int OperationsPerInvoke = 1_000_000;

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public async Task Run()
    {
        var start = _pingCursor.Value + 1;
        var end = start + OperationsPerInvoke;

        for (var s = start; s < end; s++)
        {
            _pingCursor.SetValue(s);
            _pingWaitStrategy.SignalAllWhenBlocking();

            await _pongWaitStrategy.WaitForAsync(s, _pongCursor, _pongCursor, _pingAsyncWaitState).ConfigureAwait(false);
        }
    }
}
