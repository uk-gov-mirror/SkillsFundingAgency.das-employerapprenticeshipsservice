using System;

namespace SFA.DAS.Authorization
{
    [Obsolete]
    public interface IUserContext
    {
        long Id { get; }
        Guid Ref { get; }
        string Email { get; }
    }
}