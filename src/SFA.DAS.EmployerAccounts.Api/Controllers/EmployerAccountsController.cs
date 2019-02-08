namespace SFA.DAS.EmployerAccounts.Api.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http;

    using SFA.DAS.EmployerAccounts.Api.Attributes;
    using SFA.DAS.EmployerAccounts.Api.Orchestrators;
    using SFA.DAS.EmployerAccounts.Types;

    [RoutePrefix("api/accounts")]
    public class EmployerAccountsController : ApiController
    {
        private readonly AccountsOrchestrator _orchestrator;

        public EmployerAccountsController(AccountsOrchestrator orchestrator)
        {
            this._orchestrator = orchestrator;
        }

        [Route("", Name = "AccountsIndex")]
        [ApiAuthorize(Roles = "ReadAllEmployerAccountBalances")]
        [HttpGet]   
        public async Task<IHttpActionResult> GetAccounts(string toDate = null, int pageSize = 1000, int pageNumber = 1)
        {
            var result = await this._orchestrator.GetAllAccountsWithBalances(toDate, pageSize, pageNumber);
            
            if (result.Status == HttpStatusCode.OK)
            {
                result.Data.Data.ForEach(x => x.Href = this.Url.Route("GetAccount", new { hashedAccountId = x.AccountHashId }));
                return Ok(result.Data);
            }
            else
            {
                //TODO: Handle unhappy paths.
                return this.Conflict();
            }
        }


        [Route("{hashedAccountId}", Name = "GetAccount")]
        [ApiAuthorize(Roles = "ReadAllEmployerAccountBalances")]
        [HttpGet]
        public async Task<IHttpActionResult> GetAccount(string hashedAccountId)
        {
            var result = await this._orchestrator.GetAccount(hashedAccountId);

            if (result.Data == null)
            {
                return this.NotFound();
            }

            result.Data.LegalEntities.ForEach(x => this.CreateGetLegalEntityLink(hashedAccountId, x));
            result.Data.PayeSchemes.ForEach(x => this.CreateGetPayeSchemeLink(hashedAccountId, x));
            return Ok(result.Data);
        }

        [Route("internal/{accountId}", Name = "GetAccountByInternalId")]
        [ApiAuthorize(Roles = "ReadAllEmployerAccountBalances")]
        [HttpGet]
        public async Task<IHttpActionResult> GetAccount(long accountId)
        {
            var result = await this._orchestrator.GetAccount(accountId);

            if (result.Data == null)
            {
                return this.NotFound();
            }

            result.Data.LegalEntities.ForEach(x => this.CreateGetLegalEntityLink(result.Data.HashedAccountId, x));
            result.Data.PayeSchemes.ForEach(x => this.CreateGetPayeSchemeLink(result.Data.HashedAccountId, x));
            return Ok(result.Data);
        }




        [Route("{hashedAccountId}/users", Name = "GetAccountUsers")]
        [ApiAuthorize(Roles = "ReadAllAccountUsers")]
        [HttpGet]
        public async Task<IHttpActionResult> GetAccountUsers(string hashedAccountId)
        {
            var result = await this._orchestrator.GetAccountTeamMembers(hashedAccountId);

            if (result.Data == null)
            {
                return this.NotFound();
            }
           
            return Ok(result.Data);
        }

        [Route("internal/{accountId}/users", Name = "GetAccountUsersByInternalAccountId")]
        [ApiAuthorize(Roles = "ReadAllAccountUsers")]
        [HttpGet]
        public async Task<IHttpActionResult> GetAccountUsers(long accountId)
        {
            var result = await this._orchestrator.GetAccountTeamMembers(accountId);

            if (result.Data == null)
            {
                return this.NotFound();
            }

            return Ok(result.Data);
        }
        

        private void CreateGetLegalEntityLink(string hashedAccountId, ResourceViewModel legalEntity)
        {
            legalEntity.Href = this.Url.Route("GetLegalEntity", new { hashedAccountId, legalEntityId = legalEntity.Id });
        }

        private void CreateGetPayeSchemeLink(string hashedAccountId, ResourceViewModel payeScheme)
        {
            payeScheme.Href = this.Url.Route("GetPayeScheme", new { hashedAccountId, payeSchemeRef = HttpUtility.UrlEncode(payeScheme.Id) });
        }
    }
}
