using System.Collections;
using System.Collections.Generic;
using BugsnagNetworking;
using UnityEngine;

public class NetworkPostSuccess : Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        BugsnagUnityWebRequest.Post("https://httpbin.org/post", "1234567890").SendWebRequest();
    }

}