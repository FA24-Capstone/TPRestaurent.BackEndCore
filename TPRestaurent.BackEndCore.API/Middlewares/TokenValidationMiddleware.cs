using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.API.Middlewares
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITokenService _tokenService;   
        public TokenValidationMiddleware(RequestDelegate next, ITokenService tokenService)
        {
            _next = next;   
            _tokenService = tokenService;   
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(token))
            {
                var ipAddress = context.Connection.RemoteIpAddress?.ToString();
                var checkUserToken = await _tokenService.GetUserToken(token);
                var userToken = checkUserToken.Result as Token ;
                if (userToken == null || userToken.DeviceIP != ipAddress || userToken.IsActive == false)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid or expired token.");
                    return;
                }
            }
            await _next(context);
        }
    }
}
