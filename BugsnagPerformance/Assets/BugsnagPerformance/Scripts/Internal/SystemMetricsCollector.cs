using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BugsnagUnityPerformance;
using UnityEngine;
namespace BugsnagUnityPerformance
{
    internal struct SystemMetricsSnapshot
    {
        public long Timestamp;
        //CPU usage
        public double ProcessCPUPercent;
        public double MainThreadCPUPercent;
        public double MonitorThreadCPUPercent;

        //Android specifics
        // Free memory (bytes) on the Java heap.
        public long? FreeMemory;
        // Total memory (bytes) on the Java heap.
        public long? TotalMemory;
        // Max memory (bytes) for the Java heap.
        public long? MaxMemory;
        // Proportional Set Size (bytes).
        public long? PSS;

        //iOS
        // The current physical memory usage by this process (bytes).
        public long? PhysicalMemoryInUse;
        // The total physical memory on the device (bytes).
        public long? TotalDeviceMemory;

    }
    internal class SystemMetricsCollector : IPhasedStartup
    {
        private bool _cpuMetricsEnabled = true;
        private bool _memoryMetricsEnabled = true;
        private List<SystemMetricsSnapshot> _snapshots = new List<SystemMetricsSnapshot>(MAX_SNAPSHOTS);
        private const int MAX_SNAPSHOTS = 1200; // 20 minutes at 1 second intervals
        private const float SNAPSHOT_INTERVAL = 1.0f;
        private RuntimePlatform _platform;

        public SystemMetricsCollector()
        {
            _platform = Application.platform;
            StartPollingForMetrics();
        }
        public void Configure(PerformanceConfiguration config)
        {
            if (config.EnabledMetrics != null)
            {
                _cpuMetricsEnabled = config.EnabledMetrics.CPU;
                _memoryMetricsEnabled = config.EnabledMetrics.Memory;
            }
            else
            {
                _cpuMetricsEnabled = false;
                _memoryMetricsEnabled = false;
            }
        }

        public void Start()
        {
            // No-op
        }

        private void StartPollingForMetrics()
        {
            MainThreadDispatchBehaviour.Instance().StartCoroutine(PollForMetrics());
        }

        private IEnumerator PollForMetrics()
        {
            if (!_cpuMetricsEnabled && !_memoryMetricsEnabled)
            {
                yield break;
            }
            while (true)
            {
                var snapshot = GetSystemMetricsSnapshot();
                if (snapshot.HasValue)
                {
                    _snapshots.Add(snapshot.Value);
                    if (_snapshots.Count > MAX_SNAPSHOTS)
                    {
                        _snapshots.RemoveAt(0); // Ring-buffer behavior
                    }
                }

                yield return new WaitForSeconds(SNAPSHOT_INTERVAL);
            }
        }

        private SystemMetricsSnapshot? GetSystemMetricsSnapshot()
        {
            if (_platform == RuntimePlatform.Android)
            {
                return AndroidNative.GetSystemMetricsSnapshot();
            }
            else if (_platform == RuntimePlatform.IPhonePlayer)
            {
                return iOSNative.GetSystemMetricsSnapshot();
            }
            return GetDummyData(); // Unsupported platform
        }

        public void OnSpanEnd(Span span)
        {
            if (!_cpuMetricsEnabled && !_memoryMetricsEnabled)
            {
                return;
            }
            if (_snapshots == null || _snapshots.Count == 0)
                return;

            long startNs = BugsnagPerformanceUtil.GetNanoSeconds(span.StartTime);
            long endNs = BugsnagPerformanceUtil.GetNanoSeconds(span.EndTime);

            // Snapshot immediately before span start
            var beforeSnapshot = _snapshots.LastOrDefault(s => s.Timestamp < startNs);

            // Snapshots during span + up to 1.1s after
            var duringAndAfter = _snapshots
                .Where(s => s.Timestamp >= startNs && s.Timestamp <= (endNs + 1_100_000_000))
                .ToList();

            var relevantSnapshots = new List<SystemMetricsSnapshot>();
            if (beforeSnapshot.Timestamp > 0)
            {
                relevantSnapshots.Add(beforeSnapshot);
            }

            relevantSnapshots.AddRange(duringAndAfter);

            // If still fewer than 2, grab the last 2 available
            if (relevantSnapshots.Count < 2)
            {
                //hold span until we have 2
            }

            // Must be 2 by now, maybe this check is not needed
            if (relevantSnapshots.Count > 0)
            {
                MetricsFormatter.AttachMetricsToSpan(span, relevantSnapshots);
            }
        }

        private SystemMetricsSnapshot GetDummyData()
        {
            return new SystemMetricsSnapshot
            {
                Timestamp = BugsnagPerformanceUtil.GetNanoSeconds(DateTimeOffset.UtcNow),
                ProcessCPUPercent = 0.5,
                MainThreadCPUPercent = 0.6,
                MonitorThreadCPUPercent = 0.7,
                FreeMemory = 111,
                TotalMemory = 222,
                MaxMemory = 333,
                PSS = 444,
                PhysicalMemoryInUse = 555,
                TotalDeviceMemory = 666
            };
        }


    }

    internal static class MetricsFormatter
    {
        public static void AttachMetricsToSpan(Span span, List<SystemMetricsSnapshot> snapshots)
        {
            if (snapshots == null || snapshots.Count == 0)
                return;

            // === Timestamps ===
            var timestamps = snapshots.Select(s => s.Timestamp).ToArray();
            span.SetAttribute("bugsnag.system.cpu_measures_timestamps", timestamps);
            span.SetAttribute("bugsnag.system.memory.timestamps", timestamps);

            // === CPU ===
            var processCpu = snapshots.Select(s => s.ProcessCPUPercent).ToArray();
            var mainThreadCpu = snapshots.Select(s => s.MainThreadCPUPercent).ToArray();
            var monitorThreadCpu = snapshots.Select(s => s.MonitorThreadCPUPercent).ToArray();

            span.SetAttribute("bugsnag.system.cpu_measures_total", processCpu);
            span.SetAttribute("bugsnag.system.cpu_measures_main_thread", mainThreadCpu);
            span.SetAttribute("bugsnag.system.cpu_measures_overhead", monitorThreadCpu);

            span.SetAttribute("bugsnag.metrics.cpu_mean_total", processCpu.Average());
            span.SetAttribute("bugsnag.system.cpu_mean_main_thread", mainThreadCpu.Average());

            // === Memory ===
            // Android Java Heap
            if (snapshots.Any(s => s.TotalMemory.HasValue))
            {
                var totalDeviceMemory = snapshots.FirstOrDefault(s => s.TotalMemory.HasValue).TotalMemory ?? 0;

                span.SetAttribute("bugsnag.device.physical_device_memory", totalDeviceMemory);

                var used = snapshots.Select(s => (s.TotalMemory ?? 0) - (s.FreeMemory ?? 0)).ToArray();
                var size = snapshots.Max(s => s.TotalMemory ?? 0);

                span.SetAttribute("bugsnag.system.memory.spaces.space_names", new[] { "art", "device" });

                span.SetAttribute("bugsnag.system.memory.spaces.device.size", size);
                span.SetAttribute("bugsnag.system.memory.spaces.device.used", used);
                span.SetAttribute("bugsnag.system.memory.spaces.device.mean", (long)used.Average());

                span.SetAttribute("bugsnag.system.memory.spaces.art.size", size);
                span.SetAttribute("bugsnag.system.memory.spaces.art.used", used);
                span.SetAttribute("bugsnag.system.memory.spaces.art.mean", (long)used.Average());
            }

            // iOS Physical Memory
            if (snapshots.Any(s => s.PhysicalMemoryInUse.HasValue))
            {
                var physicalUsed = snapshots.Select(s => s.PhysicalMemoryInUse ?? 0).ToArray();
                var physicalTotal = snapshots.FirstOrDefault(s => s.TotalDeviceMemory.HasValue).TotalDeviceMemory ?? 0;

                span.SetAttribute("bugsnag.device.physical_device_memory", physicalTotal);

                span.SetAttribute("bugsnag.system.memory.spaces.space_names", new[] { "device" });
                span.SetAttribute("bugsnag.system.memory.spaces.device.size", physicalTotal);
                span.SetAttribute("bugsnag.system.memory.spaces.device.used", physicalUsed);
                span.SetAttribute("bugsnag.system.memory.spaces.device.mean", (long)physicalUsed.Average());
            }
        }
    }
}