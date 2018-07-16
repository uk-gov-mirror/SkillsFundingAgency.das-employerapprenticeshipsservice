using System;
using System.Threading.Tasks;
using SFA.DAS.EAS.Support.Web.Models;

namespace SFA.DAS.EAS.Support.Web.Services
{
    public interface IChallengeRepository<T> where T : ChallengeViewModelBase
    {
        Task<T> Retrieve(Guid challengeId);
        Task Store(T challenge);
    }
}