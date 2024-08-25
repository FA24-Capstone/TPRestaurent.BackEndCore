using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using TPRestaurent.BackEndCore.Application.Contract.IServices;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HashingController : ControllerBase
    {
        private IHashingService _hashingService;
        public HashingController(IHashingService hashingService)
        {
            _hashingService = hashingService;
        }

        [HttpGet("hashing")]
        public string GetHashedHMAC(string password, string key)
        {
           return _hashingService.Hashing(password, key);
        }

        [HttpGet("decode-hashing")]
        public string GetUnHashedHMAC(string password, string key)
        {
            return _hashingService.DeHashing(password, key);
        }
    }
}
