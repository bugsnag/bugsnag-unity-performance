using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace BugsnagUnityPerformance
{
    internal class TracePayload
    {

        public Dictionary<string, string> Headers;

        private Span _span;

        public TracePayload(Span span, PerformanceConfiguration performanceConfiguration)
        {
            Headers = new Dictionary<string, string>()
            {
                { "Bugsnag-Api-Key", performanceConfiguration.ApiKey},
                { "Content-Type" , "application/json" }
            };
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

            Debug.Log("Get Body: " + json);

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