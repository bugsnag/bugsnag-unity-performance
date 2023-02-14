using System;
using UnityEngine;

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
                IsStarted = true;
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
