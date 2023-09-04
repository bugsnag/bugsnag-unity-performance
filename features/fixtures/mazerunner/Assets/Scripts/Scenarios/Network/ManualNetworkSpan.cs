using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class ManualNetworkSpan : Scenario
{

    private Span _span;

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchAgeSeconds(1);
    }

    public override void Run()
    {
        base.Run();
        _span = BugsnagPerformance.StartNetworkSpan(Main.MazeHost, HttpVerb.PATCH);
        Invoke("EndSpan", 1);
    }

    private void EndSpan()
    {
        _span.EndNetworkSpan(202,123,321);
    }
}
