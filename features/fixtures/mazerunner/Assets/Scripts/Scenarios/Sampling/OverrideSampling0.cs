using BugsnagUnityPerformance;

public class OverrideSampling0 : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        Configuration.SamplingProbability = 0;
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        base.Run();
        BugsnagPerformance.StartSpan("ManualSpan1").End();
        BugsnagPerformance.StartSpan("ManualSpan2").End();
        BugsnagPerformance.StartSpan("ManualSpan3").End();
        BugsnagPerformance.StartSpan("ManualSpan4").End();
    }
}

