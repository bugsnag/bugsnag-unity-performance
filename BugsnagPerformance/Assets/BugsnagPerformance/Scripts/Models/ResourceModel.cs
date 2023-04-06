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
                    new AttributeModel("deployment.environment", BugsnagPerformance.Configuration.ReleaseStage),

                    new AttributeModel("telemetry.sdk.name", "bugsnag.performance.unity"),

                    new AttributeModel("telemetry.sdk.version", Version.VersionString),

                    new AttributeModel("os.version", Environment.OSVersion.VersionString),

                    new AttributeModel("device.id", CacheManager.GetDeviceId()),

                    new AttributeModel("device.model.identifier", SystemInfo.deviceModel),

                    new AttributeModel("service.version", Application.version),

                    new AttributeModel("bugsnag.app.platform", GetPlatform()),

                    new AttributeModel("bugsnag.runtime_versions.unity", Application.unityVersion),

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
#if UNITY_EDITOR
            return null;
#elif UNITY_IOS
            return new AttributeModel("device.bundle_version", iOSNative.GetBundleVersion());
#elif UNITY_ANDROID
            return new AttributeModel("device.version_code", AndroidNative.GetVersionCode());
#endif
        }

        private static AttributeModel GetMobileArch()
        {
#if UNITY_EDITOR
            return null;
#elif UNITY_IOS
            return new AttributeModel("host.arch", iOSNative.GetArch());
#elif UNITY_ANDROID
            return new AttributeModel("host.arch", AndroidNative.GetArch());
#endif
        }



        private static AttributeModel GetMobileManufacturer()
        {
#if UNITY_EDITOR
            return null;
#elif UNITY_IOS
            return new AttributeModel("device.manufacturer", "Apple");
#elif UNITY_ANDROID
            return new AttributeModel("device.manufacturer", AndroidNative.GetManufacture());
#endif
        }


    }
}
