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
    public void OnBatch(EventBatch<PairCandle> batch, long sequence)
    {
        var data = batch[0];
        Console.WriteLine($"StepGridStrategy Batch - Count: {batch.Length} Seq: {sequence}");
        Console.WriteLine($"StepGridStrategy Item[0] - Seq: {sequence} Low: {data.low} High: {data.high} Open: {data.open} Close: {data.close} Vol: {data.volume} Trans: {data.transactions}");
        //batch.ToArray().ToList().ForEach(data =>
        //    Console.WriteLine($"StepGridStrategy - Seq: {sequence} Low: {data.low} High: {data.high} Open: {data.open} Close: {data.close} Vol: {data.volume} Trans: {data.transactions}")
        //);
    }
}
