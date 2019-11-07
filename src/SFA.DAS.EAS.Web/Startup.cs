using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using NLog;
using Owin;
using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.EAS.Web;
using SFA.DAS.EAS.Web.App_Start;
using SFA.DAS.EAS.Web.Authentication;
using SFA.DAS.EmployerUsers.WebClientComponents;
using SFA.DAS.OidcMiddleware;

[assembly: OwinStartup(typeof(Startup))]

namespace SFA.DAS.EAS.Web
{
    public class Startup
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private const string ServiceName = "SFA.DAS.EmployerApprenticeshipsService";

        public void Configuration(IAppBuilder app)
        {
            var config = StructuremapMvc.StructureMapDependencyScope.Container.GetInstance<EmployerApprenticeshipsServiceConfiguration>();
            var constants = new Constants(config.Identity);
            var urlHelper = new UrlHelper();
            ConfigurationFactory.Current = new IdentityServerConfigurationFactory(config);
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();
        }
    }

    public class Constants
    {
        private readonly string _baseUrl;
        private readonly IdentityServerConfiguration _configuration;

        public Constants(IdentityServerConfiguration configuration)
        {
            _baseUrl = configuration.ClaimIdentifierConfiguration.ClaimsBaseUrl;
            _configuration = configuration;
        }

        public string AuthorizeEndpoint() => $"{_configuration.BaseAddress}{_configuration.AuthorizeEndPoint}";
        public string ChangeEmailLink() => _configuration.BaseAddress.Replace("/identity", "") + string.Format(_configuration.ChangeEmailLink, _configuration.ClientId);
        public string ChangePasswordLink() => _configuration.BaseAddress.Replace("/identity", "") + string.Format(_configuration.ChangePasswordLink, _configuration.ClientId);
        public string DisplayName() => _baseUrl + _configuration.ClaimIdentifierConfiguration.DisplayName;
        public string Email() => _baseUrl + _configuration.ClaimIdentifierConfiguration.Email;
        public string FamilyName() => _baseUrl + _configuration.ClaimIdentifierConfiguration.FaimlyName;
        public string GivenName() => _baseUrl + _configuration.ClaimIdentifierConfiguration.GivenName;
        public string Id() => _baseUrl + _configuration.ClaimIdentifierConfiguration.Id;
        public string LogoutEndpoint() => $"{_configuration.BaseAddress}{_configuration.LogoutEndpoint}";
        public string RegisterLink() => _configuration.BaseAddress.Replace("/identity", "") + string.Format(_configuration.RegisterLink, _configuration.ClientId);
        public string RequiresVerification() => _baseUrl + "requires_verification";
        public string TokenEndpoint() => $"{_configuration.BaseAddress}{_configuration.TokenEndpoint}";
        public string UserInfoEndpoint() => $"{_configuration.BaseAddress}{_configuration.UserInfoEndpoint}";
    }
}