using System;
using BugsnagNetworking;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class SpanFactory
    {

        private static string GetNewTraceId()
        {
            return Guid.NewGuid().ToString();
        }

        private static string GetNewSpanId()
        {
            var newId = Guid.NewGuid().ToString().Replace("-",string.Empty);
            return newId.Substring(0,16);
        }

        internal static Span StartCustomSpan(string name, DateTimeOffset startTime)
        {
            return CreateSpan(name,SpanKind.SPAN_KIND_INTERNAL, startTime);
        }

        private static Span CreateSpan(string name, SpanKind kind, DateTimeOffset startTime)
        {
            return new Span(name, kind, GetNewSpanId(), GetNewTraceId(), startTime);
        }

        internal static Span CreateNetworkSpan(BugsnagUnityWebRequest request)
        {
            var verb = request.method.ToUpper();
            var span = CreateSpan("HTTP/" + verb, SpanKind.SPAN_KIND_CLIENT, DateTimeOffset.Now);
            span.SetAttribute("bugsnag.span_category", "network");
            span.SetAttribute("http.url", request.url);
            span.SetAttribute("http.method", verb);
            span.SetAttribute("net.host.connection.type", GetConnectionType());
            return span;
        }


        private static string GetConnectionType()
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

        internal static Span CreateSceneLoadSpan()
        {
            var span = CreateSpan(string.Empty, SpanKind.SPAN_KIND_INTERNAL, DateTimeOffset.Now);
            return span;
        }
    }
}
