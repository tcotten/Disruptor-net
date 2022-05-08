using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor.StrategyService.Models;
using Neo4jClient;

namespace Disruptor.StrategyService.Data;

public static class Neo4jDBQueries
{
    public static async Task<List<TradingPair>> GetTradingPairs(this IBoltGraphClient boltGraphClient)
    {
        if (!boltGraphClient.IsConnected)
        {
            await boltGraphClient.ConnectAsync();
        }
        var results = await boltGraphClient.Cypher
            .Match("(n:Pair)")
            .Return(n => n.As<TradingPair>())
            .ResultsAsync;
        return results.ToList();
    }
    [BenchmarkDotNet.Attributes.Benchmark]
    public static async Task MergePairCandle(this IBoltGraphClient boltGraphClient, PairCandle pairCandle)
    {
        if (pairCandle == null)
        {
            return;
        }
        if (!boltGraphClient.IsConnected)
        {
            await boltGraphClient.ConnectAsync();
        }
        // Make sure the base data has been populated
        if (pairCandle.TimeUUID == default)
        {
            pairCandle.Init(pairCandle.CandleTS);
        }
        await boltGraphClient.Cypher
            .Merge("(c:Candle { CandleTS: $candleTS })")
            .Set("c = $pairCandle")
            .WithParams(new
            {
                candleTS = pairCandle.CandleTS,
                pairCandle
            })
            .ExecuteWithoutResultsAsync();

        try
        {
            await boltGraphClient.Cypher
                .Match("(pc:Candle)")
                .Where("pc.CandleTS < $candleTS")
                .With("Max(pc) as prevCandle")
                .Match("(c:Candle { CandleTS: $candleTS })")
                .WithParams(new
                {
                    candleTS = pairCandle.CandleTS,
                    ts1 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    timeUUID1 = pairCandle.TimeUUID,
                    candleTo = pairCandle.CandleTS
                })
                .Merge("(prevCandle)-[r1:NEXT_CANDLE]->(c)")
                .OnCreate()
                .Set("r1.from = prevCandle.CandleTS")
                .Set("r1.to = $candleTo")
                .Set("r1.Created = $ts1")
                .Set("r1.TimeUUID = $timeUUID1")
                .ExecuteWithoutResultsAsync();
        }
        catch (Neo4j.Driver.DatabaseException dex)
        {
            if (dex != null)
            {
                // Skip this error if it is the first node being inserted or a node prior to the current string of time based nodes
                string msg = "Failed to create relationship `r1`, node `prevCandle` is missing.";
                if (!dex.Message.Contains(msg))
                {
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            if (ex != null)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    public static async Task InitialDataSetup(this IBoltGraphClient boltGraphClient)
    {
        if (!boltGraphClient.IsConnected)
        {
            await boltGraphClient.ConnectAsync();
        }
        var kExchange = new Exchange() { Name = "Kraken" };
        kExchange.Init();
        var pair1 = new TradingPair() { Name = "BTCUSD" };
        pair1.Init();
        var pair2 = new TradingPair() { Name = "ETHUSD" };
        pair2.Init();

        await boltGraphClient.Cypher
            .Merge("(e:Exchange:CEFI { Name: $kE })")
            .OnMatch()
            .Set("e.LastUpdatedUTC = $pairLastUpdated")
            .OnCreate()
            .Set("e = $kExchange")
            .WithParams(new
            {
                kE = kExchange.Name,
                pairLastUpdated = kExchange.LastUpdatedUTC,
                kExchange
            })
            .Merge("(p1:Pair:Trading_Pair { Name: $pairName1 })")
            .OnMatch()
            .Set("p1.LastUpdatedUTC = $pairLastUpdated1")
            .OnCreate()
            .Set("p1 = $pair1")
            .WithParams(new
            {
                pairName1 = pair1.Name,
                pairLastUpdated1 = pair1.LastUpdatedUTC,
                ts1 = pair1.CreatedTS,
                timeUUID1 = pair1.TimeUUID,
                pair1
            })
            .Merge("(e)-[p1r1:HAS_PAIR]->(p1)")
            .OnMatch()
            .Set("p1r1.LastUpdatedUTC = $pairLastUpdated1")
            .OnCreate()
            .Set("p1r1.Created = $ts1")
            .Set("p1r1.TimeUUID = $timeUUID1")
            .Merge("(p2:Pair:Trading_Pair { Name: $pairName2 })")
            .OnMatch()
            .Set("p2.LastUpdatedUTC = $pairLastUpdated2")
            .OnCreate()
            .Set("p2.Created = $ts2")
            .Set("p2.TimeUUID = $timeUUID2")
            .WithParams(new
            {
                pairName2 = pair2.Name,
                pairLastUpdated2 = pair1.LastUpdatedUTC,
                ts2 = pair2.CreatedTS,
                timeUUID2 = pair2.TimeUUID,
                pair2
            })
            .Merge("(e)-[p2r1:HAS_PAIR]->(p2)")
            .OnMatch()
            .Set("p2r1.LastUpdatedUTC = $pairLastUpdated2")
            .OnCreate()
            .Set("p2r1.Created = $ts2")
            .Set("p2r1.TimeUUID = $timeUUID2")
            .ExecuteWithoutResultsAsync();


    }

}
