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
        private const string CPU_MEAN_TOTAL_KEY = "bugsnag.metrics.cpu_mean_total";
        private const string CPU_MEAN_MAIN_THREAD_KEY = "bugsnag.system.cpu_mean_main_thread";
        private const string PHYSICAL_DEVICE_MEMORY_KEY = "bugsnag.device.physical_device_memory";
        private const string MEMORY_SPACES_DEVICE_SIZE_KEY = "bugsnag.system.memory.spaces.device.size";
        private const string MEMORY_SPACES_DEVICE_USED_KEY = "bugsnag.system.memory.spaces.device.used";
        private const string MEMORY_SPACES_DEVICE_MEAN_KEY = "bugsnag.system.memory.spaces.device.mean";
        private const string MEMORY_SPACES_ART_SIZE_KEY = "bugsnag.system.memory.spaces.art.size";
        private const string MEMORY_SPACES_ART_USED_KEY = "bugsnag.system.memory.spaces.art.used";
        private const string MEMORY_SPACES_ART_MEAN_KEY = "bugsnag.system.memory.spaces.art.mean";

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

        internal void SetAttributeInternal(string key, long[] value) => SetAttributeWithoutChecks(key, value);
        internal void SetAttributeInternal(string key, double[] value) => SetAttributeWithoutChecks(key, value);
        internal void SetAttributeInternal(string key, bool[] value) => SetAttributeWithoutChecks(key, value);
        internal void SetAttributeInternal(string key, string[] value) => SetAttributeWithoutChecks(key, value);
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

        internal void ApplyCPUMetrics(List<SystemMetricsSnapshot> snapshots)
        {
            // Timestamps
            var timestamps = snapshots.Select(s => s.Timestamp).ToArray();
            SetAttributeInternal(CPU_MEASURES_TIMESTAMPS_KEY, timestamps);
            // CPU
            var processCpu = snapshots.Select(s => Math.Round(s.ProcessCPUPercent, 2)).ToArray();
            var mainThreadCpu = snapshots.Select(s => Math.Round(s.MainThreadCPUPercent, 2)).ToArray();

            SetAttributeInternal(CPU_MEASURES_TOTAL_KEY, processCpu);
            SetAttributeInternal(CPU_MEASURES_MAIN_THREAD_KEY, mainThreadCpu);
            SetAttributeInternal(CPU_MEAN_TOTAL_KEY, Math.Round(processCpu.Average(), 2));
            SetAttributeInternal(CPU_MEAN_MAIN_THREAD_KEY, Math.Round(mainThreadCpu.Average(), 2));
        }
        internal void ApplyMemoryMetrics(List<SystemMetricsSnapshot> snapshots)
        {
            var timestamps = snapshots.Select(s => s.Timestamp).ToArray();
            SetAttributeInternal(MEMORY_TIMESTAMPS_KEY, timestamps);

            // Android
            var androidSnapshots = snapshots
                .Where(s => s.AndroidMetrics.HasValue)
                .Select(s => s.AndroidMetrics.Value)
                .ToList();

            if (androidSnapshots.Count > 0)
            {
                // Device Memory 
                long devicePhysical = androidSnapshots.First().DeviceTotalMemory.GetValueOrDefault(0);
                SetAttributeInternal(PHYSICAL_DEVICE_MEMORY_KEY, devicePhysical);
                SetAttributeInternal(MEMORY_SPACES_DEVICE_SIZE_KEY, devicePhysical);

                var pssValues = androidSnapshots
                    .Select(a => a.PSS.GetValueOrDefault(0L))
                    .ToArray();
                long sumPss = pssValues.Sum();
                long meanPss = pssValues.Length > 0 ? sumPss / pssValues.Length : 0;
                SetAttributeInternal(MEMORY_SPACES_DEVICE_USED_KEY, pssValues);
                SetAttributeInternal(MEMORY_SPACES_DEVICE_MEAN_KEY, meanPss);

                // ART 
                long artMax = androidSnapshots.Max(a => a.ArtMaxMemory.GetValueOrDefault(0L));
                SetAttributeInternal(MEMORY_SPACES_ART_SIZE_KEY, artMax);

                var artUsedValues = androidSnapshots
                    .Select(a =>
                    {
                        long t = a.ArtTotalMemory.GetValueOrDefault(0L);
                        long f = a.ArtFreeMemory.GetValueOrDefault(0L);
                        return (t - f); // current usage
                    })
                    .ToArray();
                long sumArtUsed = artUsedValues.Sum();
                long meanArtUsed = artUsedValues.Length > 0 ? sumArtUsed / artUsedValues.Length : 0;

                SetAttributeInternal(MEMORY_SPACES_ART_USED_KEY, artUsedValues);
                SetAttributeInternal(MEMORY_SPACES_ART_MEAN_KEY, meanArtUsed);
            }

            //ios
            var iosSnapshots = snapshots
                .Where(s => s.iOSMetrics.HasValue)
                .Select(s => s.iOSMetrics.Value)
                .ToList();

            if (iosSnapshots.Count > 0)
            {
                var physicalUsed = iosSnapshots
                    .Select(m => m.PhysicalMemoryInUse ?? 0)
                    .ToArray();
                long sumPhysicalUsed = physicalUsed.Sum();
                long meanPhysicalUsed = physicalUsed.Length > 0
                    ? sumPhysicalUsed / physicalUsed.Length
                    : 0;

                long totalDeviceMemory = iosSnapshots.FirstOrDefault()
                    .TotalDeviceMemory ?? 0;

                SetAttributeInternal(PHYSICAL_DEVICE_MEMORY_KEY, totalDeviceMemory);
                SetAttributeInternal(MEMORY_SPACES_DEVICE_SIZE_KEY, totalDeviceMemory);
                SetAttributeInternal(MEMORY_SPACES_DEVICE_USED_KEY, physicalUsed);
                SetAttributeInternal(MEMORY_SPACES_DEVICE_MEAN_KEY, meanPhysicalUsed);
            }
        }

        internal void RemoveSystemCPUMetrics()
        {
            _attributes.Remove(CPU_MEASURES_TIMESTAMPS_KEY);
            _attributes.Remove(CPU_MEASURES_TOTAL_KEY);
            _attributes.Remove(CPU_MEASURES_MAIN_THREAD_KEY);
            _attributes.Remove(CPU_MEAN_TOTAL_KEY);
            _attributes.Remove(CPU_MEAN_MAIN_THREAD_KEY);
        }

        internal void RemoveSystemMemoryMetrics()
        {
            _attributes.Remove(PHYSICAL_DEVICE_MEMORY_KEY);
            _attributes.Remove(MEMORY_TIMESTAMPS_KEY);
            _attributes.Remove(MEMORY_SPACES_DEVICE_SIZE_KEY);
            _attributes.Remove(MEMORY_SPACES_DEVICE_USED_KEY);
            _attributes.Remove(MEMORY_SPACES_DEVICE_MEAN_KEY);
            _attributes.Remove(MEMORY_SPACES_ART_SIZE_KEY);
            _attributes.Remove(MEMORY_SPACES_ART_USED_KEY);
            _attributes.Remove(MEMORY_SPACES_ART_MEAN_KEY);
        }
    }
}