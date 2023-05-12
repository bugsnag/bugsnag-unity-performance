using System;
using System.Security.Cryptography;
using BugsnagNetworking;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class SpanFactory
    {

        private static RNGCryptoServiceProvider _rNGCryptoServiceProvider = new RNGCryptoServiceProvider();

        private static string GetNewTraceId()
        {
            byte[] byteArray = new byte[16];
            _rNGCryptoServiceProvider.GetBytes(byteArray);
            return ByteArrayToHex(byteArray);
        }

        private static string GetNewSpanId()
        {
            byte[] byteArray = new byte[8];
            _rNGCryptoServiceProvider.GetBytes(byteArray);
            return ByteArrayToHex(byteArray);
        }

        private static string ByteArrayToHex(byte[] barray)
        {
            char[] c = new char[barray.Length * 2];
            byte b;
            for (int i = 0; i < barray.Length; ++i)
            {
                b = ((byte)(barray[i] >> 4));
                c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);
                b = ((byte)(barray[i] & 0xF));
                c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }
            return new string(c);
        }

        internal static Span StartCustomSpan(string name, SpanOptions spanOptions)
        {
            // custom spans are always first class
            spanOptions.IsFirstClass = true;
            return CreateSpan(name,SpanKind.SPAN_KIND_INTERNAL, spanOptions);
        }

        private static Span CreateSpan(string name, SpanKind kind, SpanOptions spanOptions)
        {
                        
            if (spanOptions.ParentContext != null)
            {
                //TODO if not already in stack, add provided parent span to the context stack 
            }

            // TODO check for existing context
            // if context exists, use the trace id from the existing context
            // also use the current context spanid as the ParentSpanId

            string spanId = GetNewSpanId();
            string parentSpanId = string.Empty;

            // TODO if no context exists then create a new trace id
            string traceId = GetNewTraceId();


            var newSpan = new Span(name, kind, spanId, traceId, parentSpanId, spanOptions.StartTime, spanOptions.IsFirstClass);

            if (spanOptions.MakeCurrentContext)
            {
                //TODO add new span to the context stack
            }

            return newSpan;
        }

        internal static Span CreateAutomaticNetworkSpan(BugsnagUnityWebRequest request)
        {
            var verb = request.method.ToUpper();

            // as most code is running on the same thread and we want to avoid any spans
            // starting while the network call is inflight becoming children of the network span.
            var spanOptions = new SpanOptions { MakeCurrentContext = false };

            var span = CreateSpan("HTTP/" + verb, SpanKind.SPAN_KIND_CLIENT, spanOptions);
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

        internal static Span CreateAutomaticSceneLoadSpan()
        {
            // Scene load spans are always first class
            var spanOptions = new SpanOptions { IsFirstClass = true };
            var span = CreateSpan(string.Empty, SpanKind.SPAN_KIND_INTERNAL, spanOptions);
            return span;
        }
    }
}
