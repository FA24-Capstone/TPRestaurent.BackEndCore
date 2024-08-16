using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("data-upload")]
    [ApiController]
    public class DataUploadController : ControllerBase
    {
        private IFirebaseService _service;
        public DataUploadController(IFirebaseService service)
        {
            _service = service;
        }

        [HttpPost("upload")]
        public async Task<AppActionResult> UpdloadFileToFireBase(IFormFile file)
        {
            string path = $"{SD.FirebasePathName.DISH_PREFIX}{Guid.NewGuid()}.jpg";
            return await _service.UploadFileToFirebase(file, path);
        }
    }
}
