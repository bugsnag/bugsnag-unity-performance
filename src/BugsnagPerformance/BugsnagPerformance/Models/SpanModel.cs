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
        public AttributeModel[] attributes;

        public SpanModel(Span span)
        {
            name = span.Name;
            kind = span.Kind.ToString();
            spanId = span.Id.ToString("x");
            traceId = span.TraceId.ToString();
            startTimeUnixNano = span.StartTime.Ticks * 100;
            endTimeUnixNano = span.EndTime.Ticks * 100;
            attributes = new AttributeModel[]
            {
                new AttributeModel()
                {
                    key = "bugsnag.span_category",
                    value = new AttributeStringValueModel("custom")
                }
            };
        }
    }
}
