using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.StrategyService.Models;

public record class PairCandle : GraphModelBaseRecord
{
    public PairCandle() : base() { }
    public PairCandle(long unixTimeStampUTC) : base(unixTimeStampUTC) { }

    /// <summary>
    /// This is the unixTimestamp milliseconds UTC value.
    /// </summary>
    public long CandleTS { get; set; }
    public double open { get; set; }
    public double high { get; set; }
    public double low { get; set; }
    public double close { get; set; }
    public double volume { get; set; }
    public long transactions { get; set; }
    public static PairCandle FromCSV(string csvLine)
    {
        string[] values = csvLine.Split(',');
        var tickerEvent = new PairCandle(values[0].ToInt64())
        {
            CandleTS = values[0].ToInt64(),
            open = values[1].ToDouble(),
            high = values[2].ToDouble(),
            low = values[3].ToDouble(),
            close = values[4].ToDouble(),
            volume = values[5].ToDouble(),
            transactions = values[6].ToInt64()
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
    public static double ToDouble(this string str)
    {
        return Convert.ToDouble(str);
    }
}
