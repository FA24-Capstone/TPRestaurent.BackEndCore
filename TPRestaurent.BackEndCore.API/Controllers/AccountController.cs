    using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.API.Middlewares;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
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
        //[TokenValidationMiddleware("CUSTOMER")]
        public async Task<AppActionResult> GetAllAccount(int pageIndex = 1, int pageSize = 10)
        {
            return await _accountService.GetAllAccount(pageIndex, pageSize);
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
            return await _accountService.ChangePassword(dto);
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

        [HttpDelete("delete-customer-info-address")]
        public async Task<AppActionResult> DeleteCustomerInfoAddress(Guid customerInfoAddresId)
        {
            return await _accountService.DeleteCustomerInfoAddress(customerInfoAddresId);       
        }

        [HttpGet("load-available")]
        public async Task<AppActionResult> LoadAvailableShipper()
        {
            return await _accountService.LoadAvailableShipper();
        }

        [HttpPut("update-delivering-status/{shipperId}")]
        public async Task<AppActionResult> UpdateDeliveringStatus(string shipperId)
        {
            return await _accountService.UpdateDeliveringStatus(shipperId);
        }
    }
}
