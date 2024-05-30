using System.Collections;
using System.Collections.Generic;
using BugsnagNetworking;
using UnityEngine;

public class NetworkGetSuccess : Scenario
{

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        BugsnagUnityWebRequest.Get(Main.MazeHost).SendWebRequest();
    }

}
