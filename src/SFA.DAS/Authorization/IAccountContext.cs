using System;

namespace SFA.DAS.Authorization
{
    [Obsolete]
    public interface IAccountContext
    {
        long Id { get; }
        string HashedId { get; }
        string PublicHashedId { get; }
    }
}