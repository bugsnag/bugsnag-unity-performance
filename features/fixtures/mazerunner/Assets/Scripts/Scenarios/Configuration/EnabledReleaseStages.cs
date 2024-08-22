using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class EnabledReleaseStages : Scenario
{

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        Configuration.ReleaseStage = "EnabledReleaseStages";
        Configuration.EnabledReleaseStages = new[] { "EnabledReleaseStages" };
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        base.Run();
        BugsnagPerformance.StartSpan("EnabledReleaseStages").End();
    }

}
