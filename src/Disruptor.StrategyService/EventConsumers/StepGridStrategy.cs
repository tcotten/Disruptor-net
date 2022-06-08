using Disruptor.StrategyService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.StrategyService.EventConsumers;
public interface IStepGridStrategy : IBatchEventHandler<PairCandle> { }
public class StepGridStrategy : IStepGridStrategy
{
    private readonly IIndicatorConsumer _indicatorConsumer;
    public StepGridStrategy(IIndicatorConsumer indicatorConsumer) { _indicatorConsumer = indicatorConsumer; }
    // TODO:
    //          Load the current order list for access to determine what trades are outstanding
    //          
    public void OnBatch(EventBatch<PairCandle> batch, long sequence)
    {
        var data = batch[0];
        Console.WriteLine($"StepGridStrategy Batch - Count: {batch.Length} Seq: {sequence} Indicator Count: {_indicatorConsumer.Candles.Indicators.Count}");
        Console.WriteLine($"StepGridStrategy Item[0] - Seq: {sequence} Low: {data.Low} High: {data.High} Open: {data.Open} Close: {data.Close} Vol: {data.Volume} Trans: {data.Transactions}");
        //batch.ToArray().ToList().ForEach(data =>
        //    Console.WriteLine($"StepGridStrategy - Seq: {sequence} Low: {data.low} High: {data.high} Open: {data.open} Close: {data.close} Vol: {data.volume} Trans: {data.transactions}")
        //);
    }
    
}
