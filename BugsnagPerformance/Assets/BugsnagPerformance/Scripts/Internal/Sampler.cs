using UnityEngine;

namespace BugsnagUnityPerformance
{
    public class Sampler : IPhasedStartup
    {
        public double probability;

        public void Configure(PerformanceConfiguration config)
        {
            probability = config.SamplingProbability;
        }

        public void Start()
        {
            // Nothing to do
        }

        public bool Sampled(Span span)
        {
            var p = probability;
            var isSampled = IsSampled(span, GetUpperBound(p));
            if (isSampled)
            {
                span.UpdateSamplingProbability(p);
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
