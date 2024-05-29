using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class AppVersion : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
        Configuration.AppVersion = "1.2.3_AppVersion";
    }

    public override void Run()
    {
        var span = BugsnagPerformance.StartSpan("AppVersion");
        span.End();
    }

}
