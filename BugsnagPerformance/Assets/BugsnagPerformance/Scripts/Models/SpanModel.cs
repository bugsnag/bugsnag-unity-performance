using System.Collections.Generic;

namespace BugsnagUnityPerformance
{
    internal class SpanModel
    {
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
            startTimeUnixNano = (span.StartTime.Ticks * 100).ToString();
            endTimeUnixNano = (span.EndTime.Ticks * 100).ToString();
            foreach (var attr in span.Attributes)
            {
                if (!string.IsNullOrEmpty(attr.key))
                {
                    attributes.Add(attr);
                }
            }
        }
    }
}
