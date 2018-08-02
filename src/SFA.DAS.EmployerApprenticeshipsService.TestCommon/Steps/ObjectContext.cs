using System;
using System.Collections.Generic;
using HMRC.ESFA.Levy.Api.Client;
using Moq;
using NServiceBus;
using SFA.DAS.EAS.Domain.Interfaces;
using SFA.DAS.EAS.Domain.Models.Account;
using SFA.DAS.EAS.Domain.Models.AccountTeam;
using SFA.DAS.EAS.Domain.Models.PAYE;
using SFA.DAS.EAS.Domain.Models.UserProfile;
using SFA.DAS.EAS.Infrastructure.Authentication;
using SFA.DAS.EAS.Infrastructure.Authorization;
using SFA.DAS.EAS.Web.Controllers;
using SFA.DAS.EAS.Web.Orchestrators;
using SFA.DAS.EAS.Web.ViewModels;
using SFA.DAS.Events.Api.Client;

namespace SFA.DAS.EAS.TestCommon.Steps
{
    public class ObjectContext
    {
        public Dictionary<string, Account> Accounts = new Dictionary<string, Account>();
        public Dictionary<string, User> Users = new Dictionary<string, User>();
        public Dictionary<string, PayeScheme> PayeSchemes = new Dictionary<string, PayeScheme>();
        public Dictionary<string, LegalEntity> LegalEntities = new Dictionary<string, LegalEntity>();
        public List<Membership> Memberships = new List<Membership>();
        public Mock<IEventsApi> EventsApiMock = new Mock<IEventsApi>();
        public Mock<IApprenticeshipLevyApiClient> ApprenticeshipLevyApiClient = new Mock<IApprenticeshipLevyApiClient>();
        public Mock<IAuthenticationService> AuthenticationService = new Mock<IAuthenticationService>();
        public IEndpointInstance FinanceMessageHandlersEndPoint { get; set; }
        public IEndpointInstance NServiceBusEndpoint { get; set; }
        public IEndpointInstance FinanceJobsEndPoint { get; set; }
        public IDictionary<long, DateTime?> CurrentlyProcessingSubmissionIds { get; set; }
        public EmployerAccountTransactionsOrchestrator EatOrchestrator { get; set; }
        public EmployerAccountTransactionsController EatController { get; set; }
        public EmployerAccountPayeOrchestrator EapOrchestrator { get; set; }
        public EmployerAccountPayeController EapController { get; set; }
        public Mock<ICookieStorageService<FlashMessageViewModel>> CookieFlashMessageViewModelStorageService = new Mock<ICookieStorageService<FlashMessageViewModel>>();
        public Mock<ICookieStorageService<EmployerAccountData>> CookieEmployerAccountDataStorageService = new Mock<ICookieStorageService<EmployerAccountData>>();
        public Mock<IAuthorizationService> AuthorizationService = new Mock<IAuthorizationService>(); 
    }
}