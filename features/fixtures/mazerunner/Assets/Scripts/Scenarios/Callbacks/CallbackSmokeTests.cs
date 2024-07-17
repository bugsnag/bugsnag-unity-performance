using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnityPerformance;
using System;

public class CallbackSmokeTests : Scenario
{
    public override void PreparePerformanceConfig(string apiKey, string host)
    {
        base.PreparePerformanceConfig(apiKey, host);
        SetMaxBatchSize(1);
        Configuration.AddOnSpanEndCallback(MyConfigCallback);
    }

    private bool MyConfigCallback(Span span)
    {
        double result = 0;
        for (int i = 0; i < 1000000; i++)
        {
            result += Math.Sqrt(i);
        }
        for (int i = 0; i < 1000000; i++)
        {
            result += Math.Sin(i) * Math.Cos(i);
        }
        for (int i = 0; i < 1000000; i++)
        {
            result += Math.Log(i + 1) * Math.Exp(i % 1000);
        }
        for (int i = 0; i < 1000000; i++)
        {
            result += Math.Pow(i, 3) - Math.Sqrt(i);
        }

        if(span.Name == "discard-me")
        {
            return false;
        }
        return true;
    }

    public override void Run()
    {
        base.Run();
        BugsnagPerformance.StartSpan("discard-me").End();
        BugsnagPerformance.StartSpan("report-me").End();
    }

}

