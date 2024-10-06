using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("configuration")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private IConfigService _service;
        public ConfigurationController(IConfigService service)
        {
            _service = service;
        }

        [HttpPost("create-config-service")]
        public async Task<AppActionResult> CreateConfigurationService(ConfigurationVersionDto configurationVersionDto)
        {
            return await _service.CreateConfigurationVersion(configurationVersionDto);
        }

        [HttpPost("create-config")]
        public Task<AppActionResult> CreateConfiguration(ConfigurationDto dto)
        {
            return _service.CreateConfiguration(dto);
        }

        [HttpGet("get-all-configuration-version/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllConfigurationVersion(int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllConfigurationVersion(pageNumber, pageSize);       
        }

        [HttpPut("update-config")]
        public Task<AppActionResult> UpdateConfiguration(UpdateConfigurationDto dto)
        {
            return _service.UpdateConfiguration(dto);
        }

        [HttpGet("get-all-config/{pageNumber:int}/{pageSize:int}")]
        public Task<AppActionResult> GetAll(int pageNumber, int pageSize)
        {
            return _service.GetAll(pageNumber, pageSize);
        }

        [HttpGet("get-config-by-name/{name}")]
        public Task<AppActionResult> GetByName(string name)
        {
            return _service.GetByName(name);
        }


    }
}
