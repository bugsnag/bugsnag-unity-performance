using UnityEngine;

namespace BugsnagUnityPerformance
{
    public class Sampler : MonoBehaviour
    {
        public double probability;

        public bool Sampled(Span span)
        {
            double p = probability;
            bool isSampled = IsSampled(span, GetUpperBound(p));
            if (isSampled)
            {
                span.UpdateSamplingProbability(p);
            }
            return isSampled;
        }

        private bool IsSampled(Span span, uint upperBound)
        {
            uint traceId = uint.Parse(span.TraceId.Substring(0, 15), System.Globalization.NumberStyles.HexNumber);
            return traceId <= upperBound;
        }

        private uint GetUpperBound(double p)
        {
            if (p <= 0)
            {
                return 0;
            }
            if (p >= 1.0)
            {
                return uint.MaxValue;
            }
            return (uint)(p * (double)uint.MaxValue);
        }
    }
}
