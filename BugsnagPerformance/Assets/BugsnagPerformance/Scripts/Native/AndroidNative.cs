using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class AndroidNative
    {

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaClass _unityPlayerClass;
        private static AndroidJavaClass UnityPlayerClass
        {
            get
            {
                if (_unityPlayerClass == null)
                {
                    _unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                }
                return _unityPlayerClass;
            }
        }

        private static AndroidJavaObject _activity;
        private static AndroidJavaObject Activity
        {
            get
            {
                if (_activity == null)
                {
                    _activity = UnityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                }
                return _activity;
            }
        }

        private static AndroidJavaClass _buildClass;
        private static AndroidJavaClass BuildClass
        {
            get
            {
                if (_buildClass == null)
                {
                    _buildClass = new AndroidJavaClass("android.os.Build");
                }
                return _buildClass;
            }
        }
        private static AndroidJavaClass _versionClass;
        private static AndroidJavaClass VersionClass
        {
            get
            {
                if (_versionClass == null)
                {
                    _versionClass = new AndroidJavaClass("android.os.Build$VERSION");
                }
                return _versionClass;
            }
        }

        private static AndroidJavaClass _processClass;
        private static AndroidJavaClass ProcessClass
        {
            get
            {
                if (_processClass == null)
                {
                    _processClass = new AndroidJavaClass("android.os.Process");
                }
                return _processClass;
            }
        }

        private static AndroidJavaClass _runtimeClass;
        private static AndroidJavaClass RuntimeClass
        {
            get
            {
                if (_runtimeClass == null)
                {
                    _runtimeClass = new AndroidJavaClass("java.lang.Runtime");
                }
                return _runtimeClass;
            }
        }

        private static AndroidJavaObject _runtimeInstance;
        private static AndroidJavaObject RuntimeInstance
        {
            get
            {
                if (_runtimeInstance == null)
                {
                    _runtimeInstance = RuntimeClass.CallStatic<AndroidJavaObject>("getRuntime");
                }
                return _runtimeInstance;
            }
        }

        private static int _androidActivityPid = -1;
        private static int AndroidActivityPid
        {
            get
            {
                if (_androidActivityPid < 0)
                {
                    _androidActivityPid = ProcessClass.CallStatic<int>("myPid");
                }
                return _androidActivityPid;
            }
        }

        private static AndroidJavaObject _activityManager;
        private static AndroidJavaObject ActivityManager
        {
            get
            {
                if (_activityManager == null)
                {
                    _activityManager = Activity.Call<AndroidJavaObject>("getSystemService", "activity");
                }
                return _activityManager;
            }
        }

        private static long _maxArtMemory = -1;
#endif

        public static string GetVersionCode()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            // only called once so no need to cache these native objects
            var context = Activity.Call<AndroidJavaObject>("getApplicationContext");
            var packageManager = context.Call<AndroidJavaObject>("getPackageManager");
            var packageName = context.Call<AndroidJavaObject>("getPackageName");
            var packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
            return packageInfo.Get<int>("versionCode").ToString();
#else
            return null;
#endif
        }

        public static int GetAndroidSDKInt()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return VersionClass.GetStatic<int>("SDK_INT");
#else
            return 0;
#endif
        }

        public static string GetArch()
        {
#if UNITY_ANDROID && !UNITY_EDITOR


            if (GetAndroidSDKInt() >= 21)
            {
                var abis = BuildClass.GetStatic<string[]>("SUPPORTED_ABIS");
                if (abis != null && abis.Length > 0)
                {
                    return AbiToArchitecture(abis[0]);
                }
            }
            else
            {
                var abi = BuildClass.GetStatic<string>("CPU_ABI");
                if (!string.IsNullOrEmpty(abi))
                {
                    return AbiToArchitecture(abi);
                }
            }
#endif
            return null;
        }

        public static string GetManufacturer()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return BuildClass.GetStatic<string>("MANUFACTURER");
#else
            return null;
#endif
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
#if UNITY_ANDROID && !UNITY_EDITOR
            return VersionClass.GetStatic<string>("RELEASE");
#else
            return null;
#endif
        }

        public static SystemMetricsSnapshot? GetSystemMetricsSnapshot()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var snapshot = new SystemMetricsSnapshot();
            snapshot.Timestamp = BugsnagPerformanceUtil.GetNanoSecondsNow();

            var amMemoryInfo = new AndroidJavaObject("android.app.ActivityManager$MemoryInfo");
            ActivityManager.Call("getMemoryInfo", amMemoryInfo);

            var memInfoArray = ActivityManager.Call<AndroidJavaObject[]>(
                "getProcessMemoryInfo",
                new object[] { new int[] { AndroidActivityPid } }
            );
            var debugMemInfo = memInfoArray[0];
            var totalPss = (long)debugMemInfo.Call<int>("getTotalPss");

            snapshot.AndroidMetrics = new AndroidMemoryMetrics
            {
                DeviceFreeMemory = amMemoryInfo.Get<long>("availMem"),
                DeviceTotalMemory = amMemoryInfo.Get<long>("totalMem"),
                PSS = totalPss * 1024L, // getTotalPss is in KB, multiply by 1024 for bytes if needed
                ArtMaxMemory = _maxArtMemory,
                ArtTotalMemory = RuntimeInstance.Call<long>("totalMemory"),
                ArtFreeMemory = RuntimeInstance.Call<long>("freeMemory")
            };

            return snapshot;
#else
            return null;
#endif
        }

    }
}