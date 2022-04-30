using Disruptor.StrategyService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.StrategyService.EventConsumers;
/// <summary>
/// Consume and persist the events to the backing data store
/// </summary>
public class JournalConsumer : IBatchEventHandler<TickerEvent>
{
    public void OnBatch(EventBatch<TickerEvent> batch, long sequence)
    {
        var data = batch[0];
        Console.WriteLine($"Journal Batch - Count: {batch.Length} Seq: {sequence}");
        Console.WriteLine($"Journal Item[0] - Seq: {sequence} Low: {data.low} High: {data.high} Open: {data.open} Close: {data.close} Vol: {data.volume} Trans: {data.transactions}");
        //batch.ToArray().ToList().ForEach(data =>
        //    Console.WriteLine($"Journal - Seq: {sequence} Low: {data.low} High: {data.high} Open: {data.open} Close: {data.close} Vol: {data.volume} Trans: {data.transactions}")
        //);
    }
}
