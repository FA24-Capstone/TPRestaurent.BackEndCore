using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.ConfigurationModel;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class JwtService : GenericBackendService, IJwtService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Account> _userManager;
        private BackEndLogger _logger;
        private JWTConfiguration _jwtConfiguration;

        public JwtService(IUnitOfWork unitOfWork,
            UserManager<Account> userManager,
            IServiceProvider serviceProvider,
            BackEndLogger logger) : base(serviceProvider)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
            _jwtConfiguration = Resolve<JWTConfiguration>()!;
        }

        public async Task<string> GenerateAccessToken(LoginRequestDto loginRequest)
        {
            try
            {
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var utility = Resolve<Common.Utils.Utility>();
                var user = await accountRepository!.GetByExpression(u =>
                    u!.PhoneNumber.ToLower() == loginRequest.PhoneNumber.ToLower());

                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles != null)
                    {
                        var claims = new List<Claim>
                    {
                        new(ClaimTypes.MobilePhone, loginRequest.PhoneNumber),
                        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new("AccountId", user.Id)
                    };
                        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role.ToUpper())));
                        var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Key!));
                        var token = new JwtSecurityToken(
                            _jwtConfiguration.Issuer,
                            _jwtConfiguration.Audience,
                            expires: utility!.GetCurrentDateInTimeZone().AddDays(1),
                            claims: claims,
                            signingCredentials: new SigningCredentials(authenKey, SecurityAlgorithms.HmacSha512Signature)
                        );
                        return new JwtSecurityTokenHandler().WriteToken(token);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, this);
            }

            return string.Empty;
        }

        public async Task<string> GenerateAccessTokenForDevice(LoginDeviceRequestDto loginDeviceRequestDto)
        {
            try
            {
                var deviceRepository = Resolve<IGenericRepository<Device>>();
                var utility = Resolve<Common.Utils.Utility>();
                var device = await deviceRepository!.GetByExpression(u => u.DeviceCode == loginDeviceRequestDto.DeviceCode, null);

                if (device != null)
                {

                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.NameIdentifier, loginDeviceRequestDto.DeviceCode),
                        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new("DeviceCode", device.DeviceCode.ToString())
                    };
                    claims.Add(new(ClaimTypes.Role, "Device"));
                    var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Key!));
                    var token = new JwtSecurityToken(
                        _jwtConfiguration.Issuer,
                        _jwtConfiguration.Audience,
                        expires: utility!.GetCurrentDateInTimeZone().AddYears(1),
                        claims: claims,
                        signingCredentials: new SigningCredentials(authenKey, SecurityAlgorithms.HmacSha512Signature)
                    );
                    return new JwtSecurityTokenHandler().WriteToken(token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, this);
            }

            return string.Empty;
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<TokenDto> GetNewToken(string refreshToken, string accountId)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var accessTokenNew = "";
                var refreshTokenNew = "";
                try
                {
                    var accountRepository = Resolve<IGenericRepository<Account>>();
                    var utility = Resolve<Common.Utils.Utility>();

                    var user = await accountRepository!.GetByExpression(u => u!.Id.ToLower() == accountId);

                    if (user != null && user.RefreshToken == refreshToken)
                    {
                        var roles = await _userManager.GetRolesAsync(user);
                        var claims = new List<Claim>
                        {
                        new(ClaimTypes.MobilePhone, user.PhoneNumber!),
                        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new("AccountId", user.Id)
                        };

                        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
                        var authenKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Key!));
                        var token = new JwtSecurityToken
                        (
                            _jwtConfiguration.Issuer,
                            _jwtConfiguration.Audience,
                            expires: utility!.GetCurrentDateInTimeZone().AddDays(1),
                            claims: claims,
                            signingCredentials: new SigningCredentials(authenKey, SecurityAlgorithms.HmacSha512Signature)
                        );

                        accessTokenNew = new JwtSecurityTokenHandler().WriteToken(token);
                        if (user.RefreshTokenExpiryTime <= utility.GetCurrentDateInTimeZone())
                        {
                            user.RefreshToken = GenerateRefreshToken();
                            user.RefreshTokenExpiryTime = utility.GetCurrentDateInTimeZone().AddDays(1);
                            refreshTokenNew = user.RefreshToken;
                        }
                        else
                        {
                            refreshTokenNew = refreshToken;
                        }
                    }

                    await _unitOfWork.SaveChangesAsync();
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, this);
                }

                return new TokenDto { Token = accessTokenNew, RefreshToken = refreshTokenNew };
            }
        }
    }
}
