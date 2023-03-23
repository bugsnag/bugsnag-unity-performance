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

        public static string GetArch()
        {
            var system = new AndroidJavaClass("java.lang.System");
            return system.CallStatic<string>("getProperty", "os.arch");
        }

        public static string GetManufacture()
        {
            var build = new AndroidJavaClass("android.os.Build");
            return build.GetStatic<string>("MANUFACTURER");
        }
#endif
    }
}