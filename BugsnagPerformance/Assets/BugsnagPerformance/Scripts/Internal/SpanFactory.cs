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
