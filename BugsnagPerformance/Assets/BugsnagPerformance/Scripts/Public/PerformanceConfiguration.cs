using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    public class PerformanceConfiguration
    {

        //Internal config

        internal int MaxBatchSize = 100;
        internal float MaxBatchAgeSeconds = 30f;
        internal int MaxPersistedBatchAgeSeconds = 86400; //24 hours
        internal float PValueTimeoutSeconds = 86400f;
        internal float PValueCheckIntervalSeconds = 30f;
        internal double SamplingProbability = 1.0;

        //Public config

        public string ApiKey;

        public AutoInstrumentAppStartSetting AutoInstrumentAppStart = AutoInstrumentAppStartSetting.FULL;

        public string AppVersion = Application.version;
        public int VersionCode = -1;
        public string BundleVersion;
      
        public bool GenerateAnonymousId = true;

        public string[] EnabledReleaseStages;

        public string Endpoint = "https://otlp.bugsnag.com/v1/traces";

        public Func<BugsnagNetworkRequestInfo, BugsnagNetworkRequestInfo> NetworkRequestCallback;

        public string ReleaseStage = Debug.isDebugBuild ? "development" : "production";

        public String[] TracePropagationUrlMatchPatterns;


        public void AddOnSpanEnd(Func<Span, bool> callback)
        {
            _onSpanEndCallbacks.Add(callback);
        }

        public void RemoveOnSpanEnd(Func<Span, bool> callback)
        {
            _onSpanEndCallbacks.Remove(callback);
        }

        internal List<Func<Span, bool>> GetOnSpanEndCallbacks()
        {
            return _onSpanEndCallbacks;
        }
        
        private List<Func<Span, bool>> _onSpanEndCallbacks = new List<Func<Span, bool>>();

        public PerformanceConfiguration(string apiKey)
        {
            ApiKey = apiKey;
        }

      
    }
}