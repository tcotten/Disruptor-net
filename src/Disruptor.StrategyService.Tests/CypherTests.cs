using Microsoft.VisualStudio.TestTools.UnitTesting;
using Neo4jClient;
using System;
using Disruptor.StrategyService.Models;
using System.Threading.Tasks;
using Disruptor.StrategyService.Helpers;
using System.Diagnostics;
using System.Linq;

namespace Disruptor.StrategyService.Tests
{
    [TestClass]
    public class CypherTests
    {
        BoltGraphClient neoClient = new BoltGraphClient(new Uri("neo4j://192.168.1.60:7687"), "neo4j", "Avalanch3");

        internal void CheckConnect()
        {
            if (!neoClient.IsConnected)
            {
                neoClient.ConnectAsync().GetAwaiter().GetResult();
            }
        }

        [TestMethod]
        public async Task CypherConnectionTest()
        {
            //var neoClient = new GraphClient(new Uri("http://neo4j:Avalanch3@172.2.96.1:7474/db/data"));
            //var neoClient = new GraphClient(new Uri("http://neo4j:Avalanch3@192.168.1.60:7474"));
            //var neoClient = new BoltGraphClient(new Uri("neo4j://192.168.1.60:7687"), "neo4j", "Avalanch3");
            //var config = new NeoServerConfiguration();

            CheckConnect();
            var results = await neoClient.Cypher
                .Match("(n:Pair)")
                .Return(n => n.As<TradingPair>())
                .ResultsAsync;

            foreach (var result in results)
            {
                Console.WriteLine($"Trading Pair - Name: {result.Name} Created: {result.CreatedTS}");
            }
        }

        [TestMethod]
        public async Task CypherMergeTest()
        {
            CheckConnect();
            string kExchange = "Kraken";
            await neoClient.Cypher
                .Merge("e:Exchange:CEFI { Name: {kExchange} }")
                .OnCreate()
                .Set("e.Created = {ts}")
                .Set("e.TimeUUID = {timeUUID}")
                .WithParams(new
                {
                    kExchange,
                    ts = DateTime.UnixEpoch,
                    timeUUID = GuidGenerator.GenerateTimeBasedGuid()
                })
                .ExecuteWithoutResultsAsync();

        }
        [TestMethod]
        public async Task CypherDeleteAllDataTest()
        {
            CheckConnect();
            await neoClient.Cypher
                .Match("(n)")
                .DetachDelete("n")
                .ExecuteWithoutResultsAsync();
        }
        [TestMethod]
        public async Task CypherDataCreationTest()
        {
            CheckConnect();
            string kExchange = "Kraken";
            await neoClient.Cypher
                .Merge("(e:Exchange:CEFI { Name: $kExchange })")
                .OnCreate()
                .Set("e.Created = $ts")
                .Set("e.TimeUUID = $timeUUID")
                .WithParams(new
                {
                    kExchange,
                    ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    timeUUID = GuidGenerator.GenerateTimeBasedGuid()
                })
                .Merge("(p1:Pair:Trading_Pair { Name: $pair1 })")
                .OnCreate()
                .Set("p1.Created = $ts1")
                .Set("p1.TimeUUID = $timeUUID1")
                .WithParams(new
                {
                    pair1 = "EDHUSD",
                    ts1 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    timeUUID1 = GuidGenerator.GenerateTimeBasedGuid()
                })
                .Merge("(e)-[p1r1:HAS_PAIR]->(p1)")
                .OnCreate()
                .Set("p1r1.Created = $ts1")
                .Set("p1r1.TimeUUID = $timeUUID1")
                .Merge("(p2:Pair:Trading_Pair { Name: $pair2 })")
                .OnCreate()
                .Set("p2.Created = $ts2")
                .Set("p2.TimeUUID = $timeUUID2")
                .WithParams(new
                {
                    pair2 = "BTCUSD",
                    ts2 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    timeUUID2 = GuidGenerator.GenerateTimeBasedGuid()
                })
                .Merge("(e)-[p2r1:HAS_PAIR]->(p2)")
                .OnCreate()
                .Set("p2r1.Created = $ts1")
                .Set("p2r1.TimeUUID = $timeUUID2")
                .ExecuteWithoutResultsAsync();

            var results = await neoClient.Cypher
                .Match("(e:Exchange)")
                .Return(e => e.As<string>())
                .ResultsAsync;
            Assert.IsNotNull(results);

        }

        [TestMethod]
        public async Task CypherDataCreationFromObjectTest()
        {
            CheckConnect();
            var kExchange = new Exchange() { Name = "Kraken" };
            kExchange.Init();
            var pair1 = new TradingPair() { Name = "BTCUSD" };
            pair1.Init();
            var pair2 = new TradingPair() { Name = "ETHUSD" };
            pair2.Init();

            await neoClient.Cypher
                .Merge("(e:Exchange:CEFI { Name: $kE })")
                .OnCreate()
                .Set("e = $kExchange")
                .WithParams(new
                {
                    kE = kExchange.Name,
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

            var pairResult1 = await neoClient.Cypher
                .Match("(p1:Pair)")
                .Return(p1 => p1.As<TradingPair>())
                .ResultsAsync;
            Assert.IsNotNull(pairResult1);
            var r1Pair = pairResult1.ToList().First();
            Assert.AreEqual(pair1.Name, r1Pair.Name);
            Assert.AreEqual(pair1.CreatedTS, r1Pair.CreatedTS);
            Assert.AreEqual(pair1.CreatedLocal, r1Pair.CreatedLocal);
            Assert.AreEqual(pair1.CreatedUTC, r1Pair.CreatedUTC);
            Assert.AreEqual(pair1.TimeUUID, r1Pair.TimeUUID);

            var results = await neoClient.Cypher
                .Match("(e:Exchange)")
                .Return(e => e.As<Exchange>())
                .ResultsAsync;            
            Assert.IsNotNull(results);
            var rExchange = results.ToList().First();
            Assert.AreEqual(kExchange.Name, rExchange.Name);
            Assert.AreEqual(kExchange.CreatedTS, rExchange.CreatedTS);
            Assert.AreEqual(kExchange.CreatedLocal, rExchange.CreatedLocal);
            Assert.AreEqual(kExchange.CreatedUTC, rExchange.CreatedUTC);
            Assert.AreEqual(kExchange.TimeUUID, rExchange.TimeUUID);

        }
        [TestMethod]
        public async Task CypherCreatePairCandleFromObjectTest()
        {
            CheckConnect();
            //c2.open = 1915.7, c2.high = 1918.89, c2.low = 1915.7, c2.close = 1918.89, c2.volume = 6.21153841, c2.transactions = 20
            var pairCandle = new PairCandle() {
                CandleTS = 1617235140,
                Open = 1915.7m,
                High = 1918.89m,
                Low = 1915.7m,
                Close = 1918.89m,
                Volume = 6.21153841m,
                Transactions = 20
            };
            // Make sure the base data has been populated
            if (pairCandle.TimeUUID == default)
            {
                pairCandle.Init(pairCandle.CandleTS);
            }
            var query3 = neoClient.Cypher
                .Match("(n:Pair)")
                .Where((TradingPair n) => n.Name == "ETHUSD")
                .Return(n => n.As<TradingPair>())
                .Query;

            var query = neoClient.Cypher
                .Match("(pc:Candle)")
                .Where("pc.CandleTS < $candleTS")
                .WithParam("candleTS",  pairCandle.CandleTS)
                .Return(pc => pc.As<PairCandle>())
                .Query;

            var query2 = neoClient.Cypher
                .Merge("(c:Candle { CandleTS: $candleTS })")
                .Set("c = $pairCandle")
                .WithParams(new
                {
                    candleTS = pairCandle.CandleTS,
                    pairCandle
                })
                .Query;

            // Create the new node since we need it either way then try to connect it up if the one preivous exists
            await neoClient.Cypher
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
                await neoClient.Cypher
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
                    .CreateUnique("(prevCandle)-[r1:NEXT_CANDLE]->(c)")
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

        }

        [TestMethod]
        public async Task MyNeo4jClientAsyncCypherExampleTest()
        {
            var graphClient = new BoltGraphClient(new Uri("neo4j://192.168.1.60:7687"), "neo4j", "Avalanch3");
            await graphClient.ConnectAsync();

            var query = graphClient.Cypher
                .Match("(n:Pair)")
                .Where((TradingPair n) => n.Name == "ETHUSD")
                .Return(n => n.As<TradingPair>())
                .Query;

            var results = await graphClient.Cypher
                .Match("(n:Pair)")
                .Where((TradingPair n) => n.Name == "ETHUSD")
                .Return(n => n.As<TradingPair>())
                .ResultsAsync;

            Assert.AreEqual(typeof(TradingPair), results.GetType());
            
        }
        //[TestMethod]
        public async Task Neo4jClientAsyncCypherExampleTest()
        {
            var newUser = new User { Id = 456, Name = "Jim" };
            var graphClient = new BoltGraphClient(new Uri("neo4j://localhost:7687"), "neo4juser", "neo4jpassword");
            await graphClient.ConnectAsync();

            // Create the user - Note the change in using variables from {newUser} to $newUser
            await graphClient.Cypher
                .Create("(user:User $newUser)")
                .WithParam("newUser", newUser)
                .ExecuteWithoutResultsAsync();

            var results = await graphClient.Cypher
                .Match("(user:User)")
                .Where((User user) => user.Id == 456)
                .Return(user => user.As<User>())
                .ResultsAsync;

            Assert.AreEqual(newUser.Id, results.ToList().First().Id);
        }
    }
    public class User
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? Email { get; set; }
    }
}
