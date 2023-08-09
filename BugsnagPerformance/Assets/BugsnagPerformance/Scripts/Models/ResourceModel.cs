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
                new AttributeModel("device.model.identifier", SystemInfo.deviceModel),
                new AttributeModel("service.version", string.IsNullOrEmpty(config.AppVersion) ? Application.version : config.AppVersion),
                new AttributeModel("bugsnag.app.platform", GetPlatform()),
                new AttributeModel("bugsnag.runtime_versions.unity", Application.unityVersion),
                GetNativeVersionInfo(config),
                GetManufacturer(),
                GetArch(),
                GetNativeOsVersion()
            };
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
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return "MacOS";
            }
            return string.Empty;
        }

        private AttributeModel GetNativeVersionInfo(PerformanceConfiguration config)
        {
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                case RuntimePlatform.OSXPlayer:
                    return GetCocoaBundleVersion(config);
                case RuntimePlatform.Android:
                    return GetAndroidVersionCode(config);
            }
            return null;
        }

        private AttributeModel GetCocoaBundleVersion(PerformanceConfiguration config)
        {
            if (!string.IsNullOrEmpty(config.BundleVersion))
            {
                return new AttributeModel("device.bundle_version",  config.BundleVersion);
            }
            return new AttributeModel("device.bundle_version", iOSNative.GetBundleVersion());
        }

        private AttributeModel GetAndroidVersionCode(PerformanceConfiguration config)
        {
            if (config.VersionCode > -1)
            {
                return new AttributeModel("device.version_code", config.VersionCode.ToString());
            }
            return new AttributeModel("device.version_code", AndroidNative.GetVersionCode());
        }

        private AttributeModel GetArch()
        {
            switch (Application.platform)
            {
                
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return new AttributeModel("host.arch", MacOSNative.GetArch());
                case RuntimePlatform.IPhonePlayer:
                    return new AttributeModel("host.arch", iOSNative.GetArch());
                case RuntimePlatform.Android:
                    return new AttributeModel("host.arch", AndroidNative.GetArch());
            }
            return null;
        }

        private AttributeModel GetManufacturer()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.IPhonePlayer:
                    return new AttributeModel("device.manufacturer", "Apple");
                case RuntimePlatform.Android:
                    return new AttributeModel("device.manufacturer", AndroidNative.GetManufacturer());
            }
            return null;
        }

        private AttributeModel GetNativeOsVersion()
        {
            string osVersion = string.Empty;
            switch (Application.platform)
            {
                case RuntimePlatform.OSXPlayer:
                    osVersion = MacOSNative.GetOsVersion();
                    break;
                case RuntimePlatform.IPhonePlayer:
                    osVersion = iOSNative.GetOsVersion();
                    break;
                case RuntimePlatform.Android:
                    osVersion = AndroidNative.GetOsVersion();
                    break;
            }
            return new AttributeModel("os.version", osVersion);
        }
    }
}
