using System;

namespace BugsnagUnityPerformance
{

    public class AttributeValueModel{}

    public class AttributeStringValueModel : AttributeValueModel
    {
        public string stringValue;

        public AttributeStringValueModel(string theValue)
        {
            stringValue = theValue;
        }
    }

    public class AttributeIntValueModel : AttributeValueModel
    {
        public string intValue;

        public AttributeIntValueModel(int theValue)
        {
            intValue = theValue.ToString();
        }
    }

    public class AttributeBoolValueModel : AttributeValueModel
    {
        public bool boolValue;

        public AttributeBoolValueModel(bool theValue)
        {
            boolValue = theValue;
        }
    }

    public class AttributeDoubleValueModel : AttributeValueModel
    {
        public double doubleValue;

        public AttributeDoubleValueModel(double theValue)
        {
            doubleValue = theValue;
        }
    }
}
