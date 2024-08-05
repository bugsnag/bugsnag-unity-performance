using System.Collections.Generic;
using System;

namespace BugsnagUnityPerformance
{
   internal class SpanModel
{
    static readonly DateTimeOffset _unixStart = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

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
        traceId = span.TraceId.Replace("-", string.Empty);
        parentSpanId = span.ParentSpanId;
        startTimeUnixNano = GetNanoSeconds(span.StartTime);
        endTimeUnixNano = GetNanoSeconds(span.EndTime);

        foreach (var attr in span.GetAttributes())
        {
            if (attr.Value is string[] stringArray)
            {
                attributes.Add(new AttributeModel(attr.Key, new AttributeStringArrayValueModel(stringArray)));
            }
            else if (attr.Value is long[] intArray)
            {
                attributes.Add(new AttributeModel(attr.Key, new AttributeIntArrayValueModel(intArray)));
            }
            else if (attr.Value is bool[] boolArray)
            {
                attributes.Add(new AttributeModel(attr.Key, new AttributeBoolArrayValueModel(boolArray)));
            }
            else if (attr.Value is double[] doubleArray)
            {
                attributes.Add(new AttributeModel(attr.Key, new AttributeDoubleArrayValueModel(doubleArray)));
            }
            else if (attr.Value is string strValue)
            {
                attributes.Add(new AttributeModel(attr.Key, new AttributeStringValueModel(strValue)));
            }
            else if (attr.Value is long intValue)
            {
                attributes.Add(new AttributeModel(attr.Key, new AttributeIntValueModel(intValue)));
            }
            else if (attr.Value is bool boolValue)
            {
                attributes.Add(new AttributeModel(attr.Key, new AttributeBoolValueModel(boolValue)));
            }
            else if (attr.Value is double doubleValue)
            {
                attributes.Add(new AttributeModel(attr.Key, new AttributeDoubleValueModel(doubleValue)));
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
