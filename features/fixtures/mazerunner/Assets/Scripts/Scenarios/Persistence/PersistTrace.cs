using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistTrace : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(3);
    }

    public override void Run()
    {
        DoMultipleSpans(3);
    }
}
