using System.Collections.Generic;
using System;

namespace BugsnagUnityPerformance
{
    internal class SpanModel
    {
        private const int MAXIMUM_ATTRIBUTE_KEY_LENGTH_LIMIT = 128;
        static readonly DateTimeOffset _unixStart = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public string name;
        public int kind;
        public string spanId;
        public string traceId;
        public string startTimeUnixNano;
        public string endTimeUnixNano;
        public string parentSpanId;
        public List<AttributeModel> attributes = new List<AttributeModel>();

        public SpanModel(Span span, int attributeArrayLengthLimit, int attributeStringValueLimit)
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

                if (attr.Key.Length > MAXIMUM_ATTRIBUTE_KEY_LENGTH_LIMIT)
                {
                    span.DroppedAttributesCount++;
                    continue;
                }

                if (attr.Value is string[] stringArray)
                {
                    var truncatedStringArray = TruncateArrayIfNeeded(stringArray, attributeArrayLengthLimit, attr.Key, span.Name);
                    var valueLengthCheckedStringArray = new string[truncatedStringArray.Length];
                    for (int i = 0; i < truncatedStringArray.Length; i++)
                    {
                        valueLengthCheckedStringArray[i] = TruncateStringIfNeeded(truncatedStringArray[i], attributeStringValueLimit);
                    }
                    attributes.Add(new AttributeModel(attr.Key, new AttributeStringArrayValueModel(valueLengthCheckedStringArray)));
                }
                else if (attr.Value is long[] intArray)
                {
                    var truncatedIntArray = TruncateArrayIfNeeded(intArray, attributeArrayLengthLimit, attr.Key, span.Name);
                    attributes.Add(new AttributeModel(attr.Key, new AttributeIntArrayValueModel(truncatedIntArray)));
                }
                else if (attr.Value is bool[] boolArray)
                {
                    var truncatedBoolArray = TruncateArrayIfNeeded(boolArray, attributeArrayLengthLimit, attr.Key, span.Name);
                    attributes.Add(new AttributeModel(attr.Key, new AttributeBoolArrayValueModel(truncatedBoolArray)));
                }
                else if (attr.Value is double[] doubleArray)
                {
                    var truncatedDoubleArray = TruncateArrayIfNeeded(doubleArray, attributeArrayLengthLimit, attr.Key, span.Name);
                    attributes.Add(new AttributeModel(attr.Key, new AttributeDoubleArrayValueModel(truncatedDoubleArray)));
                }
                else if (attr.Value is string strValue)
                {
                    attributes.Add(new AttributeModel(attr.Key, new AttributeStringValueModel(TruncateStringIfNeeded(strValue, attributeStringValueLimit))));
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
                var truncatedArray = new T[limit];
                Array.Copy(array, truncatedArray, limit);
                return truncatedArray;
            }
            return array;
        }

        private string TruncateStringIfNeeded(string strValue, int limit)
        {
            if (strValue.Length > limit)
            {
                int truncatedLength = strValue.Length - limit;
                return strValue.Substring(0, limit) + $"*** {truncatedLength} CHARS TRUNCATED";
            }
            return strValue;
        }

        private string GetNanoSeconds(DateTimeOffset time)
        {
            var duration = time - _unixStart;
            return (duration.Ticks * 100).ToString();
        }
    }

}
