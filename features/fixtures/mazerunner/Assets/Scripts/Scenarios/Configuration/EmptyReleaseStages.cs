using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnityPerformance;

public class EmptyReleaseStages : Scenario
{

    private Span _span;

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        Configuration.EnabledReleaseStages = new string[] {};
        SetMaxBatchAgeSeconds(1);
    }

    public override void Run()
    {
        base.Run();
        _span = BugsnagPerformance.StartSpan("EmptyReleaseStages");
        Invoke("EndSpan", 1);
    }

    private void EndSpan()
    {
        _span.End();
    }
}