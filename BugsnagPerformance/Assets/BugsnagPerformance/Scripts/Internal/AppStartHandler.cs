using System;
using System.Collections;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    public class AppStartHandler : IPhasedStartup
    {
        private const string UNITY_RUNTIME_SPAN_NAME = "[AppStart/UnityRuntime]";
        private const string APP_START_CATEGORY = "app_start";

        private const string LOAD_ASSEMBLIES_SPAN_NAME = "[AppStartPhase/LoadAssemblies]";
        private const string APP_START_PHASE_CATEGORY = "app_start_phase";
        private const string BUGSNAG_PHASE_KEY = "bugsnag.phase";
        private const string LOAD_ASSEMBLIES_PHASE = "LoadAssemblies";

        private const string SPLASH_SCREEN_SPAN_NAME = "[AppStartPhase/SplashScreen]";
        private const string SPLASH_SCREEN_PHASE = "SplashScreen";

        private const string LOAD_FIRST_SCENE_SPAN_NAME = "[AppStartPhase/LoadFirstScene]";
        private const string LOAD_FIRST_SCENE_PHASE = "LoadFirstScene";

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
                MainThreadDispatchBehaviour.Enqueue(CheckForAppStartCompletion());
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
            return _spanFactory.CreateAutoAppStartSpan(name, category, category.Equals(APP_START_CATEGORY));
        }

        internal void SubsystemRegistration()
        {
            _rootSpan = CreateAppStartSpan(UNITY_RUNTIME_SPAN_NAME, APP_START_CATEGORY);
            _loadAssembliesSpan = CreateAppStartSpan(LOAD_ASSEMBLIES_SPAN_NAME, APP_START_PHASE_CATEGORY);
            _loadAssembliesSpan.SetAttributeInternal(BUGSNAG_PHASE_KEY, LOAD_ASSEMBLIES_PHASE);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void AfterAssembliesLoaded()
        {
            _loadAssembliesSpan.End();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void BeforeSplashScreen()
        {
            _splashScreenSpan = CreateAppStartSpan(SPLASH_SCREEN_SPAN_NAME, APP_START_PHASE_CATEGORY);
            _splashScreenSpan.SetAttributeInternal(BUGSNAG_PHASE_KEY, SPLASH_SCREEN_PHASE);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            _firstSceneSpan = CreateAppStartSpan(LOAD_FIRST_SCENE_SPAN_NAME, APP_START_PHASE_CATEGORY);
            _firstSceneSpan.SetAttributeInternal(BUGSNAG_PHASE_KEY, LOAD_FIRST_SCENE_PHASE);
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