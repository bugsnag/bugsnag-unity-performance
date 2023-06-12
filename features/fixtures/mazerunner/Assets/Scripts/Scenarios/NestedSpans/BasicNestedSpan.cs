using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class BasicNestedSpan : Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(2);
    }

    public override void Run()
    {
        var span1 = BugsnagPerformance.StartSpan("span1");
        var span2 = BugsnagPerformance.StartSpan("span2");
        span1.End();
        span2.End();
    }
}
