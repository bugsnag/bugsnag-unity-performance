using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class SceneLoadWithMetrics : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
        Configuration.EnabledMetrics = new EnabledMetrics
        {
            CPU = true,
            Memory = true,
            Rendering = true
        };
    }

    public override void Run()
    {
        Invoke("DoSceneLoad", 2.0f);
    }

    private void DoSceneLoad()
    {
        BugsnagSceneManager.LoadScene("Scene1");
    }
}
