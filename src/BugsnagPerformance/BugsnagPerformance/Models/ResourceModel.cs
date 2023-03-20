using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    [Serializable]
    internal class ResourceModel
    {

        private const string VERSION = "SET_VERSION";

        private static AttributeModel[] _resourceData;

        public AttributeModel[] attributes;

        public ResourceModel()
        {
            attributes = GetResourceData();
        }

        private static AttributeModel[] GetResourceData()
        {
            if (_resourceData == null)
            {
                _resourceData = new AttributeModel[]
               {
                    CreateStringAttribute("deployment.environment", BugsnagPerformance.Configuration.ReleaseStage),

                    CreateStringAttribute("telemetry.sdk.name", "bugsnag.performance.unity"),

                    CreateStringAttribute("telemetry.sdk.version", VERSION), //TODO

                    CreateStringAttribute("os.version", Environment.OSVersion.VersionString),

                    CreateStringAttribute("device.id", CacheManager.GetDeviceId()),

                    CreateStringAttribute("device.model.identifier", SystemInfo.deviceModel),

                    CreateStringAttribute("service.version", Application.version),

                    CreateStringAttribute("device.model.identifier", SystemInfo.deviceModel),

                    CreateStringAttribute("bugsnag.app.platform", GetPlatform()),

                    GetMobileBuildNumber(),

                    GetMobileManufacturer(),

                    GetMobileArch()


               };
            }
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
            //TODO  Native layer to get platform specific build metadata
            return new AttributeModel()
            {
                key = "device.version_code",
                value = new AttributeStringValueModel("NA")
            };
        }

        private static AttributeModel GetMobileArch()
        {
            //TODO Use Native layer to get device arch
            return new AttributeModel()
            {
                key = "host.arch",
                value = new AttributeStringValueModel("NA")
            };
        }

        private static AttributeModel GetMobileManufacturer()
        {
            var model = new AttributeModel()
            {
                key = "device.manufacturer",
                value = new AttributeStringValueModel("NA")
            };
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer:
                    model.value = new AttributeStringValueModel("Apple");
                    break;
                case RuntimePlatform.Android:
                    //TODO Use Android native layer to get android specific manufacturer
                    break;
            }
            return model;
        }


    }
}
