using System;
using System.Collections.Generic;
using BugsnagNetworking;
using UnityEngine;
using UnityEngine.Networking;

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

        private static void SetupNetworkListener()
        {
            BugsnagUnityWebRequest.OnSend.AddListener(OnRequestSend);
            BugsnagUnityWebRequest.OnComplete.AddListener(OnRequestComplete);
            BugsnagUnityWebRequest.OnAbort.AddListener(OnRequestAbort);
        }

        private static void OnRequestSend(BugsnagUnityWebRequest request)
        {
            var span = SpanFactory.CreateNetworkSpan(request);
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
