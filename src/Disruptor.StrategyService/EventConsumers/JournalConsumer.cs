using Disruptor.StrategyService.Data;
using Disruptor.StrategyService.Models;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.StrategyService.EventConsumers;
public interface IJournalConsumer : IBatchEventHandler<PairCandle> { }
/// <summary>
/// Consume and persist the events to the backing data store
/// </summary>
public class JournalConsumer : IJournalConsumer
{
    private IBoltGraphClient boltGraphClient { get; init; }
    //private int firstRuns = 0;
    public JournalConsumer(IBoltGraphClient boltGraphClient)
    {
        this.boltGraphClient = boltGraphClient;
    }

    public void OnBatch(EventBatch<PairCandle> batch, long sequence)
    {
        var data = batch[0];
        Console.WriteLine($"Journal Batch - Count: {batch.Length} Seq: {sequence}");
        Console.WriteLine($"Journal Item[0] - Seq: {sequence} Low: {data.low} High: {data.high} Open: {data.open} Close: {data.close} Vol: {data.volume} Trans: {data.transactions}");
        var watch = System.Diagnostics.Stopwatch.StartNew();
        foreach (var candle in batch.ToArray())
        {
            boltGraphClient.MergePairCandle(candle).GetAwaiter().GetResult();
            //System.Threading.Thread.Sleep(10);
        }
        watch.Stop();
        Console.WriteLine($"Total Execution Time: {watch.Elapsed.TotalSeconds} seconds");
        //batch.ToArray().ToList().ForEach(data => boltGraphClient.MergePairCandle(data).GetAwaiter());
        //if (firstRuns < 2)
        //{
        //    var results = boltGraphClient.GetTradingPairs().GetAwaiter().GetResult();

        //    foreach (var result in results)
        //    {
        //        Console.WriteLine($"Trading Pair - Name: {result.Name} Created: {result.CreatedTS}");
        //    }
        //    firstRuns++;
        //}
        //boltGraphClient.Cypher.Merge()
        //batch.ToArray().ToList().ForEach(data =>
        //    Console.WriteLine($"Journal - Seq: {sequence} Low: {data.low} High: {data.high} Open: {data.open} Close: {data.close} Vol: {data.volume} Trans: {data.transactions}")
        //);
    }
}
