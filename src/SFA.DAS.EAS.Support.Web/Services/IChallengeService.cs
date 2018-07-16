using System;
using System.Threading.Tasks;
using SFA.DAS.EAS.Support.Web.Models;

namespace SFA.DAS.EAS.Support.Web.Services
{
    public interface IChallengeService
    {
        Task<Guid> IsNeeded(
            string identity,
            string entityType,
            string entityKey
            );

        Task Store(SupportAgentChallenge challenge);
    }
}