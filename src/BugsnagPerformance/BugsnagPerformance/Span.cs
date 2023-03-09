using System;

namespace BugsnagUnityPerformance
{
    public class Span
    {
        
        public string Name { get; }
        public SpanKind Kind { get; }
        public string Id { get; }
        public string TraceId { get; }
        public DateTimeOffset StartTime { get; }
        public DateTimeOffset EndTime { get; private set; }
        private Tracer _tracer;
        private bool _ended;

        internal Span(string name, SpanKind kind, string id, string traceId, DateTimeOffset startTime, Tracer tracer)
        {
            Name = name;
            Kind = kind;
            Id = id;
            TraceId = traceId;
            StartTime = startTime;
            _tracer = tracer;
        }

        public void End()
        {
            if (_ended)
            {
                return;
            }
            _ended = true;
            EndTime = DateTimeOffset.Now;
            _tracer.OnSpanEnd(this);
        }

    }
}
