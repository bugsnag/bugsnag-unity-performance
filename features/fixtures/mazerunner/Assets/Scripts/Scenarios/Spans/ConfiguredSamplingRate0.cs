using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class ConfiguredSamplingRate0 : Scenario
{

    private Span _span;

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetSamplingProbability(0);
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        base.Run();
        _span = BugsnagPerformance.StartSpan("ManualSpan");
        Invoke("EndSpan", 1);
    }

    private void EndSpan()
    {
        _span.End();
    }
}
