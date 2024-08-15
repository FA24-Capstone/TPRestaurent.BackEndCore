using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IJwtService
    {
        string GenerateRefreshToken();

        Task<string> GenerateAccessToken(LoginRequestDto loginRequest);
        Task<string> GenerateAccessToken(LoginDeviceRequestDto loginDeviceRequest);
        Task<TokenDto> GetNewToken(string refreshToken, string accountId);
    }
}
