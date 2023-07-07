
public class PValueRetry : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetPValueCheckIntervalSeconds(3);
    }
}
