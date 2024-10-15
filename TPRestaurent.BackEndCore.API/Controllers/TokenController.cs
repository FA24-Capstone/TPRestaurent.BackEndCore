﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("token")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        private ITokenService _tokenService;
        public TokenController(ITokenService tokenService)
        {
            _tokenService = tokenService;       
        }

        [HttpGet("get-all-token-by-user/{accountId}/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllTokenByUser(string accountId, int pageNumber = 1, int pageSize = 10)
        {
            return await _tokenService.GetAllTokenByUser(accountId, pageNumber, pageSize);
        }

        [HttpPost("log-out-all-device")]
        public async Task<AppActionResult> LogOutAllDevice(string accountId)
        {
            return await _tokenService.LogOutAllDevice(accountId);  
        }

        [HttpPost("enable-notification")]
        public async Task<AppActionResult> EnableNotification(string deviceName, string token, string deviceToken)
        {
            return await _tokenService.EnableNotification(deviceName, token, deviceToken, HttpContext);
        }
    }
}
