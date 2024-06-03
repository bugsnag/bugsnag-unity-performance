using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioRunner : MonoBehaviour
{

    public void RunScenario(string scenarioName, string apiKey, string host)
    {
        var scenario = GetScenario(scenarioName);
        scenario.PreparePerformanceConfig(apiKey, host);
        scenario.PrepareNotifierConfig(apiKey, host);
        scenario.StartBugsnag();
        scenario.Run();
    }

    private Scenario GetScenario(string scenarioName)
    {

        var scenarios = gameObject.GetComponentsInChildren<Scenario>();

        foreach (var scenario in scenarios)
        {
            if (scenario.GetType().Name.Equals(scenarioName))
            {
                return scenario;
            }
        }
        Main.Log("Scenario not found: " + scenarioName);
        throw new System.Exception("Scenario not found: " + scenarioName);
    }

}
