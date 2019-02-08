namespace SFA.DAS.EmployerAccounts.Api.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;

    using SFA.DAS.EmployerAccounts.Api.Attributes;
    using SFA.DAS.EmployerAccounts.Api.Orchestrators;

    [RoutePrefix("api/accounts/{hashedAccountId}/levy")]
    public class AccountLevyController : ApiController
    {
        private readonly AccountsOrchestrator _orchestrator;

        public AccountLevyController(AccountsOrchestrator orchestrator)
        {
            this._orchestrator = orchestrator;
        }

        [Route("", Name = "GetLevy")]
        [ApiAuthorize(Roles = "ReadAllEmployerAccountBalances")]
        [HttpGet]
        public async Task<IHttpActionResult> Index(string hashedAccountId)
        {
            var result = await this._orchestrator.GetLevy(hashedAccountId);

            if (result.Data == null)
            {
                return this.NotFound();
            }

            return Ok(result.Data);
        }

        [Route("{payrollYear}/{payrollMonth}", Name = "GetLevyForPeriod")]
        [ApiAuthorize(Roles = "ReadAllEmployerAccountBalances")]
        [HttpGet]
        public async Task<IHttpActionResult> GetLevy(string hashedAccountId, string payrollYear, short payrollMonth)
        {
            var result = await this._orchestrator.GetLevy(hashedAccountId, payrollYear, payrollMonth);

            if (result.Data == null)
            {
                return this.NotFound();
            }

            return Ok(result.Data);
        }
    }
}