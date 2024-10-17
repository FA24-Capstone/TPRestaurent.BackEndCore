using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface ITokenService
    {
        Task<AppActionResult> GetUserToken(string token);
        Task<AppActionResult> InvalidateTokensForUser(string accountId);
        Task<AppActionResult> GetAllTokenByUser(string accountId, int pageNumber, int pageSize);
        Task<AppActionResult> LogOutAllDevice(string accountId);
        Task<AppActionResult> EnableNotification(string deviceToken ,HttpContext httpContext);
        Task<AppActionResult> DeleteTokenById(Guid tokenId);
        Task<AppActionResult> GetUserTokenByIp(HttpContext httpContext);
    }
}
