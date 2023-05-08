namespace BugsnagUnityPerformance
{

    internal class AttributeModel
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
    }

}

