using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    public class AppStartHandler : IPhasedStartup
    {

        private static Span _rootSpan;
        private static Span _loadAssembliesSpan;
        private static Span _splashScreenSpan;
        private static Span _firstSceneSpan;

        private static bool _complete;
        private static DateTimeOffset? _defaultAppStartEndTime = null; 

        private static List<Span> _appStartSpans = new List<Span>();

        private static AutoInstrumentAppStartSetting _appStartSetting = AutoInstrumentAppStartSetting.NONE;

        public void Configure(PerformanceConfiguration config)
        {
            _appStartSetting = config.AutoInstrumentAppStart;
        }

        public void Start()
        {
            if (_appStartSetting != AutoInstrumentAppStartSetting.OFF)
            {
                MainThreadDispatchBehaviour.Instance().Enqueue(CheckForAppStartCompletion());
            }
        }

        private IEnumerator CheckForAppStartCompletion()
        {
            while (!_complete)
            {
                CheckForAutomaticAppStartEnd();
                yield return new WaitForSeconds(1);
            }
            BugsnagPerformance.ProccessAppStartSpans(_appStartSpans);
        }

        private static Span CreateAppStartSpan(string name, string category)
        {
            return BugsnagPerformance.CreateAutoAppStartSpan(name, category);
        }

        private static void EndAppStartSpan(Span span, DateTimeOffset? endTime = null)
        {
            if (span != null)
            {
                span.EndAppStartSpan(endTime);
                _appStartSpans.Add(span);
            }
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
            EndAppStartSpan(_loadAssembliesSpan);
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
            EndAppStartSpan(_splashScreenSpan);
            EndAppStartSpan(_firstSceneSpan);

            _defaultAppStartEndTime = DateTimeOffset.UtcNow;
        }

        private void CheckForAutomaticAppStartEnd()
        {
            if (_defaultAppStartEndTime != null)
            {
                if (_appStartSetting == AutoInstrumentAppStartSetting.FULL)
                {
                    if (_rootSpan != null && !_rootSpan.Ended)
                    {
                        EndAppStartSpan(_rootSpan, _defaultAppStartEndTime);
                        _complete = true;
                    }
                }
            }
        }

        public static void ReportAppStarted()
        {
            if (_rootSpan != null && !_rootSpan.Ended)
            {
                EndAppStartSpan(_rootSpan);
                _complete = true;
            }
        }

    }
}