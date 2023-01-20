using System;
namespace BugsnagUnityPerformance
{
    [Serializable]
    public class AttributeValueModel
    {
        
    }

    [Serializable]
    public class AttributeStringValueModel : AttributeValueModel
    {

        public string stringValue;

        public AttributeStringValueModel(string theValue)
        {
            stringValue = theValue;
        }
    }
}
