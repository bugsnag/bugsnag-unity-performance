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
        internal List<AttributeModel> Attributes = new List<AttributeModel>();
        internal double samplingProbability { get; private set; }
        internal bool Ended;
        private object _endLock = new object();
        private OnSpanEnd _onSpanEnd;
        internal bool IsAppStartSpan;
        internal bool WasAborted;
        private bool _processed;

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
                SetAttribute("bugsnag.span.first_class", isFirstClass.Value);
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
                if (Ended)
                {
                    return;
                }
                WasAborted = true;
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

            SetAttribute("http.status_code", (int)request.responseCode);

            if (request.uploadHandler != null && request.uploadHandler.data != null)
            {
                SetAttribute("http.request_content_length", request.uploadHandler.data.Length);
            }

            if (request.downloadHandler != null && request.downloadHandler.data != null)
            {
                SetAttribute("http.response_content_length", request.downloadHandler.data.Length);
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
                SetAttribute("http.status_code", statusCode);
            }

            if (requestContentLength > -1)
            {
                SetAttribute("http.request_content_length", requestContentLength);
            }

            if (responseContentLength > -1)
            {
                SetAttribute("http.response_content_length", responseContentLength);
            }

            _onSpanEnd(this);
        }

        internal void EndSceneLoadSpan(string sceneName)
        {
            // no need for thread safe checks as all scene load events happen on the main thread.
            Ended = true;
            EndTime = DateTimeOffset.UtcNow;
            Name = "[ViewLoad/UnityScene]" + sceneName;
            SetAttribute("bugsnag.span.category", "view_load");
            SetAttribute("bugsnag.view.type", "UnityScene");
            SetAttribute("bugsnag.view.name", sceneName);
            _onSpanEnd(this);
        }

        public void UpdateSamplingProbability(double value)
        {
            if (samplingProbability > value)
            {
                samplingProbability = value;
            }
        }

        public void AddAttribute(string key, string value)
        {
            if (_processed)
            {
                return;
            }
            SetAttribute(key, value);
        }

        public void AddAttribute(string key, int value)
        {
            if (_processed)
            {
                return;
            }
            SetAttribute(key, value);
        }

        public void AddAttribute(string key, bool value)
        {
            if (_processed)
            {
                return;
            }
            SetAttribute(key, value);
        }

        public void AddAttribute(string key, double value)
        {
            if (_processed)
            {
                return;
            }
            SetAttribute(key, value);
        }

        public void AddAttribute(string key, string[] value)
        {
            if (_processed)
            {
                return;
            }
            SetAttribute(key, value);
        }

        public void AddAttribute(string key, int[] value)
        {
            if (_processed)
            {
                return;
            }
            SetAttribute(key, value);
        }

        public void AddAttribute(string key, bool[] value)
        {
            if (_processed)
            {
                return;
            }
            SetAttribute(key, value);
        }

        public void AddAttribute(string key, double[] value)
        {
            if (_processed)
            {
                return;
            }
            SetAttribute(key, value);
        }

        internal void SetAttribute(string key, string value)
        {
            var existing = Attributes.Find(a => a.key == key);
            if (existing != null)
            {
                Attributes.Remove(existing);
            }
            Attributes.Add(new AttributeModel(key, value));
        }

        internal void SetAttribute(string key, int value)
        {
            var existing = Attributes.Find(a => a.key == key);
            if (existing != null)
            {
                Attributes.Remove(existing);
            }
            Attributes.Add(new AttributeModel(key, value));
        }

        internal void SetAttribute(string key, bool value)
        {
            var existing = Attributes.Find(a => a.key == key);
            if (existing != null)
            {
                Attributes.Remove(existing);
            }
            Attributes.Add(new AttributeModel(key, value));
        }

        internal void SetAttribute(string key, double value)
        {
            var existing = Attributes.Find(a => a.key == key);
            if (existing != null)
            {
                Attributes.Remove(existing);
            }
            Attributes.Add(new AttributeModel(key, value));
        }

        internal void SetAttribute(string key, string[] value)
        {
            var existing = Attributes.Find(a => a.key == key);
            if (existing != null)
            {
                Attributes.Remove(existing);
            }
            Attributes.Add(new AttributeModel(key, value));
        }

        internal void SetAttribute(string key, int[] value)
        {
            var existing = Attributes.Find(a => a.key == key);
            if (existing != null)
            {
                Attributes.Remove(existing);
            }
            Attributes.Add(new AttributeModel(key, value));
        }

        internal void SetAttribute(string key, bool[] value)
        {
            var existing = Attributes.Find(a => a.key == key);
            if (existing != null)
            {
                Attributes.Remove(existing);
            }
            Attributes.Add(new AttributeModel(key, value));
        }

        internal void SetAttribute(string key, double[] value)
        {
            var existing = Attributes.Find(a => a.key == key);
            if (existing != null)
            {
                Attributes.Remove(existing);
            }
            Attributes.Add(new AttributeModel(key, value));
        }

    }
}
