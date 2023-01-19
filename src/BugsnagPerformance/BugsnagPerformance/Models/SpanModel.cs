using System;
namespace BugsnagUnityPerformance
{
    [Serializable]
    internal class SpanModel
    {
        public string name;
        public string kind;
        public string spanId;
        public string traceId;
        public long startTimeUnixNano;
        public long endTimeUnixNano;

        public SpanModel(Span span)
        {
            name = span.Name;
            kind = span.Kind.ToString();
            spanId = span.Id.ToString();
            traceId = span.TraceId.ToString();
            // no support for nano seconds, but there are 100 in every tick.
            // we hope to improve this in future
            startTimeUnixNano = span.StartTime.Ticks * 100;
            endTimeUnixNano = span.EndTime.Ticks * 100;
        }
    }
}
