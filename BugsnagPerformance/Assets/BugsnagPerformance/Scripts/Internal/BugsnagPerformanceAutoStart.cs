using BugsnagUnityPerformance;
using UnityEngine;

internal class BugsnagPerformanceAutoStart : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        var settings = Resources.Load<BugsnagPerformanceSettingsObject>("Bugsnag/BugsnagPerformanceSettingsObject");
        if (settings != null)
        {
            var config = settings.GetConfig();
            if (settings.StartAutomaticallyAtLaunch)
            {
                if (string.IsNullOrEmpty(config.ApiKey))
                {
                    Debug.LogError("BugSnag Performance can't be automatically started as the API key is not set.");
                    return;
                }
                BugsnagPerformance.Start(config);
            }
        }
    }
}
