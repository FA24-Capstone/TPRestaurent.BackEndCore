using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using TPRestaurent.BackEndCore.Application.Contract.IServices;

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
        public string GetHashedHMAC(string value, string key)
        {
           return _hashingService.Hashing(value, key);
        }

        [HttpGet("decode-hashing")]
        public string GetUnHashedHMAC(string value, string key)
        {
            return _hashingService.DeHashing(value, key);
        }
    }
}
