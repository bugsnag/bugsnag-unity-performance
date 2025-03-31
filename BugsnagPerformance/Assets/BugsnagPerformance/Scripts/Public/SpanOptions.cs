using System;
namespace BugsnagUnityPerformance
{
    public class SpanMetrics
    {
        public bool Rendering = false;
        public bool CPU = false;
        public bool Memory = false;
    }

    public class SpanOptions
    {
        public DateTimeOffset StartTime = DateTimeOffset.UtcNow;
        public ISpanContext ParentContext = null;
        public bool MakeCurrentContext = true;
        public bool? IsFirstClass = null;
        public SpanMetrics SpanMetrics = null;
    }
}
