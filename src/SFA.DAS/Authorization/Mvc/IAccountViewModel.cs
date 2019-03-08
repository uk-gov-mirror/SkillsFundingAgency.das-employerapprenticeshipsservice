using System;

namespace SFA.DAS.Authorization.Mvc
{
    [Obsolete]
    public interface IAccountViewModel
    {
        long AccountId { get; set; }
        string AccountHashedId { get; set; }
    }
}