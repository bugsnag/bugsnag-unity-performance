using System;
using System.Collections.Generic;
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

        private static object _networkLock = new object();

        private static Dictionary<BugsnagUnityWebRequest, DateTimeOffset> _networkRequestTimes = new Dictionary<BugsnagUnityWebRequest, DateTimeOffset>();

        private static Dictionary<object,DateTimeOffset> _sceneLoadTimes = new Dictionary<object, DateTimeOffset>();

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
            _sceneLoadTimes.Add(sceneId, DateTimeOffset.UtcNow);
        }

        private static void OnSceneLoadEnd(Scene scene, LoadSceneMode mode)
        { 
            var endTime = DateTimeOffset.UtcNow;
            var found = false;
            var startTime = GetSceneLoadStartTime(scene, out found);
            if (found)
            {
                SpanFactory.ReportSceneLoadSpan(scene.name, startTime,endTime);
            }
        }

        private static DateTimeOffset GetSceneLoadStartTime(Scene scene, out bool found)
        {
            found = false;
            if (_sceneLoadTimes.ContainsKey(scene.name))
            {
                found = true;
                return _sceneLoadTimes[scene.name];
            }
            if (_sceneLoadTimes.ContainsKey(scene.buildIndex))
            {
                found = true;
                return _sceneLoadTimes[scene.buildIndex];
            }
            return DateTimeOffset.MinValue;
        }

        private static void SetupNetworkListener()
        {
            BugsnagUnityWebRequest.OnSend.AddListener(OnRequestSend);
            BugsnagUnityWebRequest.OnComplete.AddListener(OnRequestComplete);
            BugsnagUnityWebRequest.OnAbort.AddListener(OnRequestAbort);
        }

        private static void OnRequestSend(BugsnagUnityWebRequest request)
        {
            var startTime = DateTimeOffset.UtcNow;
            lock (_networkLock)
            {
                _networkRequestTimes[request] = startTime;
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
            lock (_networkLock)
            {
                var endTime = DateTimeOffset.UtcNow;
                if (_networkRequestTimes.ContainsKey(request))
                {
                    var startTime = _networkRequestTimes[request];
                    SpanFactory.ReportNetworkSpan(request, startTime, endTime);
                }
                _networkRequestTimes.Remove(request);
            }
        }

        private static void LogAlreadyStartedWarning()
        {
            Debug.LogWarning(ALREADY_STARTED_WARNING);
        }

        public static Span StartSpan(string name)
        {
            return StartSpan(name, DateTimeOffset.Now);
        }

        public static Span StartSpan(string name, DateTimeOffset startTime)
        {
            lock (_startSpanLock)
            {
                return SpanFactory.StartCustomSpan(name, startTime);
            }
        }
    }
}
