namespace SFA.DAS.EmployerAccounts.Api.Controllers
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;

    using SFA.DAS.EmployerAccounts.Api.Attributes;
    using SFA.DAS.EmployerAccounts.Api.Orchestrators;

    [RoutePrefix("api/accounts/{hashedAccountId}/transactions")]
    public class AccountTransactionsController : ApiController
    {
        private readonly AccountTransactionsOrchestrator _orchestrator;

        public AccountTransactionsController(AccountTransactionsOrchestrator orchestrator)
        {
            this._orchestrator = orchestrator;
        }

        [Route("", Name = "GetTransactionSummary")]
        [HttpGet]
        public async Task<IHttpActionResult> Index(string hashedAccountId)
        {
            var result = await this._orchestrator.GetAccountTransactionSummary(hashedAccountId);

            if (result.Data == null)
            {
                return this.NotFound();
            }

            result.Data.ForEach(x => x.Href = this.Url.Route("GetTransactions", new { hashedAccountId, year = x.Year, month = x.Month }));

            return Ok(result.Data);
        }

        [Route("{year?}/{month?}", Name = "GetTransactions")]
        [ApiAuthorize(Roles = "ReadAllEmployerAccountBalances")]
        [HttpGet]
        public async Task<IHttpActionResult> GetTransactions(string hashedAccountId, int year = 0, int month = 0)
        {
            var result = await this.GetAccountTransactions(hashedAccountId, year, month);

            if (result.Data == null)
            {
                return this.NotFound();
            }

            if (result.Data.HasPreviousTransactions)
            {
                var previousMonth = new DateTime(result.Data.Year, result.Data.Month, 1).AddMonths(-1);
                result.Data.PreviousMonthUri = this.Url.Route("GetTransactions", new { hashedAccountId, year = previousMonth.Year, month = previousMonth.Month });
            }

            return Ok(result.Data);
        }

        private async Task<OrchestratorResponse<TransactionsViewModel>> GetAccountTransactions(string hashedAccountId, int year, int month)
        {
            if (year == 0)
            {
                year = DateTime.Now.Year;
            }

            if (month == 0)
            {
                month = DateTime.Now.Month;
            }

            var result = await this._orchestrator.GetAccountTransactions(hashedAccountId, year, month, this.Url);
            return result;
        }
    }
}