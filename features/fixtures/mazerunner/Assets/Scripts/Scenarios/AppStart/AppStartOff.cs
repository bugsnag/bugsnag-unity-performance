using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class AppStartOff : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(1);
        Configuration.AutoInstrumentAppStart = AutoInstrumentAppStartSetting.OFF;
    }

    public override void Run()
    {
        Invoke("DoSingleSpan",5);
    }

    private void DoSingleSpan()
    {
        var span = BugsnagPerformance.StartSpan("AppStartOff");
        span.End();
    }

}
