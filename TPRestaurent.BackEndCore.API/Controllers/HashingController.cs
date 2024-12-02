using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("api/hashing")]
    [ApiController]
    public class HashingController : ControllerBase
    {
        private IHashingService _hashingService;

        public HashingController(IHashingService hashingService)
        {
            _hashingService = hashingService;
        }

        [HttpGet("hashing")]
        public AppActionResult GetHashedHMAC(string value, string key)
        {
            return _hashingService.Hashing(value, key);
        }

        [HttpGet("decode-hashing")]
        public AppActionResult GetUnHashedHMAC(string value, string key)
        {
            return _hashingService.DeHashing(value, key);
        }
    }
}