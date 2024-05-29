using System.Collections;
using System.Collections.Generic;
using BugsnagNetworking;
using BugsnagUnityPerformance;
using UnityEngine;

public class NetworkCallbackReturnNull : Scenario
{
    int _numChecked;

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        Configuration.NetworkRequestCallback = NetworkCallback;
        SetMaxBatchSize(1);
    }

    private BugsnagNetworkRequestInfo NetworkCallback(BugsnagNetworkRequestInfo info)
    {
        if (_numChecked == 1)
        {
            info.Url = "https://www.callback.com/";
        }
        else
        {
            info.Url = null;
        }
        _numChecked++;
        return info;
    }

    public override void Run()
    {
        BugsnagUnityWebRequest.Get(Main.MazeHost).SendWebRequest();
        BugsnagUnityWebRequest.Get(Main.MazeHost).SendWebRequest();
    }

}
