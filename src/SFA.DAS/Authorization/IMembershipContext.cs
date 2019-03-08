using System;

namespace SFA.DAS.Authorization
{
    [Obsolete]
    public interface IMembershipContext
    {
        Role Role { get; }
    }
}