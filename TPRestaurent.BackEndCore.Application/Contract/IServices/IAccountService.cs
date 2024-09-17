using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IAccountService
    {
        Task<AppActionResult> Login(LoginRequestDto loginRequest, HttpContext httpContext);
        Task<AppActionResult> VerifyLoginGoogle(string email, string verifyCode);
        Task<AppActionResult> CreateAccount(SignUpRequestDto signUpRequest, bool isGoogle);
        Task<AppActionResult> VerifyNumberAccount(string phoneNumber, string optCode);
        Task<AppActionResult> UpdateAccountPhoneNumber(UpdatePhoneRequestDto applicationUser);
        Task<AppActionResult> SendOTP(string phoneNumber, OTPType otp);
        Task<AppActionResult> VerifyForReservation(string phoneNumber, string code);
        Task<AppActionResult> ChangePassword(ChangePasswordDto changePasswordDto);
        Task<AppActionResult> GetAccountByUserId(string id);
        Task<AppActionResult> GetAllAccount(int pageIndex, int pageSize);
        Task<AppActionResult> GetNewToken(string refreshToken, string userId);
        Task<AppActionResult> ForgotPassword(ForgotPasswordDto dto);
        Task<AppActionResult> ActiveAccount(string email, string verifyCode);
        Task<string> GenerateVerifyCode(string email, bool isForForgettingPassword);
        Task<string> GenerateVerifyCodeSms(string phoneNumber, bool isForForgettingPassword);
        Task<AppActionResult> GoogleCallBack(string accessTokenFromGoogle);
        Task<AppActionResult> GetAccountsByRoleName(string roleName, int pageNumber, int pageSize);
        Task<AppActionResult> GetAccountsByRoleId(Guid Id, int pageNumber, int pageSize);
        Task<AppActionResult> GenerateOTP(string phoneNumber);
        Task<AppActionResult> UpdateAccount(UpdateAccountInfoRequest updateAccountRequest);
        Task<AppActionResult> DeleteAccount(Guid customerId);
        Task<AppActionResult> SendEmailForActiveCode(string email);
        Task<AppActionResult> GenerateCustomerInfoOTP(Account customerInfo, OTPType otpType);
        Task<AppActionResult> VerifyAccountOTP(string phoneNUmber, string code, OTPType otpType);
        Task<AppActionResult> SendAccountOTP(string phoneNumber, OTPType otpType);
        Task<AppActionResult> GetAccountByPhoneNumber(string phoneNumber);
        Task DeleteOverdueOTP();

    }
}
