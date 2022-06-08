using Microsoft.VisualStudio.TestTools.UnitTesting;
using Disruptor.StrategyService.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disruptor.StrategyService.Models;
using Microsoft.Data.Analysis;

namespace Disruptor.StrategyService.Helpers.Tests
{
    [TestClass()]
    public class TulipTests
    {
        private List<PairCandle> candles;
        private PairCandleList pairList;

        public TulipTests()
        {
            candles = new List<PairCandle>()
            {
                new PairCandle() { Open = 4, High = 7, Low = 3, Close = 5, Volume = 200, Transactions = 20 },
                new PairCandle() { Open = 5, High = 8, Low = 4, Close = 7, Volume = 220, Transactions = 10 },
                new PairCandle() { Open = 15, High = 18, Low = 14, Close = 17, Volume = 120, Transactions = 30 },
                new PairCandle() { Open = 25, High = 28, Low = 24, Close = 28, Volume = 300, Transactions = 40 },
                new PairCandle() { Open = 33, High = 55, Low = 30, Close = 55, Volume = 400, Transactions = 50 },
                new PairCandle() { Open = 55, High = 66, Low = 55, Close = 64, Volume = 500, Transactions = 60 }
            };
            pairList = new PairCandleList();
            pairList.AddRange(candles);
        }
        //[TestMethod()]
        //public void EMATest()
        //{
        //    var results = TulipExtensions.EMA(candles, 3);
        //    Assert.IsNotNull(results);
        //}

        [TestMethod()]
        public void GetCandlesByOHLCTypeTest()
        {
            var results = candles.GetCandlesByOHLCType();
            Assert.IsNotNull(results);
            Assert.AreEqual(candles.Count, results.Count());
            Assert.AreEqual(candles[0].Close, results[0]);
            results = candles.GetCandlesByOHLCType(CandleOHLC.Open);
            Assert.AreEqual(candles.Count, results.Count());
            Assert.AreEqual(candles[0].Open, results[0]);
            results = candles.GetCandlesByOHLCType(CandleOHLC.High);
            Assert.AreEqual(candles.Count, results.Count());
            Assert.AreEqual(candles[0].High, results[0]);
            results = candles.GetCandlesByOHLCType(CandleOHLC.Low);
            Assert.AreEqual(candles.Count, results.Count());
            Assert.AreEqual(candles[0].Low, results[0]);
            results = candles.GetCandlesByOHLCType(CandleOHLC.Close);
            Assert.AreEqual(candles.Count, results.Count());
            Assert.AreEqual(candles[0].Close, results[0]);
        }

        [TestMethod]
        public void DataFrameTest()
        {

            PrimitiveDataFrameColumn<decimal> open = new PrimitiveDataFrameColumn<decimal>("Open", pairList.OpenList);
            PrimitiveDataFrameColumn<decimal> high = new PrimitiveDataFrameColumn<decimal>("High", pairList.HighList);
            PrimitiveDataFrameColumn<decimal> low = new PrimitiveDataFrameColumn<decimal>("Low", pairList.LowList);
            PrimitiveDataFrameColumn<decimal> close = new PrimitiveDataFrameColumn<decimal>("Close", pairList.CloseList);
            PrimitiveDataFrameColumn<decimal> volume = new PrimitiveDataFrameColumn<decimal>("Volume", pairList.VolumeList);
            PrimitiveDataFrameColumn<decimal> transactions = new PrimitiveDataFrameColumn<decimal>("Transactions", pairList.TransactionList);

            DataFrame df = new DataFrame(open, high, low, close, volume, transactions);
            Assert.IsNotNull(df);
        }

    }
}
