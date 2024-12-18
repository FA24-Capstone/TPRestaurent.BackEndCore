using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.API.Middlewares;

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

        [HttpGet("remove-cache")]
        [RemoveCacheAtrribute("/")]
        public IActionResult RemoveCache()
        {
            return Ok();
        }
    }
}
