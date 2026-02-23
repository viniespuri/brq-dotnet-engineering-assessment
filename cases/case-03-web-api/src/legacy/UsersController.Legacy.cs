using System.Web.Http;

namespace Case3.Legacy
{
    public class UsersController : ApiController
    {
        [Route("v1/users/")]
        [HttpGet]
        public IHttpActionResult GetUser([FromUri] int id)
        {
            UserService USER_Service = new UserService();
            return USER_Service.GetUser(id);
        }
    }
}
