using System;
namespace BugsnagPerformance
{
    internal class SpanFactory
    {

        private Random _rand = new Random();

        private Tracer _tracer;

        internal SpanFactory(Tracer tracer)
        {
            _tracer = tracer;
        }

        private string GetNewTraceId()
        {
            return new Guid().ToString();
        }

        private long GetNewSpanId()
        {
            return _rand.Next() + _rand.Next();
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
