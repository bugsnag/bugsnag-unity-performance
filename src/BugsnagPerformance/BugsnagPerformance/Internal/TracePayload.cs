using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace BugsnagUnityPerformance
{
    internal class TracePayload
    {


        public string PayloadId;

        private List<SpanModel> _spans = new List<SpanModel>();

        private string _jsonbody;

        public TracePayload(List<Span> spans)
        {
            PayloadId = Guid.NewGuid().ToString();
            foreach (var span in spans)
            {
                _spans.Add(new SpanModel(span));
            }
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
                var resourceSpans = new ResourceSpanModel[] { new ResourceSpanModel(scopeSpans) };
                var serialiseablePayload = new TracePayloadBody()
                {
                    resourceSpans = resourceSpans
                };
                _jsonbody = JsonUtility.ToJson(serialiseablePayload);
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