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
        public double ProcessCPUPercent;
        public double MainThreadCPUPercent;
        public AndroidMemoryMetrics? AndroidMetrics;
        public iOSMemoryMetrics? iOSMetrics;
    }

    internal struct AndroidMemoryMetrics
    {
        public long? DeviceFreeMemory; // The amount of free memory on the device.       
        public long? DeviceTotalMemory; // The total amount of memory on the device.
        public long? PSS; // The amount of memory allocated to this process.
        public long? ArtMaxMemory; // The maximum heap memory your app is allowed to use.
        public long? ArtTotalMemory; // The amount of heap memory currently allocated by the runtime.
        public long? ArtFreeMemory; // The amount of heap memory currently free and available for allocation.
    }

    internal struct iOSMemoryMetrics
    {
        public long? PhysicalMemoryInUse;
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
                return AndroidNative.GetSystemMetricsSnapshot(_cpuMetricsEnabled, _memoryMetricsEnabled);
            }
            else if (_platform == RuntimePlatform.IPhonePlayer)
            {
                return iOSNative.GetSystemMetricsSnapshot();
            }
            return null;
        }

        public void OnSpanEnd(Span span)
        {
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

            if (_memoryMetricsEnabled && relevantSnapshots.Count > 0)
            {
                span.ApplyMemoryMetrics(relevantSnapshots);
            }
            // We require minimum 2 snapshots to calculate CPU metrics and sometimes this is not possible
            if (_cpuMetricsEnabled && relevantSnapshots.Count > 1)
            {
                span.ApplyCPUMetrics(relevantSnapshots);
            }

        }
    }
}