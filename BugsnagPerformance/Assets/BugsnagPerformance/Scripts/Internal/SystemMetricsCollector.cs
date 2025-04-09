using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace BugsnagUnityPerformance
{
    internal struct SystemMetricsSnapshot
    {
        public long Timestamp;

        // CPU usage
        public double ProcessCPUPercent;
        public double MainThreadCPUPercent;
        public double MonitorThreadCPUPercent;

        // Platform-specific memory metrics
        public AndroidMemoryMetrics? AndroidMetrics;
        public iOSMemoryMetrics? iOSMetrics;
    }

    internal struct AndroidMemoryMetrics
    {
        // Device-level memory:
        public long? FreeMemory;        // from ActivityManager.MemoryInfo.availMem
        public long? TotalMemory;       // from ActivityManager.MemoryInfo.totalMem (physical device RAM)
        public long? MaxMemory;         // from ActivityManager.MemoryInfo.threshold
        // PSS
        public long? PSS;               // from Debug.MemoryInfo.getTotalPss()
        // Java heap usage
        public long? JavaMaxMemory;     // from Runtime.getRuntime().maxMemory()
        public long? JavaTotalMemory;   // from Runtime.getRuntime().totalMemory()
        public long? JavaFreeMemory;    // from Runtime.getRuntime().freeMemory()
    }

    internal struct iOSMemoryMetrics
    {
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
            MainThreadDispatchBehaviour.Enqueue(PollForMetrics());
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
                        _snapshots.RemoveAt(0);
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
            return null;
        }

        public void OnSpanEnd(Span span)
        {
            if (!_cpuMetricsEnabled && !_memoryMetricsEnabled)
            {
                return;
            }
            if (_snapshots == null || _snapshots.Count == 0)
            {
                return;
            }
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
            if (relevantSnapshots.Count > 0)
            {
                if (_cpuMetricsEnabled)
                {
                    span.CalculateCPUMetrics(relevantSnapshots);
                }
                if (_memoryMetricsEnabled)
                {
                    span.CalculateMemoryMetrics(relevantSnapshots);
                }
            }
        }

        private SystemMetricsSnapshot GetTestingMetrics()
        {
            return new SystemMetricsSnapshot
            {
                Timestamp = BugsnagPerformanceUtil.GetNanoSeconds(DateTimeOffset.UtcNow),
                ProcessCPUPercent = UnityEngine.Random.Range(0.0f, 100.0f),
                MainThreadCPUPercent = UnityEngine.Random.Range(0.0f, 100.0f),
                MonitorThreadCPUPercent = UnityEngine.Random.Range(0.0f, 100.0f),
                AndroidMetrics = new AndroidMemoryMetrics
                {
                    FreeMemory = UnityEngine.Random.Range(0, 1000),
                    TotalMemory = UnityEngine.Random.Range(0, 1000),
                    MaxMemory = UnityEngine.Random.Range(0, 1000),
                    PSS = UnityEngine.Random.Range(0, 1000),
                },
                iOSMetrics = new iOSMemoryMetrics
                {
                    PhysicalMemoryInUse = UnityEngine.Random.Range(0, 1000),
                    TotalDeviceMemory = UnityEngine.Random.Range(0, 1000),
                }
            };
        }
    }
}