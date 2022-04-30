using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo4jClient;
using System;
using Disruptor.StrategyService.Models;
using System.Threading.Tasks;

namespace Disruptor.StrategyService.Tests
{
    [TestClass]
    public class CypherTests
    {
        
        [TestMethod]
        public Task CypherConnectionTest()
        {
            //var neoClient = new GraphClient(new Uri("http://neo4j:Avalanch3@172.2.96.1:7474/db/data"));
            var neoClient = new GraphClient(new Uri("http://neo4j:Avalanch3@192.168.1.60:7474"));
            //var config = new NeoServerConfiguration();

            neoClient.ConnectAsync().GetAwaiter().GetResult();
            var results = neoClient.Cypher
                .Match("(n:Pair)")
                .Return(n => n.As<TradingPair>())
                .ResultsAsync.GetAwaiter().GetResult();

            foreach (var result in results)
            {
                Console.WriteLine($"Trading Pair - Name: {result.Name} Created: {result.created}");
            }

            return Task.CompletedTask;
        }
    }
}
