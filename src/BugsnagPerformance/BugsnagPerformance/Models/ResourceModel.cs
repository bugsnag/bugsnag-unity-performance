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
                },
                new AttributeModel()
                {
                    key = "telemetry.sdk.name",
                    value = new AttributeStringValueModel( "bugsnag.performance.unity" )
                },
                new AttributeModel()
                {
                    key = "telemetry.sdk.version",
                    value = new AttributeStringValueModel( "1.1" )
                }
           };
        }
    }
}
