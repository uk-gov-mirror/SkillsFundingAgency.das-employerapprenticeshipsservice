// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultRegistry.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace SFA.DAS.EAS.Support.Web.DependencyResolution
{
    using Microsoft.Azure;
    using SFA.DAS.Configuration;
    using SFA.DAS.Configuration.AzureTableStorage;
    using SFA.DAS.EAS.Account.Api.Client;
    using SFA.DAS.EAS.Support.Infrastructure.DependencyResolution;
    using SFA.DAS.EAS.Support.Web.Configuration;
    using SFA.DAS.EAS.Support.Web.Models;
    using SFA.DAS.EAS.Support.Web.Services;
    using SFA.DAS.NLog.Logger;
    using SFA.DAS.Support.Shared.Authentication;
    using SFA.DAS.Support.Shared.Challenge;
    using SFA.DAS.Support.Shared.Discovery;
    using SFA.DAS.Support.Shared.Navigation;
    using SFA.DAS.Support.Shared.SiteConnection;
    using SFA.DAS.TokenService.Api.Client;
    using StructureMap;
    using StructureMap.Configuration.DSL;
    using StructureMap.Graph;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net.Http;
    using System.Web;

    [ExcludeFromCodeCoverage]
    public class DefaultRegistry : Registry
    {

        private const string ServiceName = "SFA.DAS.Support.EAS";
        private const string Version = "1.0";

        #region Constructors and Destructors

        public DefaultRegistry()
        {
            Scan(
                scan =>
                {
                    scan.TheCallingAssembly();
                    scan.WithDefaultConventions();
                    scan.With(new ControllerConvention());
                });
            For<ILoggingPropertyFactory>().Use<LoggingPropertyFactory>();


            HttpContextBase conTextBase = null;
            if (HttpContext.Current != null)
                conTextBase = new HttpContextWrapper(HttpContext.Current);

            For<IWebLoggingContext>().Use(x => new WebLoggingContext(conTextBase));

            For<ILog>().Use(x => new NLogLogger(
                x.ParentType,
                x.GetInstance<IWebLoggingContext>(),
                x.GetInstance<ILoggingPropertyFactory>().GetProperties())).AlwaysUnique();

            WebConfiguration configuration = GetConfiguration();

            For<IWebConfiguration>().Use(configuration);
            For<IAccountApiConfiguration>().Use(configuration.AccountApi);
            For<HttpClient>().AlwaysUnique().Use(c => new HttpClient());

            For<ISiteConnectorSettings>().Use(configuration.SiteConnector);
            
            For<ISiteValidatorSettings>().Use(configuration.SiteValidator);
            For<ISiteSettings>().Use(configuration.Site);
            For<ICryptoSettings>().Use(configuration.Crypto);
            

            Uri portalUri = new Uri(
                configuration.Site.BaseUrls
                    .Split(',').FirstOrDefault(x => x.StartsWith($"{SupportServiceIdentity.SupportPortal}"))?
                    .Split('|').LastOrDefault() ?? "/", UriKind.RelativeOrAbsolute);

            For<Uri>().Singleton().Use((portalUri));



            For<ISiteConnector>().Use<SiteConnector>();
            For<IHttpStatusCodeStrategy>().Use<StrategyForSystemErrorStatusCode>();
            For<IHttpStatusCodeStrategy>().Use<StrategyForClientErrorStatusCode>();
            For<IHttpStatusCodeStrategy>().Use<StrategyForRedirectionStatusCode>();
            For<IHttpStatusCodeStrategy>().Use<StrategyForSuccessStatusCode>();
            For<IHttpStatusCodeStrategy>().Use<StrategyForInformationStatusCode>();
            For<IClientAuthenticator>().Use<ActiveDirectoryClientAuthenticator>();
            For<IIdentityHandler>().Use<RequestHeaderIdentityHandler>();
            For<IServiceAddressMapper>().Use<ServiceAddressMapper>();
            For<IChallengeService>().Singleton().Use(c => new InMemoryChallengeService(new Dictionary<Guid, SupportAgentChallenge>(), c.GetInstance<IChallengeSettings>()));
            For<ICrypto>().Use<Crypto>();
            For<IIdentityHash>().Use<IdentityHash>();
            For<IMenuTemplateTransformer>().Singleton().Use<MenuTemplateTransformer>();
            For<IMenuTemplateDatasource>().Singleton().Use(x => new MenuTemplateDatasource("~/App_Data", x.GetInstance<ILog>()));
            For<IMenuClient>().Singleton().Use<MenuClient>();
            For<IMenuService>().Singleton().Use<MenuService>();
            For<IChallengeSettings>().Use(configuration.Challenge);
            For<IChallengeService>().Singleton().Use(c => new InMemoryChallengeService(new Dictionary<Guid, SupportAgentChallenge>(), c.GetInstance<IChallengeSettings>()));
            For<IChallengeRepository<PayeSchemeChallengeViewModel>>().Singleton().Use(x=> new PayeSchemeChallengeRepository(new Dictionary<Guid, PayeSchemeChallengeViewModel>(), x.GetInstance<IChallengeService>().ChallengeExpiryMinutes ));

        }

        private WebConfiguration GetConfiguration()
        {
            var environment = CloudConfigurationManager.GetSetting("EnvironmentName") ??
                              "local";
            var storageConnectionString = CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString") ??
                                          "UseDevelopmentStorage=true";

            var configurationRepository = new AzureTableStorageConfigurationRepository(storageConnectionString); ;

            var configurationOptions = new ConfigurationOptions(ServiceName, environment, Version);

            var configurationService = new ConfigurationService(configurationRepository, configurationOptions);

            var webConfiguration = configurationService.Get<WebConfiguration>();

            return webConfiguration;
        }

        #endregion
    }
}