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

        private static bool _appStartComplete;
        private static DateTimeOffset? _defaultAppStartEndTime = null; 

        private static List<Span> _appStartSpans = new List<Span>();

        private static AutoInstrumentAppStartSetting _appStartSetting;

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
            while (!_appStartComplete)
            {
                if (_appStartSetting == AutoInstrumentAppStartSetting.FULL)
                {
                    CheckForAutomaticAppStartEnd();
                }
                yield return new WaitForSeconds(1);
            }
            BugsnagPerformance.ProccessAppStartSpans(_appStartSpans);
        }

        private void CheckForAutomaticAppStartEnd()
        {
            if (_defaultAppStartEndTime != null)
            {
                ReportAppStarted(_defaultAppStartEndTime);
            }
        }

        internal static void ReportAppStarted(DateTimeOffset? endTime = null)
        {
            if (_rootSpan != null && !_rootSpan.Ended)
            {
                EndAppStartSpan(_rootSpan, endTime);
                _appStartComplete = true;
            }
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
        private static void SubsystemRegistration()
        {
            _rootSpan = CreateAppStartSpan("[AppStart/UnityRuntime]", "app_start");

            _loadAssembliesSpan = CreateAppStartSpan("[AppStartPhase/LoadAssemblies]", "app_start_phase");
            _loadAssembliesSpan.SetAttribute("bugsnag.phase", "LoadAssemblies");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void AfterAssembliesLoaded()
        {
            EndAppStartSpan(_loadAssembliesSpan);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void BeforeSplashScreen()
        {
            _splashScreenSpan = CreateAppStartSpan("[AppStartPhase/SplashScreen]", "app_start_phase");
            _splashScreenSpan.SetAttribute("bugsnag.phase", "SplashScreen");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            _firstSceneSpan = CreateAppStartSpan("[AppStartPhase/LoadFirstScene]", "app_start_phase");
            _firstSceneSpan.SetAttribute("bugsnag.phase", "LoadFirstScene");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AfterSceneLoad()
        {
            EndAppStartSpan(_splashScreenSpan);
            EndAppStartSpan(_firstSceneSpan);

            // Save the time so that we can use it later if full auto instrumentation is set
            _defaultAppStartEndTime = DateTimeOffset.UtcNow;
        }

    }
}