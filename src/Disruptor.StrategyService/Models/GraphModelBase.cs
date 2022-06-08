using Disruptor.StrategyService.Helpers;
namespace Disruptor.StrategyService.Models;
/// <summary>
/// Use this for all models used in the graph to assist with time management and faster querying
/// </summary>
public abstract class GraphModelBase
{
    public GraphModelBase() { }
    public GraphModelBase(long unixTimeStampUTC)
    {
        CreatedTS = unixTimeStampUTC;
        CreatedLocal = new DateTime(unixTimeStampUTC, DateTimeKind.Utc).ToLocalTime();
        CreatedUTC = new DateTime(unixTimeStampUTC, DateTimeKind.Utc);
    }
    public long CreatedTS { get; set; } = default!;
    public DateTime CreatedLocal { get; set; } = default!;
    public DateTime CreatedUTC { get; set; } = default!;
    public Guid TimeUUID { get; set; } = default!;
    public DateTime LastUpdatedUTC { get; set; } = default!;

}

public abstract record class GraphModelBaseRecord
{
    public GraphModelBaseRecord() { }
    public GraphModelBaseRecord(long unixTimeStampUTC)
    {
        this.Init(unixTimeStampUTC);
    }
    public long CreatedTS { get; set; } = default!;
    public DateTime CreatedLocal { get; set; } = default!;
    public DateTime CreatedUTC { get; set; } = default!;
    public Guid TimeUUID { get; set; } = default!;
    public DateTime LastUpdatedUTC { get; set; } = default!;
    //public long CreatedTS { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    //public DateTime CreatedLocal { get; init; } = DateTime.Now;
    //public DateTime CreatedUTC { get; init; } = DateTime.UtcNow;
    //public Guid TimeUUID { get; init; } = GuidGenerator.GenerateTimeBasedGuid();
}
public static class GraphModelBaseExtensions
{
    /// <summary>
    /// Initializes the values if they haven't been previously set
    /// </summary>
    /// <param name="graphRecord"></param>
    /// <param name="unixTimeStampUTC"></param>
    /// <returns></returns>
    public static GraphModelBase Init(this GraphModelBase graphRecord, long? unixTimeStampUTC = null)
    {
        long unixTimeStamp = unixTimeStampUTC.HasValue ? unixTimeStampUTC.Value : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        // If the timestamp is less than 13 then it is most likely seconds instead of milliseconds
        int tmpLen = unixTimeStamp.ToString().Length;
        int tmpNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString().Length;
        if (tmpLen < tmpNow)
        {
            int mult = (int)Math.Pow(10, (tmpNow - tmpLen));
            unixTimeStamp *= mult;
        }

        if (graphRecord.CreatedTS == default) graphRecord.CreatedTS = unixTimeStamp;
        if (graphRecord.CreatedLocal == default) graphRecord.CreatedLocal = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp).UtcDateTime.ToLocalTime();
        if (graphRecord.CreatedUTC == default) graphRecord.CreatedUTC = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp).UtcDateTime;
        if (graphRecord.TimeUUID == default) graphRecord.TimeUUID = GuidGenerator.GenerateTimeBasedGuid();
        graphRecord.LastUpdatedUTC = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp).UtcDateTime;

        return graphRecord;
    }
    /// <summary>
    /// Initializes the values if they haven't been previously set
    /// </summary>
    /// <param name="graphRecord"></param>
    /// <param name="unixTimeStampUTC"></param>
    /// <returns></returns>
    public static GraphModelBaseRecord Init(this GraphModelBaseRecord graphRecord, long? unixTimeStampUTC = null)
    {
        long unixTimeStamp = unixTimeStampUTC.HasValue ? unixTimeStampUTC.Value : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        // If the timestamp is less than 13 then it is most likely seconds instead of milliseconds
        unixTimeStamp = unixTimeStamp.ValidateUnixTimeStamp();

        if (graphRecord.CreatedTS == default) graphRecord.CreatedTS = unixTimeStamp;
        if (graphRecord.CreatedLocal == default) graphRecord.CreatedLocal = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp).UtcDateTime.ToLocalTime();
        if (graphRecord.CreatedUTC == default) graphRecord.CreatedUTC = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp).UtcDateTime;
        if (graphRecord.TimeUUID == default) graphRecord.TimeUUID = GuidGenerator.GenerateTimeBasedGuid();
        graphRecord.LastUpdatedUTC = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp).UtcDateTime;

        return graphRecord;
    }

    public static long ValidateUnixTimeStamp(this long unixTimeStamp)
    {
        int tmpLen = unixTimeStamp.ToString().Length;
        int tmpNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString().Length;
        if (tmpLen < tmpNow)
        {
            int mult = (int)Math.Pow(10, (tmpNow - tmpLen));
            unixTimeStamp *= mult;
        }
        return unixTimeStamp;
    }
}
