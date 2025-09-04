using BugsnagUnityPerformance;

public class Dev : Scenario
{

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
    }

    public override void Run()
    {
        base.Run();
        DoSpan("Span-1", 1f);
        DoSpan("Span-2", 10f);
    }

}
