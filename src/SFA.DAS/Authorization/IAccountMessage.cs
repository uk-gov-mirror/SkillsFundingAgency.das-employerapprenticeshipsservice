using System;

namespace SFA.DAS.Authorization
{
    [Obsolete]
    public interface IAccountMessage
    {
        string AccountHashedId { get; set; }
        long? AccountId { get; set; }
    }
}