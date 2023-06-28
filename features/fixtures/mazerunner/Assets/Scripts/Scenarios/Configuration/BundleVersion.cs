using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class BundleVersion : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(1);
        Configuration.BundleVersion = "1.2.3_BundleVersion";
    }

    public override void Run()
    {
        var span = BugsnagPerformance.StartSpan("BundleVersion");
        span.End();
    }

}
