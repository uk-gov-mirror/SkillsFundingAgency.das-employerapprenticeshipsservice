using Microsoft.Azure;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using NLog;
using Owin;
using SFA.DAS.Authentication;
using SFA.DAS.EmployerAccounts.Configuration;
using SFA.DAS.EmployerAccounts.Web;
using SFA.DAS.EmployerAccounts.Web.Authentication;
using SFA.DAS.EmployerAccounts.Web.ViewModels;
using SFA.DAS.EmployerUsers.WebClientComponents;
using SFA.DAS.OidcMiddleware;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Mvc;
using SFA.DAS.EmployerAccounts.Interfaces;
using SFA.DAS.EmployerAccounts.Models.Account;
using SFA.DAS.EmployerAccounts.Web.FeatureToggles;
using SFA.DAS.EmployerAccounts.Web.Models;

[assembly: OwinStartup(typeof(Startup))]

namespace SFA.DAS.EmployerAccounts.Web
{
    public class Startup
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private const string AccountDataCookieName = "sfa-das-employerapprenticeshipsservice-employeraccount";

        public void Configuration(IAppBuilder app)
        {
            var config = StructuremapMvc.StructureMapDependencyScope.Container.GetInstance<EmployerAccountsConfiguration>();
            var accountDataCookieStorageService = StructuremapMvc.StructureMapDependencyScope.Container.GetInstance<ICookieStorageService<EmployerAccountData>>();
            var hashedAccountIdCookieStorageService = StructuremapMvc.StructureMapDependencyScope.Container.GetInstance<ICookieStorageService<HashedAccountIdModel>>();
            var constants = new Constants(config.Identity);
            var urlHelper = new UrlHelper();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies",
                ExpireTimeSpan = new TimeSpan(0, 10, 0),
                SlidingExpiration = true
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "TempState",
                AuthenticationMode = AuthenticationMode.Passive
            });

            //this works but principal has AuthenticationType of "Cookies".
            //relationship between AuthenticationType set when calling UseCookieAuthentication (which afaik just says to serialize and encrypt the principal and put in a cookie (and decrypt/deserialize when already authorized, and doesn't set the actual authentication scheme). actually looks like cookie auth is enough on own (https://brockallen.com/2013/10/24/a-primer-on-owin-cookie-authentication-middleware-for-the-asp-net-developer/) how does it fit in with the oidc middleware?
            // this site says cookie and oidc both use cookie middleware ... https://benfoster.io/blog/aspnet-identity-stripped-bare-mvc-part-1
            // and the (default) AuthenticationType set when using a particular scheme
            // need to pass the authentication type when signing-out. can get how authenticated from principal, so we can sign out the user for the auth scheme they used to sign in, see..
            // https://stackoverflow.com/questions/35653428/what-is-cookieauthenticationoptions-authenticationtype-used-for
            
            // is cookie active, and oidc passive?
            // https://stackoverflow.com/questions/25845420/mvc-5-application-implement-oauth-authorization-code-flow
            
            //app.SetDefaultSignInAsAuthenticationType("code");
            app.SetDefaultSignInAsAuthenticationType("Cookies");

            var oidcMiddlewareOptions = new OidcMiddlewareOptions
            {
                BaseUrl = config.Identity.BaseAddress,
                ClientId = config.Identity.ClientId,
                ClientSecret = config.Identity.ClientSecret,
                Scopes = config.Identity.Scopes,
                AuthorizeEndpoint = constants.AuthorizeEndpoint(),
                TokenEndpoint = constants.TokenEndpoint(),
                UserInfoEndpoint = constants.UserInfoEndpoint(),
                TokenSigningCertificateLoader = GetSigningCertificate(config.Identity.UseCertificate),
                TokenValidationMethod = config.Identity.UseCertificate
                    ? TokenValidationMethod.SigningKey
                    : TokenValidationMethod.BinarySecret,
                AuthenticatedCallback = identity =>
                {
                    PostAuthentiationAction(
                        identity,
                        constants,
                        accountDataCookieStorageService,
                        hashedAccountIdCookieStorageService);
                }
            };
            
            app.UseCodeFlowAuthentication(oidcMiddlewareOptions);

            ConfigurationFactory.Current = new IdentityServerConfigurationFactory(config);
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            UserLinksViewModel.ChangePasswordLink = $"{constants.ChangePasswordLink()}{urlHelper.Encode(config.EmployerAccountsBaseUrl + "/service/password/change")}";
            UserLinksViewModel.ChangeEmailLink = $"{constants.ChangeEmailLink()}{urlHelper.Encode(config.EmployerAccountsBaseUrl + "/service/email/change")}";

            FeatureToggles.Features.BooleanToggleValueProvider = StructuremapMvc.StructureMapDependencyScope.Container.GetInstance<IBooleanToggleValueProvider>();
        }

        private static Func<X509Certificate2> GetSigningCertificate(bool useCertificate)
        {
            if (!useCertificate)
            {
                return null;
            }

            return () =>
            {
                var store = new X509Store(StoreLocation.CurrentUser);

                store.Open(OpenFlags.ReadOnly);

                try
                {
                    var thumbprint = CloudConfigurationManager.GetSetting("TokenCertificateThumbprint");
                    var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

                    if (certificates.Count < 1)
                    {
                        throw new Exception($"Could not find certificate with thumbprint '{thumbprint}' in CurrentUser store.");
                    }

                    return certificates[0];
                }
                finally
                {
                    store.Close();
                }
            };
        }

        private static void PostAuthentiationAction(ClaimsIdentity identity,
            Constants constants,
            ICookieStorageService<EmployerAccountData> accountDataCookieStorageService,
            ICookieStorageService<HashedAccountIdModel> hashedAccountIdCookieStorageService)
        {
            Logger.Info("Retrieving claims from OIDC server.");

            var userRef = identity.Claims.FirstOrDefault(claim => claim.Type == constants.Id())?.Value;
          
            Logger.Info($"Retrieved claims from OIDC server for user with external ID '{userRef}'.");

            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, identity.Claims.First(c => c.Type == constants.Id()).Value));
            identity.AddClaim(new Claim(ClaimTypes.Name, identity.Claims.First(c => c.Type == constants.DisplayName()).Value));
            identity.AddClaim(new Claim("sub", identity.Claims.First(c => c.Type == constants.Id()).Value));
            identity.AddClaim(new Claim("email", identity.Claims.First(c => c.Type == constants.Email()).Value));
            identity.AddClaim(new Claim("firstname", identity.Claims.First(c => c.Type == constants.GivenName()).Value));
            identity.AddClaim(new Claim("lastname", identity.Claims.First(c => c.Type == constants.FamilyName()).Value));

            Task.Run(() => accountDataCookieStorageService.Delete(AccountDataCookieName)).Wait();
            Task.Run(() => hashedAccountIdCookieStorageService.Delete(typeof(HashedAccountIdModel).FullName)).Wait();
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