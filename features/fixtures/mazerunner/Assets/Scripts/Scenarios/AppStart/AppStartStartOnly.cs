using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class AppStartStartOnly : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchAgeSeconds(12);
        Configuration.AutoInstrumentAppStart = AutoInstrumentAppStartSetting.START_ONLY;
    }

    public override void Run()
    {
        Invoke("ReportAppStart",8);
    }

    private void ReportAppStart()
    {
        BugsnagPerformance.ReportAppStarted();
    }
}
