using UnityEngine;
namespace BugsnagUnityPerformance
{
    public class BugsnagPerformanceAppLifecycleListener : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                BugsnagPerformance.AppBackgrounded();
            }
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                BugsnagPerformance.AppBackgrounded();
            }
        }

    }
}