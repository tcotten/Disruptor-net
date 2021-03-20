using System;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Disruptor.Benchmarks.Reference;

namespace Disruptor.Benchmarks
{
    [DisassemblyDiagnoser]
    public class RingBufferDataProviderBenchmarks
    {
        private const int _batchSize = 10;

        private readonly RingBuffer<Event> _ringBuffer;
        private readonly ReferenceRingBuffer<Event> _refRingBuffer;

        public RingBufferDataProviderBenchmarks()
        {
            _ringBuffer = new RingBuffer<Event>(() => new Event(), new SingleProducerSequencer(4096, new BusySpinWaitStrategy()));
            _refRingBuffer = new ReferenceRingBuffer<Event>(() => new Event(), new SingleProducerSequencer(4096, new BusySpinWaitStrategy()));
        }

        public int Index { get; set; } = 75;

        //[Benchmark]
        public void SetValue()
        {
            _ringBuffer[Index].Value = 42;
        }

        //[Benchmark(OperationsPerInvoke = _batchSize)]
        public void SetValueBatchConst()
        {
            var index = Index;
            var lo = index;
            var hi = index + _batchSize;
            for (var i = lo; i < hi; i++)
            {
                _ringBuffer[i].Value = 42;
            }
        }

        [Benchmark]
        public long GetValue()
        {
            return _ringBuffer[Index].Value;
        }

        [Benchmark(Baseline = true)]
        public long GetValueRef()
        {
            return _refRingBuffer[Index].Value;
        }

        public class Event
        {
            public long Value { get; set; }
        }
    }
}
