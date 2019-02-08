namespace SFA.DAS.EmployerAccounts.Api.Controllers
{
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;

    [RoutePrefix("api/user/{userRef}")]
    public class EmployerUserController : ApiController
    {
        private readonly UsersOrchestrator _orchestrator;

        public EmployerUserController(UsersOrchestrator orchestrator)
        {
            this._orchestrator = orchestrator;
        }

        [Route("accounts", Name = "Accounts")]
        [ApiAuthorize(Roles = "ReadUserAccounts")]
        [HttpGet]
        public async Task<IHttpActionResult> GetUserAccounts(string userRef)
        {
            var result = await this._orchestrator.GetUserAccounts(userRef);

            if (result.Status == HttpStatusCode.OK)
            {
                return Ok(result.Data);
            }
            
            //TODO: Handle unhappy paths.
            return this.Conflict();
        }
    }
}