using BugsnagUnityPerformance;

public class AppStartCustomisation : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(4);
        Configuration.AutoInstrumentAppStart = AutoInstrumentAppStartSetting.START_ONLY;
    }

    public override void Run()
    {
        BugsnagPerformance.GetSpanControl(SpanType.AppStart)?.SetType("ColdStart");
        BugsnagPerformance.ReportAppStarted();
    }
}
