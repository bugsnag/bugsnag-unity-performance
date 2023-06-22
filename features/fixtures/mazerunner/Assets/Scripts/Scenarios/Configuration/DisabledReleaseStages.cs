using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class DisabledReleaseStages : Scenario
{

    private Span _span;

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        Configuration.ReleaseStage = "EnabledReleaseStages";
        Configuration.EnabledReleaseStages = new[] { "DisabledReleaseStages" };
        SetMaxBatchAgeSeconds(1);
    }

    public override void Run()
    {
        base.Run();
        _span = BugsnagPerformance.StartSpan("DisabledReleaseStages");
        Invoke("EndSpan", 1);
    }

    private void EndSpan()
    {
        _span.End();
    }
}
