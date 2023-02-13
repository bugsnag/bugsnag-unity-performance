using System;
namespace BugsnagUnityPerformance
{
    internal class Tracer
    {

        public void OnSpanEnd(Span span)
        {
            var payload = new TracePayload(span);
            if (BugsnagPerformance.IsStarted)
            {
                BugsnagPerformance.Delivery.Deliver(payload);
            }
            else
            {
                //TODO Batch the span
            }
        }
            
    }
}
