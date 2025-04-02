using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BugsnagNetworking;

namespace BugsnagUnityPerformance
{
    public delegate void OnSpanEnd(Span span);

    public class Span : ISpanContext
    {

        private const string FROZEN_FRAMES_KEY = "bugsnag.rendering.frozen_frames";
        private const string SLOW_FRAMES_KEY = "bugsnag.rendering.slow_frames";
        private const string TOTAL_FRAMES_KEY = "bugsnag.rendering.total_frames";
        private const string FPS_MAX_KEY = "bugsnag.rendering.fps_maximum";
        private const string FPS_MIN_KEY = "bugsnag.rendering.fps_minimum";
        private const string FPS_AVERAGE_KEY = "bugsnag.rendering.fps_average";
        private const string FPS_TARGET_KEY = "bugsnag.rendering.fps_target";
        private const string CPU_MEASURES_TIMESTAMPS_KEY = "bugsnag.system.cpu_measures_timestamps";
        private const string MEMORY_TIMESTAMPS_KEY = "bugsnag.system.memory.timestamps";
        private const string CPU_MEASURES_TOTAL_KEY = "bugsnag.system.cpu_measures_total";
        private const string CPU_MEASURES_MAIN_THREAD_KEY = "bugsnag.system.cpu_measures_main_thread";
        private const string CPU_MEASURES_OVERHEAD_KEY = "bugsnag.system.cpu_measures_overhead";
        private const string CPU_MEAN_TOTAL_KEY = "bugsnag.metrics.cpu_mean_total";
        private const string CPU_MEAN_MAIN_THREAD_KEY = "bugsnag.system.cpu_mean_main_thread";
        private const string PHYSICAL_DEVICE_MEMORY_KEY = "bugsnag.device.physical_device_memory";
        private const string MEMORY_SPACES_SPACE_NAMES_KEY = "bugsnag.system.memory.spaces.space_names";
        private const string MEMORY_SPACES_DEVICE_SIZE_KEY = "bugsnag.system.memory.spaces.device.size";
        private const string MEMORY_SPACES_DEVICE_USED_KEY = "bugsnag.system.memory.spaces.device.used";
        private const string MEMORY_SPACES_DEVICE_MEAN_KEY = "bugsnag.system.memory.spaces.device.mean";

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
        internal bool IsFrozenFrameSpan;

        public Span(string name, SpanKind kind, string id,
        string traceId, string parentSpanId, DateTimeOffset startTime,
        bool? isFirstClass, OnSpanEnd onSpanEnd, int maxCustomAttributes)
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
            _onSpanEnd = onSpanEnd;
        }

        void LogSpanEndingWarning()
        {
            MainThreadDispatchBehaviour.LogWarning($"Attempting to call End on span: {Name} after the span has already ended.");
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
                MainThreadDispatchBehaviour.LogWarning($"Attempting to set attribute: {key} on span: {Name} after the span has ended.");
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

        internal void CalculateFrameRateMetrics(SpanRenderingMetrics metrics)
        {
            var beginningMetrics = metrics.BeginningMetrics;
            var endMetrics = metrics.EndingMetrics;
            if (beginningMetrics == null || endMetrics == null)
            {
                return;
            }
            var numFrozenFrames = endMetrics.FrozenFrames - beginningMetrics.FrozenFrames;
            var frozenFrameDurations = endMetrics.FrozenFrameBuffer.GetLastFrames(numFrozenFrames);
            for (int i = 0; i < endMetrics.FrozenFrames; i++)
            {
                if (i >= frozenFrameDurations.Count)
                {
                    break;
                }
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

            var totalFrames = endMetrics.TotalFrames - beginningMetrics.TotalFrames;
            SetAttributeInternal(TOTAL_FRAMES_KEY, totalFrames);

            if (totalFrames == 0)
            {
                return;
            }
            var sumFrametime = endMetrics.FrameTimeSum - beginningMetrics.FrameTimeSum;
            var averageFps = (int)(1.0f / (sumFrametime / totalFrames));

            SetAttributeInternal(FPS_AVERAGE_KEY, averageFps);
            SetAttributeInternal(FROZEN_FRAMES_KEY, numFrozenFrames);
            SetAttributeInternal(SLOW_FRAMES_KEY, endMetrics.SlowFrames - beginningMetrics.SlowFrames);
            SetAttributeInternal(FPS_MAX_KEY, metrics.MaxFrameRate);
            SetAttributeInternal(FPS_MIN_KEY, metrics.MinFrameRate);
            SetAttributeInternal(FPS_TARGET_KEY, beginningMetrics.TargetFrameRate);
        }

        internal void RemoveFrameRateMetrics()
        {
            _attributes.Remove(FROZEN_FRAMES_KEY);
            _attributes.Remove(SLOW_FRAMES_KEY);
            _attributes.Remove(TOTAL_FRAMES_KEY);
            _attributes.Remove(FPS_MAX_KEY);
            _attributes.Remove(FPS_MIN_KEY);
            _attributes.Remove(FPS_AVERAGE_KEY);
            _attributes.Remove(FPS_TARGET_KEY);
        }

        internal void CalculateCPUMetrics(List<SystemMetricsSnapshot> snapshots)
        {
            // Timestamps
            var timestamps = snapshots.Select(s => s.Timestamp).ToArray();
            SetAttribute(CPU_MEASURES_TIMESTAMPS_KEY, timestamps);
            // CPU
            var processCpu = snapshots.Select(s => s.ProcessCPUPercent).ToArray();
            var mainThreadCpu = snapshots.Select(s => s.MainThreadCPUPercent).ToArray();
            var monitorThreadCpu = snapshots.Select(s => s.MonitorThreadCPUPercent).ToArray();

            SetAttribute(CPU_MEASURES_TOTAL_KEY, processCpu);
            SetAttribute(CPU_MEASURES_MAIN_THREAD_KEY, mainThreadCpu);
            SetAttribute(CPU_MEASURES_OVERHEAD_KEY, monitorThreadCpu);
            SetAttribute(CPU_MEAN_TOTAL_KEY, processCpu.Average());
            SetAttribute(CPU_MEAN_MAIN_THREAD_KEY, mainThreadCpu.Average());
        }
        internal void CalculateMemoryMetrics(List<SystemMetricsSnapshot> snapshots)
        {
            // Timestamps
            var timestamps = snapshots.Select(s => s.Timestamp).ToArray();
            SetAttribute(MEMORY_TIMESTAMPS_KEY, timestamps);

            // Memory
            // Android Java Heap
            if (snapshots.Any(s => s.TotalMemory.HasValue))
            {
                var totalDeviceMemory = snapshots.FirstOrDefault(s => s.TotalMemory.HasValue).TotalMemory ?? 0;
                SetAttribute(PHYSICAL_DEVICE_MEMORY_KEY, totalDeviceMemory);

                var used = snapshots.Select(s => (s.TotalMemory ?? 0) - (s.FreeMemory ?? 0)).ToArray();
                var size = snapshots.Max(s => s.TotalMemory ?? 0);

                SetAttribute(MEMORY_SPACES_SPACE_NAMES_KEY, new[] { "device" });
                SetAttribute(MEMORY_SPACES_DEVICE_SIZE_KEY, size);
                SetAttribute(MEMORY_SPACES_DEVICE_USED_KEY, used);
                SetAttribute(MEMORY_SPACES_DEVICE_MEAN_KEY, (long)used.Average());
            }

            // iOS Physical Memory
            if (snapshots.Any(s => s.PhysicalMemoryInUse.HasValue))
            {
                var physicalUsed = snapshots.Select(s => s.PhysicalMemoryInUse ?? 0).ToArray();
                var physicalTotal = snapshots.FirstOrDefault(s => s.TotalDeviceMemory.HasValue).TotalDeviceMemory ?? 0;

                SetAttribute(PHYSICAL_DEVICE_MEMORY_KEY, physicalTotal);

                SetAttribute(MEMORY_SPACES_SPACE_NAMES_KEY, new[] { "device" });
                SetAttribute(MEMORY_SPACES_DEVICE_SIZE_KEY, physicalTotal);
                SetAttribute(MEMORY_SPACES_DEVICE_USED_KEY, physicalUsed);
                SetAttribute(MEMORY_SPACES_DEVICE_MEAN_KEY, (long)physicalUsed.Average());
            }
        }

        internal void RemoveSystemCPUMetrics()
        {
            _attributes.Remove(CPU_MEASURES_TIMESTAMPS_KEY);
            _attributes.Remove(CPU_MEASURES_TOTAL_KEY);
            _attributes.Remove(CPU_MEASURES_MAIN_THREAD_KEY);
            _attributes.Remove(CPU_MEASURES_OVERHEAD_KEY);
            _attributes.Remove(CPU_MEAN_TOTAL_KEY);
            _attributes.Remove(CPU_MEAN_MAIN_THREAD_KEY);
        }

        internal void RemoveSystemMemoryMetrics()
        {
            _attributes.Remove(MEMORY_TIMESTAMPS_KEY);
            _attributes.Remove(MEMORY_SPACES_SPACE_NAMES_KEY);
            _attributes.Remove(MEMORY_SPACES_DEVICE_SIZE_KEY);
            _attributes.Remove(MEMORY_SPACES_DEVICE_USED_KEY);
            _attributes.Remove(MEMORY_SPACES_DEVICE_MEAN_KEY);
        }
    }
}