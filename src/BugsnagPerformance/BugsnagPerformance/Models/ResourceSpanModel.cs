using System;
namespace BugsnagUnityPerformance
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
