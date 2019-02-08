namespace SFA.DAS.EmployerAccounts.Api.Controllers
{
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;

    using SFA.DAS.EmployerAccounts.Api.Attributes;
    using SFA.DAS.EmployerAccounts.Api.Orchestrators;
    using SFA.DAS.EmployerAccounts.Types;

    [RoutePrefix("api/accounts/{hashedAccountId}/payeschemes")]
    public class AccountPayeSchemesController : ApiController
    {
        private readonly AccountsOrchestrator _orchestrator;

        public AccountPayeSchemesController(AccountsOrchestrator orchestrator)
        {
            this._orchestrator = orchestrator;
        }

        [Route("", Name = "GetPayeSchemes")]
        [ApiAuthorize(Roles = "ReadAllEmployerAccountBalances")]
        [HttpGet]
        public async Task<IHttpActionResult> GetPayeSchemes(string hashedAccountId)
        {
            var result = await this._orchestrator.GetAccount(hashedAccountId);

            if (result.Data == null)
            {
                return this.NotFound();
            }

            result.Data.PayeSchemes.ForEach(x => this.CreateGetPayeSchemeLink(hashedAccountId, x));
            return Ok(result.Data.PayeSchemes);
        }

        [Route("{payeschemeref}", Name = "GetPayeScheme")]
        [ApiAuthorize(Roles = "ReadAllEmployerAccountBalances")]
        [HttpGet]
        public async Task<IHttpActionResult> GetPayeScheme(string hashedAccountId, string payeSchemeRef)
        {
            var result = await this._orchestrator.GetPayeScheme(hashedAccountId, HttpUtility.UrlDecode(payeSchemeRef));

            if (result.Data == null)
            {
                return this.NotFound();
            }

            return Ok(result.Data);
        }

        private void CreateGetPayeSchemeLink(string hashedAccountId, ResourceViewModel payeScheme)
        {
            payeScheme.Href = this.Url.Route("GetPayeScheme", new { hashedAccountId, payeSchemeRef = HttpUtility.UrlEncode(payeScheme.Id) });
        }
    }
}