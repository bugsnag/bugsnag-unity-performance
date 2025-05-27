using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BugsnagPerformanceEditor")]

namespace BugsnagUnityPerformance
{
    [Serializable]
    public class EnabledMetrics
    {
        public bool Rendering = false;
        public bool CPU = false;
        public bool Memory = false;
    }

    public class PerformanceConfiguration
    {

        private const string LEGACY_DEFAULT_ENDPOINT = "https://otlp.bugsnag.com/v1/traces";
        private const string DEFAULT_ENDPOINT = "https://{0}.otlp.bugsnag.com/v1/traces";
        private const string HUB_ENDPOINT = "https://{0}.insighthub.smartbear.com/v1/traces";
        private const string HUB_API_PREFIX = "00000";

        internal const int DEFAULT_ATTRIBUTE_STRING_VALUE_LIMIT = 1024;
        private const int MAXIMUM_ATTRIBUTE_STRING_VALUE_LIMIT = 10000;
        internal const int DEFAULT_ATTRIBUTE_ARRAY_LENGTH_LIMIT = 1000;
        private const int MAXIMUM_ATTRIBUTE_ARRAY_LENGTH_LIMIT = 10000;
        internal const int DEFAULT_ATTRIBUTE_COUNT_LIMIT = 128;
        private const int MAXIMUM_ATTRIBUTE_COUNT_LIMIT = 1000;


        public PerformanceConfiguration(string apiKey)
        {
            ApiKey = apiKey;
        }

        //Internal config

        internal int MaxBatchSize = 100;
        private float _maxBatchAgeSeconds = 30f;
        internal float MaxBatchAgeSeconds
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return 5;
#else
                return _maxBatchAgeSeconds;
#endif
            }
            set
            {
                _maxBatchAgeSeconds = value;
            }
        }
        internal int MaxPersistedBatchAgeSeconds = 86400; //24 hours
        internal float PValueTimeoutSeconds = 86400f;
        internal float PValueCheckIntervalSeconds = 30f;

        //Public config
        private int _attributeStringValueLimit = DEFAULT_ATTRIBUTE_STRING_VALUE_LIMIT;
        public int AttributeStringValueLimit
        {
            get => _attributeStringValueLimit;
            set
            {
                if (value > 0 && value <= MAXIMUM_ATTRIBUTE_STRING_VALUE_LIMIT)
                {
                    _attributeStringValueLimit = value;
                }
                else
                {
                    MainThreadDispatchBehaviour.LogWarning("AttributeStringValueLimit must be greater than 0 and no larger than " + MAXIMUM_ATTRIBUTE_STRING_VALUE_LIMIT);
                }
            }
        }

        private int _attributeArrayLengthLimit = DEFAULT_ATTRIBUTE_ARRAY_LENGTH_LIMIT;
        public int AttributeArrayLengthLimit
        {
            get => _attributeArrayLengthLimit;
            set
            {
                if (value > 0 && value <= MAXIMUM_ATTRIBUTE_ARRAY_LENGTH_LIMIT)
                {
                    _attributeArrayLengthLimit = value;
                }
                else
                {
                    MainThreadDispatchBehaviour.LogWarning("AttributeArrayLengthLimit must be greater than 0 and no larger than " + MAXIMUM_ATTRIBUTE_ARRAY_LENGTH_LIMIT);

                }
            }
        }

        private int _attributeCountLimit = DEFAULT_ATTRIBUTE_COUNT_LIMIT;
        public int AttributeCountLimit
        {
            get => _attributeCountLimit;
            set
            {
                if (value > 0 && value <= MAXIMUM_ATTRIBUTE_COUNT_LIMIT)
                {
                    _attributeCountLimit = value;
                }
                else
                {
                    MainThreadDispatchBehaviour.LogWarning("AttributeCountLimit must be greater than 0 and no larger than " + MAXIMUM_ATTRIBUTE_COUNT_LIMIT);
                }
            }
        }

        public string ApiKey;

        public AutoInstrumentAppStartSetting AutoInstrumentAppStart = AutoInstrumentAppStartSetting.FULL;
        public string AppVersion = Application.version;
        public int VersionCode = -1;
        public string BundleVersion;

        public bool GenerateAnonymousId = true;

        public string[] EnabledReleaseStages;
        [Obsolete("AutoInstrumentRendering is deprecated and will be removed in a future version. Please use EnabledMetrics.Rendering instead.")]
        public bool AutoInstrumentRendering { get => EnabledMetrics.Rendering; set => EnabledMetrics.Rendering = value; }
        public EnabledMetrics EnabledMetrics = new EnabledMetrics();

        public string Endpoint = string.Empty;

        public Func<BugsnagNetworkRequestInfo, BugsnagNetworkRequestInfo> NetworkRequestCallback;

        public string ReleaseStage = Debug.isDebugBuild ? "development" : "production";

        public Regex[] TracePropagationUrls;

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

        public double SamplingProbability = -1.0;

        internal bool IsFixedSamplingProbability => SamplingProbability >= 0;

        public string ServiceName = string.Empty;
        private bool IsHubApiKey(string apiKey)
        {
            return !string.IsNullOrEmpty(apiKey) && apiKey.StartsWith(HUB_API_PREFIX);
        }
        public string GetEndpoint()
        {
            if (string.IsNullOrEmpty(Endpoint) || Endpoint == LEGACY_DEFAULT_ENDPOINT)
            {
                return string.Format(IsHubApiKey(ApiKey) ? HUB_ENDPOINT : DEFAULT_ENDPOINT, ApiKey);
            }
            return Endpoint;
        }

    }
}