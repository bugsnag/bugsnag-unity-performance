using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class ManualSpan : Scenario
{

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        base.Run();
        BugsnagPerformance.StartSpan("ManualSpan").End();
    }

}
