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
        internal static extern string bugsnag_performance_getBundleVersion();

        [DllImport(Import)]
        internal static extern string bugsnag_performance_get_arch();

        [DllImport(Import)]
        internal static extern string bugsnag_performance_get_os_version();
        
#endif

        public static string GetBundleVersion()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return bugsnag_performance_getBundleVersion();
#endif
            return null;
        }

        public static string GetArch()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return bugsnag_performance_get_arch();
#endif
            return null;
        }

        public static string GetOsVersion()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return bugsnag_performance_get_os_version();
#endif
            return null;
        }
    }
}