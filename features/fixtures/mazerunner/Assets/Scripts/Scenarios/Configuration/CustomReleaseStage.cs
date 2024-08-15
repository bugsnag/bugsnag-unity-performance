using BugsnagUnityPerformance;

public class CustomReleaseStage : Scenario
{

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        Configuration.ReleaseStage = "CustomReleaseStage";
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        base.Run();
        BugsnagPerformance.StartSpan("CustomReleaseStage").End();
    }

}
