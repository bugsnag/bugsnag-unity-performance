using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using System.Threading;
using UnityEngine;
using UnityEngine.Scripting;
public class NewThreadNewContext : Scenario
{
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(3);
    }
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    public override void Run()
    {
        StartCoroutine(DoRun());
    }

    // Unity 2021 IL2CPP keeps messing up the threads for some reason, so we stick them in a coroutine because it leaves them alone
    private IEnumerator DoRun()
    {
        var span1 = BugsnagPerformance.StartSpan("span1");
        new Thread(() =>
        {
            var span2 = BugsnagPerformance.StartSpan("span2");
            var span3 = BugsnagPerformance.StartSpan("span3");
            span3.End();
            span2.End();
        }).Start();
        yield return new WaitForSeconds(1);
        span1.End();
    }
}
