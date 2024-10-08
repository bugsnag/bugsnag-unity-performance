﻿using System;
using System.Collections;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    public class AppStartHandler : IPhasedStartup
    {

        private static Span _rootSpan;
        private static Span _loadAssembliesSpan;
        private static Span _splashScreenSpan;
        private static Span _firstSceneSpan;
        private PerformanceConfiguration _config;
        private static bool _appStartComplete;
        private static DateTimeOffset? _defaultAppStartEndTime = null; 

        private static SpanFactory _spanFactory;

        internal AppStartHandler(SpanFactory spanFactory)
        {
            _spanFactory = spanFactory;
        }

        public void Configure(PerformanceConfiguration config)
        {
            _config = config;
            if (_config.AutoInstrumentAppStart == AutoInstrumentAppStartSetting.OFF)
            {
                AbortAppStartSpans();
            }
        }

        private void AbortAppStartSpans()
        {
            if (_rootSpan != null && !_rootSpan.Ended)
            {
                _rootSpan.Discard();
            }
            if (_loadAssembliesSpan != null && !_loadAssembliesSpan.Ended)
            {
                _loadAssembliesSpan.Discard();
            }
            if (_splashScreenSpan != null && !_splashScreenSpan.Ended)
            {
                _splashScreenSpan.Discard();
            }
            if (_firstSceneSpan != null && !_firstSceneSpan.Ended)
            {
                _firstSceneSpan.Discard();
            }
        }

        public void Start()
        {
            if (_config.AutoInstrumentAppStart == AutoInstrumentAppStartSetting.FULL)
            {
                MainThreadDispatchBehaviour.Instance().Enqueue(CheckForAppStartCompletion());
            }
        }

        private IEnumerator CheckForAppStartCompletion()
        {
            while (!_appStartComplete)
            {
                if (_defaultAppStartEndTime != null)
                {
                    ReportAppStarted(_defaultAppStartEndTime);
                }
                yield return new WaitForSeconds(1);
            }
        }

        internal static void ReportAppStarted(DateTimeOffset? endTime = null)
        {
            if (_rootSpan != null && !_rootSpan.Ended)
            {
                _rootSpan.End(endTime);
                _appStartComplete = true;
            }
        }

        private static Span CreateAppStartSpan(string name, string category)
        {
            return _spanFactory.CreateAutoAppStartSpan(name, category);
        }

        internal void SubsystemRegistration()
        {
            _rootSpan = CreateAppStartSpan("[AppStart/UnityRuntime]", "app_start");
            _loadAssembliesSpan = CreateAppStartSpan("[AppStartPhase/LoadAssemblies]", "app_start_phase");
            _loadAssembliesSpan.SetAttributeInternal("bugsnag.phase", "LoadAssemblies");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void AfterAssembliesLoaded()
        {
            _loadAssembliesSpan.End();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void BeforeSplashScreen()
        {
            _splashScreenSpan = CreateAppStartSpan("[AppStartPhase/SplashScreen]", "app_start_phase");
            _splashScreenSpan.SetAttributeInternal("bugsnag.phase", "SplashScreen");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            _firstSceneSpan = CreateAppStartSpan("[AppStartPhase/LoadFirstScene]", "app_start_phase");
            _firstSceneSpan.SetAttributeInternal("bugsnag.phase", "LoadFirstScene");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AfterSceneLoad()
        {
            _splashScreenSpan.End();
            _firstSceneSpan.End();

            // Save the time so that we can use it later if full auto instrumentation is set
            _defaultAppStartEndTime = DateTimeOffset.UtcNow;
        }

    }
}