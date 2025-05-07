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
        // var config = BugsnagPerformanceSettingsObject.LoadConfiguration();
        // BugsnagPerformance.Start(config);
        Application.targetFrameRate = 30;
    }

    public void DoSpan()
    {
        StartCoroutine(SpanRoutine());
    }

    public void DoManySpans()
    {
        for (int i = 0; i < 100; i++)
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
        var span = BugsnagPerformance.StartSpan("UAT v3");
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 5.0f));
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

    private void Update()
    {
#if UNITY_ANDROID
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
#endif
    }

}