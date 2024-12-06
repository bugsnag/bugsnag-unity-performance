using System.Collections;
using System.Collections.Generic;
using BugsnagNetworking;
using UnityEngine;

public class NetworkPostFail : Scenario
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
#if UNITY_2022_2_OR_NEWER
        WWWForm form = new WWWForm();
        form.AddField("data", "1234567890");
        yield return BugsnagUnityWebRequest.Post(FAIL_URL, form).SendWebRequest();
#else
        yield return BugsnagUnityWebRequest.Post(FAIL_URL, "1234567890").SendWebRequest();
#endif
        yield return new WaitForSeconds(1);
#if UNITY_2022_2_OR_NEWER
        yield return BugsnagUnityWebRequest.Post(Main.MazeHost, form).SendWebRequest();
#else
        yield return BugsnagUnityWebRequest.Post(Main.MazeHost, "1234567890").SendWebRequest();
#endif
    }

}