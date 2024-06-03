using System.Collections;
using System.Collections.Generic;
using BugsnagNetworking;
using UnityEngine;

public class NetworkGetFail : Scenario
{

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        StartCoroutine(DoRun());
    }

    private IEnumerator DoRun()
    {
        yield return BugsnagUnityWebRequest.Get(FAIL_URL).SendWebRequest();
        yield return new WaitForSeconds(1);
        yield return BugsnagUnityWebRequest.Get(Main.MazeHost).SendWebRequest();
    }

}