using System;
namespace BugsnagPerformance
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
