using System.Web.Http;

namespace Case3.Legacy
{
    [RoutePrefix("v1/users")]
    public class UsersController : ApiController
    {
        /// <summary>
        /// Corrige o endpoint legado para o contrato GET /v1/users/{id}, validando id invalido (400),
        /// retornando 404 quando o usuario nao for encontrado e 200 no sucesso.
        /// </summary>
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetUser([FromUri] int id)
        {
            if (id <= 0)
            {
                return BadRequest("id must be greater than zero.");
            }

            var userService = new UserService();
            var user = userService.GetUser(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }
    }
}
