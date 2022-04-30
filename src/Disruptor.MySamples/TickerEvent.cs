namespace Disruptor.MySamples;

public struct TickerEvent
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
        var tickerEvent = new TickerEvent();
        string[] values = csvLine.Split(',');
        tickerEvent.unixTimestamp = Convert.ToInt64(values[0]);
        tickerEvent.open = Convert.ToDouble(values[1]);
        tickerEvent.high = Convert.ToDouble(values[2]);
        tickerEvent.low = Convert.ToDouble(values[3]);
        tickerEvent.close = Convert.ToDouble(values[4]);
        tickerEvent.volume = Convert.ToDouble(values[5]);
        tickerEvent.transactions = Convert.ToInt64(values[6]);
        return tickerEvent;
    }
}
