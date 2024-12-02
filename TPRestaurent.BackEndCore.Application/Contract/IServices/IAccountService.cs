using Microsoft.AspNetCore.Http;
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

        Task<AppActionResult> GetAccountByUserId(string id);

        Task<AppActionResult> GetAllAccount(string? keyword, int pageIndex, int pageSize);

        Task<AppActionResult> GetNewToken(string refreshToken, string userId);

        Task<string> GenerateVerifyCode(string email, bool isForForgettingPassword);

        Task<string> GenerateVerifyCodeSms(string phoneNumber, bool isForForgettingPassword);

        Task<AppActionResult> GoogleCallBack(string accessTokenFromGoogle);

        Task<AppActionResult> GetAccountsByRoleName(string roleName, int pageNumber, int pageSize);

        Task<AppActionResult> GetAccountsByRoleId(Guid Id, int pageNumber, int pageSize);

        Task<AppActionResult> GenerateOTP(string phoneNumber);

        Task<AppActionResult> UpdateAccount(UpdateAccountInfoRequest updateAccountRequest);

        Task<AppActionResult> DeleteAccount(string customerId);

        Task<AppActionResult> SendEmailForActiveCode(string email);

        Task<AppActionResult> GenerateCustomerOTP(Account customerInfo, OTPType otpType);

        Task<AppActionResult> VerifyAccountOTP(string phoneNumber, string code, OTPType type);

        //Task<AppActionResult> SendAccountOTP(string phoneNumber, OTPType otpType);
        Task<AppActionResult> GetAccountByPhoneNumber(string phoneNumber);

        Task<AppActionResult> CreateCustomerInfoAddress(CustomerInfoAddressRequest customerInfoAddressRequest);

        Task<AppActionResult> UpdateCustomerInfoAddress(UpdateCustomerInforAddressRequest updateCustomerInforAddress);

        Task<AppActionResult> DeleteCustomerInfoAddress(Guid customerInfoAddresId);

        Task<AppActionResult> LoadAvailableShipper();

        Task<AppActionResult> UpdateDeliveringStatus(string accountId, bool isDelivering);

        Task<AppActionResult> ChangeEmailRequest(string accountId, string newEmail);

        Task<AppActionResult> VerifyChangeEmail(string email, string accountId, string otpCode);

        Task<AppActionResult> UpRole(string accountId, string roleName);

        Task<AppActionResult> BanUser(string accountId);

        Task<AppActionResult> CreateAccountForRestaurantEmployees(EmployeeSignUpRequest signUpRequestDto, bool isGoogle);

        Task<AppActionResult> GetAccountByPhoneNumberKeyword(string phoneNumber, int pageNumber, int pageSize);

        Task DeleteOverdueOTP();
        Task<AppActionResult> IsExistAccount(string phoneNumber);
    }
}