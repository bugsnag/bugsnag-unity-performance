using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class OverrideSampling1 : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        Configuration.SamplingProbability = 1;
        SetMaxBatchSize(4);
    }

    public override void Run()
    {
        base.Run();
        BugsnagPerformance.StartSpan("ManualSpan1").End();
        BugsnagPerformance.StartSpan("ManualSpan2").End();
        BugsnagPerformance.StartSpan("ManualSpan3").End();
        BugsnagPerformance.StartSpan("ManualSpan4").End();
    }
}

