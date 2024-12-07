using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.API.Middlewares;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;

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
        [TokenValidationMiddleware(Permission.ADMIN)]
        [RemoveCacheAtrribute("configuration")]
        public async Task<AppActionResult> CreateConfigurationService(ConfigurationVersionDto configurationVersionDto)
        {
            return await _service.CreateConfigurationVersion(configurationVersionDto);
        }

        [HttpPost("create-config")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        [RemoveCacheAtrribute("configuration")]
        public Task<AppActionResult> CreateConfiguration(ConfigurationDto dto)
        {
            return _service.CreateConfiguration(dto);
        }

        [HttpGet("get-all-configuration-version/{pageNumber}/{pageSize}")]
        [CacheAttribute(259200)]
        public async Task<AppActionResult> GetAllConfigurationVersion(int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllConfigurationVersion(pageNumber, pageSize);
        }

        [HttpPut("update-config")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        [RemoveCacheAtrribute("configuration")]
        public Task<AppActionResult> UpdateConfiguration(UpdateConfigurationDto dto)
        {
            return _service.UpdateConfiguration(dto);
        }

        [HttpGet("get-all-config/{pageNumber:int}/{pageSize:int}")]
        [CacheAttribute(259200)]
        public Task<AppActionResult> GetAll(int pageNumber, int pageSize)
        {
            return _service.GetAll(pageNumber, pageSize);
        }

        [HttpGet("get-all-configuration-version/{configId}/{pageNumber}/{pageSize}")]
        [CacheAttribute(259200)]
        public Task<AppActionResult> GetAllConfigurationVersion(Guid configId, int pageNumber = 1, int pageSize = 10)
        {
            return _service.GetAllConfigurationVersion(configId, pageNumber, pageSize);
        }

        [HttpGet("get-config-by-name/{name}")]
        [CacheAttribute(259200)]
        public Task<AppActionResult> GetByName(string name)
        {
            return _service.GetByName(name);
        }
    }
}