using System;
namespace BugsnagPerformance
{
    [Serializable]
    internal class ResourceSpanModel
    {
        public ScopeSpanModel[] scopeSpans;

        public ResourceSpanModel(ScopeSpanModel[] scopeSpans)
        {
            this.scopeSpans = scopeSpans;
        }
    }
}
