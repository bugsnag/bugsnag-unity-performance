using System.Collections;
using System.Collections.Generic;
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
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
            return bugsnag_performance_get_arch();
#endif
            return null;
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