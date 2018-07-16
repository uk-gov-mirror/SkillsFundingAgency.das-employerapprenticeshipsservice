using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EAS.Support.Web.Models;

namespace SFA.DAS.EAS.Support.Web.Services
{
    public class InMemoryChallengeService : IChallengeService
    {
        private readonly Dictionary<Guid, SupportAgentChallenge> _challenges;
        private readonly int _challengeTimeoutMinutes;

        public InMemoryChallengeService(Dictionary<Guid, SupportAgentChallenge> challenges, int challengeTimeoutMinutes)
        {
            _challenges = challenges;
            _challengeTimeoutMinutes = challengeTimeoutMinutes;
        }
        
        public async Task<Guid> IsNeeded(string identity, string entityType, string entityKey)
        {
            if (_challenges.Values.FirstOrDefault(x =>
                   x.Identity == identity
                   && x.EntityType == entityType
                   && x.EntityKey == entityKey
                   && x.Expires > DateTimeOffset.UtcNow) == null)
            {
                var challenge = new SupportAgentChallenge()
                {
                    Id = Guid.NewGuid(),
                    Identity = identity,
                    EntityType = entityType,
                    EntityKey = entityKey,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(_challengeTimeoutMinutes)
                };
                _challenges.Add(challenge.Id, challenge);
                return await Task.FromResult(challenge.Id);
            }

            return await Task.FromResult(Guid.Empty);
        }

        public Task Store(SupportAgentChallenge challenge)
        {
            if (_challenges.ContainsKey(challenge.Id))
            {
                _challenges[challenge.Id] = challenge;
            }
            else
            {
                _challenges.Add(challenge.Id, challenge);
            }

            return Task.CompletedTask;
        }
    }
}