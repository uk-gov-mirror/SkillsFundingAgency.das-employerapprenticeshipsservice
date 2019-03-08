using System;
using System.Threading.Tasks;

namespace SFA.DAS.Authorization
{
    [Obsolete]
    public interface IAuthorizationService
    {
        IAuthorizationContext GetAuthorizationContext();
        Task<IAuthorizationContext> GetAuthorizationContextAsync();
        AuthorizationResult GetAuthorizationResult(FeatureType featureType);
        Task<AuthorizationResult> GetAuthorizationResultAsync(FeatureType featureType);
        bool IsAuthorized(FeatureType featureType);
        Task<bool> IsAuthorizedAsync(FeatureType featureType);
        void ValidateMembership();
    }
}