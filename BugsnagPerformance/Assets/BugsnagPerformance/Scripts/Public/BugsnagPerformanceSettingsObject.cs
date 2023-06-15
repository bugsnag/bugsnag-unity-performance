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

        public bool ShareNotifierSettings = false;

        public bool StartAutomaticallyAtLaunch = true;

        public string ApiKey;

        public AutoInstrumentAppStartSetting AutoInstrumentAppStart = AutoInstrumentAppStartSetting.FULL;

        public string Endpoint = "https://otlp.bugsnag.com/v1/traces";

        public string ReleaseStage;

        public double SamplingProbability = 1.0;

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
                throw new Exception("No Bugsnag Performance Settings Object found.");
            }
        }

        internal PerformanceConfiguration GetConfig()
        {
            PerformanceConfiguration config = null;

            if (ShareNotifierSettings && NotifierConfigAvaliable())
            {
                config = GetSettingsFromNotifier(out StartAutomaticallyAtLaunch);
            }
            else
            {
                config = new PerformanceConfiguration(ApiKey);
            }

            config.Endpoint = Endpoint;
            if (string.IsNullOrEmpty(ReleaseStage))
            {
                config.ReleaseStage = Debug.isDebugBuild ? "development" : "production";
            }
            else
            {
                config.ReleaseStage = ReleaseStage;
            }
            config.ReleaseStage =  ReleaseStage;
            config.SamplingProbability = SamplingProbability;
            return config;
        }

        private PerformanceConfiguration GetSettingsFromNotifier(out bool autoStart)
        {
            var notifierSettings = Resources.Load("Bugsnag/BugsnagSettingsObject");
            var settingsType = notifierSettings.GetType();
            var apiKey = settingsType.GetField("ApiKey").GetValue(notifierSettings).ToString();
            var config = new PerformanceConfiguration(apiKey);
            config.ReleaseStage = settingsType.GetField("ReleaseStage").GetValue(notifierSettings).ToString();
            autoStart = (bool)notifierSettings.GetType().GetField("StartAutomaticallyAtLaunch").GetValue(notifierSettings);
            return config;
        }

        private bool NotifierConfigAvaliable()
        {
            return File.Exists(Application.dataPath + "/Resources/Bugsnag/BugsnagSettingsObject.asset");
        }
    }
}