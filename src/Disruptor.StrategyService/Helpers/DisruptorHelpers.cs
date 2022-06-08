using Disruptor.StrategyService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.StrategyService;

public static class DisruptorHelpers
{
    public static void PublishTickerEvent(this Dsl.Disruptor<PairCandle> disruptor, PairCandle eventToPublish)
    {
        using (var unpublishedEventScope = disruptor.PublishEvent())
        {
            var evt = unpublishedEventScope.Event();
            evt.CandleTS = eventToPublish.CandleTS;
            evt.Low = eventToPublish.Low;
            evt.High = eventToPublish.High;
            evt.Open = eventToPublish.Open;
            evt.Close = eventToPublish.Close;
            evt.Volume = eventToPublish.Volume;
            evt.Transactions = eventToPublish.Transactions;
        }
    }

    public static void PublishTickerEvents(this Dsl.Disruptor<PairCandle> disruptor, IEnumerable<PairCandle> eventsToPublish)
    {
        if (eventsToPublish == null || eventsToPublish.Count() == 0) return;

        using (var unpublishedEventScope = disruptor.PublishEvents(eventsToPublish.Count()))
        {
            int i = 0;
            foreach (var eventToPublish in eventsToPublish)
            {
                var evt = unpublishedEventScope.Event(i);
                evt.CandleTS = eventToPublish.CandleTS;
                evt.Low = eventToPublish.Low;
                evt.High = eventToPublish.High;
                evt.Open = eventToPublish.Open;
                evt.Close = eventToPublish.Close;
                evt.Volume = eventToPublish.Volume;
                evt.Transactions = eventToPublish.Transactions;
                i++;
            }
        }
    }

    //internal sealed class DefaultBatchEventHandler : IDisruptorServiceBuilder
    //{
    //    public DefaultBatchEventHandler(IServiceCollection services)
    //    {
    //        Services = services ?? throw new ArgumentNullException(nameof(services));
    //        Instances = new List<IBatchEventHandler>();
    //    }

    //    public IServiceCollection Services { get; set; }
    //    public List<IBatchEventHandler> Instances { get; set; }
    //}

}
