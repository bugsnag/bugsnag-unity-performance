using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using BugsnagNetworking;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class SpanFactory
    {

        [ThreadStatic]
        private static Stack<ISpanContext> _contextStack;

        private RNGCryptoServiceProvider _rNGCryptoServiceProvider = new RNGCryptoServiceProvider();

        private OnSpanEnd _onSpanEnd;

        public SpanFactory(OnSpanEnd onSpanEnd)
        {
            _onSpanEnd = onSpanEnd;
        }

        private string GetNewTraceId()
        {
            byte[] byteArray = new byte[16];
            _rNGCryptoServiceProvider.GetBytes(byteArray);
            return ByteArrayToHex(byteArray);
        }

        private string GetNewSpanId()
        {
            byte[] byteArray = new byte[8];
            _rNGCryptoServiceProvider.GetBytes(byteArray);
            return ByteArrayToHex(byteArray);
        }

        private string ByteArrayToHex(byte[] barray)
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

        internal Span StartCustomSpan(string name, SpanOptions spanOptions)
        {
            if (spanOptions.IsFirstClass == null)
            {
                spanOptions.IsFirstClass = true;
            }
            var span = CreateSpan(name, SpanKind.SPAN_KIND_INTERNAL, spanOptions);
            span.SetAttribute("bugsnag.span.category", "custom");
            return span;
        }

        private Span CreateSpan(string name, SpanKind kind, SpanOptions spanOptions)
        {
                        
            if (spanOptions.ParentContext != null)
            {
                AddToContextStack(spanOptions.ParentContext);
            }

            string traceId = string.Empty;
            string parentSpanId = null;
            string spanId = GetNewSpanId();

            var existingContext = GetCurrentContext();
            if (existingContext != null)
            {
                traceId = existingContext.TraceId;
                parentSpanId = existingContext.SpanId;
            }
            else
            {
                traceId = GetNewTraceId();
            }

            var newSpan = new Span(name, kind, spanId, traceId, parentSpanId, spanOptions.StartTime, spanOptions.IsFirstClass, _onSpanEnd);

            if (spanOptions.MakeCurrentContext)
            {
                AddToContextStack(newSpan);
            }

            return newSpan;
        }

        internal Span CreateAutomaticNetworkSpan(BugsnagUnityWebRequest request,string url)
        {
            var verb = request.method.ToUpper();

            // as most code is running on the same thread and we want to avoid any spans
            // starting while the network call is inflight becoming children of the network span.
            var spanOptions = new SpanOptions { MakeCurrentContext = false };

            var span = CreateSpan("HTTP/" + verb, SpanKind.SPAN_KIND_CLIENT, spanOptions);
            span.SetAttribute("bugsnag.span.category", "network");
            span.SetAttribute("http.url", url);
            span.SetAttribute("http.method", verb);
            span.SetAttribute("net.host.connection.type", GetConnectionType());
            return span;
        }

        internal Span CreateManualNetworkSpan(string url, HttpVerb httpVerb, SpanOptions spanOptions)
        {
            // as most code is running on the same thread and we want to avoid any spans
            // starting while the network call is inflight becoming children of the network span.
            SpanOptions options;
            if (spanOptions != null)
            {
                options = spanOptions;
            }
            else
            {
                options = new SpanOptions { MakeCurrentContext = false };
            }
            var span = CreateSpan("HTTP/" + httpVerb, SpanKind.SPAN_KIND_CLIENT, options);
            span.SetAttribute("bugsnag.span.category", "network");
            span.SetAttribute("http.url", url);
            span.SetAttribute("http.method", httpVerb.ToString());
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

        internal Span CreateAutomaticSceneLoadSpan()
        {
            // Scene load spans are always first class
            var spanOptions = new SpanOptions { IsFirstClass = true };
            var span = CreateSpan(string.Empty, SpanKind.SPAN_KIND_INTERNAL, spanOptions);
            return span;
        }

        internal Span CreateManualSceneLoadSpan(string sceneName, SpanOptions spanOptions)
        {
            SpanOptions options;
            if (spanOptions != null)
            {
                options = spanOptions;
            }
            else
            {
                options = new SpanOptions { IsFirstClass = true };
            }
            var span = CreateSpan("[ViewLoad/UnityScene]" + sceneName, SpanKind.SPAN_KIND_INTERNAL, options);
            span.SetAttribute("bugsnag.span.category", "view_load");
            span.SetAttribute("bugsnag.view.type", "UnityScene");
            span.SetAttribute("bugsnag.view.name", sceneName);            
            return span;
        }

        internal ISpanContext GetCurrentContext()
        {
            if (_contextStack == null || _contextStack.Count == 0)
            {
                return null;
            }

            while (_contextStack.Count > 0)
            {
                var top = (Span)_contextStack.Peek();
                if (top.Ended)
                {
                    _contextStack.Pop();
                }
                else
                {
                    return top;
                }
            }

            return null;
        }

        private void AddToContextStack(ISpanContext spanContext)
        {
            if (_contextStack == null)
            {
                _contextStack = new Stack<ISpanContext>();
            }
            _contextStack.Push(spanContext);
        }

        internal Span CreateAutoAppStartSpan(string name, string category)
        {
            var span = CreateSpan(name, SpanKind.SPAN_KIND_CLIENT,new SpanOptions());
            span.SetAttribute("bugsnag.span.category", category);
            span.SetAttribute("bugsnag.app_start.type", "UnityRuntime");
            span.IsAppStartSpan = true;
            return span;
        }
    }
}
