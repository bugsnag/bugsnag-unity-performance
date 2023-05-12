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

        internal static PerformanceConfiguration Configuration;

        internal static bool IsStarted = false;

        private const string ALREADY_STARTED_WARNING = "BugsnagPerformance.start has already been called";

        private static object _startLock = new object();

        private static object _startSpanLock = new object();

        private static object _networkSpansLock = new object();

        private static Dictionary<BugsnagUnityWebRequest, Span> _networkSpans = new Dictionary<BugsnagUnityWebRequest, Span>();

        // All scene load events and operations happen on the main thread, so there is no need for concurrency protection
        private static Dictionary<string, SceneLoadSpanContainer> _sceneLoadSpans = new Dictionary<string, SceneLoadSpanContainer>();

        internal class SceneLoadSpanContainer
        {
            public string SceneName;
            public List<Span> Spans = new List<Span>();
        }

        public static void Start(PerformanceConfiguration configuration)
        {
            lock (_startLock)
            {
                if (IsStarted)
                {
                    LogAlreadyStartedWarning();
                    return;
                }
                Configuration = configuration;
                Delivery.FlushCache();
                SetupNetworkListener();
                SetupSceneLoadListeners();
                Tracer.StartTracerWorker();
                IsStarted = true;
            }
        }

        private static void SetupSceneLoadListeners()
        {
            BugsnagSceneManager.OnSeceneLoad.AddListener(OnSceneLoadStart);
            SceneManager.sceneLoaded += OnSceneLoadEnd;
        }

        private static void OnSceneLoadStart(object sceneId)
        {
            var sceneName = GetSceneNameFromSceneId(sceneId);
            AddSceneLoadInstance(sceneName);
        }

        private static string GetSceneNameFromSceneId(object sceneId)
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

        private static void AddSceneLoadInstance(string sceneName)
        {
            if (!_sceneLoadSpans.ContainsKey(sceneName))
            {
                var spanLoadInstance = new SceneLoadSpanContainer();
                _sceneLoadSpans.Add(sceneName, spanLoadInstance);
            }
            _sceneLoadSpans[sceneName].Spans.Add(SpanFactory.CreateAutomaticSceneLoadSpan());
        }

        private static void OnSceneLoadEnd(Scene scene, LoadSceneMode mode)
        {
            var span = GetSceneLoadSpan(scene);
            if (span != null)
            {
                span.EndSceneLoadSpan(scene.name);
            }
        }

        private static Span GetSceneLoadSpan(Scene scene)
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

        private static void SetupNetworkListener()
        {
            BugsnagUnityWebRequest.OnSend.AddListener(OnRequestSend);
            BugsnagUnityWebRequest.OnComplete.AddListener(OnRequestComplete);
            BugsnagUnityWebRequest.OnAbort.AddListener(OnRequestAbort);
        }

        private static void OnRequestSend(BugsnagUnityWebRequest request)
        {
            var span = SpanFactory.CreateAutomaticNetworkSpan(request);
            lock (_networkSpansLock)
            {
                _networkSpans[request] = span;
            }
        }

        private static void OnRequestAbort(BugsnagUnityWebRequest request)
        {
            EndNetworkSpan(request);
        }

        private static void OnRequestComplete(BugsnagUnityWebRequest request)
        {
            EndNetworkSpan(request);
        }

        private static void EndNetworkSpan(BugsnagUnityWebRequest request)
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

        private static void LogAlreadyStartedWarning()
        {
            Debug.LogWarning(ALREADY_STARTED_WARNING);
        }

        public static Span StartSpan(string name)
        {
            return StartSpan(name, new SpanOptions());
        }

        public static Span StartSpan(string name, SpanOptions spanOptions)
        {
            lock (_startSpanLock)
            {
                return SpanFactory.StartCustomSpan(name, spanOptions);
            }
        }
    }
}
