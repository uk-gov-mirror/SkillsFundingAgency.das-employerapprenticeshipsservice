using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.EAS.Support.ApplicationServices;
using SFA.DAS.EAS.Support.ApplicationServices.Services;
using SFA.DAS.EAS.Support.Infrastructure.Services;
using SFA.DAS.EAS.Support.Web.Services;
using SFA.DAS.Support.Shared.Authentication;
using SFA.DAS.Support.Shared.Challenge;
using SFA.DAS.Support.Shared.Navigation;
using StructureMap;

namespace SFA.DAS.EAS.Support.Web.DependencyResolution
{
    [ExcludeFromCodeCoverage]
    public class ApplicationRegistry : Registry
    {
        public ApplicationRegistry()
        {
            For<IAccountHandler>().Use<AccountHandler>();
            For<IChallengeRepository>().Use<ChallengeRepository>();
            For<IChallengeEvaluator>().Use<ChallengeEvaluator>();
            For<IDatetimeService>().Use<DatetimeService>();
            For<IChallengeHandler>().Use<ChallengeHandler>();
            For<ILevySubmissionsRepository>().Use<LevySubmissionsRepository>();
            For<IPayeLevySubmissionsHandler>().Use<PayeLevySubmissionsHandler>();
            For<IPayeLevyMapper>().Use<PayeLevyMapper>();

            For<IServiceAddressMapper>().Use<ServiceAddressMapper>();
            For<IChallengeService>().Singleton().Use(c=> new InMemoryChallengeService(new Dictionary<Guid, SupportAgentChallenge>(),c.GetInstance<IChallengeSettings>() ));
            For<IIdentityHandler>().Use<RequestHeaderIdentityHandler>();
            For<IIdentityHash>().Use<IdentityHash>();
        }
    }
}