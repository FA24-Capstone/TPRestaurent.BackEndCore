﻿using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.API.Middlewares;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;

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

        [HttpPost("get-user-token-by-ip")]
        [TokenValidationMiddleware(Permission.ALL_ACTORS)]
        public async Task<AppActionResult> GetUserTokenByIp()
        {
            return await _tokenService.GetUserTokenByIp(HttpContext);
        }

        [HttpPost("enable-notification")]
        [TokenValidationMiddleware(Permission.ALL_ACTORS)]
        public async Task<AppActionResult> EnableNotification(string? deviceToken)
        {
            return await _tokenService.EnableNotification(deviceToken, HttpContext);
        }

        [HttpDelete("delete-token")]
        [TokenValidationMiddleware(Permission.ALL)]
        public async Task<AppActionResult> DeleteTokenById(Guid tokenId)
        {
            return await _tokenService.DeleteTokenById(tokenId);
        }


    }
}