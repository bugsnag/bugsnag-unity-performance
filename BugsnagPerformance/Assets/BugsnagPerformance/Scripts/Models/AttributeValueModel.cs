
using System.Collections.Generic;

namespace BugsnagUnityPerformance
{

    public class AttributeValueModel { }

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
        public ArrayValueModel arrayValue;

        public AttributeStringArrayValueModel(string[] values)
        {
            arrayValue = new ArrayValueModel(values);
        }
    }

    public class AttributeIntArrayValueModel : AttributeValueModel
    {
        public ArrayValueModel arrayValue;

        public AttributeIntArrayValueModel(int[] values)
        {
            arrayValue = new ArrayValueModel(values);
        }
    }

    public class AttributeBoolArrayValueModel : AttributeValueModel
    {
        public ArrayValueModel arrayValue;

        public AttributeBoolArrayValueModel(bool[] values)
        {
            arrayValue = new ArrayValueModel(values);
        }
    }

    public class AttributeDoubleArrayValueModel : AttributeValueModel
    {
        public ArrayValueModel arrayValue;

        public AttributeDoubleArrayValueModel(double[] values)
        {
            arrayValue = new ArrayValueModel(values);
        }
    }

    public class ArrayValueModel
    {
        public List<AttributeValueModel> values;

        public ArrayValueModel(string[] values)
        {
            this.values = new List<AttributeValueModel>();
            foreach (var value in values)
            {
                this.values.Add(new AttributeStringValueModel(value));
            }
        }

        public ArrayValueModel(int[] values)
        {
            this.values = new List<AttributeValueModel>();
            foreach (var value in values)
            {
                this.values.Add(new AttributeIntValueModel(value));
            }
        }

        public ArrayValueModel(bool[] values)
        {
            this.values = new List<AttributeValueModel>();
            foreach (var value in values)
            {
                this.values.Add(new AttributeBoolValueModel(value));
            }
        }

        public ArrayValueModel(double[] values)
        {
            this.values = new List<AttributeValueModel>();
            foreach (var value in values)
            {
                this.values.Add(new AttributeDoubleValueModel(value));
            }
        }
    }
}
