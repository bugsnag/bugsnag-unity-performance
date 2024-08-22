using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnity;
using BugsnagUnityPerformance;
using System.Threading;
public class CorrelationOnDifferentThread : Scenario
{

    private Span _mainThreadSpan;
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
        ShouldStartNotifier = true;
    }

    public override void Run()
    {
        _mainThreadSpan = BugsnagPerformance.StartSpan("Span From Main Thread");
        Thread newThread = new Thread(new ThreadStart(()=>
        {
            var newThreadSpan = BugsnagPerformance.StartSpan("Span From Background Thread");
            Bugsnag.Notify(new System.Exception("Event From Background Thread"));
            newThreadSpan.End();
        }));
        
        newThread.Start();
        Bugsnag.Notify(new System.Exception("Event From Main Thread"));
        StartCoroutine(EndMainThreadSpan());
    }

    private IEnumerator EndMainThreadSpan(){
        yield return new WaitForSeconds(2);
        _mainThreadSpan.End();
    }

}
