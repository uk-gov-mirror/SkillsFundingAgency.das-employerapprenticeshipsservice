using System.Web;
using Moq;
using SFA.DAS.CookieService;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.Infrastructure.Services;
using SFA.DAS.EAS.Web.Controllers;
using StructureMap;
using StructureMap.Pipeline;

namespace SFA.DAS.EAS.Accounts.AcceptanceTests.Steps
{
    public class CookieStorageRegistry : Registry
    {
        public CookieStorageRegistry()
        {
            For<HttpContextBase>().Use(() => new HttpContextWrapper(HttpContext.Current));
            For(typeof(ICookieService<>)).Use(typeof(HttpCookieService<>));
            For(typeof(ICookieStorageService<>)).Use(typeof(CookieStorageService<>));
            ForConcreteType<TransfersController>().Configure.SetLifecycleTo<UniquePerRequestLifecycle>();
        }
    }
}
