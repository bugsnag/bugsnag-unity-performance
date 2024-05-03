using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class CustomStartAndEndTime : Scenario
{

    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(1);
    }

    public override void Run()
    {
        base.Run();
        BugsnagPerformance.StartSpan("CustomStartAndEndTime",
            new SpanOptions {StartTime = new System.DateTimeOffset(1985,1,1,1,1,1,System.TimeSpan.Zero)})
            .End(new System.DateTimeOffset(1986, 1, 1, 1, 1, 1, System.TimeSpan.Zero));
    }

    
}
