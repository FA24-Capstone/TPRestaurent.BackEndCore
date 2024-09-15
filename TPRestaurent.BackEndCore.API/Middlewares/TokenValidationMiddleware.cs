using Microsoft.Extensions.DependencyInjection;
using System.Net;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.API.Middlewares
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        public TokenValidationMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        //public async Task InvokeAsync(HttpContext context)
        //{
        //    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        //    if (!string.IsNullOrEmpty(token))
        //    {
        //        using (var scope = _serviceProvider.CreateScope())
        //        {
        //            var deviceIp = string.Empty;   
        //            var tokenService = scope.ServiceProvider.GetRequiredService<ITokenService>();
        //            var host = Dns.GetHostEntry(Dns.GetHostName());
        //            foreach (var ipAddress in host.AddressList)
        //            {
        //                if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        //                {
        //                    deviceIp = ipAddress.ToString();
        //                }
        //            }
        //            var checkUserToken = await tokenService.GetUserToken(token);
        //            var userToken = checkUserToken.Result as Token;
        //            if (userToken == null || userToken.DeviceIP != deviceIp || userToken.IsActive == false)
        //            {
        //                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        //                await context.Response.WriteAsync("Invalid or expired token.");
        //                return;
        //            }
        //        }
        //    }
        //    await _next(context);
        //}
    }
}
