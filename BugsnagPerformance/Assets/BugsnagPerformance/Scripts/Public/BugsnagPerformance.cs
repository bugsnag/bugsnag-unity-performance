using System;
using System.Collections.Generic;
using System.IO;
using BugsnagNetworking;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace BugsnagUnityPerformance
{
    public class BugsnagPerformance
    {
        private static BugsnagPerformance _sharedInstance = new BugsnagPerformance();
        private const string ALREADY_STARTED_WARNING = "BugsnagPerformance.start has already been called";
        private static object _startLock = new object();
        internal static bool IsStarted = false;

        public static void Start(PerformanceConfiguration configuration)
        {
            lock (_startLock)
            {
                if (IsStarted)
                {
                    Debug.LogWarning(ALREADY_STARTED_WARNING);
                    return;
                }
                IsStarted = true;
            }

            _sharedInstance.Configure(configuration);
            _sharedInstance.Start();
        }

        public static Span StartSpan(string name)
        {
            return _sharedInstance.StartSpanInternal(name);
        }

        public static Span StartSpan(string name, SpanOptions spanOptions)
        {
            return _sharedInstance.StartSpanInternal(name, spanOptions);
        }

        private object _startSpanLock = new object();

        private object _networkSpansLock = new object();

        private SpanFactory _spanFactory;

        private CacheManager _cacheManager;
        private Delivery _delivery;
        private ResourceModel _resourceModel;
        private Tracer _tracer;

        private Dictionary<BugsnagUnityWebRequest, Span> _networkSpans = new Dictionary<BugsnagUnityWebRequest, Span>();

        // All scene load events and operations happen on the main thread, so there is no need for concurrency protection
        private Dictionary<string, SceneLoadSpanContainer> _sceneLoadSpans = new Dictionary<string, SceneLoadSpanContainer>();

        private BugsnagPerformance()
        {
            _cacheManager = new CacheManager(Application.persistentDataPath);
            _resourceModel = new ResourceModel(_cacheManager);
            _delivery = new Delivery(_resourceModel, _cacheManager);
            _tracer = new Tracer(_delivery);
            _spanFactory = new SpanFactory(OnSpanEnd);
        }

        internal class SceneLoadSpanContainer
        {
            public string SceneName;
            public List<Span> Spans = new List<Span>();
        }

        private void Configure(PerformanceConfiguration config)
        {
            _cacheManager.Configure(config);
            _delivery.Configure(config);
            _resourceModel.Configure(config);
            _tracer.Configure(config);
        }

        private void Start()
        {
            _cacheManager.Start();
            _delivery.Start();
            _resourceModel.Start();
            _tracer.Start();

            SetupNetworkListener();
            SetupSceneLoadListeners();
            IsStarted = true;
        }

        private void OnSpanEnd(Span span)
        {
            _tracer.OnSpanEnd(span);
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
                Debug.Log("Got scene name from index: " + sceneName);
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
            var span = _spanFactory.CreateAutomaticNetworkSpan(request);
            lock (_networkSpansLock)
            {
                _networkSpans[request] = span;
            }
        }

        private void OnRequestAbort(BugsnagUnityWebRequest request)
        {
            EndNetworkSpan(request);
        }

        private void OnRequestComplete(BugsnagUnityWebRequest request)
        {
            EndNetworkSpan(request);
        }

        private void EndNetworkSpan(BugsnagUnityWebRequest request)
        {
            lock (_networkSpansLock)
            {
                if (_networkSpans.ContainsKey(request))
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
                return _spanFactory.StartCustomSpan(name, spanOptions);
            }
        }

    }
}
