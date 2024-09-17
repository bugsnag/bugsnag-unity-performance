using System.Collections.Generic;
using System;

namespace BugsnagUnityPerformance
{
    internal class SpanModel
    {
        static readonly DateTimeOffset _unixStart = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        private const string KEY_LIMIT_WARNING_MESSAGE = "Custom Span Attribute {0} was removed from the span {1} because the key exceeds the 128 character limit.";
        private const string ARRAY_LIMIT_WARNING_MESSAGE = "Custom Span Array Attribute {0} in span {1} was truncated because it exceeded the length limit of {2} elements.";
        public string name;
        public int kind;
        public string spanId;
        public string traceId;
        public string startTimeUnixNano;
        public string endTimeUnixNano;
        public string parentSpanId;
        public List<AttributeModel> attributes = new List<AttributeModel>();

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
                if (string.IsNullOrEmpty(attr.Key) || attr.Value == null)
                {
                    continue;
                }

                if (attr.Key.Length > 128)
                {
                    span.DroppedAttributesCount++;
                    MainThreadDispatchBehaviour.Instance().LogWarning(string.Format(KEY_LIMIT_WARNING_MESSAGE, attr.Key, span.Name));
                    continue;
                }

                if (attr.Value is string[] stringArray)
                {
                    var truncatedStringArray = TruncateArrayIfNeeded(stringArray, config.AttributeArrayLengthLimit, attr.Key, span.Name);
                    var valueLengthCheckedStringArray = new string[truncatedStringArray.Length];
                    for (int i = 0; i < truncatedStringArray.Length; i++)
                    {
                        var strValue = truncatedStringArray[i];
                        if (strValue.Length > config.AttributeStringValueLimit)
                        {
                            int truncatedLength = strValue.Length - config.AttributeStringValueLimit;
                            strValue = strValue.Substring(0, config.AttributeStringValueLimit) + $"*** {truncatedLength} CHARS TRUNCATED";
                        }
                        valueLengthCheckedStringArray[i] = strValue;
                    }
                    attributes.Add(new AttributeModel(attr.Key, new AttributeStringArrayValueModel(valueLengthCheckedStringArray)));
                }
                else if (attr.Value is long[] intArray)
                {
                    var truncatedIntArray = TruncateArrayIfNeeded(intArray, config.AttributeArrayLengthLimit, attr.Key, span.Name);
                    attributes.Add(new AttributeModel(attr.Key, new AttributeIntArrayValueModel(truncatedIntArray)));
                }
                else if (attr.Value is bool[] boolArray)
                {
                    var truncatedBoolArray = TruncateArrayIfNeeded(boolArray, config.AttributeArrayLengthLimit, attr.Key, span.Name);
                    attributes.Add(new AttributeModel(attr.Key, new AttributeBoolArrayValueModel(truncatedBoolArray)));
                }
                else if (attr.Value is double[] doubleArray)
                {
                    var truncatedDoubleArray = TruncateArrayIfNeeded(doubleArray, config.AttributeArrayLengthLimit, attr.Key, span.Name);
                    attributes.Add(new AttributeModel(attr.Key, new AttributeDoubleArrayValueModel(truncatedDoubleArray)));
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

            if (span.DroppedAttributesCount > 0)
            {
                attributes.Add(new AttributeModel("dropped_attributes_count", new AttributeIntValueModel(span.DroppedAttributesCount)));
            }
        }

        private T[] TruncateArrayIfNeeded<T>(T[] array, int limit, string key, string spanName)
        {
            if (array.Length > limit)
            {
                MainThreadDispatchBehaviour.Instance().LogWarning(string.Format(ARRAY_LIMIT_WARNING_MESSAGE, key, spanName, limit));
                var truncatedArray = new T[limit];
                Array.Copy(array, truncatedArray, limit);
                return truncatedArray;
            }
            return array;
        }



        private string GetNanoSeconds(DateTimeOffset time)
        {
            var duration = time - _unixStart;
            return (duration.Ticks * 100).ToString();
        }
    }

}
