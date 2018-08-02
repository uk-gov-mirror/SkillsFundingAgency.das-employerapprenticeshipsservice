using AutoMapper;
using MediatR;
using SFA.DAS.EAS.Application.Queries.GetTransactionsDownloadResultViewModel;
using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.Infrastructure.Authentication;
using SFA.DAS.EAS.Infrastructure.Authorization;
using SFA.DAS.EAS.Infrastructure.Data;
using StructureMap;
using SFA.DAS.EAS.TestCommon.Steps;
using SFA.DAS.EAS.Web.Controllers;
using SFA.DAS.EAS.Web.Orchestrators;
using SFA.DAS.EAS.Web.ViewModels;
using SFA.DAS.HashingService;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.EAS.TestCommon.Extensions
{
    public static class ObjectContextExtensions
    {
        public static ObjectContext SetupEatOrchestrator(this ObjectContext objectContext, IContainer container)
        {

            objectContext.EatOrchestrator = new EmployerAccountTransactionsOrchestrator(
                container.GetInstance<IMediator>(),
                container.GetInstance<ICurrentDateTime>(), container.GetInstance<ILog>());

            return objectContext;
        }
        public static ObjectContext SetupEatController(this ObjectContext objectContext, IContainer container)
        {
            objectContext.EatController = new EmployerAccountTransactionsController(container.GetInstance<IAuthenticationService>(),
                container.GetInstance<IAuthorizationService>(),
                container.GetInstance<IHashingService>(),
                container.GetInstance<IMediator>(),
                objectContext.EatOrchestrator,
                container.GetInstance<IMultiVariantTestingService>(),
                container.GetInstance<ICookieStorageService<FlashMessageViewModel>>(),
                container.GetInstance<ITransactionFormatterFactory>(),
                container.GetInstance<IMapper>());

            return objectContext;
        }

        public static ObjectContext SetupEapOrchestrator(this ObjectContext objectContext, IContainer container)
        {

            objectContext.EapOrchestrator = new EmployerAccountPayeOrchestrator(container.GetInstance<IMediator>(),
                container.GetInstance<ILog>(), container.GetInstance<ICookieStorageService<EmployerAccountData>>()
                , container.GetInstance<EmployerApprenticeshipsServiceConfiguration>());

            return objectContext;
        }

        public static ObjectContext SetupEapController(this ObjectContext objectContext, IContainer container)
        {

            objectContext.EapController = new EmployerAccountPayeController(container.GetInstance<IAuthenticationService>(),
                objectContext.EapOrchestrator,
                container.GetInstance<IAuthorizationService>(),
                container.GetInstance<IMultiVariantTestingService>(),
                container.GetInstance<ICookieStorageService<FlashMessageViewModel>>());
            return objectContext;
        }

        public static ObjectContext EmployerAccountsDbContextBeginTransaction(this ObjectContext objectContext, IContainer container)
        {

            container.GetInstance<EmployerAccountsDbContext>().Database.BeginTransaction();

            return objectContext;
        }

        public static ObjectContext EmployerFinanceDbContextBeginTransaction(this ObjectContext objectContext, IContainer container)
        {
            container.GetInstance<EmployerFinanceDbContext>().Database.BeginTransaction();

            return objectContext;
        }
    }
}
