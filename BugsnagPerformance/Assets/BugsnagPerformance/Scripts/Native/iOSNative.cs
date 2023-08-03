using System.Collections;
using System.Collections.Generic;
#if (UNITY_STANDALONE_OSX || UNITY_IOS) && !UNITY_EDITOR

using System.Runtime.InteropServices;

#endif



using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class iOSNative
    {

#if UNITY_STANDALONE_OSX
        const string Import = "BugsnagUnityPerformanceMacOS";
#else
        const string Import = "__Internal";
#endif

#if (UNITY_STANDALONE_OSX || UNITY_IOS) && !UNITY_EDITOR
        [DllImport(Import)]
        internal static extern string bugsnag_performance_getBundleVersion();

        [DllImport(Import)]
        internal static extern string bugsnag_performance_get_arch();
#endif

        public static string GetBundleVersion()
        {
#if (UNITY_STANDALONE_OSX || UNITY_IOS) && !UNITY_EDITOR
            return bugsnag_performance_getBundleVersion();
#endif
            return null;
        }

        public static string GetArch()
        {
#if (UNITY_STANDALONE_OSX || UNITY_IOS) && !UNITY_EDITOR
            return bugsnag_performance_get_arch();
#endif
            return null;
        }
    }
}