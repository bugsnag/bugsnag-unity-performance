using System;
namespace BugsnagUnityPerformance
{
    [Serializable]
    internal class ResourceSpanModel
    {
        public ScopeSpanModel[] scopeSpans;

        public ResourceModel resource;

        public ResourceSpanModel(ScopeSpanModel[] scopeSpans)
        {
            this.resource = new ResourceModel();
            this.scopeSpans = scopeSpans;
        }
    }
}
