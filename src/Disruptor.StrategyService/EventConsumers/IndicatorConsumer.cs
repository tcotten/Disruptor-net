using Disruptor.StrategyService.Data;
using Disruptor.StrategyService.Helpers;
using Disruptor.StrategyService.Models;
using Neo4jClient;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.StrategyService.EventConsumers;

public interface IIndicatorConsumer : IBatchEventHandler<PairCandle>, IBatchEventHandler<PairTicker>
{
    PairCandleList Candles { get; }
}
public class IndicatorConsumer : IIndicatorConsumer
{
    private PairCandleList candles;
    private IBoltGraphClient boltGraphClient { get; init; }

    public PairCandleList Candles { get { return candles; } }

    //private int firstRuns = 0;
    public IndicatorConsumer(IBoltGraphClient boltGraphClient)
    {
        this.boltGraphClient = boltGraphClient;
        candles = new PairCandleList();
    }

    public void OnBatch(EventBatch<PairCandle> batch, long sequence)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        // Add the new candles
        candles.AddRange(batch.ToArray());
        // Filter out any duplicates and go newest to oldest
        var data = batch[0];
        Console.WriteLine($"Indicator Batch - Count: {batch.Length} Seq: {sequence}");
        Console.WriteLine($"Indicator Item[0] - Seq: {sequence} Low: {data.Low} High: {data.High} Open: {data.Open} Close: {data.Close} Vol: {data.Volume} Trans: {data.Transactions}");
        // Create the indicator(s)
        BuildIndicators();
        //foreach (var candle in batch.ToArray())
        //{
        //    boltGraphClient.MergePairCandle(candle).GetAwaiter().GetResult();
        //    //System.Threading.Thread.Sleep(10);
        //}
        watch.Stop();
        Console.WriteLine($"Total Execution Time: {watch.Elapsed.TotalSeconds} seconds");
    }

    public void OnBatch(EventBatch<PairTicker> batch, long sequence)
    {
        throw new NotImplementedException();
    }

    private void BuildIndicators()
    {
        var ema5 = candles.GetEma(5).Reverse();
        candles.AddUpdateIndicator("EMA5", ema5);
        var atrp = candles.GetPairVolitility(200);
        candles.AddUpdateIndicator("ATRP", atrp);
    }
}
