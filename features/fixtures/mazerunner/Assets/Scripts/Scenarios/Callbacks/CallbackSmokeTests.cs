using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnityPerformance;
using System;

public class CallbackSmokeTests : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
        Configuration.AddOnSpanEndCallback(MyConfigCallback);
    }

    private bool MyConfigCallback(Span span)
    {
        if(span.Name == "discard-me")
        {
            return false;
        }
        Debug.Log("In Callbacks");
        return true;
    }

    public override void Run()
    {
        base.Run();
        BugsnagPerformance.StartSpan("discard-me").End();
        BugsnagPerformance.StartSpan("report-me").End();
    }

}

