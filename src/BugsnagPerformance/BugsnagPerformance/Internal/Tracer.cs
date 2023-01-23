using System;
namespace BugsnagUnityPerformance
{
    internal class Tracer
    {

        private Delivery _delivery;
        private PerformanceConfiguration _configuration;

        public Tracer(PerformanceConfiguration performanceConfiguration)
        {
            _configuration = performanceConfiguration;
            _delivery = new Delivery(_configuration);
        }

        public void OnSpanEnd(Span span)
        {
            var payload = new TracePayload(span);
            _delivery.Deliver(payload);
        }
    }
}
