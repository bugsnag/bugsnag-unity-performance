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
        Configuration.SamplingProbability = 1;
        SetMaxBatchAgeSeconds(1);
    }

    public override void Run()
    {
        Debug.Log("### PValueUpdate.Run");
        StartCoroutine(doStuff());
    }

    private IEnumerator doStuff()
    {
        yield return new WaitForSeconds(2);
        BugsnagPerformance.StartSpan("ManualSpan").End();
    }
}
