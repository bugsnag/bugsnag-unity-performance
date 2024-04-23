using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using System.Threading;
using UnityEngine;

public class IsFirstClass : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(3);
    }

    public override void Run()
    {
        BugsnagPerformance.StartSpan("FirstClass not set").End();
        BugsnagPerformance.StartSpan("FirstClass true", new SpanOptions { IsFirstClass = true }).End();
        BugsnagPerformance.StartSpan("FirstClass false", new SpanOptions { IsFirstClass = false }).End();
    }
}
