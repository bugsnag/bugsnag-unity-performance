using System.Collections;
using System.Collections.Generic;
using BugsnagNetworking;
using UnityEngine;
using System.Text.RegularExpressions;

public class TraceParentConfig : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        Configuration.TracePropagationUrls = new Regex[]
        {
            new Regex(".*dosend.*")
        };
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
