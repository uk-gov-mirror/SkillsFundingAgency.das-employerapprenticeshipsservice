using System;

namespace SFA.DAS.Authorization
{
    [Obsolete]
    public interface IAuthorizationContextCache
    {
        IAuthorizationContext GetAuthorizationContext();
        void SetAuthorizationContext(IAuthorizationContext authorizationContext);
    }
}