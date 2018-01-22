using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Azure;
using System.Net.Http.Headers;
using System.Web.Http;
using SFA.DAS.ApiTokens.Client;
using System.Web.Http.ExceptionHandling;
using SFA.DAS.EAS.Api.DependencyResolution;
using StructureMap;

namespace SFA.DAS.EAS.Api
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

            config.MapHttpAttributeRoutes();

            config.Services.Replace(typeof(IExceptionHandler), new CustomExceptionHandler());
        }
    }

    public static class IocConfig
    {
        public static IContainer Container { get; set; }
        public static void RegisterServices(HttpConfiguration config)
        {
            Container = IoC.Initialize();

            // Register
            config.DependencyResolver = new StructureMapWebApiDependencyResolver(Container);
        }
    }
}
