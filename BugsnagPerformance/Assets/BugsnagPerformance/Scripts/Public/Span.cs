using System;
using System.Collections.Generic;
using BugsnagNetworking;

namespace BugsnagUnityPerformance
{
    public delegate void OnSpanEnd(Span span);

    public class Span : ISpanContext
    {
        public string Name { get; internal set; }
        internal SpanKind Kind { get; }
        public string SpanId { get; }
        public string TraceId { get; }
        internal string ParentSpanId { get; }
        public DateTimeOffset StartTime { get; }
        public DateTimeOffset EndTime { get; internal set; }
        internal double samplingProbability { get; private set; }
        internal bool Ended;
        private object _endLock = new object();
        private OnSpanEnd _onSpanEnd;
        internal bool IsAppStartSpan;
        internal bool WasDiscarded;
        private bool _callbackComplete;
        private Dictionary<string, object> _attributes = new Dictionary<string, object>();
        internal int DroppedAttributesCount;
        private int _customAttributeCount;

        public Span(string name, SpanKind kind, string id, string traceId, string parentSpanId, DateTimeOffset startTime, bool? isFirstClass, OnSpanEnd onSpanEnd)
        {
            Name = name;
            Kind = kind;
            SpanId = id;
            TraceId = traceId;
            StartTime = startTime;
            ParentSpanId = parentSpanId;
            samplingProbability = 1;
            if (isFirstClass != null)
            {
                SetAttributeInternal("bugsnag.span.first_class", isFirstClass.Value);
            }
            _onSpanEnd = onSpanEnd;
        }

        void LogSpanEndingWarning()
        {
            MainThreadDispatchBehaviour.Instance().LogWarning($"Attempting to call End on span: {Name} after the span has already ended.");
        }
        
        public void End(DateTimeOffset? endTime = null)
        {
            lock (_endLock)
            {
                if (Ended)
                {
                    LogSpanEndingWarning();
                    return;
                }
                Ended = true;
            }
            EndTime = endTime == null ? DateTimeOffset.UtcNow : endTime.Value;
            _onSpanEnd(this);
        }

        internal void Discard()
        {
            lock (_endLock)
            {
                WasDiscarded = true;
                Ended = true;
            }
        }

        internal void EndNetworkSpan(BugsnagUnityWebRequest request)
        {
            lock (_endLock)
            {
                if (Ended)
                {
                    LogSpanEndingWarning();
                    return;
                }
                Ended = true;
            }

            EndTime = DateTimeOffset.UtcNow;

            SetAttributeInternal("http.status_code", request.responseCode);

            if (request.uploadHandler != null && request.uploadHandler.data != null)
            {
                SetAttributeInternal("http.request_content_length", request.uploadHandler.data.Length);
            }

            if (request.downloadHandler != null && request.downloadHandler.data != null)
            {
                SetAttributeInternal("http.response_content_length", request.downloadHandler.data.Length);
            }
            _onSpanEnd(this);
        }

        public void EndNetworkSpan(int statusCode = -1, int requestContentLength = -1, int responseContentLength = -1, DateTimeOffset? endTime = null)
        {
            lock (_endLock)
            {
                if (Ended)
                {
                    LogSpanEndingWarning();
                    return;
                }
                Ended = true;
            }

            EndTime = endTime == null ? DateTimeOffset.UtcNow : endTime.Value;

            if (statusCode > -1)
            {
                SetAttributeInternal("http.status_code", statusCode);
            }

            if (requestContentLength > -1)
            {
                SetAttributeInternal("http.request_content_length", requestContentLength);
            }

            if (responseContentLength > -1)
            {
                SetAttributeInternal("http.response_content_length", responseContentLength);
            }
            _onSpanEnd(this);
        }

        internal void EndSceneLoadSpan(string sceneName)
        {
            // no need for thread safe checks as all scene load events happen on the main thread.
            Ended = true;
            EndTime = DateTimeOffset.UtcNow;
            Name = "[ViewLoad/UnityScene]" + sceneName;
            SetAttributeInternal("bugsnag.span.category", "view_load");
            SetAttributeInternal("bugsnag.view.type", "UnityScene");
            SetAttributeInternal("bugsnag.view.name", sceneName);
            _onSpanEnd(this);
        }

        public void UpdateSamplingProbability(double value)
        {
            if (samplingProbability > value)
            {
                samplingProbability = value;
            }
        }


        internal void SetAttributeInternal(string key, long value) => SetAttributeWithoutChecks(key, value);
        internal void SetAttributeInternal(string key, string value) => SetAttributeWithoutChecks(key, value);
        internal void SetAttributeInternal(string key, double value) => SetAttributeWithoutChecks(key, value);
        internal void SetAttributeInternal(string key, bool value) => SetAttributeWithoutChecks(key, value);
        private void SetAttributeWithoutChecks(string key, object value)
        {
            if (value == null)
            {
                _attributes.Remove(key);
                return;
            }
            _attributes[key] = value;
        }


        public void SetAttribute(string key, long value) => SetAttributeWithChecks(key, value);
        public void SetAttribute(string key, string value) => SetAttributeWithChecks(key, value);
        public void SetAttribute(string key, double value) => SetAttributeWithChecks(key, value);
        public void SetAttribute(string key, bool value) => SetAttributeWithChecks(key, value);
        public void SetAttribute(string key, string[] value) => SetAttributeWithChecks(key, value);
        public void SetAttribute(string key, long[] value) => SetAttributeWithChecks(key, value);
        public void SetAttribute(string key, bool[] value) => SetAttributeWithChecks(key, value);
        public void SetAttribute(string key, double[] value) => SetAttributeWithChecks(key, value);

        private void SetAttributeWithChecks(string key, object value)
        {
            if (_callbackComplete)
            {
                MainThreadDispatchBehaviour.Instance().LogWarning($"Attempting to set attribute: {key} on span: {Name} after the span has ended.");
                return;
            }

            var keyExists = _attributes.ContainsKey(key);

            if (value == null)
            {
                if (keyExists)
                {
                    _attributes.Remove(key);
                    _customAttributeCount--;
                }
                return;
            }
            if(_customAttributeCount >= 64)
            {
                DroppedAttributesCount++;
                return;
            }
            _attributes[key] = value;
            if (!keyExists)
            {
                _customAttributeCount++;
            }
        }

        internal Dictionary<string, object> GetAttributes() => new Dictionary<string, object>(_attributes);

        internal void SetCallbackComplete()
        {
            _callbackComplete = true;
        }
    }
}