using Disruptor.StrategyService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.StrategyService;

public static class DisruptorHelpers
{
    public static void PublishTickerEvent(this Dsl.Disruptor<TickerEvent> disruptor, TickerEvent eventToPublish)
    {
        using (var unpublishedEventScope = disruptor.PublishEvent())
        {
            var evt = unpublishedEventScope.Event();
            evt.unixTimestamp = eventToPublish.unixTimestamp;
            evt.low = eventToPublish.low;
            evt.high = eventToPublish.high;
            evt.open = eventToPublish.open;
            evt.close = eventToPublish.close;
            evt.volume = eventToPublish.volume;
            evt.transactions = eventToPublish.transactions;
        }
    }

    public static void PublishTickerEvents(this Dsl.Disruptor<TickerEvent> disruptor, IEnumerable<TickerEvent> eventsToPublish)
    {
        if (eventsToPublish == null || eventsToPublish.Count() == 0) return;

        using (var unpublishedEventScope = disruptor.PublishEvents(eventsToPublish.Count()))
        {
            int i = 0;
            foreach (var eventToPublish in eventsToPublish)
            {
                var evt = unpublishedEventScope.Event(i);
                evt.unixTimestamp = eventToPublish.unixTimestamp;
                evt.low = eventToPublish.low;
                evt.high = eventToPublish.high;
                evt.open = eventToPublish.open;
                evt.close = eventToPublish.close;
                evt.volume = eventToPublish.volume;
                evt.transactions = eventToPublish.transactions;
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
