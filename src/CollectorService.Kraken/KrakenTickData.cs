using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CryptoExchange.Net.Sockets;
using kk=Kraken.Net.Objects.Models;
using Confluent.Kafka;

namespace CollectorService.Kraken.Models;

[ProtoBuf.ProtoContract()]
public class KrakenTickData : INotification, IAsyncSerializer<KrakenTickData>
{
    public KrakenTickData(string tradingPair, KrakenStreamTick tickData, DateTime timestamp, string topic)
    {
        TickData = tickData;
        TradingPair = tradingPair;
        Timestamp = timestamp;
        Topic = topic;
    }
    [ProtoBuf.ProtoMember(1)]
    public KrakenStreamTick TickData { get; init; }
    [ProtoBuf.ProtoMember(2)]
    public string TradingPair { get; init; }
    [ProtoBuf.ProtoMember(3)]
    public DateTime Timestamp { get; init; }
    [ProtoBuf.ProtoMember(4)]
    public string Topic { get; init; }

    public Task<byte[]> SerializeAsync(KrakenTickData data, SerializationContext context)
    {
        var ms = new MemoryStream();
        ProtoBuf.Serializer.Serialize(ms, data);
        return Task.FromResult(ms.ToArray());
    }
}
