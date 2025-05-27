using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class CpuMetrics : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        Configuration.EnabledMetrics = new EnabledMetrics
        {
            CPU = true
        };
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        DoSpan("CpuMetrics", 3f);
    }
}
