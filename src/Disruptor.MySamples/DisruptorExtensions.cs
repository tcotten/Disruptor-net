using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.MySamples;
public static partial class DisruptorExtensions
{
    public static void PublishTickerEvent(this Dsl.ValueDisruptor<TickerEvent> disruptor, TickerEvent eventToPublish)
    {
        using (var unpublishedEventScope = disruptor.PublishEvent())
        {
            ref var evt = ref unpublishedEventScope.Event();
            evt.unixTimestamp = eventToPublish.unixTimestamp;
            evt.low = eventToPublish.low;
            evt.high = eventToPublish.high;
            evt.open = eventToPublish.open;
            evt.close = eventToPublish.close;
            evt.volume = eventToPublish.volume;
            evt.transactions = eventToPublish.transactions;
        }
    }

    public static void PublishTickerEvents(this Dsl.ValueDisruptor<TickerEvent> disruptor, IEnumerable<TickerEvent> eventsToPublish)
    {
        if (eventsToPublish == null || eventsToPublish.Count() == 0) return;

        using (var unpublishedEventScope = disruptor.PublishEvents(eventsToPublish.Count()))
        {
            int i = 0;
            foreach (var eventToPublish in eventsToPublish)
            {
                ref var evt = ref unpublishedEventScope.Event(i);
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

}
