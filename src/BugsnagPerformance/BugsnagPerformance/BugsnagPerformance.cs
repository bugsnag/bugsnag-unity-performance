using System;

namespace BugsnagUnityPerformance
{
    public class BugsnagPerformance
    {

        private static PerformanceConfiguration _configuration;

        private static bool _isStarted = false;

        private static Tracer _tracer;

        private static SpanFactory _spanFactory;


        public static void Start(PerformanceConfiguration configuration)
        {
            if (_isStarted)
            {
                // This will be replaced with a Unity warning log once the unity engine dlls are imported
                throw new Exception("Already started");
            }
            _configuration = configuration;
            _tracer = new Tracer(configuration);
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
