using BugsnagUnityPerformance;

public class NullSpanName : Scenario
{

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(2);
    }

    public override void Run()
    {
        base.Run();
        BugsnagPerformance.StartSpan(null).End();
        BugsnagPerformance.StartSpan("control").End();
    }

}
