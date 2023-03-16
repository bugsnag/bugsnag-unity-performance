using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxBatchAge : Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxPersistedBatchAgeSeconds(1);
    }

}
