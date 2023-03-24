using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class AndroidNative
    {
#if UNITY_ANDROID
        public static string GetVersionCode()
        {
            var playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            var context = activity.Call<AndroidJavaObject>("getApplicationContext");
            var packageManager = context.Call<AndroidJavaObject>("getPackageManager");
            var packageName = context.Call<AndroidJavaObject>("getPackageName");
            var packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", packageName, 0);
            return packageInfo.Get<int>("versionCode").ToString();
        }

        private static int GetAndroidSDKInt()
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
        }

        public static string GetArch()
        {
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

            return string.Empty;
        }

        public static string GetManufacture()
        {
            var build = new AndroidJavaClass("android.os.Build");
            return build.GetStatic<string>("MANUFACTURER");
        }


        private static string AbiToArchitecture(string input)
        {
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
            return string.Empty;
        }

#endif
    }
}