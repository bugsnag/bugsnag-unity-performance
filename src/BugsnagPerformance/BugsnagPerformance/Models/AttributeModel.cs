using System;
namespace BugsnagUnityPerformance
{
    [Serializable]
    internal class AttributeModel
    {
        public string key;
        // need to work out a way to support serialising attributes with different value types
        public AttributeStringValueModel value;
    }
}
