using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.StrategyService.Models;

public record class TickerEvent
{
    public long unixTimestamp;
    public double open;
    public double high;
    public double low;
    public double close;
    public double volume;
    public long transactions;
    public static TickerEvent FromCSV(string csvLine)
    {
        string[] values = csvLine.Split(',');
        var tickerEvent = new TickerEvent()
        {
            unixTimestamp = values[0].ToInt64(),
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
