using System.Collections;
using System.Collections.Generic;
using BugsnagNetworking;
using BugsnagUnityPerformance;
using UnityEngine;

public class TraceParentHeaderSmokeTest : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        BugsnagUnityWebRequest.Get(Main.MazeHost + "/reflect").SendWebRequest();
    }

}
