using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class EnabledReleaseStages : Scenario
{

    private Span _span;

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        Configuration.ReleaseStage = "EnabledReleaseStages";
        Configuration.EnabledReleaseStages = new[] { "EnabledReleaseStages" };
        SetMaxBatchAgeSeconds(1);
    }

    public override void Run()
    {
        base.Run();
        _span = BugsnagPerformance.StartSpan("EnabledReleaseStages");
        Invoke("EndSpan", 1);
    }

    private void EndSpan()
    {
        _span.End();
    }
}
