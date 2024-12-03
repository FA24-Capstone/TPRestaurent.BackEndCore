using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class TokenService : GenericBackendService, ITokenService
    {
        private readonly IGenericRepository<Token> _tokenRepository;
        private IUnitOfWork _unitOfWork;
        public TokenService(IServiceProvider serviceProvider, IGenericRepository<Token> tokenRepository, IUnitOfWork unitOfWork) : base(serviceProvider)
        {
            _tokenRepository = tokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AppActionResult> EnableNotification(string deviceToken, HttpContext httpContext)
        {
            var result = new AppActionResult();
            await _unitOfWork.ExecuteInTransaction(async () =>
            {
                try
                {
                    var headers = httpContext.Request.Headers;

                    string userAgent = headers["User-Agent"].ToString();
                    string deviceName = ParseDeviceNameFromUserAgent(userAgent);
                    string token = ExtractJwtToken(headers["Authorization"]);

                    var tokenDb =
                        await _tokenRepository.GetByExpression(p => p.AccessTokenValue == token, p => p.Account!);
                    if (tokenDb == null)
                    {
                        throw new Exception($"Không tìm thấy token với {token}");
                    }

                    var userIP = GetClientIpAddress(httpContext);

                    tokenDb.DeviceName = deviceName;
                    tokenDb.DeviceToken = deviceToken;
                    tokenDb.DeviceIP = userIP;

                    if (!BuildAppActionResultIsError(result))
                    {
                        await _tokenRepository.Update(tokenDb);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
            });
            return result;
        }

        public async Task<AppActionResult> GetAllTokenByUser(string accountId, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            try
            {
                result.Result = await _tokenRepository.GetAllDataByExpression(p => p.AccountId == accountId, pageNumber, pageSize, null, false, p => p.Account!);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetUserToken(string token)
        {
            var result = new AppActionResult();
            var utility = Resolve<Utility>();
            try
            {
                var tokenDb = await _tokenRepository.GetByExpression(p => p.AccessTokenValue == token, p => p.Account!);
                if (tokenDb == null)
                {
                    return BuildAppActionResultError(result, $"Token này không tồn tại");
                }
                result.Result = tokenDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> InvalidateTokensForUser(string accountId, string currentToken)
        {
            var result = new AppActionResult();
            var utility = Resolve<Utility>();
            try
            {
                var accountTokenList = await _tokenRepository.GetAllDataByExpression(p => p.AccountId == accountId, 0, 0, null, false, p => p.Account!);
                if (accountTokenList == null)
                {
                    return BuildAppActionResultError(result, $"Danh sách token của tài khoản {accountId} không tồn tại");
                }
                if (accountTokenList!.Items!.Count < 0 && accountTokenList.Items != null)
                {
                    foreach (var token in accountTokenList.Items)
                    {
                        token.IsActive = false;
                        token.ExpiryTimeAccessToken = utility!.GetCurrentDateTimeInTimeZone();
                        token.ExpiryTimeRefreshToken = utility.GetCurrentDateTimeInTimeZone();
                    }
                }

                var tokenDb = accountTokenList.Items.FirstOrDefault(p => p.AccessTokenValue == currentToken);
                if (tokenDb != null)
                {
                    tokenDb.IsActive = true;
                    await _tokenRepository.Update(tokenDb);
                }

                await _tokenRepository.DeleteRange(accountTokenList.Items.Where(p => p.IsActive == false));
                await _unitOfWork.SaveChangesAsync();   
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> LogOutAllDevice(string accountId)
        {
            var result = new AppActionResult();
            try
            {
                var tokenDb = await _tokenRepository.GetAllDataByExpression(p => p.AccountId == accountId, 0, 0, null, false, p => p.Account!);
                if (tokenDb == null)
                {
                    return BuildAppActionResultError(result, $"Danh sách token của tài khoản {accountId} không tồn tại");
                }

                await _tokenRepository.DeleteRange(tokenDb!.Items!);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> DeleteTokenById(Guid tokenId)
        {
            var result = new AppActionResult();
            try
            {
                var tokenDb = await _tokenRepository.GetByExpression(p => p.TokenId == tokenId);
                if (tokenDb == null)
                {
                    throw new Exception($"Token với id {tokenId} không tồn tại");
                }
                else
                {
                    await _tokenRepository.DeleteById(tokenId);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetUserTokenByIp(HttpContext httpContext)
        {
            var result = new AppActionResult();
            try
            {
                var headers = httpContext.Request.Headers;

                string userAgent = headers["User-Agent"].ToString();
                string deviceName = ParseDeviceNameFromUserAgent(userAgent);
                string accessToken = ExtractJwtToken(headers["Authorization"]);

                var userIP = GetClientIpAddress(httpContext);

                var tokenDb = await _tokenRepository.GetByExpression(p => p.AccessTokenValue == accessToken && p.DeviceIP == userIP, p => p.Account!);
                if (tokenDb == null)
                {
                    throw new Exception($"Không tìm thấy token ");
                }

                result.Result = tokenDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public string GetClientIpAddress(HttpContext context)
        {
            string ip = null;

            // Try to get IP from X-Forwarded-For header
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                ip = forwardedFor.Split(',')[0].Trim();
            }

            // If not found, try X-Real-IP header
            if (string.IsNullOrEmpty(ip) && context.Request.Headers.ContainsKey("X-Real-IP"))
            {
                ip = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            }

            // If not found, try CF-Connecting-IP header (Cloudflare)
            if (string.IsNullOrEmpty(ip) && context.Request.Headers.ContainsKey("CF-Connecting-IP"))
            {
                ip = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
            }

            // If still not found, use RemoteIpAddress
            if (string.IsNullOrEmpty(ip) && context.Connection.RemoteIpAddress != null)
            {
                ip = context.Connection.RemoteIpAddress.ToString();
            }

            // If IP is a loopback address, try to get the local IP
            if (string.IsNullOrEmpty(ip) || ip == "::1")
            {
                ip = GetLocalIpAddress();
            }

            // If still empty, return UNKNOWN
            if (string.IsNullOrEmpty(ip))
            {
                ip = "UNKNOWN";
            }

            return ip;
        }

        private string ParseDeviceNameFromUserAgent(string userAgent)
        {
            if (userAgent.Contains("Windows"))
                return "Windows PC";
            else if (userAgent.Contains("Mac"))
                return "Mac";
            else if (userAgent.Contains("iPhone"))
                return "iPhone";
            else if (userAgent.Contains("Android"))
                return "Android Device";
            else
                return "Unknown Device";
        }

        private string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ipAddress in host.AddressList)
            {
                if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ipAddress.ToString();
                }
            }
            return string.Empty;
        }

        private string ExtractJwtToken(string authorizationHeader)
        {
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return null;
            }

            string[] parts = authorizationHeader.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2 && parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
            {
                return parts[1];
            }

            return null;
        }
    }
}