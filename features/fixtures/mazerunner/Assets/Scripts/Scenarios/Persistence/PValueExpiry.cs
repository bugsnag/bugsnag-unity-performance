using System.Collections;
using System.Collections.Generic;
using System.Threading;
using BugsnagUnityPerformance;
using UnityEngine;

public class PValueExpiry : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetPValueTimeoutSeconds(1);
    }
}
