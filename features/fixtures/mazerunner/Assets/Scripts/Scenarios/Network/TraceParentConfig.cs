using System.Collections;
using System.Collections.Generic;
using BugsnagNetworking;
using UnityEngine;

public class TraceParentConfig : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(1);
        Configuration.TracePropagationUrls = new string[]{ "dosend" };
    }

    public override void Run()
    {
        StartCoroutine(DoRun());
    }

    private IEnumerator DoRun()
    {
        yield return BugsnagUnityWebRequest.Get(Main.MazeHost + "/reflect?dontSend").SendWebRequest();
        yield return new WaitForSeconds(1);
        yield return BugsnagUnityWebRequest.Get(Main.MazeHost + "/reflect?dosend").SendWebRequest();
    }
}
