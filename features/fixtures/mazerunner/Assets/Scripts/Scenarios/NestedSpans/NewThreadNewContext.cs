using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using System.Threading;
using UnityEngine;

public class NewThreadNewContext : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(3);
    }

    public override void Run()
    {
        var span1 = BugsnagPerformance.StartSpan("span1");
        var finished = false;
        new Thread(() => {
            var span2 = BugsnagPerformance.StartSpan("span2");
                var span3 = BugsnagPerformance.StartSpan("span3");
                span3.End();
            span2.End();
            finished = true;
        }).Start();
        while (!finished) { }
        span1.End();
    }
}
