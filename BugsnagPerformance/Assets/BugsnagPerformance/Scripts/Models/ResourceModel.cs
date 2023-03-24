using System;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    [Serializable]
    internal class ResourceModel
    {

        private static AttributeModel[] _resourceData;

        public AttributeModel[] attributes;

        public ResourceModel()
        {
            attributes = GetResourceData();
        }

        public static void InitResourceDataOnMainThread()
        {
            if (_resourceData == null)
            {
                _resourceData = new AttributeModel[]
               {
                    CreateStringAttribute("deployment.environment", BugsnagPerformance.Configuration.ReleaseStage),

                    CreateStringAttribute("telemetry.sdk.name", "bugsnag.performance.unity"),

                    CreateStringAttribute("telemetry.sdk.version", Version.VersionString),

                    CreateStringAttribute("os.version", Environment.OSVersion.VersionString),

                    CreateStringAttribute("device.id", CacheManager.GetDeviceId()),

                    CreateStringAttribute("device.model.identifier", SystemInfo.deviceModel),

                    CreateStringAttribute("service.version", Application.version),

                    CreateStringAttribute("bugsnag.app.platform", GetPlatform()),

                    CreateStringAttribute("bugsnag.runtime_versions.unity", Application.unityVersion),

                    GetMobileBuildNumber(),

                    GetMobileManufacturer(),

                    GetMobileArch()

               };
            }
        }

        private static AttributeModel[] GetResourceData()
        {
            return _resourceData;
        }

        private static AttributeModel CreateStringAttribute(string key, string stringValue)
        {
            return new AttributeModel()
            {
                key = key,
                value = new AttributeStringValueModel(stringValue)
            };
        }

        private static string GetPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    return "iOS";
                case RuntimePlatform.Android:
                    return "Android";
            }
            return string.Empty;
        }

        private static AttributeModel GetMobileBuildNumber()
        {
            var mobileKey = string.Empty;
            var mobileValue = string.Empty;
#if UNITY_IOS
            mobileKey = "device.bundle_version";
            mobileValue = iOSNative.GetBundleVersion();
#elif UNITY_ANDROID
            mobileKey = "device.version_code";
            mobileValue = AndroidNative.GetVersionCode();
#endif
            return new AttributeModel()
            {
                key = mobileKey,
                value = new AttributeStringValueModel(mobileValue)
            };
        }

        private static AttributeModel GetMobileArch()
        {
            var arch = string.Empty;
#if UNITY_IOS
            arch = iOSNative.GetArch();
#elif UNITY_ANDROID
            arch = AndroidNative.GetArch();
#endif
            return new AttributeModel()
            {
                key = "host.arch",
                value = new AttributeStringValueModel(arch)
            };
        }

        private static AttributeModel GetMobileManufacturer()
        {
            var key = "Apple";
#if UNITY_ANDROID
            key = AndroidNative.GetManufacture();
#endif
            return new AttributeModel()
            {
                key = "device.manufacturer",
                value = new AttributeStringValueModel(key)
            };
        }


    }
}
