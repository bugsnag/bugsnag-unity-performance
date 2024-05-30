using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxBatchAge : Scenario
{

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxPersistedBatchAgeSeconds(1);
    }

}
