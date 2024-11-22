using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IJwtService
    {
        string GenerateRefreshToken();

        Task<string> GenerateAccessToken(LoginRequestDto loginRequest);

        Task<string> GenerateAccessTokenForDevice(LoginDeviceRequestDto loginDeviceRequest);

        Task<TokenDto> GetNewToken(string refreshToken, string accountId);
    }
}