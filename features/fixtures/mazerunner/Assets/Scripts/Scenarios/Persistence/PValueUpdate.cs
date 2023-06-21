using System.Collections;
using System.Collections.Generic;
using System.Threading;
using BugsnagUnityPerformance;
using UnityEngine;

public class PValueUpdate : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
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
