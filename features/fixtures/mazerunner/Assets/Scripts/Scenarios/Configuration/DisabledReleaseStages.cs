using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class DisabledReleaseStages : Scenario
{

    private Span _span;

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        Configuration.ReleaseStage = "EnabledReleaseStages";
        Configuration.EnabledReleaseStages = new[] { "DisabledReleaseStages" };
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        base.Run();
        BugsnagPerformance.StartSpan("DisabledReleaseStages").End();
    }

}
