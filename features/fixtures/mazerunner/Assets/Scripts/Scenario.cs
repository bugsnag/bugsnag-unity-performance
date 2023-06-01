using System.Collections.Generic;
using UnityEngine;

using System;
using BugsnagUnityPerformance;
using System.Reflection;
using static System.Net.WebRequestMethods;

public class Scenario : MonoBehaviour
{

    public PerformanceConfiguration Configuration;

    public virtual void PrepareConfig(string apiKey, string host)
    {
        Configuration = new PerformanceConfiguration(apiKey)
        {
            Endpoint = host + "/traces",
            AutoInstrumentAppStart = AutoInstrumentAppStartSetting.OFF
        };
    }

    public const string FAIL_URL = "https://localhost:994";

    public virtual void StartBugsnag()
    {
        BugsnagPerformance.Start(Configuration);
    }

    public virtual void Run()
    {

    }

    public void DoMultipleSpans(int num)
    {
        for (int i = 0; i < num; i++)
        {
            var span = BugsnagPerformance.StartSpan("Span: " + i + 1);
            span.End();
        }
    }

    public void SetMaxBatchSize(int size)
    {
        var fieldInfo = typeof(PerformanceConfiguration).GetField("MaxBatchSize", BindingFlags.Instance | BindingFlags.NonPublic);
        fieldInfo.SetValue(Configuration, size);
    }

    public void SetMaxPersistedBatchAgeSeconds(int seconds)
    {
        var fieldInfo = typeof(PerformanceConfiguration).GetField("MaxPersistedBatchAgeSeconds", BindingFlags.Instance | BindingFlags.NonPublic);
        fieldInfo.SetValue(Configuration, seconds);
    }

    public void SetMaxBatchAgeSeconds(float seconds)
    {
        var fieldInfo = typeof(PerformanceConfiguration).GetField("MaxBatchAgeSeconds", BindingFlags.Instance | BindingFlags.NonPublic);
        fieldInfo.SetValue(Configuration, seconds);
    }

}
