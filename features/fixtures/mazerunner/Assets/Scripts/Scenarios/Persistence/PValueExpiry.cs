using System.Collections;
using System.Collections.Generic;
using System.Threading;
using BugsnagUnityPerformance;
using UnityEngine;

public class PValueExpiry : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetPValueTimeoutSeconds(1);
    }
}
