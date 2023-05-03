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

        internal static void ReportNetworkSpan(BugsnagUnityWebRequest request, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var verb = request.method.ToUpper();
            var span = CreateSpan("HTTP/" + verb, SpanKind.SPAN_KIND_CLIENT, startTime);
            span.SetAttribute("bugsnag.span_category", "network");
            span.SetAttribute("http.url", request.url);
            span.SetAttribute("http.method", verb);
            span.SetAttribute("net.host.connection.type", GetConnectionType());
            span.EndNetworkSpan(request, endTime);
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

        internal static void ReportSceneLoadSpan(string sceneName, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var span = CreateSpan("[ViwLoad/Scene]" + sceneName, SpanKind.SPAN_KIND_INTERNAL, startTime);
            span.SetAttribute("bugsnag.span_category", "view_load");
            span.SetAttribute("bugsnag.view.type", "scene");
            span.SetAttribute("bugsnag.view.name", "scene");
            span.EndSceneLoadSpan(endTime);
        }
    }
}
