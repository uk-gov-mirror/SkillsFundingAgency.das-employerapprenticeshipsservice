using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Support.Shared.Authentication;
using SFA.DAS.Support.Shared.SiteConnection;
using SFA.DAS.Support.Shared.Views;
using SFA.DAS.Web.Policy;

namespace SFA.DAS.EAS.Support.Web
{
    [ExcludeFromCodeCoverage]
    public class Global : HttpApplication
    {
        private void Application_Start(object sender, EventArgs e)
        {

            HostingEnvironment
                .RegisterVirtualPathProvider(
                    new EmbeddedResourceViewPathProvider());

            var ioc = DependencyResolver.Current;
            var logger = ioc.GetService<ILog>();
            logger.Info("Starting Web Role");

            MvcHandler.DisableMvcResponseHeader = true;

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            var siteConnectorSettings = ioc.GetService<ISiteValidatorSettings>();
            GlobalConfiguration.Configuration.MessageHandlers.Add(new TokenValidationHandler(siteConnectorSettings, logger));
            GlobalFilters.Filters.Add(new TokenValidationFilter(siteConnectorSettings, logger));

         
            logger.Info("Web role started");
        }

        private void Application_OnBeginRequest(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine($"{Request?.RawUrl?? "No Request Found"}");
        }

        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {
            if (HttpContext.Current == null) return;

            new HttpContextPolicyProvider(
                new List<IHttpContextPolicy>()
                {
                    new ResponseHeaderRestrictionPolicy()
                }
            ).Apply(new HttpContextWrapper(HttpContext.Current), PolicyConcern.HttpResponse);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var ex = Server.GetLastError().GetBaseException();
            var logger = DependencyResolver.Current.GetService<ILog>();
            logger.Error(ex, "App_Error");
        }
    }
}