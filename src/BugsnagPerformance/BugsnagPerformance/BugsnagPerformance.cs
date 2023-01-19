using System;

namespace BugsnagPerformance
{
    public class BugsnagPerformance
    {

        private static PerformanceConfiguration _configuration;

        private static bool _isStarted = false;

        private static Tracer _tracer;

        private static SpanFactory _spanFactory;


        public static void Start(PerformanceConfiguration performanceConfiguration)
        {
            if (_isStarted)
            {
                // This will be replaced with a Unity warning log once the unity engine dlls are imported
                throw new Exception("Already started");
            }
            _configuration = performanceConfiguration;
            _tracer = new Tracer();
            _spanFactory = new SpanFactory(_tracer);
            _isStarted = true;
        }

        public static Span StartSpan(string name)
        {
            return StartSpan(name, DateTimeOffset.Now);
        }

        public static Span StartSpan(string name, DateTimeOffset startTime)
        {
            return _spanFactory.StartCustomSpan(name, startTime);
        }
    }
}
