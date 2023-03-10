using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class MaxBatchSize : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(3);
    }

    public override void Run()
    {
        for (int i = 0; i < 3; i++)
        {
            StartCoroutine(DoSpan(i));
        }
    }

    private IEnumerator DoSpan(int index)
    {
        var span = BugsnagPerformance.StartSpan("Span " + index);
        yield return new WaitForSeconds(0.25f);
        span.End();
    }
}
