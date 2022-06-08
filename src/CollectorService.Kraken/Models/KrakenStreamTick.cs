using Confluent.Kafka;
using KafkaService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kk=Kraken.Net.Objects.Models;

namespace CollectorService.Kraken.Models;

//public class KrakenStreamTickHandler : KafkaNotificationHandler<KrakenStreamTick>
//{
//    public KrakenStreamTickHandler(KafkaDependentProducer<string, KrakenStreamTick> producer) : base(producer)
//    {
//    }
//}
public class KrakenStreamTickMediatrHandler : MediatR.INotificationHandler<KrakenStreamTick>
{
    private List<Models.KrakenStreamTick> krakenStreamTicks;
    public KrakenStreamTickMediatrHandler()
    {
        this.krakenStreamTicks = new List<Models.KrakenStreamTick>();
    }
    public Task Handle(KrakenStreamTick notification, CancellationToken cancellationToken)
    {
        krakenStreamTicks.Add(notification);
        return Task.CompletedTask;
    }
}
[ProtoBuf.ProtoContract()]
public class KrakenStreamTick : MediatR.INotification, IAsyncSerializer<KrakenTickData>
{
    public KrakenStreamTick(KrakenBestEntry bestBids, KrakenBestEntry bestAsks, KrakenTickInfo trades, KrakenLastTrade lastTrade, KrakenTickInfo volume, KrakenTickInfo high, KrakenTickInfo low, KrakenTickInfo open, KrakenTickInfo volumeWeightedAveragePrice)
    {
        BestBids = bestBids;
        BestAsks = bestAsks;
        Trades = trades;
        LastTrade = lastTrade;
        Volume = volume;
        High = high;
        Low = low;
        Open = open;
        VolumeWeightedAveragePrice = volumeWeightedAveragePrice;
    }
    [ProtoBuf.ProtoMember(1)]
    public KrakenBestEntry BestBids { get; init; }
    [ProtoBuf.ProtoMember(2)]
    public KrakenBestEntry BestAsks { get; init; }
    [ProtoBuf.ProtoMember(3)]
    public KrakenTickInfo Trades { get; init; }
    [ProtoBuf.ProtoMember(4)]
    public KrakenLastTrade LastTrade { get; init; }
    [ProtoBuf.ProtoMember(5)]
    public KrakenTickInfo Volume { get; init; }
    [ProtoBuf.ProtoMember(6)]
    public KrakenTickInfo High { get; init; }
    [ProtoBuf.ProtoMember(7)]
    public KrakenTickInfo Low { get; init; }
    [ProtoBuf.ProtoMember(8)]
    public KrakenTickInfo Open { get; init; }
    public KrakenTickInfo VolumeWeightedAveragePrice { get; init; }

    public Task<byte[]> SerializeAsync(KrakenTickData data, SerializationContext context)
    {
        throw new NotImplementedException();
    }
}
[ProtoBuf.ProtoContract()]
public class KrakenTickInfo
{
    [ProtoBuf.ProtoMember(1)]
    public decimal ValueToday { get; init; }
    [ProtoBuf.ProtoMember(2)]
    public decimal Value24H { get; init; }

}
[ProtoBuf.ProtoContract()]
public class KrakenBestEntry
{
    [ProtoBuf.ProtoMember(1)]
    public decimal Price { get; init; }
    [ProtoBuf.ProtoMember(2)]
    public decimal LotQuantity { get; init; }
    [ProtoBuf.ProtoMember(3)]
    public decimal Quantity { get; init; }
}
[ProtoBuf.ProtoContract()]
public class KrakenLastTrade
{
    [ProtoBuf.ProtoMember(1)]
    public decimal Price { get; init; }
    [ProtoBuf.ProtoMember(2)]
    public decimal Quantity { get; init; }
}

