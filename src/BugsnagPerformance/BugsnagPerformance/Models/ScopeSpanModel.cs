using System;
namespace BugsnagUnityPerformance
{
    [Serializable]
    internal class ScopeSpanModel
    {
        public SpanModel[] spans;

        public ScopeSpanModel(SpanModel[] spans)
        {
            this.spans = spans;
        }
    }
}
