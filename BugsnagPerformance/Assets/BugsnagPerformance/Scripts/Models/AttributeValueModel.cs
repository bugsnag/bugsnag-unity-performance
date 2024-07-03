
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

    public class AttributeStringArrayValueModel : AttributeValueModel
    {
        public string[] stringArrayValue;

        public AttributeStringArrayValueModel(string[] theValue)
        {
            stringArrayValue = theValue;
        }
    }

    public class AttributeIntArrayValueModel : AttributeValueModel
    {
        public int[] intArrayValue;

        public AttributeIntArrayValueModel(int[] theValue)
        {
            intArrayValue = theValue;
        }
    }

    public class AttributeBoolArrayValueModel : AttributeValueModel
    {
        public bool[] boolArrayValue;

        public AttributeBoolArrayValueModel(bool[] theValue)
        {
            boolArrayValue = theValue;
        }
    }

    public class AttributeDoubleArrayValueModel : AttributeValueModel
    {
        public double[] doubleArrayValue;

        public AttributeDoubleArrayValueModel(double[] theValue)
        {
            doubleArrayValue = theValue;
        }
    }
}
