using BugsnagUnityPerformance;
using UnityEngine;

internal class BugsnagPerformanceAutoStart : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        var settings = Resources.Load<BugsnagPerformanceSettingsObject>("Bugsnag/BugsnagPerformanceSettingsObject");
        if (settings != null)
        {
            var config = settings.GetConfig();
            if (string.IsNullOrEmpty(config.ApiKey))
            {
                Debug.LogError("BugSnag Performance can't be automatically started as the API key is not set.");
                return;
            }
            if (settings.StartAutomaticallyAtLaunch)
            {
                BugsnagPerformance.Start(config);
            }
        }
    }
}
