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

    public static List<TradingPair> GetTradingPairs(this IBoltGraphClient boltGraphClient)
    {
        if (!boltGraphClient.IsConnected)
        {
            boltGraphClient.ConnectAsync().GetAwaiter().GetResult();
        }
        var results = boltGraphClient.Cypher
            .Match("(n:Pair)")
            .Return(n => n.As<TradingPair>())
            .ResultsAsync.GetAwaiter().GetResult();
        return results.ToList();
    }
}
