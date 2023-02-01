using System.Collections.Generic;
using UnityEngine;

using System;
using BugsnagUnityPerformance;

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

}
