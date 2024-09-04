namespace BugsnagUnityPerformance
{
    public class Sampler : IPhasedStartup
    {
        private PersistentState _persistentState;

        private double _probability = -1;

        public double Probability
        {
            get
            {
                return _probability;
            }
            set
            {
                _probability = value;
                _persistentState.Probability = value;
            }
        }

        public Sampler(PersistentState persistentState)
        {
            _persistentState = persistentState;
        }

        public void Configure(PerformanceConfiguration config)
        {
            if (config.IsFixedSamplingProbability)
            {
                _probability = config.SamplingProbability;
            }
        }

        public void Start()
        {
            // If the probability is not set, try to load it from the persistent state, otherwise set it to 1.0
            if (_probability < 0)
            {
                var storedProbability = _persistentState.Probability;
                if (storedProbability >= 0)
                {
                    _probability = storedProbability;
                }
                else
                {
                    _probability = 1.0;
                    _persistentState.Probability = _probability;
                }
            }
        }

        public bool Sampled(Span span, bool shouldAddAttribute = true)
        {
            var p = Probability;
            var isSampled = IsSampled(span, GetUpperBound(p));
#if BUGSNAG_DEBUG
            Logger.I(string.Format("Span {0} is sampled: {1} with p value: {2}", span.Name, isSampled, p));
#endif
            if (isSampled && shouldAddAttribute)
            {
                span.UpdateSamplingProbability(p);
                span.SetAttributeInternal("bugsnag.sampling.p", p);
            }
            return isSampled;
        }

        private bool IsSampled(Span span, ulong upperBound)
        {
            var traceId = ulong.Parse(span.TraceId.Substring(0, 16), System.Globalization.NumberStyles.HexNumber);
            return traceId <= upperBound;
        }

        private ulong GetUpperBound(double p)
        {
            if (p <= 0)
            {
                return 0;
            }
            if (p >= 1.0)
            {
                return ulong.MaxValue;
            }
            return (ulong)(p * (double)ulong.MaxValue);
        }
    }
}
