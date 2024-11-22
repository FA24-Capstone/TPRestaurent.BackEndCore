using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.API.Middlewares
{
    public class TokenValidationMiddleware : Attribute, IAsyncActionFilter
    {
        private readonly string role;

        public TokenValidationMiddleware(string role)
        {
            this.role = $"{role}";
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var token = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

            var deviceIp = string.Empty;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ipAddress in host.AddressList)
            {
                if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    deviceIp = ipAddress.ToString();
                }
            }

            var serviceProvider = httpContext.RequestServices;
            var tokenService = serviceProvider.GetRequiredService<ITokenService>();
            var checkUserToken = await tokenService.GetUserToken(token);
            var userToken = checkUserToken.Result as Token;

            if (userToken == null || token == null || userToken.DeviceIP != deviceIp || userToken.IsActive == false)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsync("Invalid or expired token.");
                return;
            }
            else
            {
                var roleClaims = jwtToken!.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();
                if (!roleClaims.Contains(role))
                {
                    httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await httpContext.Response.WriteAsync("Invalid or expired token.");
                    return;
                }
            }

            await next();
        }
    }
}