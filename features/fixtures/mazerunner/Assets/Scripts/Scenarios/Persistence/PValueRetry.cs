
public class PValueRetry : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetPValueCheckIntervalSeconds(3);
    }
}
