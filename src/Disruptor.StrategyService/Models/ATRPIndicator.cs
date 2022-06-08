namespace Disruptor.StrategyService.Models;

public class ATRPIndicator
{
    public ATRPIndicator()
    {
        Candles = new List<decimal>();
    }
    public decimal Max { get; set; }
    public decimal Min { get; set; }
    public decimal Avg { get; set; }
    public decimal UpCnt { get; set; }
    public decimal UpAvg { get; set; }
    public decimal DownCnt { get; set; }
    public decimal DownAvg { get; set; }
    public decimal PctUpDown { get; set; }
    public List<decimal> Candles { get; set; }
}
