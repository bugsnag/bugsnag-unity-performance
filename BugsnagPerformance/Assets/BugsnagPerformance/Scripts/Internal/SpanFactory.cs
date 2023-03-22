using System;

namespace BugsnagUnityPerformance
{
    internal class SpanFactory
    {

        private Tracer _tracer;

        internal SpanFactory(Tracer tracer)
        {
            _tracer = tracer;
        }

        private string GetNewTraceId()
        {
            return Guid.NewGuid().ToString();
        }

        private string GetNewSpanId()
        {
            var newId = Guid.NewGuid().ToString().Replace("-",string.Empty);
            return newId.Substring(0,16);
        }

        internal Span StartCustomSpan(string name, DateTimeOffset startTime)
        {
            return CreateSpan(name,SpanKind.SPAN_KIND_INTERNAL, startTime);
        }

        private Span CreateSpan(string name, SpanKind kind, DateTimeOffset startTime)
        {
            return new Span(name, kind, GetNewSpanId(), GetNewTraceId(), startTime, _tracer);
        }
    }
}
