using System.Collections.Generic;
using UnityEngine;

using System;
using BugsnagUnityPerformance;
using System.Reflection;

public class Scenario : MonoBehaviour
{

    public PerformanceConfiguration Configuration;

    public virtual void PrepareConfig(string apiKey, string host)
    {
        Configuration = new PerformanceConfiguration(apiKey)
        {
            Endpoint = host + "/traces"
        };
    }

    public virtual void StartBugsnag()
    {
        BugsnagPerformance.Start(Configuration);
    }

    public virtual void Run()
    {

    }

    public void SetMaxBatchSize(int size)
    {
        var fieldInfo = typeof(PerformanceConfiguration).GetField("MaxBatchSize", BindingFlags.Static | BindingFlags.NonPublic);
        fieldInfo.SetValue(Configuration, size);
    }

    public void SetMaxBatchAgeSeconds(float seconds)
    {
        var fieldInfo = typeof(PerformanceConfiguration).GetField("MaxBatchAgeSeconds", BindingFlags.Static | BindingFlags.NonPublic);
        fieldInfo.SetValue(Configuration, seconds);
    }

}
