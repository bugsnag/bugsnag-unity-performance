using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class AppStartStartOnly : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(4);
        Configuration.AutoInstrumentAppStart = AutoInstrumentAppStartSetting.START_ONLY;
    }

    public override void Run()
    {
        Invoke("ReportAppStart",10);
    }

    private void ReportAppStart()
    {
        BugsnagPerformance.ReportAppStarted();
    }
}
