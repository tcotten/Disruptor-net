using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Disruptor;
using Disruptor.Dsl;
using Disruptor.Processing;


namespace Disruptor.MySamples;

public static class LongEventMain
{
    public static void Main()
    {
        int bufferSize = 1024;

        ValueDisruptor<LongEvent> disruptor = new(() => new LongEvent(), bufferSize);

        disruptor.HandleEventsWith(new LongEventHandler());

        disruptor.Start();

        var ringBuffer = disruptor.RingBuffer;

        for (int i = 0; ; i++)
        {
            disruptor.PublishEvent(i, (new Random()).Next(1, 9999999).ToString());
            Thread.Sleep(1000);
        }
    }

}

public static partial class DisruptorExtensions
{
    public static void PublishEvent(this ValueDisruptor<LongEvent> disruptor, int id, string value)
    {
        using (var scope = disruptor.PublishEvent())
        {
            ref var data = ref scope.Event();
            data.Id = id;
            data.Value = value;
        }
        // The event is published at the end of the scope
    }
}
public struct LongEvent
{
    public int Id { get; set; }
    public string Value { get; set; }
}
public class LongEventFactory : IEventHandler<LongEvent>
{
    public void OnEvent(LongEvent data, long sequence, bool endOfBatch)
    {
        Console.WriteLine($"Event: {data.Id} => {data.Value}");
    }
}

public class LongEventHandler : IValueEventHandler<LongEvent>
{
    public void OnEvent(ref LongEvent data, long sequence, bool endOfBatch)
    {
        Console.WriteLine($"{data.Id}: {data.Value}");
    }
}

//public class LongEventProducer
//{
//    private readonly RingBuffer<LongEvent> _ringBuffer;

//    public LongEventProducer(RingBuffer<LongEvent> ringBuffer)
//    {
//        _ringBuffer = ringBuffer;
//    }

//    public void ProduceUsingRawApi(ReadOnlyMemory<byte> input)
//    {
//        // (1) Claim the next sequence
//        var sequence = _ringBuffer.Next();
//        try
//        {
//            // (2) Get and configure the event for the sequence
//            var data = _ringBuffer[sequence];
//            data.Id = MemoryMarshal.Read<int>(input.Span);
//            data.Value = MemoryMarshal.Read<double>(input.Span.Slice(4));
//            //string val;
//            //int len;
//            //MemoryMarshal.TryGetString(input.Span.Slice(4), out val, out len);
//            //byte[] bytes = MemoryMarshal.Read<byte>(input.Span.Slice(4));
//            //data.Value = Convert.ToString(bytes);
//        }
//        finally
//        {
//            // (3) Publish the event
//            _ringBuffer.Publish(sequence);
//        }
//    }

//    public void ProduceUsingScope(ReadOnlyMemory<byte> input)
//    {
//        using (var scope = _ringBuffer.PublishEvent())
//        {
//            var data = scope.Event();
//            data.Id = MemoryMarshal.Read<int>(input.Span);
//            data.Value = MemoryMarshal.Read<double>(input.Span.Slice(4));

//            // The event is published at the end of the scope
//        }
//    }

//    public void ProduceUsingCustomWaitStrategy(ReadOnlyMemory<byte> input)
//    {
//        // Claim the next sequence
//        var sequence = _ringBuffer.Next();
//        try
//        {
//            // Get the event for the sequence
//            var data = _ringBuffer[sequence];

//            // Configure the event
//            data.Id = MemoryMarshal.Read<int>(input.Span);
//            data.Value = MemoryMarshal.Read<double>(input.Span.Slice(4));
//        }
//        finally
//        {
//            // Publish the event
//            _ringBuffer.Publish(sequence);
//        }
//    }
//}
