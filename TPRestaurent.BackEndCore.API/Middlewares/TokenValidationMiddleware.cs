using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using TPRestaurent.BackEndCore.Application;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
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
            if (string.IsNullOrEmpty(token))
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsync("Token can not be null");
                return;
            }
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
            var tokenRepository = serviceProvider.GetRequiredService<IGenericRepository<Token>>();
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            var checkUserToken = await tokenService.GetUserToken(token);
            var userToken = checkUserToken.Result as Token;

            if (userToken == null)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsync("Invalid or expired token.");
                return;
            }

            if (userToken.ExpiryTimeAccessToken < DateTime.UtcNow.AddHours(7))
            {
                await tokenRepository.DeleteById(userToken.TokenId);
                await unitOfWork.SaveChangesAsync();
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsync("Expired token.");
                return;
            }

            if (userToken.DeviceIP != deviceIp)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsync("Invalid device.");
                return;
            }

            var roleClaims = jwtToken!.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();
            if (!roleClaims.Contains(role))
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await httpContext.Response.WriteAsync("Invalid or expired token.");
                return;
            }

            await next();
        }
    }
}