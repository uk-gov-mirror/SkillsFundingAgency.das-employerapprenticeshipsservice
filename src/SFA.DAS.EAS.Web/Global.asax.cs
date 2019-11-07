using FluentValidation.Mvc;
using NLog;
using NServiceBus;
using SFA.DAS.Web.Policy;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SFA.DAS.EAS.Web.App_Start;
using SFA.DAS.Logging;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;
using SFA.DAS.EAS.Infrastructure.Logging;

namespace SFA.DAS.EAS.Web
{
    public class MvcApplication : HttpApplication
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private IEndpointInstance _endpoint;

        protected void Application_Start()
        {
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
            AntiForgeryConfig.RequireSsl = true;
            AreaRegistration.RegisterAllAreas();
            BinderConfig.RegisterBinders(ModelBinders.Binders);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            FluentValidationModelValidatorProvider.Configure();
            LoggingConfig.ConfigureLogging();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            TelemetryConfiguration.Active.InstrumentationKey = ConfigurationManager.AppSettings["APPINSIGHTS_INSTRUMENTATIONKEY"];
        }

        protected void Application_PreSendRequestHeaders(object sender, EventArgs e)
        {
            new HttpContextPolicyProvider(new List<IHttpContextPolicy> { new ResponseHeaderRestrictionPolicy() })
                .Apply(new HttpContextWrapper(HttpContext.Current), PolicyConcern.HttpResponse);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();

            if (exception is HttpException httpException && httpException.GetHttpCode() == (int)HttpStatusCode.NotFound)
            {
                return;
            }

            Dictionary<string, object> properties = null;

            try
            {
                properties = new Dictionary<string, object>
                {
                    ["HttpMethod"] = Request.HttpMethod,
                    ["IsAuthenticated"] = Request.IsAuthenticated,
                    ["Url"] = Request.Url.PathAndQuery,
                    ["UrlReferrer"] = Request.UrlReferrer?.PathAndQuery
                };
            }
            catch (Exception)
            {
                // Request not available
            }

            var message = exception.GetMessage();
            var telemetryClient = new TelemetryClient();

            Logger.Error(exception, message, properties);
            telemetryClient.TrackException(exception);
        }
    }
}