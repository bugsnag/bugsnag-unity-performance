using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppStartFull : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchAgeSeconds(10);
    }
}
