using System;
namespace BugsnagUnityPerformance
{
    [Serializable]
    internal class ResourceModel
    {

        public AttributeModel[] attributes;

        public ResourceModel()
        {
            attributes = new AttributeModel[]
           {
                new AttributeModel()
                {
                    key = "deployment.environment",
                    value = new AttributeStringValueModel( BugsnagPerformance.Configuration.ReleaseStage )
                }
           };
        }
    }
}
