using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BugsnagUnityPerformance;
using System;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour
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

    public void DoWebRequest()
    {
        var request = BugsnagNetworking.BugsnagUnityWebRequest.Post("www.fff.com", "hello");
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

        SceneManager.sceneLoaded += SceneManager_sceneLoaded;

        BugsnagSceneManager.OnSeceneLoad.AddListener((sceneIdentifier) =>
        {
            if (sceneIdentifier is int)
            {
                Debug.Log("Scene Loaded by index: " + sceneIdentifier);
            }
            else
            {
                Debug.Log("Scene Loaded by name: " + sceneIdentifier);
            }
        });

        BugsnagSceneManager.LoadScene("OtherScene");

    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Debug.Log("Scene load finished with index: " + arg0.buildIndex + " and name: " + arg0.name);
    }
}