using System;
using System.Collections.Generic;

namespace BugsnagUnityPerformance
{
    public class Span
    {
        
        public string Name { get; }
        public SpanKind Kind { get; }
        public long Id { get; }
        public string TraceId { get; }
        public DateTimeOffset StartTime { get; }
        public DateTimeOffset EndTime { get; private set; }
        private Tracer _tracer;

        internal Span(string name, SpanKind kind, long id, string traceId, DateTimeOffset startTime, Tracer tracer)
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
            EndTime = DateTimeOffset.Now;
            _tracer.OnSpanEnd(this);
        }

    }
}
