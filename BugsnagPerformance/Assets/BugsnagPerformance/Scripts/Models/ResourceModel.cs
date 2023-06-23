using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    public class ResourceModel: IPhasedStartup
    {
        private CacheManager _cacheManager;

        public List<AttributeModel> attributes;

        public ResourceModel(CacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public void Configure(PerformanceConfiguration config)
        {
            attributes = new List<AttributeModel>
               {
                    new AttributeModel("deployment.environment", config.ReleaseStage),

                    new AttributeModel("telemetry.sdk.name", "bugsnag.performance.unity"),

                    new AttributeModel("telemetry.sdk.version", Version.VersionString),

                    new AttributeModel("os.version", Environment.OSVersion.VersionString),

                    new AttributeModel("device.model.identifier", SystemInfo.deviceModel),

                    new AttributeModel("service.version", string.IsNullOrEmpty(config.AppVersion) ? Application.version : config.AppVersion),

                    new AttributeModel("bugsnag.app.platform", GetPlatform()),

                    new AttributeModel("bugsnag.runtime_versions.unity", Application.unityVersion),
               };
            var mobileBuildNumber = GetMobileBuildNumber(config);
            if (mobileBuildNumber != null)
            {
                attributes.Add(mobileBuildNumber);
            }

            var mobileManufacturer = GetMobileManufacturer();
            if (mobileManufacturer != null)
            {
                attributes.Add(mobileManufacturer);
            }

            var mobileArch = GetMobileArch();
            if (mobileArch != null)
            {
                attributes.Add(mobileArch);
            }
        }

        public void Start()
        {
            attributes.Add(new AttributeModel("device.id", _cacheManager.GetDeviceId()));
        }

        private string GetPlatform()
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

        private AttributeModel GetMobileBuildNumber(PerformanceConfiguration config)
        {
#if UNITY_EDITOR
            return null;
#elif UNITY_IOS
            return new AttributeModel("device.bundle_version", string.IsNullOrEmpty(config.BundleVersion) ? iOSNative.GetBundleVersion() : config.BundleVersion);
#elif UNITY_ANDROID
            return new AttributeModel("device.version_code", config.VersionCode < 0 ? AndroidNative.GetVersionCode() : config.VersionCode);
#endif
        }

        private AttributeModel GetMobileArch()
        {
#if UNITY_EDITOR
            return null;
#elif UNITY_IOS
            return new AttributeModel("host.arch", iOSNative.GetArch());
#elif UNITY_ANDROID
            return new AttributeModel("host.arch", AndroidNative.GetArch());
#endif
        }

        private AttributeModel GetMobileManufacturer()
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
