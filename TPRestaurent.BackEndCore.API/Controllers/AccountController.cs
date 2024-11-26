using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using TPRestaurent.BackEndCore.API.Middlewares;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("login")]
        public async Task<AppActionResult> Login(LoginRequestDto request)
        {
            return await _accountService.Login(request, HttpContext);
        }

        [HttpPost("update-account-phone-number")]
        public async Task<AppActionResult> UpdateAccountPhoneNumber(UpdatePhoneRequestDto accountRequest)
        {
            return await _accountService.UpdateAccountPhoneNumber(accountRequest);
        }

        [HttpPost("send-otp")]
        public async Task<AppActionResult> SendOTP(string phoneNumber, OTPType otp)
        {
            return await _accountService.SendOTP(phoneNumber, otp);
        }

        [HttpPost("verify-for-reservation")]
        public async Task<AppActionResult> VerifyForReservation(string phoneNumber, string code)
        {
            return await _accountService.VerifyForReservation(phoneNumber, code);
        }

        [HttpGet("get-account-by-user-id/{id}")]
        public async Task<AppActionResult> GetAccountByUserId(string id)
        {
            return await _accountService.GetAccountByUserId(id);
        }

        [HttpPost("create-account")]
        public async Task<AppActionResult> CreateAccount(SignUpRequestDto request)
        {
            return await _accountService.CreateAccount(request, false);
        }

        [HttpGet("get-all-account")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<AppActionResult> GetAllAccount(string? keyword, int pageIndex = 1, int pageSize = 10)
        {
            return await _accountService.GetAllAccount(keyword, pageIndex, pageSize);
        }

        [HttpGet("get-accounts-by-role-name/{roleName}/{pageIndex:int}/{pageSize:int}")]
        public async Task<AppActionResult> GetAccountsByRoleName(string roleName, int pageIndex = 1, int pageSize = 10)
        {
            return await _accountService.GetAccountsByRoleName(roleName, pageIndex, pageSize);
        }

        [HttpGet("get-accounts-by-role-id/{roleId}/{pageIndex:int}/{pageSize:int}")]
        public async Task<AppActionResult> GetAccountsByRoleId(Guid roleId, int pageIndex = 1, int pageSize = 10)
        {
            return await _accountService.GetAccountsByRoleId(roleId, pageIndex, pageSize);
        }

        [HttpPost("get-new-token/{userId}")]
        public async Task<AppActionResult> GetNewToken([FromBody] string refreshToken, string userId)
        {
            return await _accountService.GetNewToken(refreshToken, userId);
        }

        [HttpPut("change-password")]
        public async Task<AppActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var headers = HttpContext.Request.Headers;

            string userAgent = headers["User-Agent"].ToString();
            string token = ExtractJwtToken(headers["Authorization"]);
            if (token != null)
            {
                return await _accountService.ChangePassword(dto, token);
            }
            else
            {
                return new AppActionResult 
                { 
                    IsSuccess = false,
                    Messages = new List<string?>
                    {
                        "Không tìm thấy token"
                    },
                };
            }
        }

        [HttpPut("forgot-password")]
        public async Task<AppActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            return await _accountService.ForgotPassword(dto);
        }

        [HttpPost("google-callback")]
        public async Task<AppActionResult> GoogleCallBack([FromBody] string accessTokenFromGoogle)
        {
            return await _accountService.GoogleCallBack(accessTokenFromGoogle);
        }

        [HttpPost("send-email-for-activeCode/{email}")]
        public async Task<AppActionResult> SendEmailForActiveCode(string email)
        {
            return await _accountService.SendEmailForActiveCode(email);
        }

        [HttpPost("verify-number")]
        public async Task<AppActionResult> VerifyNumberAccount(string phoneNumber, string optCode)
        {
            return await _accountService.VerifyNumberAccount(phoneNumber, optCode);
        }

        [HttpPut("update-account")]
        public async Task<AppActionResult> UpdateAccount([FromForm] UpdateAccountInfoRequest updateAccountRequest)
        {
            return await _accountService.UpdateAccount(updateAccountRequest);
        }

        [HttpDelete("delete-account")]
        public async Task<AppActionResult> DeleteAccount(string customerId)
        {
            return await _accountService.DeleteAccount(customerId);
        }

        [HttpPost("verify-account-otp")]
        public async Task<AppActionResult> VerifyAccountOTP(string phoneNumber, string code, OTPType type)
        {
            return await _accountService.VerifyAccountOTP(phoneNumber, code, type);
        }

        //[HttpPost("send-account-otp")]
        //public async Task<AppActionResult> SendCustomerInfoOTP(string phoneNumber, OTPType otpType)
        //{
        //    return await _accountService.SendAccountOTP(phoneNumber, otpType);
        //}

        [HttpGet("get-account-by-phone-number-keyword/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAccountByPhoneNumberKeyword(string phoneNumber, int pageNumber = 1, int pageSize = 10)
        {
            return await _accountService.GetAccountByPhoneNumberKeyword(phoneNumber, pageNumber, pageSize);
        }

        [HttpGet("get-account-by-phone-number")]
        public async Task<AppActionResult> GetCustomerInfoByPhoneNumber(string phoneNumber)
        {
            return await _accountService.GetAccountByPhoneNumber(phoneNumber);
        }

        [HttpPost("create-customer-info-address")]
        public async Task<AppActionResult> CreateCustomerInfoAddress(CustomerInfoAddressRequest customerInfoAddressRequest)
        {
            return await _accountService.CreateCustomerInfoAddress(customerInfoAddressRequest);
        }

        [HttpPut("update-customer-info-address")]
        public async Task<AppActionResult> UpdateCustomerInfoAddress(UpdateCustomerInforAddressRequest updateCustomerInforAddress)
        {
            return await _accountService.UpdateCustomerInfoAddress(updateCustomerInforAddress);
        }

        [HttpPut("delete-customer-info-address")]
        public async Task<AppActionResult> DeleteCustomerInfoAddress(Guid customerInfoAddresId)
        {
            return await _accountService.DeleteCustomerInfoAddress(customerInfoAddresId);
        }

        [HttpGet("load-available-shipper")]
        public async Task<AppActionResult> LoadAvailableShipper()
        {
            return await _accountService.LoadAvailableShipper();
        }

        [HttpPut("update-delivering-status/{shipperId}")]
        public async Task<AppActionResult> UpdateDeliveringStatus(string shipperId, bool isDelivering)
        {
            return await _accountService.UpdateDeliveringStatus(shipperId, isDelivering);
        }

        [HttpPost("change-email-request")]
        public async Task<AppActionResult> ChangeEmailRequest(string accountId, string newEmail)
        {
            return await _accountService.ChangeEmailRequest(accountId, newEmail);
        }

        [HttpPost("verify-change-email")]
        public async Task<AppActionResult> VerifyChangeEmail(string email, string accountId, string otpCode)
        {
            return await _accountService.VerifyChangeEmail(email, accountId, otpCode);
        }

        [HttpPost("up-role")]
        public async Task<AppActionResult> UpLevel(string accountId, string roleName)
        {
            return await _accountService.UpRole(accountId, roleName);
        }

        [HttpPost("create-account-for-restaurant-employees")]
        public async Task<AppActionResult> CreateAccountForRestaurantEmployees(EmployeeSignUpRequest request)
        {
            return await _accountService.CreateAccountForRestaurantEmployees(request, false);
        }

        [HttpPost("ban-user/{accountId}")]
        public async Task<AppActionResult> BanUser(string accountId)
        {
            return await _accountService.BanUser(accountId);
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