using System;
using System.IO;
using System.Text.RegularExpressions;
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

        public string Endpoint = string.Empty;

        public string ReleaseStage;

        public string AppVersion;
        public int VersionCode = -1;
        public string BundleVersion;
        public string ServiceName;
        public string[] TracePropagationUrls;
        public int AttributeStringValueLimit;
        public int AttributeArrayLengthLimit;
        public int AttributeCountLimit;
        public EnabledMetrics EnabledMetrics = new EnabledMetrics();
        public bool GenerateAnonymousId = true;

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
            PerformanceConfiguration config;
            if (UseNotifierSettings && NotifierConfigAvaliable())
            {
                config = GetSettingsFromNotifier(out StartAutomaticallyAtLaunch);
            }
            else
            {
                config = GetStandaloneConfig();
            }

            GetCommonConfigValues(config);

            return config;
        }

        private void GetCommonConfigValues(PerformanceConfiguration config)
        {
            config.AutoInstrumentAppStart = AutoInstrumentAppStart;
            config.Endpoint = Endpoint;
            if (TracePropagationUrls != null && TracePropagationUrls.Length > 0)
            {
                config.TracePropagationUrls = ConvertTracePropagationUrls(TracePropagationUrls);
            }
            config.ServiceName = ServiceName;
            if (AttributeStringValueLimit > 0)
            {
                config.AttributeStringValueLimit = AttributeStringValueLimit;
            }
            if (AttributeArrayLengthLimit > 0)
            {
                config.AttributeArrayLengthLimit = AttributeArrayLengthLimit;
            }
            if (AttributeCountLimit > 0)
            {
                config.AttributeCountLimit = AttributeCountLimit;
            }
            config.EnabledMetrics = EnabledMetrics;
        }

        private Regex[] ConvertTracePropagationUrls(string[] urls)
        {
            if (urls == null)
            {
                return null;
            }

            var regexes = new Regex[urls.Length];
            for (int i = 0; i < urls.Length; i++)
            {
                try
                {
                    regexes[i] = new Regex(urls[i]);
                }
                catch (Exception e)
                {
                    MainThreadDispatchBehaviour.LogWarning("Error converting TracePropagationUrl " + urls[i] + " into a regex pattern in settings object: " + e.Message);
                }
            }

            return regexes;
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
            if (EnabledReleaseStages != null && EnabledReleaseStages.Length > 0)
            {
                config.EnabledReleaseStages = EnabledReleaseStages;
            }
            config.AppVersion = AppVersion;
            config.BundleVersion = BundleVersion;
            config.VersionCode = VersionCode;
            config.GenerateAnonymousId = GenerateAnonymousId;

            return config;
        }

        private PerformanceConfiguration GetSettingsFromNotifier(out bool autoStart)
        {
            var notifierSettings = Resources.Load("Bugsnag/BugsnagSettingsObject");

            var apiKey = (string)GetValueFromNotifer(notifierSettings, "ApiKey");

            var config = new PerformanceConfiguration(apiKey);

            config.ReleaseStage = (string)GetValueFromNotifer(notifierSettings, "ReleaseStage");

            if (string.IsNullOrEmpty(config.ReleaseStage))
            {
                config.ReleaseStage = Debug.isDebugBuild ? "development" : "production";
            }

            var notifierReleaseStages = (string[])GetValueFromNotifer(notifierSettings, "EnabledReleaseStages");

            if (notifierReleaseStages != null && notifierReleaseStages.Length > 0)
            {
                config.EnabledReleaseStages = notifierReleaseStages;
            }

            config.AppVersion = (string)GetValueFromNotifer(notifierSettings, "AppVersion");
            config.BundleVersion = (string)GetValueFromNotifer(notifierSettings, "BundleVersion");
            config.VersionCode = (int)GetValueFromNotifer(notifierSettings, "VersionCode");
            config.GenerateAnonymousId = (bool)GetValueFromNotifer(notifierSettings, "GenerateAnonymousId");


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