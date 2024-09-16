using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using System.Globalization;

namespace BugsnagUnityPerformance
{
    public class TracePayload
    {

        public string PayloadId;
        public SortedList<double, int> SamplingHistogram { get;  private set; }
        public Dictionary<string, string> Headers { get; private set; } = new Dictionary<string, string>();

        private ResourceModel _resourceModel;
        private List<SpanModel> _spans = null;

        private string _jsonbody;

        public TracePayload(ResourceModel resourceModel, List<Span> spans, PerformanceConfiguration config)
        {
            _resourceModel = resourceModel;
            if (spans != null && spans.Count > 0)
            {
                _spans = new List<SpanModel>();
                PayloadId = Guid.NewGuid().ToString();
                foreach (var span in spans)
                {
                    _spans.Add(new SpanModel(span, config));
                }
                SamplingHistogram = CalculateSamplingHistorgram(spans);
                if(!config.IsFixedSamplingProbability)
                {
                    Headers["Bugsnag-Span-Sampling"] = BuildSamplingHistogramHeader(this);
                }
            }
            else
            {
                if(!config.IsFixedSamplingProbability)
                {
                    Headers["Bugsnag-Span-Sampling"] = "1:0";
                }
            }
        }

        private TracePayload(Dictionary<string, string> headers, string cachedJson, string payloadId)
        {
            PayloadId = payloadId;
            Headers = headers;
            _jsonbody = cachedJson;
        }

        private static SortedList<double, int> CalculateSamplingHistorgram(List<Span> spans)
        {
            var histogram = new Dictionary<double, int>();
            foreach (Span span in spans)
            {
                var p = span.samplingProbability;
                if (histogram.ContainsKey(p))
                {
                    histogram[p]++;
                }
                else
                {
                    histogram[p] = 1;
                }
            }
            return new SortedList<double, int>(histogram);
        }

        internal string GetJsonBody()
        {
            if (string.IsNullOrEmpty(_jsonbody))
            {
                if (_spans == null)
                {
                    return "{\"resourceSpans\": []}";
                } else
                {
                    var scopeSpans = new ScopeSpanModel[] { new ScopeSpanModel(_spans.ToArray()) };
                    var resourceSpans = new ResourceSpanModel[] { new ResourceSpanModel(_resourceModel, scopeSpans) };
                    var serialiseablePayload = new TracePayloadBody()
                    {
                        resourceSpans = resourceSpans
                    };
                    _jsonbody = JsonConvert.SerializeObject(serialiseablePayload, new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore
                    });
                }
            }            
            return _jsonbody;
        }

        internal void StreamHeadersSection(Stream output)
        {
            var writer = new StreamWriter(output);
            foreach (var header in Headers)
            {
                writer.Write("{0}: {1}\n", header.Key, header.Value);
            }
            writer.WriteLine("");
            writer.Flush();
        }

        public void Serialize(Stream output)
        {
            StreamHeadersSection(output);
            var writer = new StreamWriter(output);
            writer.Write(GetJsonBody());
            writer.Flush();
        }

        private static Dictionary<string, string> DeserializeHeaders(StreamReader reader)
        {
            var headers = new Dictionary<string, string>();
            while (true)
            {
                var line = reader.ReadLine();
                if (line == "")
                {
                    return headers;
                }
                var kv = line.Split(new[] { ':' }, 2);
                headers[kv[0].TrimEnd()] = kv[1].TrimStart();
            }
        }

        public static TracePayload Deserialize(String id, Stream input)
        {
            var reader = new StreamReader(input);
            var headers = DeserializeHeaders(reader);
            var jsonData = reader.ReadToEnd();
            return new TracePayload(headers, jsonData, id);
        }

        private static string BuildSamplingHistogramHeader(TracePayload payload)
        {
            var builder = new StringBuilder();

            foreach (KeyValuePair<double, int> pair in payload.SamplingHistogram)
            {
                builder.Append(pair.Key.ToString(CultureInfo.InvariantCulture));
                builder.Append(':');
                builder.Append(pair.Value.ToString(CultureInfo.InvariantCulture));
                builder.Append(';');
            }
            builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        public bool PayloadsAreEqual(TracePayload other)
        {
            return GetJsonBody() == other.GetJsonBody() &&
                Headers.Count == other.Headers.Count &&
                !Headers.Except(other.Headers).Any();
        }

    }

    [Serializable]
    internal class TracePayloadBody
    {
        public ResourceSpanModel[] resourceSpans;
    }

}