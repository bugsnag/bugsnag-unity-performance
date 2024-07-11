using System;
using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class AddAttributesInCallbacks : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
        Configuration.AddOnSpanEndCallback(MyConfigCallback);
    }

    private bool MyConfigCallback(Span span)
    {
        span.SetAttribute("config-callback", true);
        return true;
    }

    public override void Run()
    {
        base.Run();
        var span = BugsnagPerformance.StartSpan("AddAttributesInCallbacks");
        span.End();
    }

}

