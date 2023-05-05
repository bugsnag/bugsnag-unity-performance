using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class SceneLoadByIndex : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        BugsnagSceneManager.LoadScene(1);
    }
}
