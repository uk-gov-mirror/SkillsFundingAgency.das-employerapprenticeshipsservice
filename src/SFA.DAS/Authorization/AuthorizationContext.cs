using System;

namespace SFA.DAS.Authorization
{
    [Obsolete]
    public class AuthorizationContext : IAuthorizationContext
    {
        public IAccountContext AccountContext { get; set; }
        public IMembershipContext MembershipContext { get; set; }
        public IUserContext UserContext { get; set; }
    };
}