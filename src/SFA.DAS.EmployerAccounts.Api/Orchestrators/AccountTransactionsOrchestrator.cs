using TransactionItemType = SFA.DAS.EAS.Account.Api.Types.TransactionItemType;

namespace SFA.DAS.EmployerAccounts.Api.Orchestrators
{
    using System.Threading.Tasks;
    using System.Web.Http.Routing;

    using MediatR;

    using SFA.DAS.NLog.Logger;

    public class AccountTransactionsOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ILog _logger;

        public AccountTransactionsOrchestrator(IMediator mediator, ILog logger)
        {
            this._mediator = mediator;
            this._logger = logger;
        }

        public async Task<OrchestratorResponse<TransactionsViewModel>> GetAccountTransactions(string hashedAccountId, int year, int month, UrlHelper urlHelper)
        {
            var data =
                await
                    this._mediator.SendAsync(new GetEmployerAccountTransactionsQuery
                    {
                        Year = year,
                        Month = month,
                        HashedAccountId = hashedAccountId
                    });

            var response = new OrchestratorResponse<TransactionsViewModel>
            {
                Data = new TransactionsViewModel
                {
                    HasPreviousTransactions = data.AccountHasPreviousTransactions,
                    Year = data.Year,
                    Month = data.Month
                }
            };

            response.Data.AddRange(data.Data.TransactionLines.Select(x => this.ConvertToTransactionViewModel(hashedAccountId, x, urlHelper)));
            return response;
        }

        public async Task<OrchestratorResponse<AccountResourceList<TransactionSummaryViewModel>>> GetAccountTransactionSummary(string hashedAccountId)
        {
            var data = await this._mediator.SendAsync(new GetAccountTransactionSummaryRequest { HashedAccountId = hashedAccountId });

            var response = new OrchestratorResponse<AccountResourceList<TransactionSummaryViewModel>>
            {
                Data = new AccountResourceList<TransactionSummaryViewModel>(data.Data.Select(ConvertToTransactionSummaryViewModel))
            };
            
            return response;
        }

        private TransactionSummaryViewModel ConvertToTransactionSummaryViewModel(TransactionSummary transactionSummary)
        {
            return new TransactionSummaryViewModel
            {
                Amount = transactionSummary.Amount,
                Year = transactionSummary.Year,
                Month = transactionSummary.Month
            };
        }

        private TransactionViewModel ConvertToTransactionViewModel(string hashedAccountId, TransactionLine transactionLine, UrlHelper urlHelper)
        {
            var viewModel = new TransactionViewModel
            {
                Amount = transactionLine.Amount,
                Balance = transactionLine.Balance,
                Description = transactionLine.Description,
                TransactionType = (TransactionItemType)transactionLine.TransactionType,
                DateCreated = transactionLine.DateCreated,
                SubTransactions = transactionLine.SubTransactions?.Select(x => this.ConvertToTransactionViewModel(hashedAccountId, x, urlHelper)).ToList(),
                TransactionDate = transactionLine.TransactionDate
            };

            if (transactionLine.TransactionType == Domain.Models.Transaction.TransactionItemType.Declaration)
            {
                viewModel.ResourceUri = urlHelper.Route("GetLevyForPeriod", new { hashedAccountId, payrollYear = transactionLine.PayrollYear, payrollMonth = transactionLine.PayrollMonth });
            }

            return viewModel;
        }
    }
}