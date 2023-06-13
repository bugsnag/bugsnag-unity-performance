using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using UnityEngine;
namespace BugsnagUnityPerformance
{
    public class BugsnagPerformanceAutoStart : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void OnBeforeSceneLoadRuntimeMethod()
        {
            var settings = Resources.Load<BugsnagPerformanceSettingsObject>("Bugsnag/BugsnagPerformanceSettingsObject");
            if (settings != null && settings.StartAutomaticallyAtLaunch)
            {
                if (string.IsNullOrEmpty(settings.ApiKey))
                {
                    Debug.LogError("Bugsnag not auto started as the API key is not set in the Bugsnag Performance Settings window.");
                    return;
                }
                var config = settings.GetConfig();
                BugsnagPerformance.Start(config);
            }
        }
    }
}