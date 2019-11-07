using SFA.DAS.EAS.Infrastructure.Data;
using StructureMap;
using SFA.DAS.Authorization.DependencyResolution.StructureMap;
using SFA.DAS.EAS.Application.DependencyResolution;

namespace SFA.DAS.EAS.Web.DependencyResolution
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            return new Container(c =>
            {
                c.AddRegistry<AuthorizationRegistry>();
                c.AddRegistry<ConfigurationRegistry>();
                c.AddRegistry<LoggerRegistry>();
                c.AddRegistry<DefaultRegistry>();
            });
        }
    }
}