using System;

namespace SFA.DAS.Authorization
{
    [Obsolete]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class FeatureAttribute : Attribute
    {
        public FeatureType FeatureType { get; set; }

        public FeatureAttribute(FeatureType featureType)
        {
            FeatureType = featureType;
        }
    }
}