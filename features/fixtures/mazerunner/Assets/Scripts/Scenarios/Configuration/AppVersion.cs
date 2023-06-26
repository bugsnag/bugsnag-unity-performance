using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class AppVersion : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(1);
        Configuration.AppVersion = "1.2.3_AppVersion";
    }

    public override void Run()
    {
        var span = BugsnagPerformance.StartSpan("AppVersion");
        span.End();
    }

}
