using System;
using BugsnagNetworking;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class SpanFactory
    {

        private Tracer _tracer;

        internal SpanFactory(Tracer tracer)
        {
            _tracer = tracer;
        }

        private string GetNewTraceId()
        {
            return Guid.NewGuid().ToString();
        }

        private string GetNewSpanId()
        {
            var newId = Guid.NewGuid().ToString().Replace("-",string.Empty);
            return newId.Substring(0,16);
        }

        internal Span StartCustomSpan(string name, DateTimeOffset startTime)
        {
            return CreateSpan(name,SpanKind.SPAN_KIND_INTERNAL, startTime);
        }

        private Span CreateSpan(string name, SpanKind kind, DateTimeOffset startTime)
        {
            return new Span(name, kind, GetNewSpanId(), GetNewTraceId(), startTime, _tracer);
        }

        internal Span CreateNetworkSpan(BugsnagUnityWebRequest request)
        {
            var verb = request.method.ToUpper();
            var span = CreateSpan("HTTP/" + verb, SpanKind.SPAN_KIND_CLIENT, DateTimeOffset.Now);
            span.SetAttribute("bugsnag.span_category", "network");
            span.SetAttribute("http.url", request.url);
            span.SetAttribute("http.method", verb);
            span.SetAttribute("net.host.connection.type", GetConnectionType());
            return span;
        }

        private string GetConnectionType()
        {
            switch (Application.internetReachability)
            {
                case NetworkReachability.NotReachable:
                    return "unavailable";
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    return "cell";
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    return "wifi";
                default:
                    return string.Empty;
            }
        }
    }
}
