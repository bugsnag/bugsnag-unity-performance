using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class VersionCode : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
        Configuration.VersionCode = 123;
    }

    public override void Run()
    {
        var span = BugsnagPerformance.StartSpan("VersionCode");
        span.End();
    }

}
