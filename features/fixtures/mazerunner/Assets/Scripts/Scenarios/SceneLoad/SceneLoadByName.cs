using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class SceneLoadByName : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        BugsnagSceneManager.LoadScene("Scene1");
    }
}
