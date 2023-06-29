using System.Collections.Generic;
using System;

namespace BugsnagUnityPerformance
{
    internal class SpanModel
    {

        static readonly DateTimeOffset _unixStart = new DateTimeOffset(1970,1,1,0,0,0, TimeSpan.Zero);

        public string name;
        public int kind;
        public string spanId;
        public string traceId;
        public string startTimeUnixNano;
        public string endTimeUnixNano;
        public string parentSpanId;
        public List<AttributeModel> attributes = new List<AttributeModel>();

        public SpanModel(Span span)
        {
            name = span.Name;
            kind = (int)span.Kind;
            spanId = span.SpanId;
            traceId = span.TraceId.Replace("-",string.Empty);
            parentSpanId = span.ParentSpanId;
            startTimeUnixNano = GetNanoSeconds(span.StartTime);
            endTimeUnixNano = GetNanoSeconds(span.EndTime);
            foreach (var attr in span.Attributes)
            {
                if (!string.IsNullOrEmpty(attr.key))
                {
                    attributes.Add(attr);
                }
            }
        }

        private string GetNanoSeconds(DateTimeOffset time)
        {
            var duration = time - _unixStart;
            return (duration.Ticks * 100).ToString();
        }

    }
}
