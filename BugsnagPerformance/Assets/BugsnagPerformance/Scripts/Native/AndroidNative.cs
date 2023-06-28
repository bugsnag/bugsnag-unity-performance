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

        private static int GetAndroidSDKInt()
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

        public static string GetManufacture()
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

    }
}