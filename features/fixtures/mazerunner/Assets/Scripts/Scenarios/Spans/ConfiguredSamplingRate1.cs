using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class ConfiguredSamplingRate1 : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.SamplingProbability = 1;
        SetMaxBatchAgeSeconds(1);
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
