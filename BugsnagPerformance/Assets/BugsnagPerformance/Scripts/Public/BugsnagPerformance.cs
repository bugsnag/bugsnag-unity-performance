using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BugsnagNetworking;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

namespace BugsnagUnityPerformance
{
    public class BugsnagPerformance
    {
        private static BugsnagPerformance _sharedInstance;
        private const string ALREADY_STARTED_WARNING = "BugsnagPerformance.start has already been called";
        private static object _startLock = new object();
        internal static bool IsStarted = false;
        private object _startSpanLock = new object();
        private object _networkSpansLock = new object();
        private SpanFactory _spanFactory;
        private CacheManager _cacheManager;
        private Delivery _delivery;
        private ResourceModel _resourceModel;
        private Sampler _sampler;
        private Tracer _tracer;
        private AppStartHandler _appStartHandler;
        private PersistentState _persistentState;
        private PValueUpdater _pValueUpdater;
        private static List<Span> _potentiallyOpenSpans = new List<Span>();
        private Func<BugsnagNetworkRequestInfo, BugsnagNetworkRequestInfo> _networkRequestCallback;
        private static Regex[] _tracePropagationUrlMatchPatterns;
        private bool _isSamplingProbabilityOverriden;


        public static void Start(PerformanceConfiguration configuration)
        {
#if BUGSNAG_DEBUG
            Logger.I("BugsnagPerformance.Start called");
#endif
            lock (_startLock)
            {
                if (IsStarted)
                {
                    Debug.LogWarning(ALREADY_STARTED_WARNING);
                    return;
                }
                IsStarted = true;
            }
            if (configuration.TracePropagationUrls != null)
            {
                _tracePropagationUrlMatchPatterns = configuration.TracePropagationUrls.ToArray();
            }
            ValidateApiKey(configuration.ApiKey);
            if (ReleaseStageEnabled(configuration))
            {
                // init main thread dispatcher and create app lifecycle listener on main thread
                MainThreadDispatchBehaviour.Instance().Enqueue(() => { CreateAppLifecycleListener(); });
                _sharedInstance.Configure(configuration);
                _sharedInstance.Start();
#if BUGSNAG_DEBUG
                Logger.I("Start Complete");
#endif
            }
        }

        private static void CreateAppLifecycleListener()
        {
            new GameObject("Bugsnag performance app lifecycle listener").AddComponent<BugsnagPerformanceAppLifecycleListener>();
        }

        private static void ValidateApiKey(string apiKey)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(apiKey, @"\A\b[0-9a-fA-F]+\b\Z") ||
                apiKey.Length != 32)
            {
                throw new System.Exception($"Invalid Bugsnag Performance configuration. apiKey should be a 32-character hexademical string, got {apiKey} ");
            }
        }

        private static bool ReleaseStageEnabled(PerformanceConfiguration configuration)
        {
            return configuration.ReleaseStage == null
                || configuration.EnabledReleaseStages == null
                || configuration.EnabledReleaseStages.Contains(configuration.ReleaseStage);
        }

        public static Span StartSpan(string name)
        {
            return _sharedInstance.StartSpanInternal(name);
        }

        public static Span StartSpan(string name, SpanOptions spanOptions)
        {
            return _sharedInstance.StartSpanInternal(name, spanOptions);
        }

        public static Span StartSceneSpan(string sceneName, SpanOptions spanOptions = null)
        {
            return _sharedInstance._spanFactory.CreateManualSceneLoadSpan(sceneName, spanOptions);
        }

        public static Span StartNetworkSpan(string url, HttpVerb httpVerb, SpanOptions spanOptions = null)
        {
            return _sharedInstance._spanFactory.CreateManualNetworkSpan(url, httpVerb, spanOptions);
        }

        private Dictionary<BugsnagUnityWebRequest, Span> _networkSpans = new Dictionary<BugsnagUnityWebRequest, Span>();

        // All scene load events and operations happen on the main thread, so there is no need for concurrency protection
        private Dictionary<string, SceneLoadSpanContainer> _sceneLoadSpans = new Dictionary<string, SceneLoadSpanContainer>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            _sharedInstance = new BugsnagPerformance();
            _sharedInstance._appStartHandler.SubsystemRegistration();
        }

        private BugsnagPerformance()
        {
            _cacheManager = new CacheManager(Application.persistentDataPath);
            _persistentState = new PersistentState(_cacheManager);
            _sampler = new Sampler(_persistentState);
            _resourceModel = new ResourceModel(_cacheManager);
            _delivery = new Delivery(_resourceModel, _cacheManager, OnProbabilityChanged);
            _tracer = new Tracer(_sampler, _delivery);
            _spanFactory = new SpanFactory(OnSpanEnd);
            _appStartHandler = new AppStartHandler(_spanFactory);
            _pValueUpdater = new PValueUpdater(_delivery, _sampler);
        }

        internal class SceneLoadSpanContainer
        {
            public List<Span> Spans = new List<Span>();
        }

        private void Configure(PerformanceConfiguration config)
        {
            _isSamplingProbabilityOverriden = config.IsFixedSamplingProbability;
            _networkRequestCallback = config.NetworkRequestCallback;
            _cacheManager.Configure(config);
            _persistentState.Configure(config);
            _delivery.Configure(config);
            _resourceModel.Configure(config);
            _sampler.Configure(config);
            if(!_isSamplingProbabilityOverriden)
            {
                _pValueUpdater.Configure(config);
            }
            _tracer.Configure(config);
            _appStartHandler.Configure(config);
        }

        private void Start()
        {
            // The ordering of Start() must be carefully curated.
            _cacheManager.Start();
            _persistentState.Start();
            _delivery.Start();
            _resourceModel.Start();
            _sampler.Start();
            if(!_isSamplingProbabilityOverriden)
            {
               _pValueUpdater.Start();
            }
            _tracer.Start();
            _appStartHandler.Start();
            SetupNetworkListener();
            SetupSceneLoadListeners();
            IsStarted = true;
        }

        private void OnSpanEnd(Span span)
        {
            _potentiallyOpenSpans.Remove(span);
            _tracer.OnSpanEnd(span);
        }

        private void OnProbabilityChanged(double newProbability)
        {
            _sampler.Probability = newProbability;
        }

        private void SetupSceneLoadListeners()
        {
            BugsnagSceneManager.OnSceneLoad.AddListener(OnSceneLoadStart);
            SceneManager.sceneLoaded += OnSceneLoadEnd;
        }

        private void OnSceneLoadStart(object sceneId)
        {
            var sceneName = GetSceneNameFromSceneId(sceneId);
            AddSceneLoadInstance(sceneName);
        }

        private string GetSceneNameFromSceneId(object sceneId)
        {
            string sceneName;
            if (sceneId is int)
            {
                var path = SceneUtility.GetScenePathByBuildIndex((int)sceneId);
                sceneName = Path.GetFileNameWithoutExtension(path);
            }
            else
            {
                sceneName = sceneId as string;
            }
            return sceneName;
        }

        private void AddSceneLoadInstance(string sceneName)
        {
            if (!_sceneLoadSpans.ContainsKey(sceneName))
            {
                var spanLoadInstance = new SceneLoadSpanContainer();
                _sceneLoadSpans.Add(sceneName, spanLoadInstance);
            }
            _sceneLoadSpans[sceneName].Spans.Add(_spanFactory.CreateAutomaticSceneLoadSpan());
        }

        private void OnSceneLoadEnd(Scene scene, LoadSceneMode mode)
        {
            var span = GetSceneLoadSpan(scene);
            if (span != null)
            {
                span.EndSceneLoadSpan(scene.name);
            }
        }

        private Span GetSceneLoadSpan(Scene scene)
        {
            if (_sceneLoadSpans.ContainsKey(scene.name))
            {
                var container = _sceneLoadSpans[scene.name];
                if (container.Spans.Count > 0)
                {
                    var span = container.Spans[0];
                    container.Spans.RemoveAt(0);
                    return span;
                }
            }
            return null;
        }

        private void SetupNetworkListener()
        {
            BugsnagUnityWebRequest.OnSend.AddListener(OnRequestSend);
            BugsnagUnityWebRequest.OnComplete.AddListener(OnRequestComplete);
            BugsnagUnityWebRequest.OnAbort.AddListener(OnRequestAbort);
        }

        private void OnRequestSend(BugsnagUnityWebRequest request)
        {
            var url = request.url;
            bool shouldCreateSpan = true;
            if (_networkRequestCallback != null)
            {
                var callbackResult = _networkRequestCallback.Invoke(new BugsnagNetworkRequestInfo(url));
                if (callbackResult == null || string.IsNullOrEmpty(callbackResult.Url))
                {
                    shouldCreateSpan = false;
                }

                url = callbackResult.Url;
            }

            Span networkSpan = null;

            if (shouldCreateSpan)
            {
                networkSpan = _spanFactory.CreateAutomaticNetworkSpan(request, url);
                lock (_networkSpansLock)
                {
                    _networkSpans[request] = networkSpan;
                }
            }

            if (ShouldAddTraceParentHeader(request.url))
            {
                string parentId = "";
                string traceId = "";
                bool sampled = false;

                if (networkSpan != null)
                {
                    parentId = networkSpan.SpanId;
                    traceId = networkSpan.TraceId;
                    sampled = _sampler.Sampled(networkSpan, false);
                }
                else
                {
                    ISpanContext currentContext = _spanFactory.GetCurrentContext();
                    if (currentContext != null)
                    {
                        parentId = currentContext.SpanId;
                        traceId = currentContext.TraceId;
                    }
                }
                if (string.IsNullOrEmpty(parentId))
                {
                    return;
                }
                request.SetRequestHeader("traceparent", BuildTraceParentHeader(traceId, parentId, sampled));
            }
        }

        private static bool ShouldAddTraceParentHeader(string url)
        {
            if (_tracePropagationUrlMatchPatterns == null || _tracePropagationUrlMatchPatterns.Length == 0)
            {
                return true;
            }
            foreach (var pattern in _tracePropagationUrlMatchPatterns)
            {
                if (pattern.IsMatch(url))
                {
                    return true;
                }
            }
            return false;
        }

        private static string BuildTraceParentHeader(string traceId, string parentSpanId, bool sampled)
        {
            return $"00-{traceId}-{parentSpanId}-{(sampled ? "01" : "00")}";
        }



        private void OnRequestAbort(BugsnagUnityWebRequest request)
        {
            EndNetworkSpan(request, true);
        }

        private void OnRequestComplete(BugsnagUnityWebRequest request)
        {
            EndNetworkSpan(request);
        }

        private void EndNetworkSpan(BugsnagUnityWebRequest request, bool abort = false)
        {
            var discard = abort || request.isHttpError || request.isNetworkError;
            lock (_networkSpansLock)
            {
                if (!discard && _networkSpans.ContainsKey(request))
                {
                    var span = _networkSpans[request];
                    span.EndNetworkSpan(request);
                }
                _networkSpans.Remove(request);
            }
        }

        private Span StartSpanInternal(string name)
        {
            return StartSpanInternal(name, new SpanOptions());
        }

        private Span StartSpanInternal(string name, SpanOptions spanOptions)
        {
            lock (_startSpanLock)
            {
                var span = _spanFactory.StartCustomSpan(name, spanOptions);
                _potentiallyOpenSpans.Add(span);
                return span;
            }
        }

        public static ISpanContext GetCurrentSpanContext()
        {
            return _sharedInstance._spanFactory.GetCurrentContext();
        }

        public static void ReportAppStarted()
        {
            AppStartHandler.ReportAppStarted();
        }

        internal static void AppBackgrounded()
        {
            CancelAllOpenSpans();
        }

        private static void CancelAllOpenSpans()
        {
            foreach (var span in _potentiallyOpenSpans.ToArray())
            {
                if (!span.Ended)
                {
                    span.Discard();
                }
            }
        }

        [Serializable]
        private class PerformanceState
        {
            public string currentContextSpanId;
            public string currentContextTraceId;
            public PerformanceState(string currentContextSpanId, string currentContextTraceId)
            {
                this.currentContextSpanId = currentContextSpanId;
                this.currentContextTraceId = currentContextTraceId;
            }
        }

        [Preserve]
        internal static string GetPerformanceState()
        {
            var context = GetCurrentSpanContext();
            if (context != null)
            {
                var performanceState = new PerformanceState(context.SpanId, context.TraceId);
                var json = JsonUtility.ToJson(performanceState);
                return json;
            }
            return string.Empty;
        }

    }
}
