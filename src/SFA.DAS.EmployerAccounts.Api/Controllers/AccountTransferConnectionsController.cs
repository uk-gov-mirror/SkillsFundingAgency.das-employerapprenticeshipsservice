namespace SFA.DAS.EmployerAccounts.Api.Controllers
{
    using System;
    using System.Web.Http;

    using SFA.DAS.EmployerAccounts.Api.Attributes;

    [RoutePrefix("api/accounts/{hashedAccountId}/transfersconnections")]
    public class AccountTransferConnectionsController : ApiController
    {
        [Route("", Name = "GetTransferConnections")]
        [ApiAuthorize(Roles = "ReadUserAccounts")]
        [HttpGet]
        public IHttpActionResult GetTransferConnections(string hashedAccountId)
        {
            throw new NotImplementedException();
        }
    }
}