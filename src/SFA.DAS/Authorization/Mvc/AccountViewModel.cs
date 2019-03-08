using System;
using AutoMapper;

namespace SFA.DAS.Authorization.Mvc
{
    [Obsolete]
    public abstract class AccountViewModel : IAccountViewModel
    {
        [IgnoreMap]
        public long AccountId { get; set; }

        [IgnoreMap]
        public string AccountHashedId { get; set; }
    }
}