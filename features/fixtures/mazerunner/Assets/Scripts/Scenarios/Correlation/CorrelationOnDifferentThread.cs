using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnity;
using BugsnagUnityPerformance;
using System.Threading;
public class CorrelationOnDifferentThread : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(2);
        ShouldStartNotifier = true;
    }

    public override void Run()
    {
        var span = BugsnagPerformance.StartSpan("Span From Main Thread");

        Thread newThread = new Thread(new ThreadStart(()=>
        {
            var span = BugsnagPerformance.StartSpan("Span From Background Thread");
            Bugsnag.Notify(new System.Exception("Event From Background Thread"));
            span.End();
        }));
        
        newThread.Start();
        Bugsnag.Notify(new System.Exception("Event From Main Thread"));
        span.End();
    }

}
