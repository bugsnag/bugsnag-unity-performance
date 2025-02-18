using System;
namespace BugsnagUnityPerformance
{
    public class SpanOptions
    {
        public DateTimeOffset StartTime = DateTimeOffset.UtcNow;
        public ISpanContext ParentContext = null;
        public bool MakeCurrentContext = true;
        public bool? IsFirstClass = null;
        public bool? InstrumentRendering = null;
    }
}
