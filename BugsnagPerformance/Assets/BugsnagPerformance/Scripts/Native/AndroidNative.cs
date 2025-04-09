using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class AndroidNative
    {
        public static string GetVersionCode()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            var context = activity.Call<AndroidJavaObject>("getApplicationContext");
            var packageManager = context.Call<AndroidJavaObject>("getPackageManager");
            var packageName = context.Call<AndroidJavaObject>("getPackageName");
            var packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
            return packageInfo.Get<int>("versionCode").ToString();
#endif
            return null;
        }

        public static int GetAndroidSDKInt()
        {
#if UNITY_ANDROID && !UNITY_EDITOR

            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
#endif
            return 0;
        }

        public static string GetArch()
        {
#if UNITY_ANDROID && !UNITY_EDITOR

            var build = new AndroidJavaClass("android.os.Build");

            if (GetAndroidSDKInt() >= 21)
            {
                var abis = build.GetStatic<string[]>("SUPPORTED_ABIS");
                if (abis != null && abis.Length > 0)
                {
                    return AbiToArchitecture(abis[0]);
                }
            }
            else
            {
                var abi = build.GetStatic<string>("CPU_ABI");
                if (!string.IsNullOrEmpty(abi))
                {
                    return AbiToArchitecture(abi);
                }
            }
#endif
            return string.Empty;
        }

        public static string GetManufacturer()
        {
#if UNITY_ANDROID && !UNITY_EDITOR

            var build = new AndroidJavaClass("android.os.Build");
            return build.GetStatic<string>("MANUFACTURER");
#endif
            return null;
        }


        private static string AbiToArchitecture(string input)
        {
#if UNITY_ANDROID && !UNITY_EDITOR

            switch (input.ToLower())
            {
                case "arm64-v8a":
                    return "arm64";
                case "x86_64":
                    return "amd64";
                case "armeabi-v7a":
                    return "arm32";
                case "x86":
                    return "x86";
            }
#endif
            return string.Empty;

        }

        public static string GetOsVersion()
        {
#if UNITY_ANDROID
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<string>("RELEASE");
            }
#endif
#pragma warning disable CS0162 // Unreachable code detected
            return string.Empty;
#pragma warning restore CS0162 // Unreachable code detected
        }

        public static SystemMetricsSnapshot GetSystemMetricsSnapshot()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
             var snapshot = new SystemMetricsSnapshot();
    snapshot.Timestamp = BugsnagPerformanceUtil.GetNanoSecondsNow();

    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    {
        // 1) get Activity & ActivityManager
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        var activityMgr = activity.Call<AndroidJavaObject>("getSystemService", "activity");

        // 2) get MemoryInfo
        var amMemoryInfo = new AndroidJavaObject("android.app.ActivityManager$MemoryInfo");
        activityMgr.Call("getMemoryInfo", amMemoryInfo);

        // 3) get Debug.MemoryInfo for PSS
        var processClass = new AndroidJavaClass("android.os.Process");
        int pid = processClass.CallStatic<int>("myPid");
        var memInfoArray = activityMgr.Call<AndroidJavaObject[]>(
            "getProcessMemoryInfo",
            new object[] { new int[] { pid } }
        );
        var debugMemInfo = memInfoArray[0];
        var totalPss = (long)debugMemInfo.Call<int>("getTotalPss");

        // 4) get Java Runtime info
        var runtimeClass = new AndroidJavaClass("java.lang.Runtime");
        var runtime = runtimeClass.CallStatic<AndroidJavaObject>("getRuntime");
        long javaMax = runtime.Call<long>("maxMemory");
        long javaTotal = runtime.Call<long>("totalMemory");
        long javaFree = runtime.Call<long>("freeMemory");

        // 5) store everything in snapshot
        snapshot.AndroidMetrics = new AndroidMemoryMetrics
        {
            // device memory info
            FreeMemory  = amMemoryInfo.Get<long>("availMem"),
            TotalMemory = amMemoryInfo.Get<long>("totalMem"), // total physical RAM
            MaxMemory   = amMemoryInfo.Get<long>("threshold"),

            // PSS
            PSS = totalPss * 1024L, // getTotalPss is in KB, multiply by 1024 for bytes if needed

            // Java runtime usage
            JavaMaxMemory   = javaMax,
            JavaTotalMemory = javaTotal,
            JavaFreeMemory  = javaFree
        };
    }
    return snapshot;
#endif
#pragma warning disable CS0162 // Unreachable code detected
            return new SystemMetricsSnapshot { };
#pragma warning restore CS0162 // Unreachable code detected
        }

    }
}