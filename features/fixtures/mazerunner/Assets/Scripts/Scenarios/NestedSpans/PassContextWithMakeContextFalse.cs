using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class PassContextWithMakeContextFalse : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(3);
    }

    public override void Run()
    {
        var span1 = BugsnagPerformance.StartSpan("span1", new SpanOptions { MakeCurrentContext = false });
        var span2 = BugsnagPerformance.StartSpan("span2", new SpanOptions { ParentContext = span1 });
        span2.End();
        var span3 = BugsnagPerformance.StartSpan("span3");
        span3.End();
        span1.End();
    }
}
