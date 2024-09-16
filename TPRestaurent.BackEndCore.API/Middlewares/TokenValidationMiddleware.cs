using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.Implementation;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.API.Middlewares
{
    public class TokenValidationMiddleware : Attribute, IAsyncActionFilter
    {
      
      

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var token = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            var deviceIp = string.Empty;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ipAddress in host.AddressList)
            {
                if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    deviceIp = ipAddress.ToString();
                }
            }

            var serviceProvider = context.HttpContext.RequestServices;
            ITokenService tokenService = serviceProvider.GetService<ITokenService>();
            var checkUserToken = await tokenService.GetUserToken(token);
            var userToken = checkUserToken.Result as Token;

            if (userToken == null || userToken.DeviceIP != deviceIp || userToken.IsActive == false)
            {
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsync("Invalid or expired token.");
                return;
            }

            await next();
        }
    }
}
