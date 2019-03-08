using System;
using System.Threading.Tasks;

namespace SFA.DAS.Authorization
{
    [Obsolete]
    public interface IAuthorizationHandler
    {
        Task<AuthorizationResult> CanAccessAsync(IAuthorizationContext authorizationContext, Feature feature);
    }
}