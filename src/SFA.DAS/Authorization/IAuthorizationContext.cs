using System;

namespace SFA.DAS.Authorization
{
    [Obsolete]
    public interface IAuthorizationContext
    {
        IAccountContext AccountContext { get; }
        IMembershipContext MembershipContext { get; }
        IUserContext UserContext { get; }
    }
}