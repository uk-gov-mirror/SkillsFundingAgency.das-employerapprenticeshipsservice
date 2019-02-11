namespace SFA.DAS.EmployerAccounts.Api.Orchestrators
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    using AutoMapper;

    using MediatR;

    using SFA.DAS.NLog.Logger;

    public class UsersOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ILog _logger;
        private readonly IMapper _mapper;

        public UsersOrchestrator(IMediator mediator, ILog logger, IMapper mapper)
        {
            if (mediator == null)
                throw new ArgumentNullException(nameof(mediator));

            this._mediator = mediator;
            this._logger = logger;
            this._mapper = mapper;
        }

        public async Task<OrchestratorResponse<ICollection<AccountDetailViewModel>>> GetUserAccounts(string userRef)
        {
            this._logger.Info($"Requesting user's accounts for user Ref  {userRef}");

            var accounts = await this._mediator.SendAsync(new GetUserAccountsQuery {UserRef = userRef});

            var viewModels = accounts.Accounts.AccountList.Select(x => this._mapper.Map<AccountDetailViewModel>(x)).ToList();

            return new OrchestratorResponse<ICollection<AccountDetailViewModel>>
            {
                Data = viewModels,
                Status = HttpStatusCode.OK
            };
        }
    }
}