using System.Collections;
using System.Collections.Generic;
using BugsnagNetworking;
using BugsnagUnityPerformance;
using UnityEngine;

public class TraceParentHeaderSmokeTest : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        BugsnagUnityWebRequest.Get(Main.MazeHost + "/reflect").SendWebRequest();
    }

}
