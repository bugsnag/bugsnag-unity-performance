using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnityPerformance;
using System;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Threading;

public class Main : MonoBehaviour
{

    public void Start()
    {
        var config = BugsnagPerformanceSettingsObject.LoadConfiguration();
        BugsnagPerformance.Start(config);
    }

    public void DoSpan()
    {
        StartCoroutine(SpanRoutine());
    }

    public void DoManySpans()
    {
        for (int i = 0; i < 110; i++)
        {
            StartCoroutine(SpanRoutine());
        }
    }

    public void DoWebRequest()
    {
        var request = BugsnagNetworking.BugsnagUnityWebRequest.Get("www.google.com");
        request.SendWebRequest();
    }

    private IEnumerator SpanRoutine()
    {
        var span = BugsnagPerformance.StartSpan("span " + Guid.NewGuid());
        span.SetAttribute("my string attribute", "some value");
        span.SetAttribute("my string[] attribute", new string[] { "a", "b", "c" });
        span.SetAttribute("my empty string[] attribute", new string[] { });
        span.SetAttribute("my int attribute", 42);
        span.SetAttribute("my int[] attribute", new long[] { 1, 2, 3 });
        span.SetAttribute("my bool attribute", true);
        span.SetAttribute("my bool[] attribute", new bool[] { true, false, true });
        span.SetAttribute("my double attribute", 3.14);
        span.SetAttribute("my double[] attribute", new double[] { 1.1, 2.2, 3.3 });

        yield return new WaitForSeconds(1.0f);
        span.End();
    }

    public void LoadOtherScene()
    {
        BugsnagSceneManager.LoadScene(1, LoadSceneMode.Additive);
    }

    public void DoNestedSpanMainThread()
    {
        var span1 = BugsnagPerformance.StartSpan("Span1");
        var span2 = BugsnagPerformance.StartSpan("Span2");
        span2.End();
        span1.End();
    }

    public void DoNestedSpanThreaded()
    {
        var span1 = BugsnagPerformance.StartSpan("Span1");
        new Thread(() =>
        {
            var span2 = BugsnagPerformance.StartSpan("Span2");
            span2.End();
        }).Start();

        span1.End();
    }

}