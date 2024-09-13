using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class ConfiguredSamplingRate1 : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(4);
    }

    public override void Run()
    {
        base.Run();
        StartCoroutine(DoSpans());
    }

    private IEnumerator DoSpans()
    {
        yield return new WaitForSeconds(4);
        BugsnagPerformance.StartSpan("ManualSpan1").End();
        BugsnagPerformance.StartSpan("ManualSpan2").End();
        BugsnagPerformance.StartSpan("ManualSpan3").End();
        BugsnagPerformance.StartSpan("ManualSpan4").End();
    }
}
