using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Disruptor.StrategyService.Models;
/// <summary>
/// Maintains a list of PairCandle objects along with corresponding lists of OHLCVT time descending
/// </summary>
public class PairCandleList : List<PairCandle>
{
    static long PairCandleSelector(PairCandle candle) => candle.CandleTS;
    public List<decimal> OpenList { get; private set; } = new List<decimal>();
    public List<decimal> HighList { get; private set; } = new List<decimal>();
    public List<decimal> LowList { get; private set; } = new List<decimal>();
    public List<decimal> CloseList { get; private set; } = new List<decimal>();
    public List<decimal> VolumeList { get; private set; } = new List<decimal>();
    public List<decimal> TransactionList { get; private set; } = new List<decimal>();
    public Dictionary<string, dynamic> Indicators { get; private set; } = new Dictionary<string, dynamic>();

    /// <summary>
    /// The date descending list of indicators based on the current candleset.
    /// </summary>
    /// <param name="indicatorName">Name of the indicator to be used by the strategy.</param>
    /// <param name="indicatorList">The list should be in date descending order. Since it is dynamic the internal can't handle that without implementing an interface.</param>
    public void AddUpdateIndicator(string indicatorName, dynamic indicatorList)
    {
        if (Indicators.ContainsKey(indicatorName))
        {
            Indicators[indicatorName] = indicatorList;
        }
        else
        {
            Indicators.Add(indicatorName, indicatorList);
        }
    }
    public T GetIndicator<T>(string indicatorName) where T : new()
    {
        if (!Indicators.ContainsKey(indicatorName))
        {
            return new T();
        }
        var kvp = Indicators[indicatorName];
        return (T)kvp;
    }
    /// <summary>
    /// Returns a strongly typed list of indicators for standard indicators (ex: EMA, SMA, etc)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="indicatorName"></param>
    /// <returns></returns>
    public List<T> GetListIndicator<T>(string indicatorName)
    {
        if (!Indicators.ContainsKey(indicatorName))
        {
            return new List<T>();
        }
        var kvp = Indicators[indicatorName];
        return (List<T>)kvp;
    }
    /// <summary>
    /// Adds the PairCandle to the beginning of the lists index 0
    /// </summary>
    /// <param name="item"></param>
    public new void Add(PairCandle item)
    {
        // Skip duplicates
        if (this.Any(i => i.CandleTS == item.CandleTS))
        {
            return;
        }
        base.Insert(0, item);
        OpenList.Insert(0, item.Open);
        HighList.Insert(0, item.High);
        LowList.Insert(0, item.Low);
        CloseList.Insert(0, item.Close);
        VolumeList.Insert(0, item.Volume);
        TransactionList.Insert(0, item.Transactions);
    }
    /// <summary>
    /// Sorts ranges in descending order by CandleTS and inserts the range into index 0
    /// </summary>
    /// <param name="items"></param>
    public new void AddRange(IEnumerable<PairCandle> items)
    {
        var newItems = items.ExceptBy(this.Select(PairCandleSelector), PairCandleSelector);
        if (newItems.Count() == 0) {  return; }
        var descList = newItems.ToList().OrderByDescending(PairCandleSelector);
        base.InsertRange(0, descList);
        OpenList.InsertRange(0, descList.GetCandlesByOHLCType(CandleOHLC.Open));
        HighList.InsertRange(0, descList.GetCandlesByOHLCType(CandleOHLC.High));
        LowList.InsertRange(0, descList.GetCandlesByOHLCType(CandleOHLC.Low));
        CloseList.InsertRange(0, descList.GetCandlesByOHLCType(CandleOHLC.Close));
        VolumeList.InsertRange(0, descList.GetCandlesByOHLCType(CandleOHLC.Volume));
        TransactionList.InsertRange(0, descList.GetCandlesByOHLCType());

    }
    public new void Remove(PairCandle item)
    {
        base.Remove(item);
        OpenList.Remove(item.Open);
        HighList.Remove(item.High);
        LowList.Remove(item.Low);
        CloseList.Remove(item.Close);
        VolumeList.Remove(item.Volume);
        TransactionList.Remove(item.Transactions);
    }
    public new void RemoveAll(Predicate<PairCandle> match)
    {
        throw new NotImplementedException();
    }
    public new void RemoveRange(int index, int count)
    {
        throw new NotImplementedException();
    }
    public new void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }
    public new void Insert(int index, PairCandle item)
    {
        throw new NotImplementedException();
    }
    public new void InsertRange(int index, IEnumerable<PairCandle> collection)
    {
        throw new NotImplementedException();
    }
}
