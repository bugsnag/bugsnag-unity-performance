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

        internal static Tracer Tracer;

        internal static SpanFactory SpanFactory;

        internal static Delivery Delivery;

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
                Delivery = new Delivery();
                if (Tracer == null)
                {
                    InitialiseComponents();
                }
                Delivery.FlushCache();
                SetupNetworkListener();
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
            Debug.Log("OnRequestSend with method type: " + request.method);
            var span = SpanFactory.CreateNetworkSpan(request);
            lock (_networkSpansLock)
            {
                _networkSpans[request] = span;
            }
        }

        private static void OnRequestAbort(BugsnagUnityWebRequest request)
        {
            Debug.Log("OnRequestAbort with method type: " + request.method);
        }

        private static void OnRequestComplete(BugsnagUnityWebRequest request)
        {
            Debug.Log("OnRequestComplete with method type: " + request.method);
            EndNetworkSpan(request);
        }

        private static void EndNetworkSpan(BugsnagUnityWebRequest request)
        {
            if (_networkSpans.ContainsKey(request))
            {
                var span = _networkSpans[request];
                span.EndNetworkSpan(request);
            }
        }

        private static void LogAlreadyStartedWarning()
        {
            Debug.LogWarning(ALREADY_STARTED_WARNING);
        }

        private static void InitialiseComponents()
        {
            Tracer = new Tracer();
            SpanFactory = new SpanFactory(Tracer);
        }

        public static Span StartSpan(string name)
        {
            return StartSpan(name, DateTimeOffset.Now);
        }

        public static Span StartSpan(string name, DateTimeOffset startTime)
        {
            lock (_startSpanLock)
            {
                if (Tracer == null)
                {
                    InitialiseComponents();
                }
                return SpanFactory.StartCustomSpan(name, startTime);
            }
        }
    }
}
