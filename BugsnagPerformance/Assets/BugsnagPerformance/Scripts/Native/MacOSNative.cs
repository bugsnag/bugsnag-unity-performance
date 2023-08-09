using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace BugsnagUnityPerformance
{
    internal class MacOSNative
    {

        const string Import = "BugsnagUnityPerformanceMacOS";

#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        [DllImport(Import)]
        internal static extern string bugsnag_performance_getBundleVersion();

        [DllImport(Import)]
        internal static extern string bugsnag_performance_get_arch();

        [DllImport(Import)]
        internal static extern string bugsnag_performance_get_os_version();
#endif

        public static string GetBundleVersion()
        {
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
            return bugsnag_performance_getBundleVersion();
#endif
            return null;
        }

        public static string GetArch()
        {
            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(SystemInfo.processorType, "ARM", CompareOptions.IgnoreCase) >= 0)
            {
                if (Environment.Is64BitProcess)
                    return "ARM64";
                else
                    return "ARM";
            }
            else
            {
                // Must be in the x86 family.
                if (Environment.Is64BitProcess)
                    return "x86_64";
                else
                    return "x86";
            }
        }

        public static string GetOsVersion()
        {
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
                return bugsnag_performance_get_os_version();
#endif
            return null;
        }
    }
}