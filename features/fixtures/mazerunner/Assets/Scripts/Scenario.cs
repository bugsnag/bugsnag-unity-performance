using System.Collections.Generic;
using UnityEngine;

using System;
using BugsnagUnityPerformance;
using System.Reflection;
using BugsnagUnity;

public class Scenario : MonoBehaviour
{

    public PerformanceConfiguration Configuration;

    public Configuration NotifierConfiguration;

    public bool ShouldStartNotifier;

    public virtual void PreparePerformanceConfig(string apiKey, string host)
    {
        Configuration = new PerformanceConfiguration(apiKey)
        {
            Endpoint = host + "/traces",
            AutoInstrumentAppStart = AutoInstrumentAppStartSetting.OFF
        };
    }

    public virtual void PrepareNotifierConfig(string apiKey, string host)
    {
        NotifierConfiguration = new Configuration(apiKey)
        {
            Endpoints = new EndpointConfiguration(host + "/notify", host + "/sessions")
        };
    }

    public const string FAIL_URL = "https://localhost:994";

    public virtual void StartBugsnag()
    {
        if(ShouldStartNotifier){
            Bugsnag.Start(NotifierConfiguration);
        }
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

    public void SetPValueTimeoutSeconds(float seconds)
    {
        var fieldInfo = typeof(PerformanceConfiguration).GetField("PValueTimeoutSeconds", BindingFlags.Instance | BindingFlags.NonPublic);
        fieldInfo.SetValue(Configuration, seconds);
    }

    public void SetPValueCheckIntervalSeconds(float seconds)
    {
        var fieldInfo = typeof(PerformanceConfiguration).GetField("PValueCheckIntervalSeconds", BindingFlags.Instance | BindingFlags.NonPublic);
        fieldInfo.SetValue(Configuration, seconds);
    }

    public void SetSamplingProbability(double p)
    {
        var fieldInfo = typeof(PerformanceConfiguration).GetField("SamplingProbability", BindingFlags.Instance | BindingFlags.NonPublic);
        fieldInfo.SetValue(Configuration, p);
    }

}
