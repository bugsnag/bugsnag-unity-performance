using System.Collections;
using System.Collections.Generic;
using System.Threading;
using BugsnagUnityPerformance;
using UnityEngine;

public class PValueUpdate : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchAgeSeconds(1);
    }

    public override void Run()
    {
        StartCoroutine(doStuff());
    }

    private IEnumerator doStuff()
    {
        yield return new WaitForSeconds(2);
        BugsnagPerformance.StartSpan("ManualSpan").End();
    }
}
