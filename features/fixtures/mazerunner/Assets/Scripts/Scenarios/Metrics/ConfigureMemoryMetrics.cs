using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class ConfigureMemoryMetrics : Scenario
{

    Span _beforeStartSpan;

    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
        Configuration.EnabledMetrics.Memory = false;
    }

    public override void StartBugsnag()
    {
        _beforeStartSpan = BugsnagPerformance.StartSpan("BeforeStart");
    }

    public override void Run()
    {
        base.Run();
        StartCoroutine(DoTest());
    }

    private IEnumerator DoTest()
    {
        yield return new WaitForSeconds(2.5f);
        _beforeStartSpan.End();
        yield return new WaitForSeconds(2.5f);
        base.StartBugsnag();
    }


}


