using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

public class SceneLoadAsync : Scenario
{
    public override void PrepareConfig(string apiKey, string host)
    {
        base.PrepareConfig(apiKey, host);
        SetMaxBatchSize(3);
    }

    public override void Run()
    {
        BugsnagSceneManager.LoadSceneAsync("Scene1", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        BugsnagSceneManager.LoadSceneAsync("Scene2", UnityEngine.SceneManagement.LoadSceneMode.Additive);
        BugsnagSceneManager.LoadSceneAsync("Scene3", UnityEngine.SceneManagement.LoadSceneMode.Additive);
    }

}
