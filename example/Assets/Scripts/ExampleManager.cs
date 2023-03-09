using System;
using System.Collections;
using BugsnagUnityPerformance;
using UnityEngine;

public class ExampleManager : MonoBehaviour
{

    public string ApiKey;
    public string Endpoint;
    public string ReleaseStage;

    void Start()
    {
        var config = new PerformanceConfiguration(ApiKey);
        if (!string.IsNullOrEmpty(Endpoint))
        {
            config.Endpoint = Endpoint;
        }
        if (!string.IsNullOrEmpty(ReleaseStage))
        {
            config.ReleaseStage = ReleaseStage;
        }
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

    private IEnumerator SpanRoutine()
    {
        var span = BugsnagPerformance.StartSpan("span " + Guid.NewGuid());
        yield return new WaitForSeconds(0.1f);
        span.End();
    }

}
