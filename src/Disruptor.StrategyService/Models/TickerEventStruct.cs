using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.StrategyService.Models;

public record struct TickerEventStruck
{
    public long unixTimestamp;
    public double open;
    public double high;
    public double low;
    public double close;
    public double volume;
    public long transactions;
    public static PairCandle FromCSV(string csvLine)
    {
        string[] values = csvLine.Split(',');
        var tickerEvent = new PairCandle()
        {
            CandleTS = values[0].ToInt64(),
            open = values[1].ToInt64(),
            high = values[2].ToInt64(),
            low = values[3].ToInt64(),
            close = values[4].ToInt64(),
            volume = values[5].ToInt64(),
            transactions = values[6].ToInt64()
        };
        return tickerEvent;
    }
}
