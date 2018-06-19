using System.Collections.Generic;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.Domain.Models.UserProfile;

namespace SFA.DAS.EAS.TestCommon.Steps
{
    public class ObjectContext
    {
        public Dictionary<string, Account> Accounts = new Dictionary<string, Account>();
        public Dictionary<string, User> Users = new Dictionary<string, User>();
    }
}