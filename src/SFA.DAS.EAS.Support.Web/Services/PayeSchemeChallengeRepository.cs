using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EAS.Support.Web.Models;
using SFA.DAS.Support.Shared.Challenge;

namespace SFA.DAS.EAS.Support.Web.Services
{
    public class PayeSchemeChallengeRepository : IChallengeRepository<PayeSchemeChallengeViewModel>
    {
        private readonly int _challengeTimeoutMinutes;
        private readonly Dictionary<Guid, PayeSchemeChallengeViewModel> _challenges;

        public PayeSchemeChallengeRepository(Dictionary<Guid, PayeSchemeChallengeViewModel> challenges, int challengeTimeoutMinutes)
        {
            _challenges = challenges;
            _challengeTimeoutMinutes = challengeTimeoutMinutes< 1? 1: challengeTimeoutMinutes;
        }

        public async Task<PayeSchemeChallengeViewModel> Retrieve(Guid challengeId)
        {
            if (_challenges.ContainsKey(challengeId))
            {
                return await Task.FromResult(_challenges[challengeId]);
            }

            return await Task.FromResult(null as PayeSchemeChallengeViewModel);
        }

      
        public Task Store(PayeSchemeChallengeViewModel challenge)
        {
            if (_challenges.ContainsKey(challenge.ChallengeId))
            {
                _challenges[challenge.ChallengeId] = challenge;
            }
            else
            {
                _challenges.Add(challenge.ChallengeId, challenge);
            }

            return Task.CompletedTask;
        }
    }
}