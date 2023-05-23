using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    public class TracePayload
    {

        public string PayloadId;
        public SortedList<double, int> SamplingHistogram { get;  private set; }

        private ResourceModel _resourceModel;
        private List<SpanModel> _spans = new List<SpanModel>();

        // Temporary method to allow hard coding the Bugsnag-Span-Sampling header until sampling is properly implemented
        public int BatchSize;

        private string _jsonbody;

        public TracePayload(ResourceModel resourceModel, List<Span> spans)
        {
            _resourceModel = resourceModel;
            PayloadId = Guid.NewGuid().ToString();
            foreach (var span in spans)
            {
                _spans.Add(new SpanModel(span));
            }
            BatchSize = spans.Count;
            SamplingHistogram = CalculateSamplingHistorgram(spans);
        }

        public TracePayload(string cachedJson, string payloadId)
        {
            PayloadId = payloadId;
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

        public string GetJsonBody()
        {
            if (string.IsNullOrEmpty(_jsonbody))
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
            return _jsonbody;
        }

    }

    [Serializable]
    internal class TracePayloadBody
    {
        public ResourceSpanModel[] resourceSpans;
    }
        
}