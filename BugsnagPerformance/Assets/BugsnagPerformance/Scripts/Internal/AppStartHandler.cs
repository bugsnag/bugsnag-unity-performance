using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    public class AppStartHandler
    {

        private static Span _rootSpan;
        private static Span _loadAssembliesSpan;
        private static Span _splashScreenSpan;
        private static Span _firstSceneSpan;

        private static Span CreateAppStartSpan(string name, string category)
        {
            return BugsnagPerformance.CreateAutoAppStartSpan(name, category);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void SubsystemRegistration()
        {
            Debug.Log("SubsystemRegistration");
            _rootSpan = CreateAppStartSpan("[AppStart/UnityRuntime]", "app_start");

            _loadAssembliesSpan = CreateAppStartSpan("[AppStartPhase/LoadAssemblies]", "app_start_phase");
            _loadAssembliesSpan.SetAttribute("bugsnag.phase", "LoadAssemblies");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void AfterAssembliesLoaded()
        {
            Debug.Log("AfterAssembliesLoaded");
            EndSpan(_loadAssembliesSpan);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void BeforeSplashScreen()
        {
            Debug.Log("BeforeSplashScreen");
            _splashScreenSpan = CreateAppStartSpan("[AppStartPhase/SplashScreen]", "app_start_phase");
            _splashScreenSpan.SetAttribute("bugsnag.phase", "SplashScreen");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void BeforeSceneLoad()
        {
            Debug.Log("BeforeSceneLoad");
            _firstSceneSpan = CreateAppStartSpan("[AppStartPhase/LoadFirstScene]", "app_start_phase");
            _firstSceneSpan.SetAttribute("bugsnag.phase", "LoadFirstScene");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AfterSceneLoad()
        {
            Debug.Log("AfterSceneLoad");
            EndSpan(_splashScreenSpan);
            EndSpan(_firstSceneSpan);
            EndSpan(_rootSpan);
        }

        private static void EndSpan(Span span)
        {
            if (span != null)
            {
                span.End();
            }
        }
    }
}