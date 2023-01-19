using System;
namespace BugsnagPerformance
{
    public class Span
    {
        
        public string Name { get; }
        public SpanKind Kind { get; }
        public long Id { get; }
        public string TraceId { get; }
        public DateTimeOffset StartTime { get; }
        public DateTimeOffset EndTime { get; private set; }

        public Span(string name, SpanKind kind, long id, string traceId, DateTimeOffset startTime)
        {
            Name = name;
            Kind = kind;
            Id = id;
            TraceId = traceId;
            StartTime = startTime;
        }

        public void End()
        {
            EndTime = DateTimeOffset.Now;
        }
     
    }
}
