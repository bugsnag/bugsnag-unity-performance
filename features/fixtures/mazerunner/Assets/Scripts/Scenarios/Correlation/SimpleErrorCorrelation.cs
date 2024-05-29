using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using BugsnagUnityPerformance;
using UnityEngine;

public class SimpleErrorCorrelation : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
        ShouldStartNotifier = true;
    }

    public override void Run()
    {
         var span = BugsnagPerformance.StartSpan("Span 1");
        Bugsnag.Notify(new System.Exception("Simple Error Correlation"));
        span.End();
    }

}
