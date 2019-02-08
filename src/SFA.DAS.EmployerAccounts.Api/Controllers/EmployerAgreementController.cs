namespace SFA.DAS.EmployerAccounts.Api.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Http;

    [RoutePrefix("api/accounts/{hashedAccountId}/legalEntities/{hashedlegalEntityId}/agreements")]
    public class EmployerAgreementController : ApiController
    {
        private readonly AgreementOrchestrator _orchestrator;
        
        public EmployerAgreementController(AgreementOrchestrator orchestrator)
        {
            this._orchestrator = orchestrator;
        }

        [Route("{agreementId}", Name = "AgreementById")]
        [ApiAuthorize(Roles = "ReadAllEmployerAgreements")]
        [HttpGet]   
        public async Task<IHttpActionResult> GetAgreement(string agreementId)
        {
            var response = await this._orchestrator.GetAgreement(agreementId);

            if (response.Data == null)
            {
                return this.NotFound();
            }

            return Ok(response.Data);
        }
    }
}
