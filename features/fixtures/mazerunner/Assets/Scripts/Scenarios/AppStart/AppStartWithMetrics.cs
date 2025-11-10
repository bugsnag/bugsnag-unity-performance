using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class AppStartWithMetrics : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(4);
        Configuration.EnabledMetrics = new EnabledMetrics
        {
            CPU = true,
            Memory = true,
            Rendering = true
        };
        Configuration.AutoInstrumentAppStart = AutoInstrumentAppStartSetting.START_ONLY;
    }

    public override void Run()
    {
        Invoke("ReportAppStarted", 5.0f);
    }

    private void ReportAppStarted()
    {
        BugsnagPerformance.ReportAppStarted();
    }
}
