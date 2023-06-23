using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    [Serializable]
    public class BugsnagPerformanceSettingsObject : ScriptableObject
    {

        public bool UseNotifierSettings = true;

        public bool StartAutomaticallyAtLaunch = true;

        public string ApiKey;

        public AutoInstrumentAppStartSetting AutoInstrumentAppStart = AutoInstrumentAppStartSetting.FULL;

        public string[] EnabledReleaseStages;

        public string Endpoint = "https://otlp.bugsnag.com/v1/traces";

        public string ReleaseStage;

        public string AppVersion;
        public int VersionCode = -1;
        public string BundleVersion;

        public static PerformanceConfiguration LoadConfiguration()
        {
            var settings = Resources.Load<BugsnagPerformanceSettingsObject>("Bugsnag/BugsnagPerformanceSettingsObject");
            if (settings != null)
            {
                var config = settings.GetConfig();
                return config;
            }
            else
            {
                throw new Exception("No BugSnag Performance Settings Object found. Please open Window>BugSnag>Performance Configuration, to create one.");
            }
        }

        internal PerformanceConfiguration GetConfig()
        {
            PerformanceConfiguration config = null;

            if (UseNotifierSettings && NotifierConfigAvaliable())
            {
                config = GetSettingsFromNotifier(out StartAutomaticallyAtLaunch);
            }
            else
            {
                config = GetStandaloneConfig();
            }

            config.AutoInstrumentAppStart = AutoInstrumentAppStart;

            config.Endpoint = Endpoint;
            
            return config;
        }

        private PerformanceConfiguration GetStandaloneConfig()
        {
            var config = new PerformanceConfiguration(ApiKey);
            if (string.IsNullOrEmpty(ReleaseStage))
            {
                config.ReleaseStage = Debug.isDebugBuild ? "development" : "production";
            }
            else
            {
                config.ReleaseStage = ReleaseStage;
            }
            config.EnabledReleaseStages = EnabledReleaseStages;
            config.AppVersion = AppVersion;
            config.BundleVersion = BundleVersion;
            config.VersionCode = VersionCode;

            return config;
        }

        private PerformanceConfiguration GetSettingsFromNotifier(out bool autoStart)
        {
            var notifierSettings = Resources.Load("Bugsnag/BugsnagSettingsObject");

            var apiKey = (string)GetValueFromNotifer(notifierSettings, "ApiKey");

            var config = new PerformanceConfiguration(apiKey);

            config.ReleaseStage = (string)GetValueFromNotifer(notifierSettings, "ReleaseStage");

            config.EnabledReleaseStages = (string[])GetValueFromNotifer(notifierSettings, "EnabledReleaseStages");

            config.AppVersion = (string)GetValueFromNotifer(notifierSettings, "AppVersion");
            config.BundleVersion = (string)GetValueFromNotifer(notifierSettings, "BundleVersion");
            config.VersionCode = (int)GetValueFromNotifer(notifierSettings, "VersionCode");

            autoStart = (bool)GetValueFromNotifer(notifierSettings, "StartAutomaticallyAtLaunch");

            return config;
        }


        private object GetValueFromNotifer(object notifierSettings, string key)
        {
            if (notifierSettings != null)
            {
                var field = notifierSettings.GetType().GetField(key);
                if (field != null)
                {
                    return field.GetValue(notifierSettings);
                }
            }
            return null;
        }



        private bool NotifierConfigAvaliable()
        {
            return File.Exists(Application.dataPath + "/Resources/Bugsnag/BugsnagSettingsObject.asset");
        }
    }
}