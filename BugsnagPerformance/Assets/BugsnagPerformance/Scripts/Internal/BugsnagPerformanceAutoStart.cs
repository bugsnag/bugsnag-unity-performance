using System.Collections;
using System.Collections.Generic;
using BugsnagUnityPerformance;
using UnityEngine;

internal class BugsnagPerformanceAutoStart : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        var settings = Resources.Load<BugsnagPerformanceSettingsObject>("Bugsnag/BugsnagPerformanceSettingsObject");
        if (settings != null && settings.StartAutomaticallyAtLaunch)
        {
            var config = settings.GetConfig();
            if (string.IsNullOrEmpty(config.ApiKey))
            {
                Debug.LogError("Bugsnag Performance not auto started as the API key is not set.");
                return;
            }
            BugsnagPerformance.Start(config);
        }
    }
}
