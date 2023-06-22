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
                config = new PerformanceConfiguration(ApiKey);
                if (string.IsNullOrEmpty(ReleaseStage))
                {
                    config.ReleaseStage = Debug.isDebugBuild ? "development" : "production";
                }
                else
                {
                    config.ReleaseStage = ReleaseStage;
                }
                config.EnabledReleaseStages = EnabledReleaseStages;
            }

            config.AutoInstrumentAppStart = AutoInstrumentAppStart;

            config.Endpoint = Endpoint;
            
            return config;
        }

        private PerformanceConfiguration GetSettingsFromNotifier(out bool autoStart)
        {
            var notifierSettings = Resources.Load("Bugsnag/BugsnagSettingsObject");

            var apiKeyValue = GetValueFromNotifer(notifierSettings, "ApiKey");

            var apiKey = apiKeyValue == null ? null : apiKeyValue.ToString();

            var config = new PerformanceConfiguration(apiKey);

            var releaseStageValue = GetValueFromNotifer(notifierSettings, "ReleaseStage");

            config.ReleaseStage = releaseStageValue == null ? null : releaseStageValue.ToString();

            var enabledReleaseStagesValue = GetValueFromNotifer(notifierSettings, "EnabledReleaseStages");

            config.EnabledReleaseStages = enabledReleaseStagesValue == null ? null : (string[])enabledReleaseStagesValue;

            var autoStartValue = GetValueFromNotifer(notifierSettings, "StartAutomaticallyAtLaunch");

            autoStart = autoStartValue == null ? false : (bool)autoStartValue;

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