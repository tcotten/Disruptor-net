using Disruptor.Dsl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.MySamples;

public static class CurrencySampleMain
{
    public static void Main()
    {
        //var dHandler = new CurrencySample(new TickerEventHandler(), 33554432);
        var disruptor = new ValueDisruptor<TickerEvent>(() => new TickerEvent(), 33554432);

        //disruptor.HandleEventsWith(new ReplicationConsumer()).Then(new JournalConsumer()).Then(new TickerEventHandler());
        disruptor.HandleEventsWith(new ReplicationConsumer(), new JournalConsumer()).Then(new TickerEventHandler());

        var producer = new CurrencyProducer(@"C:\Users\tcott\OneDrive\Apps\gunbot\Disruptor-net\src\Disruptor.MySamples\Data\ETHUSD_1.csv");

        disruptor.Start();

        producer.ProduceEvents(disruptor.PublishTickerEvents);

        Console.WriteLine("Press enter to exit.");
        Console.ReadKey();
    }

}
public class CurrencySample
{
    int bufferSize = 1024;
    ValueDisruptor<TickerEvent> disruptor;
    public CurrencySample(IValueEventHandler<TickerEvent> eventHandler)
    {
        this.bufferSize = 1024;
        disruptor = new(() => new TickerEvent(), bufferSize);
        disruptor.HandleEventsWith(eventHandler);
    }
    public CurrencySample(IValueEventHandler<TickerEvent> eventHandler, int bufferSize = 1024)
    {
        this.bufferSize = bufferSize;
        disruptor = new(() => new TickerEvent(), bufferSize);
        disruptor.HandleEventsWith(eventHandler);
    }
    public void Start()
    {
        disruptor.Start();
    }
    public void createTickerEvent(TickerEvent tickerEvent)
    {
        using (var unpublishedEventScope = disruptor.PublishEvent())
        {
            ref var evt = ref unpublishedEventScope.Event();
            evt.unixTimestamp = tickerEvent.unixTimestamp;
            evt.low = tickerEvent.low;
            evt.high = tickerEvent.high;
            evt.open = tickerEvent.open;
            evt.close = tickerEvent.close;
            evt.volume = tickerEvent.volume;
            evt.transactions = tickerEvent.transactions;
        }

    }
}
public class TickerEventHandler : IValueEventHandler<TickerEvent>
{
    public void OnEvent(ref TickerEvent data, long sequence, bool endOfBatch)
    {
        Console.WriteLine($"Data - Seq: {sequence} Low: {data.low} High: {data.high} Open: {data.open} Close: {data.close} Vol: {data.volume} Trans: {data.transactions}");
        if (endOfBatch) { Console.WriteLine("Ticker End of Batch."); }
    }
}
