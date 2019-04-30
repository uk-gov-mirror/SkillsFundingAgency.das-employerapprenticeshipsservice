using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerAccounts.Data;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccountLevel
{
    public class GetAccountLevelQueryHandler : IAsyncRequestHandler<GetAccountLevelQuery, GetAccountLevelResponse>
    {
        private readonly IEmployerAccountRepository _employerAccountRepository;

        public GetAccountLevelQueryHandler(IEmployerAccountRepository employerAccountRepository)
        {
            _employerAccountRepository = employerAccountRepository;
        }

        public async Task<GetAccountLevelResponse> Handle(GetAccountLevelQuery message)
        {
            var accountLevel = await _employerAccountRepository.GetAccountLevel(message.AccountId);
            return new GetAccountLevelResponse{ AccountLevel = accountLevel };
        }
    }
}
