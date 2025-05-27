using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class MemoryMetrics : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        Configuration.EnabledMetrics = new EnabledMetrics
        {
            Memory = true
        };
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        DoSpan("MemoryMetrics", 3f);
    }
}
