using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BugsnagUnityPerformance
{
    internal class TracePayloadGenerator : MonoBehaviour
    {
        public static TracePayload GenerateTracePayload(ResourceModel resourceModel, List<Span> spans, PerformanceConfiguration config )
        {
            return new TracePayload(resourceModel, spans, config.IsFixedSamplingProbability, config.AttributeArrayLengthLimit, config.AttributeStringValueLimit);
        }
    }
}