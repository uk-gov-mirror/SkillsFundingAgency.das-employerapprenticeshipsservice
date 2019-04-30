using MediatR;

namespace SFA.DAS.EmployerAccounts.Queries.GetAccountLevel
{
    public class GetAccountLevelQuery : IAsyncRequest<GetAccountLevelResponse>
    {
        public long AccountId { get; set; }
    }
}
