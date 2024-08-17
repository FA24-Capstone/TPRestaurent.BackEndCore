using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("device")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private IDeviceService _service;
        public DeviceController(IDeviceService service)
        {
            _service = service;
        }

        //[HttpPost("access-device")]
        //public async Task<AppActionResult> AccessDevice([FromBody] DeviceAccessRequest dto)
        //{
        //    return await _service.AccessDevice(dto);
        //}
    }
}
