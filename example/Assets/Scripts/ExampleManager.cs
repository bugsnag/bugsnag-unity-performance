using System;
using System.Collections;
using System.Threading;
using BugsnagUnityPerformance;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExampleManager : MonoBehaviour
{

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
        yield return new WaitForSeconds(0.1f);
        span.End();
    }

    public void LoadOtherScene()
    {
        BugsnagSceneManager.LoadScene(1, LoadSceneMode.Additive);
    }


}
