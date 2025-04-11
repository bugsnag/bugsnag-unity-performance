namespace BugsnagUnityPerformance
{

    public class AttributeModel
    {
        public string key;
        public AttributeValueModel value;

        public AttributeModel(string key, AttributeValueModel value)
        {
            this.key = key;
            this.value = value;
        }

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
    }


}

