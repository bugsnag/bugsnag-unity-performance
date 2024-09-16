using System.Collections.Generic;
using System;
using System.Diagnostics;
using UnityEngine;
namespace BugsnagUnityPerformance
{
    internal class SpanModel
    {
        static readonly DateTimeOffset _unixStart = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private const string KEY_LIMIT_WARNING_MESSAGE = "Custom Span Attribute {0} was removed from the span {1} because the key exceeds the 128 character limit.";

        public string name;
        public int kind;
        public string spanId;
        public string traceId;
        public string startTimeUnixNano;
        public string endTimeUnixNano;
        public string parentSpanId;
        public List<AttributeModel> attributes = new List<AttributeModel>();
        private int _droppedAttributesCount = 0;

        public SpanModel(Span span, PerformanceConfiguration config)
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
                if (string.IsNullOrEmpty(attr.Key))
                {
                    continue;
                }
                if (attr.Key.Length > 128)
                {
                    _droppedAttributesCount++;
                    UnityEngine.Debug.LogWarning(string.Format(KEY_LIMIT_WARNING_MESSAGE, attr.Key, span.Name));
                    continue;
                }
                if (attr.Value is string[] stringArray)
                {
                    var truncatedStringArray = new string[stringArray.Length];
                    for (int i = 0; i < stringArray.Length; i++)
                    {
                        var strValue = stringArray[i];
                        if (strValue.Length > config.AttributeStringValueLimit)
                        {
                            int truncatedLength = strValue.Length - config.AttributeStringValueLimit;
                            strValue = strValue.Substring(0, config.AttributeStringValueLimit) + $"*** {truncatedLength} CHARS TRUNCATED";
                        }
                        truncatedStringArray[i] = strValue;
                    }
                    attributes.Add(new AttributeModel(attr.Key, new AttributeStringArrayValueModel(truncatedStringArray)));

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
                    if (strValue.Length > config.AttributeStringValueLimit)
                    {
                        int truncatedLength = strValue.Length - config.AttributeStringValueLimit;
                        strValue = strValue.Substring(0, config.AttributeStringValueLimit) + $"*** {truncatedLength} CHARS TRUNCATED";
                    }
                    attributes.Add(new AttributeModel(attr.Key, new AttributeStringValueModel(strValue)));
                }
                else if (attr.Value is long longValue)
                {
                    attributes.Add(new AttributeModel(attr.Key, new AttributeIntValueModel(longValue)));
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

            if (_droppedAttributesCount > 0)
            {
                attributes.Add(new AttributeModel("dropped_attributes_count", new AttributeIntValueModel(_droppedAttributesCount)));
            }
        }


        private string GetNanoSeconds(DateTimeOffset time)
        {
            var duration = time - _unixStart;
            return (duration.Ticks * 100).ToString();
        }
    }

}
