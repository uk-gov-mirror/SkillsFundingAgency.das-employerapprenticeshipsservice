using System;

namespace SFA.DAS.Authorization
{
    [Obsolete]
    public class MembershipContext : IMembershipContext
    {
        public Role Role { get; set; }
    }
}