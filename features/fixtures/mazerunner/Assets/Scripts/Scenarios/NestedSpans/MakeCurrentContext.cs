using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using System.Threading;
using UnityEngine;

public class MakeCurrentContext : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(2);
    }

    public override void Run()
    {
        var span1 = BugsnagPerformance.StartSpan("span1", new SpanOptions { MakeCurrentContext = false });
        var span2 = BugsnagPerformance.StartSpan("span2");
        span2.End();
        span1.End();
    }
}
