using System;
using System.Text;
using UnityEngine;
namespace BugsnagUnityPerformance
{
    internal class TracePayload
    {
        private Span _span;

        public TracePayload(Span span)
        {
            _span = span;
        }

        public byte[] GetBody()
        {
            var spans = new SpanModel[] { new SpanModel(_span) };
            var scopeSpans = new ScopeSpanModel[] { new ScopeSpanModel(spans) };
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