using System.Diagnostics.CodeAnalysis;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Support.Infrastructure.Services;
using SFA.DAS.EAS.Support.Infrastructure.Settings;
using SFA.DAS.EAS.Support.Web.Configuration;
using SFA.DAS.HashingService;
using SFA.DAS.TokenService.Api.Client;
using StructureMap;

namespace SFA.DAS.EAS.Support.Web.DependencyResolution
{
    [ExcludeFromCodeCoverage]
    public class InfrastuctureRegistry : Registry
    {
        public InfrastuctureRegistry()
        {
           


            For<IAccountRepository>().Use<AccountRepository>();
            For<IChallengeRepository>().Use<ChallengeRepository>();

            For<IAccountApiClient>().Use<AccountApiClient>();

            For<IAccountApiClient>().Use<AccountApiClient>();

            For<ILevyTokenHttpClientFactory>().Use<LevyTokenHttpClientMaker>();

            For<IHmrcApiBaseUrlConfig>().Use(string.Empty, (ctx) =>
            {
                return ctx.GetInstance<IWebConfiguration>().LevySubmission.HmrcApiBaseUrlSetting;
            });

            For<ITokenServiceApiClientConfiguration>().Use(string.Empty, (ctx) =>
            {
                return ctx.GetInstance<IWebConfiguration>().LevySubmission.LevySubmissionsApiConfig;
            });


            For<IHashingService>().Use(string.Empty, (ctx) =>
            {
                var hashServiceconfig = ctx.GetInstance<IWebConfiguration>().HashingService;
                return new HashingService.HashingService(hashServiceconfig.AllowedCharacters, hashServiceconfig.Hashstring);
            });
            
        }

       

    }
}