    using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            return await _accountService.Login(request);
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

        [HttpPut("active-account/{email}/{verifyCode}")]
        public async Task<AppActionResult> ActiveAccount(string email, string verifyCode)
        {
            return await _accountService.ActiveAccount(email, verifyCode);
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

        [HttpPut("update-account-information")]
        public async Task<AppActionResult> UpdateAccountInformation(UpdateAccountInformationRequest request)
        {
            return await _accountService.UpdateAccountInformation(request); 
        }

        [HttpPost("add-new-customer-info")]
        public async Task<AppActionResult> AddNewCustomerInfo(CustomerInforRequest customerInforRequest)
        {
            return await _accountService.AddNewCustomerInfo(customerInforRequest);  
        }

        [HttpPut("update-customer-info")]
        public async Task<AppActionResult> UpdateCustomerInfo(UpdateCustomerInforRequest customerInforRequest)
        {
            return await _accountService.UpdateCustomerInfo(customerInforRequest);  
        }

        [HttpGet("get-all-customer-info-by-account-id/{accountId}/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllCustomerInfoByAccountId(string accountId, int pageNumber = 1, int pageSize = 10)
        {
            return await _accountService.GetAllCustomerInfoByAccountId(accountId, pageNumber, pageSize);
        }

        [HttpGet("get-customer-info")]
        public async Task<AppActionResult> GetCustomerInfo(Guid customerId)
        {
            return await _accountService.GetCustomerInfo(customerId);   
        }

        [HttpDelete("delete-customer-info")]
        public async Task<AppActionResult> DeleteCustomerInfo(Guid customerId)
        {
            return await _accountService.DeleteCustomerInfo(customerId);    
        }

        [HttpPost("verify-customer-info-otp")]
        public async Task<AppActionResult> VerifyCustomerInfoOTP(string phoneNumber, string code, OTPType otpType)
        {
            return await _accountService.VerifyCustomerInfoOTP(phoneNumber, code, otpType);
        }

        [HttpPost("send-customer-info-otp")]
        public async Task<AppActionResult> SendCustomerInfoOTP(Guid customerInfoId, OTPType otpType)
        {
            return await _accountService.SendCustomerInfoOTP(customerInfoId, otpType);
        }


    }
}
