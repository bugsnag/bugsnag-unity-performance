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
            var span = BugsnagPerformance.StartSpan(i.ToString());
            for (int x = 0; x < 1000; x++)
            {
                var f = Random.Range(0f, 1000f) / Random.Range(0f, 1000f);
            }
            span.End();
        }
    }
}
