using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class VersionCode : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(1);
        Configuration.VersionCode = 123;
    }

    public override void Run()
    {
        var span = BugsnagPerformance.StartSpan("VersionCode");
        span.End();
    }

}
