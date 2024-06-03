using System;
using System.Collections;
using System.Collections.Generic;
using BugsnagNetworking;
using BugsnagUnityPerformance;
using UnityEngine;

public class NetworkCallbackUrlEdit : Scenario
{

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        Configuration.NetworkRequestCallback = NetworkCallback;
        SetMaxBatchSize(1);
    }

    private BugsnagNetworkRequestInfo NetworkCallback(BugsnagNetworkRequestInfo info)
    {
        info.Url = "https://www.callback.com/";
        return info;
    }

    public override void Run()
    {
        BugsnagUnityWebRequest.Get(Main.MazeHost).SendWebRequest();
    }

}
