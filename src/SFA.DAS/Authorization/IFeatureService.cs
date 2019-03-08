using System;

namespace SFA.DAS.Authorization
{
    [Obsolete]
    public interface IFeatureService
    {
        Feature GetFeature(FeatureType featureType);
    }
}