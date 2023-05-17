using System.Collections;
using System.Collections.Generic;
using System.Threading;
using BugsnagUnityPerformance;
using UnityEngine;

public class PassSpanContext : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(2);
    }

    public override void Run()
    {
        var span1 = BugsnagPerformance.StartSpan("span1");
        new Thread(() => {
            var span2 = BugsnagPerformance.StartSpan("span2",new SpanOptions { ParentContext = span1 });
            span2.End();
        }).Start();

        span1.End();
    }
}
