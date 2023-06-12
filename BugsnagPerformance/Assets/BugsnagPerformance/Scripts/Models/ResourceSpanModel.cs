using System;
namespace BugsnagUnityPerformance
{
    [Serializable]
    internal class ResourceSpanModel
    {
        public ScopeSpanModel[] scopeSpans;

        public ResourceModel resource;

        public ResourceSpanModel(ResourceModel resourceModel, ScopeSpanModel[] scopeSpans)
        {
            this.resource = resourceModel;
            this.scopeSpans = scopeSpans;
        }
    }
}
