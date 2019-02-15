using Microsoft.Azure;
using Microsoft.Owin;
using Microsoft.Owin.Security.ActiveDirectory;
using Owin;
using SFA.DAS.EmployerAccounts.Api;

[assembly: OwinStartup(typeof(Startup))]

namespace SFA.DAS.EmployerAccounts.Api
{
    using Microsoft.Azure;
    using Microsoft.IdentityModel.Tokens;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseWindowsAzureActiveDirectoryBearerAuthentication(new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                {
                    Tenant = CloudConfigurationManager.GetSetting("idaTenant"),
                    TokenValidationParameters = new TokenValidationParameters()
                        {
                            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                            ValidAudience = CloudConfigurationManager.GetSetting("idaAudience")
                        }
                });
        }
    }
}
