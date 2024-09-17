using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using BugsnagNetworking;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class SpanFactory : IPhasedStartup
    {

        [ThreadStatic]
        private static Stack<WeakReference<ISpanContext>> _contextStack;

        private RNGCryptoServiceProvider _rNGCryptoServiceProvider = new RNGCryptoServiceProvider();

        private OnSpanEnd _onSpanEnd;

        private const string CONNECTION_TYPE_UNAVAILABLE = "unavailable";
        private const string CONNECTION_TYPE_CELL = "cell";
        private const string CONNECTION_TYPE_WIFI = "wifi";
    
        private string _currentConnectionType = CONNECTION_TYPE_UNAVAILABLE;

        private WaitForSeconds _connectionPollRate = new WaitForSeconds(1);

        private int _maxCustomAttributes = PerformanceConfiguration.DEFAULT_ATTRIBUTE_COUNT_LIMIT;

        public SpanFactory(OnSpanEnd onSpanEnd)
        {
            _onSpanEnd = onSpanEnd;
            MainThreadDispatchBehaviour.Instance().StartCoroutine(GetConnectionType());
        }

        public void Configure(PerformanceConfiguration config)
        {
            _maxCustomAttributes = config.AttributeCountLimit;
        }

        public void Start()
        {
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
            span.SetAttributeInternal("bugsnag.span.category", "custom");
            return span;
        }

        private Span CreateSpan(string name, SpanKind kind, SpanOptions spanOptions)
        {
            string parentSpanId = null;
            string traceId;
            string spanId = GetNewSpanId();

            if (spanOptions.ParentContext != null)
            {
                traceId = spanOptions.ParentContext.TraceId;
                parentSpanId = spanOptions.ParentContext.SpanId;
            }
            else
            {
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
            }

            var newSpan = new Span(name, kind, spanId, traceId, parentSpanId, spanOptions.StartTime, spanOptions.IsFirstClass, _onSpanEnd, _maxCustomAttributes);
            if (spanOptions.MakeCurrentContext)
            {
                AddToContextStack(newSpan);
            }
            newSpan.SetAttributeInternal("net.host.connection.type", _currentConnectionType);
            return newSpan;
        }

        internal Span CreateAutomaticNetworkSpan(BugsnagUnityWebRequest request, string url)
        {
            var verb = request.method.ToUpper();

            // as most code is running on the same thread and we want to avoid any spans
            // starting while the network call is inflight becoming children of the network span.
            var spanOptions = new SpanOptions { MakeCurrentContext = false };

            var span = CreateSpan("HTTP/" + verb, SpanKind.SPAN_KIND_CLIENT, spanOptions);
            span.SetAttributeInternal("bugsnag.span.category", "network");
            span.SetAttributeInternal("http.url", url);
            span.SetAttributeInternal("http.method", verb);
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
            span.SetAttributeInternal("bugsnag.span.category", "network");
            span.SetAttributeInternal("http.url", url);
            span.SetAttributeInternal("http.method", httpVerb.ToString());
            return span;
        }

        private IEnumerator GetConnectionType()
        {
            while(true)
            {
                switch (Application.internetReachability)
                {
                    case NetworkReachability.NotReachable:
                        _currentConnectionType = CONNECTION_TYPE_UNAVAILABLE;
                        break;
                    case NetworkReachability.ReachableViaCarrierDataNetwork:
                        _currentConnectionType = CONNECTION_TYPE_CELL;
                        break;
                    case NetworkReachability.ReachableViaLocalAreaNetwork:
                        _currentConnectionType = CONNECTION_TYPE_WIFI;
                        break;

                }
                yield return _connectionPollRate;
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
            span.SetAttributeInternal("bugsnag.span.category", "view_load");
            span.SetAttributeInternal("bugsnag.view.type", "UnityScene");
            span.SetAttributeInternal("bugsnag.view.name", sceneName);   
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
                var top = _contextStack.Peek();
                if(top == null)
                {
                    _contextStack.Pop();
                    continue;
                }
                if (top.TryGetTarget(out var spanContext))
                {
                    if (((Span)spanContext).Ended)
                    {
                        _contextStack.Pop();
                    }
                    else
                    {
                        return spanContext;
                    }
                }
                else
                {
                    _contextStack.Pop();
                    continue;
                }
            }

            return null;
        }


        private void AddToContextStack(ISpanContext spanContext)
        {
            if (_contextStack == null)
            {
                _contextStack = new Stack<WeakReference<ISpanContext>>();
            }
            _contextStack.Push(new WeakReference<ISpanContext>(spanContext));
        }

        internal Span CreateAutoAppStartSpan(string name, string category)
        {
            var span = CreateSpan(name, SpanKind.SPAN_KIND_CLIENT, new SpanOptions());
            span.SetAttributeInternal("bugsnag.span.category", category);
            span.SetAttributeInternal("bugsnag.app_start.type", "UnityRuntime");
            span.IsAppStartSpan = true;
            return span;
        }


    }
}
