using System;
using System.Collections.Generic;
using BugsnagNetworking;

namespace BugsnagUnityPerformance
{
    public delegate void OnSpanEnd(Span span);

    public class Span : ISpanContext
    {
        public string Name { get; internal set; }
        public SpanKind Kind { get; }
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
        internal bool WasAborted;
        private bool _callbackComplete;
        private Dictionary<string, object> _attributes = new Dictionary<string, object>();

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
                AddAttributeInternal("bugsnag.span.first_class", isFirstClass.Value);
            }
            _onSpanEnd = onSpanEnd;
        }

        public void End(DateTimeOffset? endTime = null)
        {
            lock (_endLock)
            {
                if (Ended)
                {
                    return;
                }
                Ended = true;
            }
            EndTime = endTime == null ? DateTimeOffset.UtcNow : endTime.Value;
            _onSpanEnd(this);
        }

        internal void Abort()
        {
            lock (_endLock)
            {
                WasAborted = true;
                if (Ended)
                {
                    return;
                }
                Ended = true;
                _onSpanEnd(this);
            }
        }

        internal void EndNetworkSpan(BugsnagUnityWebRequest request)
        {
            lock (_endLock)
            {
                if (Ended)
                {
                    return;
                }
                Ended = true;
            }

            EndTime = DateTimeOffset.UtcNow;

            AddAttributeInternal("http.status_code", (int)request.responseCode);

            if (request.uploadHandler != null && request.uploadHandler.data != null)
            {
                AddAttributeInternal("http.request_content_length", request.uploadHandler.data.Length);
            }

            if (request.downloadHandler != null && request.downloadHandler.data != null)
            {
                AddAttributeInternal("http.response_content_length", request.downloadHandler.data.Length);
            }
            _onSpanEnd(this);
        }

        public void EndNetworkSpan(int statusCode = -1, int requestContentLength = -1, int responseContentLength = -1, DateTimeOffset? endTime = null)
        {
            lock (_endLock)
            {
                if (Ended)
                {
                    return;
                }
                Ended = true;
            }

            EndTime = endTime == null ? DateTimeOffset.UtcNow : endTime.Value;

            if (statusCode > -1)
            {
                AddAttributeInternal("http.status_code", statusCode);
            }

            if (requestContentLength > -1)
            {
                AddAttributeInternal("http.request_content_length", requestContentLength);
            }

            if (responseContentLength > -1)
            {
                AddAttributeInternal("http.response_content_length", responseContentLength);
            }
            _onSpanEnd(this);
        }

        internal void EndSceneLoadSpan(string sceneName)
        {
            // no need for thread safe checks as all scene load events happen on the main thread.
            Ended = true;
            EndTime = DateTimeOffset.UtcNow;
            Name = "[ViewLoad/UnityScene]" + sceneName;
            AddAttributeInternal("bugsnag.span.category", "view_load");
            AddAttributeInternal("bugsnag.view.type", "UnityScene");
            AddAttributeInternal("bugsnag.view.name", sceneName);
            _onSpanEnd(this);
        }

        public void UpdateSamplingProbability(double value)
        {
            if (samplingProbability > value)
            {
                samplingProbability = value;
            }
        }


        internal void AddAttributeInternal(string key, int value) => AddAttributeWithoutChecks(key, value);
        internal void AddAttributeInternal(string key, string value) => AddAttributeWithoutChecks(key, value);
        internal void AddAttributeInternal(string key, double value) => AddAttributeWithoutChecks(key, value);
        internal void AddAttributeInternal(string key, bool value) => AddAttributeWithoutChecks(key, value);
        private void AddAttributeWithoutChecks(string key, object value)
        {
            _attributes[key] = value;
        }


        public void AddAttribute(string key, int value) => AddAttributeWithChecks(key, value);
        public void AddAttribute(string key, string value) => AddAttributeWithChecks(key, value);
        public void AddAttribute(string key, double value) => AddAttributeWithChecks(key, value);
        public void AddAttribute(string key, bool value) => AddAttributeWithChecks(key, value);
        public void AddAttribute(string key, string[] value) => AddAttributeWithChecks(key, value);
        public void AddAttribute(string key, int[] value) => AddAttributeWithChecks(key, value);
        public void AddAttribute(string key, bool[] value) => AddAttributeWithChecks(key, value);
        public void AddAttribute(string key, double[] value) => AddAttributeWithChecks(key, value);

        private void AddAttributeWithChecks(string key, object value)
        {
            if (_callbackComplete)
            {
                return;
            }
            _attributes[key] = value;
        }

        internal Dictionary<string, object> GetAttributes() => new Dictionary<string, object>(_attributes);

        internal void SetCallbackComplete()
        {
            _callbackComplete = true;
        }
    }
}