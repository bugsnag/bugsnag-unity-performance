using System;
using System.Collections;
using System.Collections.Generic;
using BugsnagUnity;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    [Serializable]
    public class BugsnagPerformanceSettingsObject : ScriptableObject
    {

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
            var config = new PerformanceConfiguration(ApiKey);
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
    }
}