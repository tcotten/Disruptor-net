using Disruptor.StrategyService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using tinet;

namespace Disruptor.StrategyService.Helpers;

public static class IndicatorExtensions
{
    //public static IEnumerable<double> EMA(IEnumerable<PairCandle> candles, int lookBack, CandleOHLC ohlcType = CandleOHLC.Close)
    //{
    //    var candleParts = candles.GetCandlesByOHLCType(ohlcType);
    //    double[] options = new double[] { lookBack };

    //    //Find output size and allocate output space.
    //    int output_length = candleParts.Length - tinet.indicators.ema.start(options);
    //    double[] output = new double[output_length];

    //    double[][] inputs = { candleParts };
    //    double[][] outputs = { output };
    //    int success = tinet.indicators.ema.run(inputs, options, outputs);

    //    if (success != 0)
    //    {
    //        return new List<double>();
    //    }
    //    return outputs[0];
    //}
    //public static IEnumerable<double> SMA(IEnumerable<PairCandle> candles, int lookBack, CandleOHLC ohlcType = CandleOHLC.Close)
    //{
    //    var candleParts = candles.GetCandlesByOHLCType(ohlcType);
    //    double[] options = new double[] { lookBack };

    //    //Find output size and allocate output space.
    //    int output_length = candleParts.Length - tinet.indicators.sma.start(options);
    //    double[] output = new double[output_length];

    //    double[][] inputs = { candleParts };
    //    double[][] outputs = { output };
    //    int success = tinet.indicators.sma.run(inputs, options, outputs);

    //    if (success != 0)
    //    {
    //        return new List<double>();
    //    }
    //    return outputs[0];
    //}
    public static ATRPIndicator GetPairVolitility(this PairCandleList candles, int candleCount = 0)
    {
        var atrpInd = new ATRPIndicator();
        var candlesOpen = candles.OpenList;
        var candlesHigh = candles.HighList;
        var candlesLow = candles.LowList;
        var candlesClose = candles.CloseList;
        int candlesCnt = candleCount == 0 ? candles.Count : candleCount > candles.Count ? candles.Count : candleCount;
        for (int i = 0; i < candlesCnt; i++)
        {
            decimal atrp = new decimal[] { (candlesHigh[i] - candlesLow[i]), Math.Abs(candlesHigh[i] - candlesClose[i]), Math.Abs(candlesLow[i] - candlesClose[i]) }.Max();
            atrp /= candlesClose[i];
            atrp *= 100;
            atrpInd.Candles.Add(atrp);
            atrpInd.Avg += atrp;
            if (candlesOpen[i] > candlesClose[i])
            {
                atrpInd.UpAvg += atrp;
                atrpInd.UpCnt += 1;
            }
            else
            {
                atrpInd.DownAvg += atrp;
                atrpInd.DownCnt += 1;
            }
            if (atrp > atrpInd.Max) { atrpInd.Max = atrp; }
            if (atrp < atrpInd.Min || atrpInd.Min == 0) { atrpInd.Min = atrp; }
        }
        atrpInd.Avg = atrpInd.Avg / atrpInd.Candles.Count;
        atrpInd.UpAvg = atrpInd.UpAvg / atrpInd.UpCnt;
        atrpInd.DownAvg = atrpInd.DownAvg / atrpInd.DownCnt;
        atrpInd.PctUpDown = atrpInd.UpCnt / atrpInd.DownCnt;
        return atrpInd;
    }
}

