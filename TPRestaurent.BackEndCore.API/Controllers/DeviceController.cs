﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet("get-all-device")]
        public async Task<AppActionResult> GetAllDevice(int pageNumber = 1, int pageIndex = 10)
        {
            return await _service.GetAllDevice(pageNumber, pageIndex);      
        }

        [HttpGet("get-device-by-id/{deviceId}")]
        public async Task<AppActionResult> GetDeviceById(Guid deviceId)
        {
            return await _service.GetDeviceById(deviceId);
        }

        [HttpPost("create-device")]
        public async Task<AppActionResult> CreateNewDevice(DeviceAccessRequest deviceAccess)
        {
            return await _service.CreateNewDevice(deviceAccess);        
        }

        [HttpPost("login-device")]
        public async Task<AppActionResult> LoginDevice(LoginDeviceRequestDto loginDeviceRequestDto)
        {
            return await _service.LoginDevice(loginDeviceRequestDto);       
        }

    }
}
