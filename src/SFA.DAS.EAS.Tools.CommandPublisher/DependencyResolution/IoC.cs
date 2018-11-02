using SFA.DAS.EmployerFinance.DependencyResolution;
using StructureMap;

namespace SFA.DAS.EAS.Tools.CommandPublisher.DependencyResolution
{
    public static class IoC
    {
        public static IContainer Initialize()
        {
            return new Container(c =>
            {
                c.AddRegistry<ConfigurationRegistry>();
               //c.AddRegistry<DataRegistry>();
                c.AddRegistry<DefaultRegistry>();
                c.AddRegistry<LoggerRegistry>();
            });
        }
    }
}