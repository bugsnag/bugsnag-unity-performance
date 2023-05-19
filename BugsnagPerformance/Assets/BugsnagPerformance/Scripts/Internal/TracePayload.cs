using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class TracePayload
    {

        public string PayloadId;

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
        }

        public TracePayload(string cachedJson, string payloadId)
        {
            PayloadId = payloadId;
            _jsonbody = cachedJson;
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