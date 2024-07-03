namespace BugsnagUnityPerformance
{

    public class AttributeModel
    {
        public string key;

        public AttributeValueModel value;

        public AttributeModel() { }

        public AttributeModel(string key, string value)
        {
            this.key = key;
            this.value = new AttributeStringValueModel(value);
        }

        public AttributeModel(string key, int value)
        {
            this.key = key;
            this.value = new AttributeIntValueModel(value);
        }

        public AttributeModel(string key, bool value)
        {
            this.key = key;
            this.value = new AttributeBoolValueModel(value);
        }

        public AttributeModel(string key, double value)
        {
            this.key = key;
            this.value = new AttributeDoubleValueModel(value);
        }

        public AttributeModel(string key, string[] value)
        {
            this.key = key;
            this.value = new AttributeStringArrayValueModel(value);
        }

        public AttributeModel(string key, int[] value)
        {
            this.key = key;
            this.value = new AttributeIntArrayValueModel(value);
        }

        public AttributeModel(string key, bool[] value)
        {
            this.key = key;
            this.value = new AttributeBoolArrayValueModel(value);
        }

        public AttributeModel(string key, double[] value)
        {
            this.key = key;
            this.value = new AttributeDoubleArrayValueModel(value);
        }
    }

}

