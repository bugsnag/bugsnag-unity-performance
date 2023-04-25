using System.Collections;
using System.Collections.Generic;
using BugsnagNetworking;
using UnityEngine;

public class NetworkGetSuccess : Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        BugsnagUnityWebRequest.Get("https://www.bugsnag.com/").SendWebRequest();
    }

}
