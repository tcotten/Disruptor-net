using Disruptor.StrategyService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.StrategyService.EventConsumers;
/// <summary>
/// Replicate the event for live/live and DR operations. Including the sequence allows for exact replication to other instances.
/// </summary>
public class ReplicationConsumer : IBatchEventHandler<TickerEvent>
{
    public void OnBatch(EventBatch<TickerEvent> batch, long sequence)
    {
        var data = batch[0];
        Console.WriteLine($"Replicate Batch - Count: {batch.Length} Seq: {sequence}");
        Console.WriteLine($"Replicate Item[0] - Seq: {sequence} Low: {data.low} High: {data.high} Open: {data.open} Close: {data.close} Vol: {data.volume} Trans: {data.transactions}");
        //batch.ToArray().ToList().ForEach(data =>
        //    Console.WriteLine($"Replicate - Seq: {sequence} Low: {data.low} High: {data.high} Open: {data.open} Close: {data.close} Vol: {data.volume} Trans: {data.transactions}")
        //);
    }
}
