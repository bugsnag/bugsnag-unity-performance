using BugsnagUnityPerformance;

public class CustomReleaseStage : Scenario
{

    private Span _span;

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.ReleaseStage = "CustomReleaseStage";
        SetMaxBatchAgeSeconds(1);
    }

    public override void Run()
    {
        base.Run();
        _span = BugsnagPerformance.StartSpan("CustomReleaseStage");
        Invoke("EndSpan", 1);
    }

    private void EndSpan()
    {
        _span.End();
    }
}
