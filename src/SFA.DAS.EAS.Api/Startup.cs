using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Mvc;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure;
using Microsoft.Owin;
using Owin;
using SFA.DAS.EAS.Api.DependencyResolution;
using SFA.DAS.EAS.Infrastructure.Logging;
using StructureMap;

[assembly: OwinStartup(typeof(SFA.DAS.EAS.Api.Startup))]

namespace SFA.DAS.EAS.Api
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            LoggingConfig.ConfigureLogging();

            var instrumentationKey = CloudConfigurationManager.GetSetting("InstrumentationKey");
            if (!string.IsNullOrEmpty(instrumentationKey))
            {
                TelemetryConfiguration.Active.InstrumentationKey = instrumentationKey;
            }

            ConfigureAuth(app);
            var config = new HttpConfiguration();
            IocConfig.RegisterServices(config);
            WebApiConfig.Register(config);

            app.UseWebApi(config);
        }
    }
}
