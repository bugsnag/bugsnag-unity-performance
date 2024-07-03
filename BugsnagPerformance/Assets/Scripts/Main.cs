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
    public string ApiKey;
    public string Endpoint;
    public bool StartPerf;
    void Start()
    {
        var config = new PerformanceConfiguration(ApiKey);

        config.Endpoint = Endpoint;

        config.NetworkRequestCallback = NetworkCallback;

        if (StartPerf)
        {
            BugsnagPerformance.Start(config);
        }
    }

    private BugsnagNetworkRequestInfo NetworkCallback(BugsnagNetworkRequestInfo info)
    {
        info.Url = "SANITISED";
        return info;
    }

    private void ReportAppStart()
    {
        BugsnagPerformance.ReportAppStarted();
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
        span.AddAttribute("my string attribute", "some value");
        span.AddAttribute("my string[] attribute", new string[]{"a","b","c"});
        span.AddAttribute("my int attribute", 42);
        span.AddAttribute("my int[] attribute", new int[]{1, 2, 3});
        span.AddAttribute("my bool attribute", true);
        span.AddAttribute("my bool[] attribute", new bool[]{true, false, true});
        span.AddAttribute("my double attribute", 3.14);
        span.AddAttribute("my double[] attribute", new double[]{1.1, 2.2, 3.3});
        yield return new WaitForSeconds(0.1f);
        span.End();
    }

    public void LoadOtherScene()
    {
        BugsnagSceneManager.LoadScene(1,LoadSceneMode.Additive);
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
        new Thread(() => {
            var span2 = BugsnagPerformance.StartSpan("Span2");
            span2.End();
        }).Start();
        
        span1.End();
    }

}