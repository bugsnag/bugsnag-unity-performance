using System;
using System.Collections.Generic;
using BugsnagNetworking;

namespace BugsnagUnityPerformance
{
    public delegate void OnSpanEnd(Span span);

    public class Span : ISpanContext
    {

        private const string FROZEN_FRAMES_KEY = "bugsnag.rendering.frozen_frames";
        private const string SLOW_FRAMES_KEY = "bugsnag.rendering.slow_frames";
        private const string TOTAL_FRAMES_KEY = "bugsnag.rendering.total_frames";

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
        private int _maxCustomAttributes;
        private FrameMetricsSnapshot _startFrameRateMetricsSnapshot;
        internal bool IsFrozenFrameSpan;

        public Span(string name, SpanKind kind, string id,
        string traceId, string parentSpanId, DateTimeOffset startTime,
        bool? isFirstClass, OnSpanEnd onSpanEnd, int maxCustomAttributes,
        FrameMetricsSnapshot startFrameRateMetricsSnapshot)
        {
            Name = name ?? string.Empty;
            Kind = kind;
            SpanId = id;
            TraceId = traceId;
            StartTime = startTime;
            ParentSpanId = parentSpanId;
            samplingProbability = 1;
            _maxCustomAttributes = maxCustomAttributes;
            if (isFirstClass != null)
            {
                SetAttributeInternal("bugsnag.span.first_class", isFirstClass.Value);
            }
            _startFrameRateMetricsSnapshot = startFrameRateMetricsSnapshot;
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

            if (_attributes.ContainsKey(key))
            {
                if (value == null)
                {
                    _attributes.Remove(key);
                    _customAttributeCount--;
                }
                else
                {
                    _attributes[key] = value;
                }
                return;
            }

            if (_customAttributeCount >= _maxCustomAttributes)
            {
                DroppedAttributesCount++;
                return;
            }
            _attributes[key] = value;
            _customAttributeCount++;
        }

        internal Dictionary<string, object> GetAttributes() => new Dictionary<string, object>(_attributes);

        internal void SetCallbackComplete()
        {
            _callbackComplete = true;
        }

        internal void CalculateFrameRateMetrics(FrameMetricsSnapshot endFrameRateMetricsSnapshot)
        {
            if (_startFrameRateMetricsSnapshot == null || endFrameRateMetricsSnapshot == null)
            {
                return;
            }

            var numFrozenFrames = endFrameRateMetricsSnapshot.FrozenFrames - _startFrameRateMetricsSnapshot.FrozenFrames;
            var startingIndex = endFrameRateMetricsSnapshot.FrozenFrameDurations.Count - numFrozenFrames;
            var frozenFrameDurations = endFrameRateMetricsSnapshot.FrozenFrameDurations.GetRange(startingIndex, numFrozenFrames);
            for (int i = 0; i < endFrameRateMetricsSnapshot.FrozenFrames; i++)
            {
                var frameTimes = frozenFrameDurations[i];
                var options = new SpanOptions
                {
                    ParentContext = this,
                    IsFirstClass = false,
                    MakeCurrentContext = false,
                    StartTime = frameTimes.StartTime
                };
                var span = BugsnagPerformance.StartSpan("FrozenFrame", options);
                span.IsFrozenFrameSpan = true;
                span.SetAttributeInternal("bugsnag.span.category", "frozen_frame");
                span.End(frameTimes.EndTime);
            }
            SetAttributeInternal(FROZEN_FRAMES_KEY, numFrozenFrames);
            SetAttributeInternal(SLOW_FRAMES_KEY, endFrameRateMetricsSnapshot.SlowFrames - _startFrameRateMetricsSnapshot.SlowFrames);
            SetAttributeInternal(TOTAL_FRAMES_KEY, endFrameRateMetricsSnapshot.TotalFrames - _startFrameRateMetricsSnapshot.TotalFrames);
        }

        internal void RemoveFrameRateMetrics()
        {
            _attributes.Remove(FROZEN_FRAMES_KEY);
            _attributes.Remove(SLOW_FRAMES_KEY);
            _attributes.Remove(TOTAL_FRAMES_KEY);
        }
    }
}