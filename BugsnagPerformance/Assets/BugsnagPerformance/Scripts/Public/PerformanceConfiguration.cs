using UnityEngine;

namespace BugsnagUnityPerformance
{
    public class PerformanceConfiguration
    {

        //Internal config

        internal int MaxBatchSize = 100;
        internal float MaxBatchAgeSeconds = 30f;
        internal int MaxPersistedBatchAgeSeconds = 86400; //24 hours

        //Public config

        private string _apiKey;

        public string ApiKey
        {
            get
            {
                return _apiKey;
            }
            set
            {
                if (ValidateApiKey(value))
                {
                    _apiKey = value;
                }
                else
                {
                    throw new System.Exception($"Invalid Bugsnag Performance configuration. apiKey should be a 32-character hexademical string, got {value} ");
                }
            }
        }

        public enum AutoInstrumentAppStartSetting
        {
            OFF,
            START_ONLY,
            FULL
        }

        public AutoInstrumentAppStartSetting AutoInstrumentAppStart = AutoInstrumentAppStartSetting.FULL;

        public string Endpoint = "https://otlp.bugsnag.com/v1/traces";

        public string ReleaseStage = Debug.isDebugBuild ? "development" : "production";

        public double SamplingProbability = 1.0;

        public PerformanceConfiguration(string apiKey)
        {
            ApiKey = apiKey;
        }

        private bool ValidateApiKey(string apiKey)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(apiKey, @"\A\b[0-9a-fA-F]+\b\Z") &&
                apiKey.Length == 32;
        }
    }
}
