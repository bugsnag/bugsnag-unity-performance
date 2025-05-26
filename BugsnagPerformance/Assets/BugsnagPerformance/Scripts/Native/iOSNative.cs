using System.Collections;
using System.Collections.Generic;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class iOSNative
    {
        const string Import = "__Internal";

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport(Import)]
        internal static extern string bugsnag_unity_performance_getBundleVersion();
        [DllImport(Import)]
        internal static extern string bugsnag_unity_performance_get_arch();
        [DllImport(Import)]
        internal static extern string bugsnag_unity_performance_get_os_version();
        [DllImport(Import)]
        private static extern void bugsnag_unity_performance_cpu_percents(
            out double processPct, out double mainThreadPct);
        [DllImport(Import)]
        private static extern ulong bugsnag_unity_performance_physical_memory_in_use();
        [DllImport(Import)]
        private static extern ulong bugsnag_unity_performance_total_device_memory();
#endif

        public static string GetBundleVersion()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return bugsnag_unity_performance_getBundleVersion();
#endif
            return null;
        }

        public static string GetArch()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return bugsnag_unity_performance_get_arch();
#endif
            return null;
        }

        public static string GetOsVersion()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return bugsnag_unity_performance_get_os_version();
#endif
            return null;
        }


        internal static SystemMetricsSnapshot? GetSystemMetricsSnapshot()
        {
#if UNITY_IOS && !UNITY_EDITOR
            bugsnag_unity_performance_cpu_percents(
                out double processCpu, out double mainCpu);

            var snap = new SystemMetricsSnapshot
            {
                Timestamp            = BugsnagPerformanceUtil.GetNanoSecondsNow(),
                ProcessCPUPercent    = processCpu,     // 0-100 = whole device
                MainThreadCPUPercent = mainCpu,        // 0-100 = one core
                iOSMetrics = new iOSMemoryMetrics
                {
                    PhysicalMemoryInUse = (long)bugsnag_unity_performance_physical_memory_in_use(),
                    TotalDeviceMemory   = (long)bugsnag_unity_performance_total_device_memory()
                }
            };
            return snap;
#else
            return null;
#endif
        }
    }
}