using System;

namespace SFA.DAS.Authorization
{
    [Obsolete]
    public enum AuthorizationResult
    {
        Ok,
        FeatureDisabled,
        FeatureAgreementNotSigned,
        FeatureUserNotWhitelisted
    }
}