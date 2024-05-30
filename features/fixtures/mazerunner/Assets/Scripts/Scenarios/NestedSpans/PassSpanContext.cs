using System.Collections;
using System.Collections.Generic;
using System.Threading;
using BugsnagUnityPerformance;
using UnityEngine;

public class PassSpanContext : Scenario
{

    private Span _span1, _span2;

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(2);
    }

    public override void Run()
    {
        StartCoroutine(DoRun());
    }

    private IEnumerator DoRun()
    {
        _span1 = BugsnagPerformance.StartSpan("span1");
        new Thread(() => {
            _span2 = BugsnagPerformance.StartSpan("span2", new SpanOptions { ParentContext = _span1 });
        }).Start();
        yield return new WaitForSeconds(1);
        _span1.End();
        _span2.End();

    }
}
