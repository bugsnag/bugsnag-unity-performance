using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace BugsnagUnityPerformance
{
    internal class TracePayload
    {
        private List<SpanModel> _spans = new List<SpanModel>();

        public TracePayload(List<Span> spans)
        {
            foreach (var span in spans)
            {
                _spans.Add(new SpanModel(span));
            }
        }

        public byte[] GetBody()
        {
            var scopeSpans = new ScopeSpanModel[] { new ScopeSpanModel(_spans.ToArray()) };
            var resourceSpans = new ResourceSpanModel[] { new ResourceSpanModel(scopeSpans) };
            var serialiseablePayload = new TracePayloadBody()
            {
                resourceSpans = resourceSpans
            };
            var json = JsonUtility.ToJson(serialiseablePayload);
            var bytes = Encoding.ASCII.GetBytes(json);
            return bytes;
        }

    }

    [Serializable]
    internal class TracePayloadBody
    {
        public ResourceSpanModel[] resourceSpans;
    }
        
}