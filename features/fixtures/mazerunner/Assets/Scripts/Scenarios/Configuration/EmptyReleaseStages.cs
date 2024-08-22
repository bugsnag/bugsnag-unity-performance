using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnityPerformance;

public class EmptyReleaseStages : Scenario
{

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        Configuration.EnabledReleaseStages = new string[] {};
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        base.Run();
        BugsnagPerformance.StartSpan("EmptyReleaseStages").End();
    }

}