using System.Collections;
using System.Collections.Generic;
using BugsnagNetworking;
using UnityEngine;

public class NetworkPostFail : Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        StartCoroutine(DoRun());
    }

    private IEnumerator DoRun()
    {
        yield return BugsnagUnityWebRequest.Post(FAIL_URL, "1234567890").SendWebRequest();
        yield return new WaitForSeconds(1);
        BugsnagUnityWebRequest.Post(Main.MazeHost, "1234567890").SendWebRequest();
    }

}