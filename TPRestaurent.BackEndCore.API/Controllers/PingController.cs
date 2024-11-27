using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("ping")]
    [ApiController]
    public class PingController : ControllerBase
    {
        public PingController() { }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}
