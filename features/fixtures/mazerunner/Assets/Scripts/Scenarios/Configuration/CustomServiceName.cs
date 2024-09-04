using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class CustomServiceName : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
        Configuration.ServiceName = "custom.service.name";
    }

    public override void Run()
    {
        var span = BugsnagPerformance.StartSpan("CustomServiceName");
        span.End();
    }

}
