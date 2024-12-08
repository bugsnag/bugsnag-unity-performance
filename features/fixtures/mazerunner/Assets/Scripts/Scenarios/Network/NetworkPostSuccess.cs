using System.Collections;
using System.Collections.Generic;
using BugsnagNetworking;
using UnityEngine;

public class NetworkPostSuccess : Scenario
{

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
    #if UNITY_2022_2_OR_NEWER
        WWWForm form = new WWWForm();
        form.AddField("data", "1234567890");
        BugsnagUnityWebRequest.Post(Main.MazeHost, form).SendWebRequest();
    #else
        BugsnagUnityWebRequest.Post(Main.MazeHost, "1234567890").SendWebRequest();
    #endif
    }

}