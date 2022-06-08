using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.StrategyService.Models;

public record class PairCandle : GraphModelBaseRecord, IQuote
{
    public PairCandle() : base() { }
    public PairCandle(long unixTimeStampUTC) : base(unixTimeStampUTC) { }

    private long candleTS;
    /// <summary>
    /// This is the unixTimestamp milliseconds UTC value.
    /// </summary>
    public long CandleTS
    {
        get
        { return candleTS; }
        set
        {
            if (candleTS != value)
            {
                candleTS = value.ValidateUnixTimeStamp();
            }
        }
    }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public decimal Transactions { get; set; }

    public DateTime Date { get { return DateTimeOffset.FromUnixTimeMilliseconds(CandleTS).DateTime; } }

    public static PairCandle FromCSV(string csvLine)
    {
        string[] values = csvLine.Split(',');
        var tickerEvent = new PairCandle(values[0].ToInt64())
        {
            CandleTS = values[0].ToInt64(),
            Open = values[1].ToDecimal(),
            High = values[2].ToDecimal(),
            Low = values[3].ToDecimal(),
            Close = values[4].ToDecimal(),
            Volume = values[5].ToDecimal(),
            Transactions = values[6].ToDecimal()
        };
        return tickerEvent;
    }
}
public static class ObjectExtensions
{
    public static long ToInt64(this string str)
    {
        return Convert.ToInt64(str);
    }
    public static long ToLong(this string str)
    {
        return Convert.ToInt64(str);
    }
    public static decimal ToDecimal(this string str)
    {
        return Decimal.Parse(str, System.Globalization.NumberStyles.Any);
    }
    public static double ToDouble(this string str)
    {
        return Convert.ToDouble(str);
    }
    public static List<decimal> GetOpenList(this IEnumerable<PairCandle> candles)
    {
        return candles.GetCandlesByOHLCType(CandleOHLC.Open).ToList();
    }
    public static List<decimal> GetHighList(this IEnumerable<PairCandle> candles)
    {
        return candles.GetCandlesByOHLCType(CandleOHLC.High).ToList();
    }
    public static List<decimal> GetLowList(this IEnumerable<PairCandle> candles)
    {
        return candles.GetCandlesByOHLCType(CandleOHLC.Low).ToList();
    }
    public static List<decimal> GetCloseList(this IEnumerable<PairCandle> candles)
    {
        return candles.GetCandlesByOHLCType(CandleOHLC.Close).ToList();
    }
    public static decimal[] GetCandlesByOHLCType(this IEnumerable<PairCandle> candles, CandleOHLC ohlcType = CandleOHLC.Close)
    {
        int len = candles.Count();
        var candleArray = candles.ToArray();
        decimal[] candleParts = new decimal[len];
        if (ohlcType == CandleOHLC.Transactions)
        {
            throw new NotImplementedException();
        }
        Parallel.For(0, len, i => {
            switch (ohlcType)
            {
                case CandleOHLC.Open:
                    candleParts[i] = candleArray[i].Open;
                    break;
                case CandleOHLC.High:
                    candleParts[i] = candleArray[i].High;
                    break;
                case CandleOHLC.Low:
                    candleParts[i] = candleArray[i].Low;
                    break;
                case CandleOHLC.Close:
                    candleParts[i] = candleArray[i].Close;
                    break;
                case CandleOHLC.Volume:
                    candleParts[i] = candleArray[i].Volume;
                    break;
                case CandleOHLC.Transactions:
                    candleParts[i] = candleArray[i].Transactions;
                    break;
                default:
                    break;
            }
        });
        return candleParts;
    }
}
public enum CandleOHLC
{
    Open,
    High,
    Low,
    Close,
    Volume,
    Transactions
}
