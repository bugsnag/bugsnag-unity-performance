using System.Collections;
using System.Collections.Generic;
using BugsnagNetworking;
using BugsnagUnityPerformance;
using UnityEngine;

public class NetworkCallbackReturnNull : Scenario
{
    int _numChecked;

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.NetworkRequestCallback = NetworkCallback;
        SetMaxBatchSize(1);
    }

    private BugsnagNetworkRequestInfo NetworkCallback(BugsnagNetworkRequestInfo info)
    {
        if (_numChecked == 1)
        {
            info.Url = "EDITEDNULL";
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