using AutoMapper;
using Castle.DynamicProxy.Generators;
using Firebase.Auth;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using OfficeOpenXml.FormulaParsing.FormulaExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.ConfigurationModel;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;
using Twilio.Types;
using static System.Net.WebRequestMethods;
using Utility = TPRestaurent.BackEndCore.Common.Utils.Utility;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class AccountService : GenericBackendService, IAccountService
    {
        private readonly IGenericRepository<Account> _accountRepository;
        private readonly IGenericRepository<IdentityUserRole<string>> _userRoleRepository;
        private readonly IMapper _mapper;
        private readonly SignInManager<Account> _signInManager;
        private readonly TokenDto _tokenDto;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Account> _userManager;
        private readonly IEmailService _emailService;
        private readonly IExcelService _excelService;
        private readonly IGenericRepository<OTP> _otpRepository;
        public AccountService(
            IGenericRepository<Account> accountRepository,
            IUnitOfWork unitOfWork,
            UserManager<Account> userManager,
            SignInManager<Account> signInManager,
            IEmailService emailService,
            IExcelService excelService,
            IMapper mapper,
            IServiceProvider serviceProvider,
            IGenericRepository<IdentityUserRole<string>> userRoleRepository,
            IGenericRepository<OTP> otpRepository
        ) : base(serviceProvider)
        {
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _excelService = excelService;
            _otpRepository = otpRepository;
            _tokenDto = new TokenDto();
            _mapper = mapper;
            _userRoleRepository = userRoleRepository;
            _otpRepository = otpRepository;
        }

        public async Task<AppActionResult> Login(LoginRequestDto loginRequest, HttpContext httpContext)
        {
            var result = new AppActionResult();
            try
            {
                var utility = Resolve<Utility>();
                var tokenRepository = Resolve<IGenericRepository<Token>>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var user = await _accountRepository.GetByExpression(u =>
                    u!.PhoneNumber!.ToLower() == loginRequest.PhoneNumber.ToLower() && u.IsDeleted == false);
                var customerInfo = await _accountRepository!.GetByExpression(p => p.CustomerId == user.CustomerId, null);
                var otpCodeListDb = await _otpRepository.GetAllDataByExpression(p => p.Code == loginRequest.OTPCode && (p.Type == OTPType.Login || p.Type == OTPType.ConfirmPhone) && p.ExpiredTime > currentTime && !p.IsUsed, 0, 0, null, false, null);

                if (otpCodeListDb.Items.Count > 1)
                {
                    result = BuildAppActionResultError(result, "Xảy ra lỗi trong quá trình xử lí, có nhiều hơn 1 otp khả dụng. Vui lòng thử lại sau ít phút");
                    return result;
                }

                if (otpCodeListDb.Items.Count == 0)
                {
                    result = BuildAppActionResultError(result, "Mã otp đã nhập không khả dụng. Vui lòng thử lại");
                    return result;
                }
                var otpCodeDb = otpCodeListDb.Items[0];

                if (user == null)
                    result = BuildAppActionResultError(result, $" {loginRequest.PhoneNumber} này không tồn tại trong hệ thống");

                if (user.IsVerified == false)
                {
                    user.IsVerified = true;
                    user.VerifyCode = null;
                }
                if (otpCodeDb!.IsUsed == true)
                {
                    result = BuildAppActionResultError(result, $"Mã OTP đã được sử dụng");
                }
                if (otpCodeDb!.ExpiredTime < currentTime)
                {
                    result = BuildAppActionResultError(result, $"Mã OTP đã hết hạn");
                }
                if (otpCodeDb == null)
                {
                    result = BuildAppActionResultError(result, $"Mã OTP không tồn tại");
                }

                if (otpCodeDb!.Code != loginRequest.OTPCode) result = BuildAppActionResultError(result, "Đăng nhâp thất bại");
                if (!BuildAppActionResultIsError(result)) result = await LoginDefault(loginRequest.PhoneNumber, user);

                var tokenDto = result.Result as TokenDto;

                if (tokenDto != null)
                {
                    // Create Token object
                    var token = new Token
                    {
                        TokenId = Guid.NewGuid(),
                        DeviceIP = GetClientIpAddress(httpContext),
                        AccountId = user.Id,
                        CreateDateAccessToken = utility.GetCurrentDateTimeInTimeZone(),
                        CreateRefreshToken = utility.GetCurrentDateTimeInTimeZone(),
                        ExpiryTimeAccessToken = utility.GetCurrentDateTimeInTimeZone().AddDays(30),
                        ExpiryTimeRefreshToken = utility.GetCurrentDateTimeInTimeZone().AddDays(30),
                        AccessTokenValue = tokenDto.Token!,
                        RefreshTokenValue = tokenDto.RefreshToken!,
                        IsActive = true,    
                    };

                    otpCodeDb!.IsUsed = true;
                    await _accountRepository.Update(user);
                    await _otpRepository.Update(otpCodeDb);
                    await tokenRepository!.Insert(token);
                  
                }
                await _unitOfWork.SaveChangesAsync();
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
        public async Task<AppActionResult> VerifyLoginGoogle(string email, string verifyCode)
        {
            var result = new AppActionResult();
            try
            {
                var user = await _accountRepository.GetByExpression(u =>
                    u!.Email.ToLower() == email.ToLower() && u.IsDeleted == false);
                if (user == null)
                    result = BuildAppActionResultError(result, $"Email này không tồn tại");
                else if (user.IsVerified == false)
                    result = BuildAppActionResultError(result, "Tài khoản này chưa xác thực !");
                else if (user.VerifyCode != verifyCode)
                    result = BuildAppActionResultError(result, "Mã xác thực sai!");

                if (!BuildAppActionResultIsError(result))
                {
                    result = await LoginDefault(email, user);
                    user!.VerifyCode = null;
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> CreateAccount(SignUpRequestDto signUpRequest, bool isGoogle)
        {
            var result = new AppActionResult();
            try
            {
                if (await _accountRepository.GetByExpression(r => r!.PhoneNumber == signUpRequest.PhoneNumber) != null)
                    result = BuildAppActionResultError(result, "Số điện thoại đã tồn tại!");

                if (!BuildAppActionResultIsError(result))
                {
                    var emailService = Resolve<IEmailService>();
                    var smsService = Resolve<ISmsService>();
                    var random = new Random();
                    var verifyCode = string.Empty;
                    verifyCode = random.Next(100000, 999999).ToString();

                    var user = new Account
                    {
                        Email = signUpRequest.Email,
                        UserName = signUpRequest.Email,
                        FirstName = signUpRequest.FirstName,
                        LastName = signUpRequest.LastName,
                        PhoneNumber = signUpRequest.PhoneNumber,
                        Gender = signUpRequest.Gender,
                        VerifyCode = verifyCode,
                        LoyaltyPoint = 0,
                        IsVerified = isGoogle ? true : false,
                        IsManuallyCreated = true
                    };
                    var resultCreateUser = await _userManager.CreateAsync(user, signUpRequest.Password);
                    if (resultCreateUser.Succeeded)
                    {
                        result.Result = user;
                        if (!isGoogle)
                            emailService!.SendEmail(user.Email, SD.SubjectMail.VERIFY_ACCOUNT,
                                TemplateMappingHelper.GetTemplateOTPEmail(
                                    TemplateMappingHelper.ContentEmailType.VERIFICATION_CODE, verifyCode,
                                    user.LastName));
                    }
                    else
                    {
                        result = BuildAppActionResultError(result, $"Tạo tài khoản không thành công");
                    }

                    var resultCreateRole = await _userManager.AddToRoleAsync(user, "CUSTOMER");
                    if (!resultCreateRole.Succeeded) result = BuildAppActionResultError(result, $"Cấp quyền khách hàng không thành công");
                    bool customerAdded = await AddCustomerInformation(user);
                    if (!customerAdded)
                    {
                        result = BuildAppActionResultError(result, $"Tạo thông tin khách hàng không thành công");
                    }

                    var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                    var configurationDb = await configurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.EXPIRE_TIME_FOR_STORE_CREDIT), null);
                    if (configurationDb == null)
                    {
                        result = BuildAppActionResultError(result, $"Tạo ví không thành công");
                    }
                    //var expireTimeInDay = double.Parse(configurationDb.PreValue);
                    //var storeCreditRepository = Resolve<IGenericRepository<StoreCredit>>();
                    //var utility = Resolve<Utility>();
                    //var newStoreCreditDb = new StoreCredit
                    //{
                    //    StoreCreditId = Guid.NewGuid(),
                    //    Amount = 0,
                    //    ExpiredDate = utility.GetCurrentDateInTimeZone().AddDays(expireTimeInDay),
                    //    AccountId = user.Id
                    //};
                    //await storeCreditRepository.Insert(newStoreCreditDb);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        private async Task<bool> AddCustomerInformation(Account user)
        {
            bool isSuccessful = false;
            try
            {
                var customer = new Account
                {
                    CustomerId = Guid.NewGuid().ToString(),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Address = ""
                };
                var customerRepository = Resolve<IGenericRepository<Account>>();
                await customerRepository!.Insert(customer);
                await _unitOfWork.SaveChangesAsync();
                isSuccessful = true;
            }
            catch (Exception ex)
            {
                isSuccessful = false;
            }
            return isSuccessful;
        }

        public async Task<AppActionResult> UpdateAccountPhoneNumber(UpdatePhoneRequestDto accountRequest)
        {
            var result = new AppActionResult();
            try
            {
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var account =
                    await _accountRepository.GetByExpression(
                        a => a!.PhoneNumber == accountRequest.PhoneNumber!.ToLower());
                var otpCodeDb = await _otpRepository.GetByExpression(p => p.Code == accountRequest.OTPCode && p.Type == OTPType.ChangePhone);
                if (account == null)
                    result = BuildAppActionResultError(result, $"Tài khoản với số điện thoại {accountRequest.PhoneNumber} không tồn tại!");
                if (otpCodeDb!.IsUsed == true)
                {
                    result = BuildAppActionResultError(result, $"Mã OTP đã được sử dụng");
                }
                if (otpCodeDb!.ExpiredTime < currentTime)
                {
                    result = BuildAppActionResultError(result, $"Mã OTP đã hết hạn");
                }
                if (otpCodeDb == null)
                {
                    result = BuildAppActionResultError(result, $"Mã OTP không tồn tại");
                }
                if (!BuildAppActionResultIsError(result))
                {
                    account.PhoneNumber = accountRequest.PhoneNumber;
                    result.Result = await _accountRepository.Update(account);

                    otpCodeDb.IsUsed = true;
                    await _otpRepository.Update(otpCodeDb);
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> SendOTP(string phoneNumber, OTPType otp)
        {
            var result = new AppActionResult();
            try
            {
                var utility = Resolve<Utility>();
                var availableOtp = await _otpRepository.GetAllDataByExpression(o => o.Account.PhoneNumber.Equals(phoneNumber) && otp == o.Type && o.ExpiredTime > utility.GetCurrentDateTimeInTimeZone() && !o.IsUsed, 0, 0, null, false, null);
                if (availableOtp.Items.Count > 1)
                {
                    result = BuildAppActionResultError(result, "Xảy ra lỗi trong quá trình xử lí, có nhiều hơn 1 otp khả dụng. Vui lòng thử lại sau ít phút");
                    return result;
                }

                if (availableOtp.Items.Count == 1)
                {
                    result.Result = availableOtp.Items[0];
                    return result;
                }

                var user = await _accountRepository.GetByExpression(a =>
                  a!.PhoneNumber == phoneNumber && a.IsDeleted == false);
                if (user == null)
                {
                    if (otp != OTPType.Register && otp != OTPType.Login)
                    {
                        result = BuildAppActionResultError(result, "Tài khoản không tồn tại hoặc chưa được xác thực");
                    }
                }
                if (!BuildAppActionResultIsError(result))
                {
                    var smsService = Resolve<ISmsService>();
                    string code = "";

                    if ((otp == OTPType.Register || otp == OTPType.Login) && user == null)
                    {
                        var createAccountResult = await RegisterAccountByPhoneNumber(phoneNumber);
                        if (!createAccountResult.IsSuccess)
                        {
                            result = BuildAppActionResultError(result, $"Đăng tài khoản cho số điện thoại {phoneNumber} thất bại. Vui lòng thử lại");
                            return result;
                        }
                        user = (Account?)createAccountResult.Result;
                        if (user == null)
                        {
                            result = BuildAppActionResultError(result, $"Đăng tài khoản cho số điện thoại {phoneNumber} thất bại. Vui lòng thử lại");
                            return result;
                        }
                        code = user.VerifyCode;
                        //var response = await smsService!.SendMessage($"Mã xác thực của bạn là là: {code}", phoneNumber);
                    }
                    else
                    {
                        code = await GenerateVerifyCodeSms(phoneNumber, true);
                    }
                    var otpsDb = new OTP
                    {
                        OTPId = Guid.NewGuid(),
                        Type = otp,
                        AccountId = user.Id,
                        Code = code,
                        ExpiredTime = utility.GetCurrentDateTimeInTimeZone().AddMinutes(5),
                        IsUsed = false,
                    };
                    await _otpRepository.Insert(otpsDb);
                    await _unitOfWork.SaveChangesAsync();
                    otpsDb.Account = null;
                    result.Result = otpsDb;
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;

        }

        private async Task<AppActionResult> RegisterAccountByPhoneNumber(string phoneNumber)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var random = new Random();
                var verifyCode = string.Empty;
                verifyCode = random.Next(100000, 999999).ToString();
                int accountRandomNumber = random.Next(1000000, 9999999);
                var user = new Account
                {
                    Email = $"{SD.AccountDefaultInfomation.DEFAULT_EMAIL}{accountRandomNumber}{SD.DEFAULT_EMAIL_DOMAIN}",
                    UserName = $"{SD.AccountDefaultInfomation.DEFAULT_EMAIL}{accountRandomNumber}{SD.DEFAULT_EMAIL_DOMAIN}",
                    LastName = SD.AccountDefaultInfomation.DEFAULT_FIRSTNAME + " " + SD.AccountDefaultInfomation.DEFAULT_LASTNAME,
                    PhoneNumber = phoneNumber,
                    Gender = true,
                    VerifyCode = verifyCode,
                    IsVerified = false,
                    IsManuallyCreated = false
                };
                var resultCreateUser = await _userManager.CreateAsync(user, SD.DEFAULT_PASSWORD);
                if (resultCreateUser.Succeeded)
                {
                    result.Result = user;
                }
                else
                {
                    result = BuildAppActionResultError(result, $"Tạo tài khoản không thành công");
                }

                var resultCreateRole = await _userManager.AddToRoleAsync(user, "CUSTOMER");
                if (!resultCreateRole.Succeeded) result = BuildAppActionResultError(result, $"Cấp quyền khách hàng không thành công");
                bool customerAdded = await AddCustomerInformation(user);
                if (!customerAdded)
                {
                    result = BuildAppActionResultError(result, $"Tạo thông tin khách hàng không thành công");
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAccountByUserId(string id)
        {
            var result = new AppActionResult();
            try
            {
                var account = await _accountRepository.GetById(id);
                if (account == null) result = BuildAppActionResultError(result, $"Tài khoản với id {id} không tồn tại !");
                if (!BuildAppActionResultIsError(result)) result.Result = account;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> GetAllAccount(int pageIndex, int pageSize)
        {
            var result = new AppActionResult();
            var list = await _accountRepository.GetAllDataByExpression(null, pageIndex, pageSize, null, false, null);

            var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
            var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
            var listRole = await roleRepository!.GetAllDataByExpression(null, 1, 100, null, false, null);
            var listMap = _mapper.Map<List<AccountResponse>>(list.Items);
            foreach (var item in listMap)
            {
                var userRole = new List<IdentityRole>();
                var role = await userRoleRepository!.GetAllDataByExpression(a => a.UserId == item.Id, 1, 100, null, false, null);
                foreach (var itemRole in role.Items!)
                {
                    var roleUser = listRole.Items!.ToList().FirstOrDefault(a => a.Id == itemRole.RoleId);
                    if (roleUser != null) userRole.Add(roleUser);
                }

                item.Roles = userRole;
                var roleNameList = userRole.DistinctBy(i => i.Id).Select(i => i.Name).ToList();

                if (roleNameList.Contains("MANAGER"))
                {
                    item.MainRole = "MANAGER";
                }
                else if (roleNameList.Contains("STAFF"))
                {
                    item.MainRole = "STAFF";
                }
                else if (roleNameList.Contains("CHEF"))
                {
                    item.MainRole = "CHEF";
                }
                else if (roleNameList.Count > 1)
                {
                    item.MainRole = roleNameList.FirstOrDefault(n => !n.Equals("CUSTOMER"));
                }
                else
                {
                    item.MainRole = "CUSTOMER";
                }
            }

            result.Result =
                new PagedResult<AccountResponse>
                { Items = listMap, TotalPages = list.TotalPages };
            return result;
        }

        public async Task<AppActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            var result = new AppActionResult();

            try
            {
                var utility = Resolve<Utility>();
                var tokenService = Resolve<ITokenService>();
                var otpCodeListDb = await _otpRepository.GetAllDataByExpression(p => p.Code == changePasswordDto.OTPCode && p.Type == OTPType.ChangePassword && p.ExpiredTime > utility.GetCurrentDateTimeInTimeZone(), 0, 0, null, false, null);
                if (otpCodeListDb.Items.Count == 0)
                {
                    result = BuildAppActionResultError(result, "Không tìm thấy mã xác nhận");
                    return result;
                }
                else if (otpCodeListDb.Items.Count > 1)
                {
                    result = BuildAppActionResultError(result, "Có nhiều hơn 1 mã xác nhận còn hiệu lực. Vui lòng thử lại sau 5 phút");
                    return result;
                }
                var otpCodeDb = otpCodeListDb.Items.FirstOrDefault();
                if (await _accountRepository.GetByExpression(c =>
                        c!.PhoneNumber == changePasswordDto.PhoneNumber && c.IsDeleted == false) == null)
                    result = BuildAppActionResultError(result,
                        $"Tài khoản có số điện thoại {changePasswordDto.PhoneNumber} không tồn tại!");
                if (otpCodeDb!.IsUsed == true)
                {
                    result = BuildAppActionResultError(result, $"Mã OTP đã được sử dụng");
                }
                if (otpCodeDb!.ExpiredTime < utility.GetCurrentDateTimeInTimeZone())
                {
                    result = BuildAppActionResultError(result, $"Mã OTP đã hết hạn");
                }
                if (otpCodeDb == null)
                {
                    result = BuildAppActionResultError(result, $"Mã OTP không tồn tại");
                }
                if (!BuildAppActionResultIsError(result))
                {
                    var user = await _accountRepository.GetByExpression(c =>
                        c!.PhoneNumber == changePasswordDto.PhoneNumber && c.IsDeleted == false);
                    var changePassword = await _userManager.ChangePasswordAsync(user!, changePasswordDto.OldPassword,
                        changePasswordDto.NewPassword);
                    if (!changePassword.Succeeded)
                        result = BuildAppActionResultError(result, "Thay đổi mật khẩu thất bại");

                    var userTokenDb = await tokenService!.InvalidateTokensForUser(user.Id);
                    otpCodeDb.IsUsed = true;
                    await _otpRepository.Update(otpCodeDb);
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> GetNewToken(string refreshToken, string userId)
        {
            var result = new AppActionResult();

            try
            {
                var user = await _accountRepository.GetById(userId);
                if (user == null)
                    result = BuildAppActionResultError(result, "Tài khoản không tồn tại");
                else if (user.RefreshToken != refreshToken)
                    result = BuildAppActionResultError(result, "Mã làm mới không chính xác");

                if (!BuildAppActionResultIsError(result))
                {
                    var jwtService = Resolve<IJwtService>();
                    result.Result = await jwtService!.GetNewToken(refreshToken, userId);
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> ForgotPassword(ForgotPasswordDto dto)
        {
            var result = new AppActionResult();

            try
            {
                var utility = Resolve<Utility>();
                var user = _userManager.Users.FirstOrDefault(a =>
                    a!.PhoneNumber == dto.PhoneNumber && a.IsDeleted == false && a.IsVerified == true);
                if (user == null)
                {
                    result = BuildAppActionResultError(result, "Tài khoản không tồn tại hoặc chưa được xác thực!");
                    return result;
                }
                var otpCodeListDb = await _otpRepository.GetAllDataByExpression(p => p!.AccountId == user!.Id && p.Type == OTPType.ForgotPassword && p.ExpiredTime > utility.GetCurrentDateTimeInTimeZone(), 0, 0, null, false, null);
                if (otpCodeListDb.Items.Count == 0)
                {
                    result = BuildAppActionResultError(result, "Không tìm thấy mã xác nhận");
                    return result;
                }
                else if (otpCodeListDb.Items.Count > 1)
                {
                    result = BuildAppActionResultError(result, "Có nhiều hơn 1 mã xác nhận còn hiệu lực. Vui lòng thử lại sau 5 phút");
                    return result;
                }
                var otpCodeDb = otpCodeListDb.Items.FirstOrDefault();
                DateTime currentTime = utility.GetCurrentDateTimeInTimeZone();
                if (otpCodeDb!.Code != dto.RecoveryCode)
                    result = BuildAppActionResultError(result, "Mã xác nhận sai");
                else if (otpCodeDb.IsUsed == true)
                    result = BuildAppActionResultError(result, "Mã xác nhận đã sử dụng");
                else if (otpCodeDb.ExpiredTime < currentTime)
                    result = BuildAppActionResultError(result, "Mã xác nhận đã hết hạn sử dụng");

                if (!BuildAppActionResultIsError(result))
                {
                    await _userManager.RemovePasswordAsync(user!);
                    var addPassword = await _userManager.AddPasswordAsync(user!, dto.NewPassword);
                    if (addPassword.Succeeded)
                        user!.VerifyCode = null;
                    else
                        result = BuildAppActionResultError(result, "Thay đổi mật khẩu thất bại. Vui lòng thử lại");

                    otpCodeDb.IsUsed = true;
                    await _otpRepository.Update(otpCodeDb);
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> ActiveAccount(string email, string verifyCode)
        {
            var result = new AppActionResult();
            try
            {
                var user = await _accountRepository.GetByExpression(a =>
                    a!.Email == email && a.IsDeleted == false && a.IsVerified == false);
                if (user == null)
                    result = BuildAppActionResultError(result, "Tài khoản không tồn tại ");
                else if (user.VerifyCode != verifyCode)
                    result = BuildAppActionResultError(result, "Mã xác thực sai");

                if (!BuildAppActionResultIsError(result))
                {
                    user!.IsVerified = true;
                    user.VerifyCode = null;
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> SendEmailForActiveCode(string email)
        {
            var result = new AppActionResult();

            try
            {
                var user = await _accountRepository.GetByExpression(a =>
                    a!.Email == email && a.IsDeleted == false && a.IsVerified == false);
                if (user == null) result = BuildAppActionResultError(result, "Tài khoản không tồn tại hoặc chưa xác thực");

                if (!BuildAppActionResultIsError(result))
                {
                    var emailService = Resolve<IEmailService>();
                    var code = await GenerateVerifyCode(user!.Email, false);
                    emailService!.SendEmail(email, SD.SubjectMail.VERIFY_ACCOUNT,
                        TemplateMappingHelper.GetTemplateOTPEmail(TemplateMappingHelper.ContentEmailType.VERIFICATION_CODE,
                            code, user.LastName!));
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<string> GenerateVerifyCode(string email, bool isForForgettingPassword)
        {
            var code = string.Empty;

            var user = await _accountRepository.GetByExpression(a =>
                a!.Email == email && a.IsDeleted == false && a.IsVerified == isForForgettingPassword);

            if (user != null)
            {
                code = Guid.NewGuid().ToString("N").Substring(0, 6);
                user.VerifyCode = code;
            }

            await _unitOfWork.SaveChangesAsync();

            return code;
        }

        public async Task<string> GenerateVerifyCodeSms(string phoneNumber, bool isForForgettingPassword)
        {
            var code = string.Empty;

            var user = await _accountRepository.GetByExpression(a =>
                a!.PhoneNumber == phoneNumber && a.IsDeleted == false);

            if (user != null)
            {
                var random = new Random();
                code = random.Next(100000, 999999).ToString();
                user.VerifyCode = code;
                var smsService = Resolve<ISmsService>();
                //var response = await smsService!.SendMessage($"Mã xác thực tại nhà hàng TP là: {code}",
                //    phoneNumber);
            }

            await _unitOfWork.SaveChangesAsync();

            return code;
        }

        public async Task<AppActionResult> GoogleCallBack(string accessTokenFromGoogle)
        {
            var result = new AppActionResult();
            try
            {
                var existingFirebaseApp = FirebaseApp.DefaultInstance;
                if (existingFirebaseApp == null)
                {
                    var config = Resolve<FirebaseAdminSDK>();
                    var credential = GoogleCredential.FromJson(JsonConvert.SerializeObject(new
                    {
                        type = config!.Type,
                        project_id = config.Project_id,
                        private_key_id = config.Private_key_id,
                        private_key = config.Private_key,
                        client_email = config.Client_email,
                        client_id = config.Client_id,
                        auth_uri = config.Auth_uri,
                        token_uri = config.Token_uri,
                        auth_provider_x509_cert_url = config.Auth_provider_x509_cert_url,
                        client_x509_cert_url = config.Client_x509_cert_url
                    }));
                    var firebaseApp = FirebaseApp.Create(new AppOptions
                    {
                        Credential = credential
                    });
                }

                var verifiedToken = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance
                    .VerifyIdTokenAsync(accessTokenFromGoogle);
                var emailClaim = verifiedToken.Claims.FirstOrDefault(c => c.Key == "email");
                var nameClaim = verifiedToken.Claims.FirstOrDefault(c => c.Key == "name");
                var name = nameClaim.Value.ToString();
                var userEmail = emailClaim.Value.ToString();

                if (userEmail != null)
                {
                    var user = await _accountRepository.GetByExpression(a => a!.Email == userEmail && a.IsDeleted == false);
                    if (user == null)
                    {
                        var resultCreate =
                            await CreateAccount(
                                new SignUpRequestDto
                                {
                                    Email = userEmail,
                                    FirstName = name!,
                                    Gender = true,
                                    LastName = string.Empty,
                                    Password = "Google123@",
                                    PhoneNumber = string.Empty
                                }, true);
                        if (resultCreate.IsSuccess)
                        {
                            var account = (Account)resultCreate.Result!;
                            result = await LoginDefault(userEmail, account);
                        }
                    }

                    result = await LoginDefault(userEmail, user);
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        private async Task<AppActionResult> LoginDefault(string phoneNumber, Account? user)
        {
            var result = new AppActionResult();

            var jwtService = Resolve<IJwtService>();
            var utility = Resolve<Utility>();
            var token = await jwtService!.GenerateAccessToken(new LoginRequestDto { PhoneNumber = phoneNumber });

            if (user!.RefreshToken == null)
            {
                user.RefreshToken = jwtService.GenerateRefreshToken();
                user.RefreshTokenExpiryTime = utility!.GetCurrentDateInTimeZone().AddDays(1);
            }

            if (user.RefreshTokenExpiryTime <= utility!.GetCurrentDateInTimeZone())
            {
                user.RefreshTokenExpiryTime = utility.GetCurrentDateInTimeZone().AddDays(1);
                user.RefreshToken = jwtService.GenerateRefreshToken();
            }

            _tokenDto.Token = token;
            _tokenDto.RefreshToken = user.RefreshToken;
            _tokenDto.Account = _mapper.Map<AccountResponse>(user);
            
            var roleList = new List<string>();
            var roleListDb = await _userRoleRepository.GetAllDataByExpression(r => r.UserId.Equals(user.Id), 0, 0, null, false, null);
            if (roleListDb.Items == null || roleListDb.Items.Count == 0)
            {
                roleList = new List<string>();
            }
            roleList = roleListDb.Items!.Select(r => r.RoleId).ToList();

            var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
            var roleNameList = new List<string>();
            var roleNameDb = await roleRepository!.GetAllDataByExpression(p => roleList.Contains(p.Id), 0, 0, null, false, null);
            if (roleNameDb.Items!.Count == 0)
            {
                roleNameList = new List<string>();
            }
            roleNameList = roleNameDb.Items!.DistinctBy(i => i.Id).Select(i => i.Name).ToList();
            if (roleNameList.Contains("MANAGER"))
            {
                _tokenDto.MainRole = "MANAGER";
            }
            else if (roleNameList.Contains("STAFF"))
            {
                _tokenDto.MainRole = "STAFF";
            }
            else if (roleNameList.Contains("CHEF"))
            {
                _tokenDto.MainRole = "CHEF";
            }
            else if (roleNameList.Count > 1)
            {
                _tokenDto.MainRole = roleNameList.FirstOrDefault(n => !n.Equals("CUSTOMER"));
            }
            else
            {
                _tokenDto.MainRole = "CUSTOMER";
            }
            _tokenDto.Account.Roles = roleNameDb.Items;
            _tokenDto.Account.MainRole = _tokenDto.MainRole;
            result.Result = _tokenDto;
            await _unitOfWork.SaveChangesAsync();

            return result;
        }

        public async Task<List<string>> GetRoleListByAccountId(string userId)
        {
            var roleListDb = await _userRoleRepository.GetAllDataByExpression(r => r.UserId.Equals(userId), 0, 0, null, false, null);
            if (roleListDb.Items == null || roleListDb.Items.Count == 0)
            {
                return new List<string>();
            }
            return roleListDb.Items.Select(r => r.RoleId).ToList();
        }

        public async Task<AppActionResult> AssignRoleForUserId(string userId, IList<string> roleId)
        {
            var result = new AppActionResult();
            try
            {
                var user = await _accountRepository.GetById(userId);
                var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
                var identityRoleRepository = Resolve<IGenericRepository<IdentityRole>>();
                foreach (var role in roleId)
                    if (await identityRoleRepository!.GetById(role) == null)
                        result = BuildAppActionResultError(result, $"Vai trò với id {role} không tồn tại");

                if (!BuildAppActionResultIsError(result))
                    foreach (var role in roleId)
                    {
                        var roleDb = await identityRoleRepository!.GetById(role);
                        var resultCreateRole = await _userManager.AddToRoleAsync(user, roleDb.NormalizedName);
                        if (!resultCreateRole.Succeeded)
                            result = BuildAppActionResultError(result, $"Cấp quyền với vai trò {role} không thành công");
                    }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> RemoveRoleForUserId(string userId, IList<string> roleId)
        {
            var result = new AppActionResult();

            try
            {
                var user = await _accountRepository.GetById(userId);
                var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
                var identityRoleRepository = Resolve<IGenericRepository<IdentityRole>>();
                if (user == null)
                    result = BuildAppActionResultError(result, $"Người dùng với {userId} không tồn tại");
                foreach (var role in roleId)
                    if (await identityRoleRepository.GetById(role) == null)
                        result = BuildAppActionResultError(result, $"Vai trò với {role} không tồn tại");

                if (!BuildAppActionResultIsError(result))
                    foreach (var role in roleId)
                    {
                        var roleDb = await identityRoleRepository!.GetById(role);
                        var resultCreateRole = await _userManager.RemoveFromRoleAsync(user!, roleDb.NormalizedName);
                        if (!resultCreateRole.Succeeded)
                            result = BuildAppActionResultError(result, $"Xóa quyền {role} thất bại");
                    }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> GetAccountsByRoleName(string roleName, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();

            try
            {
                var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
                var roleDb = await roleRepository!.GetByExpression(r => r.NormalizedName.Equals(roleName.ToLower()));
                if (roleDb != null)
                {
                    var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
                    var userRoleDb = await userRoleRepository!.GetAllDataByExpression(u => u.RoleId == roleDb.Id, 0, 0, null, false, null);
                    if (userRoleDb.Items != null && userRoleDb.Items.Count > 0)
                    {
                        var accountIds = userRoleDb.Items.Select(u => u.UserId).Distinct().ToList();
                        var accountDb = await _accountRepository.GetAllDataByExpression(a => accountIds.Contains(a.Id), pageNumber, pageSize, null, false, null);
                        result.Result = accountDb;
                    }
                }
                else
                {
                    result = BuildAppActionResultError(result, $"Không tìm thấy vai trò {roleName}");
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> GetAccountsByRoleId(Guid Id, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();

            try
            {
                var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
                var roleDb = await roleRepository!.GetById(Id);
                if (roleDb != null)
                {
                    var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
                    var userRoleDb = await userRoleRepository!.GetAllDataByExpression(u => u.RoleId == roleDb.Id, 0, 0, null, false, null);
                    if (userRoleDb.Items != null && userRoleDb.Items.Count > 0)
                    {
                        var accountIds = userRoleDb.Items.Select(u => u.UserId).Distinct().ToList();
                        var accountDb = await _accountRepository.GetAllDataByExpression(a => accountIds.Contains(a.Id), pageNumber, pageSize, null, false, null);
                        result.Result = accountDb;
                    }
                }
                else
                {
                    result = BuildAppActionResultError(result, $"Không tìm thấy vai trò với id {Id}");
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> GenerateOTP(string phoneNumber)
        {
            AppActionResult result = new AppActionResult();
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();
            var smsService = Resolve<ISmsService>();
            //var response = await smsService!.SendMessage($"Mã xác thực tại nhà hàng TP là: {code}",
            //    phoneNumber);

            result.Result = code;

            //if (response.IsSuccess)
            //{
            //    result.Result = code;
            //}
            //else
            //{
            //    foreach (var error in response.Messages)
            //    {
            //        result.Messages.Add(error);
            //    }
            //}
            return result;
        }

        public async Task<AppActionResult> VerifyNumberAccount(string phoneNumber, string optCode)
        {
            var result = new AppActionResult();
            try
            {
                var user = await _accountRepository.GetByExpression(p => p.PhoneNumber == phoneNumber && p.IsDeleted == false);
                if (user == null)
                {
                    result = BuildAppActionResultError(result, $"Số điện thoại này không tồn tại!");
                }

                var optUser = await _otpRepository.GetByExpression(p => p!.Code == optCode && p.Type == OTPType.ConfirmPhone, p => p.Account!);
                if (optUser == null)
                {
                    result = BuildAppActionResultError(result, $"Mã Otp không tồn tại!");
                }
                else if (optUser.IsUsed == true)
                {
                    result = BuildAppActionResultError(result, $"Mã Otp này đã được sử dụng!");
                }
                else if (optUser.Code != optCode && user.VerifyCode != optCode)
                {
                    result = BuildAppActionResultError(result, $"Mã Otp không đúng!");
                }

                if (!BuildAppActionResultIsError(result))
                {
                    result = await LoginDefault(phoneNumber, user);
                    user!.VerifyCode = null;
                    user.IsVerified = true;


                    optUser.IsUsed = true;
                    await _otpRepository.Update(optUser);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> UpdateAccountInformation(UpdateAccountInformationRequest request)
        {
            var result = new AppActionResult();
            try
            {
                var customerInforRepository = Resolve<IGenericRepository<Account>>();
                var account =
                   await _accountRepository.GetByExpression(p => p.PhoneNumber == request.PhoneNumber, null);
                if (account == null)
                {
                    result = BuildAppActionResultError(result, $"Tài khoản với số điện thoại {request.PhoneNumber} không tồn tại!");
                }
                if (!BuildAppActionResultIsError(result))
                {
                    account.FirstName = request.FirstName + request.LastName;
                    account.LastName = request.FirstName + request.LastName;
                    account.PhoneNumber = request.PhoneNumber;
                    if (!string.IsNullOrEmpty(request.Avatar))
                    {
                        account.Avatar = request.Avatar;
                    }
                    result.Result = await _accountRepository.Update(account);
                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> AddNewCustomerInfo(CustomerInforRequest customerInforRequest)
        {
            var result = new AppActionResult();
            try
            {
                if (!string.IsNullOrEmpty(customerInforRequest.AccountId))
                {
                    var accountDb = await _accountRepository.GetByExpression(p => p.Id == customerInforRequest.AccountId);
                    if (accountDb == null)
                    {
                        result = BuildAppActionResultError(result, $"Tài khoản với số điện thoại {customerInforRequest.AccountId} không tồn tại!");
                    }
                }

                var customerInfoDb = await _accountRepository.GetAllDataByExpression(c => c.PhoneNumber.Equals(customerInforRequest.PhoneNumber), 0, 0, null, false, null);
                if (customerInfoDb.Items.Count > 1)
                {
                    result = BuildAppActionResultError(result, $"Có nhiều hơn 1 thông tin người dùng với sđt {customerInforRequest.PhoneNumber} đã tồn tại");
                    return result;
                }

                if (customerInfoDb.Items.Count == 1)
                {
                    result = BuildAppActionResultError(result, $"Thông tin người dùng với sđt {customerInforRequest.PhoneNumber} đã tồn tại");
                    result.Result = customerInfoDb.Items.SingleOrDefault();
                    return result;
                }

                var customerInfo = new Account
                {
                    CustomerId = Guid.NewGuid().ToString(),
                    FirstName = customerInforRequest.FirstName,
                    LastName = customerInforRequest.LastName,
                    PhoneNumber = customerInforRequest.PhoneNumber,
                    Address = customerInforRequest.Address,
                    DOB = customerInforRequest.DOB,
                    Gender = customerInforRequest.Gender,
                    IsVerified = false
                };

                await _accountRepository.Insert(customerInfo);
                await _unitOfWork.SaveChangesAsync();
                await SendCustomerInfoOTP(customerInfo.PhoneNumber, OTPType.Register);
                result.Result = customerInfo;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GenerateCustomerInfoOTP(Account customerDb, OTPType otpType)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                //Check Customermust be unverified
                if (customerDb == null)
                {
                    result = BuildAppActionResultError(result, $"Không tìm thấy thông tin người dùng với id {customerDb.CustomerId}");
                    return result;
                }

                if (customerDb.IsVerified && otpType == OTPType.Register)
                {
                    result = BuildAppActionResultError(result, $"Thông tin người dùng đã được xác thực");
                    return result;
                }
                Random random = new Random();
                string code = random.Next(100000, 999999).ToString();
                var utility = Resolve<Utility>();

                var otpsDb = new OTP
                {
                    OTPId = Guid.NewGuid(),
                    Type = otpType,
                    AccountId = customerDb.CustomerId,
                    Code = code,
                    ExpiredTime = utility.GetCurrentDateTimeInTimeZone().AddMinutes(5),
                    IsUsed = false,
                };
                await _otpRepository.Insert(otpsDb);
                await _unitOfWork.SaveChangesAsync();
                result.Result = otpsDb.Code;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> SendCustomerInfoOTP(string phoneNumber, OTPType otpType)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var customerListDb = await _accountRepository.GetAllDataByExpression(c => c.PhoneNumber.Equals(phoneNumber), 0, 0, null, false, null);

                if (customerListDb.Items.Count > 1)
                {
                    result = BuildAppActionResultError(result, $"Xảy ra lỗi khi tìm thông tin người dùng với sđt {phoneNumber}");
                    return result;
                }

                if (customerListDb.Items.Count == 0)
                {
                    result = BuildAppActionResultError(result, $"Không tìm thấy thông tin người dùng với sđt {phoneNumber}");
                    return result;
                }
                var customerDb = customerListDb.Items[0];
                if (otpType == OTPType.Register)
                {
                    if (customerDb.IsVerified)
                    {
                        result = BuildAppActionResultError(result, $"Thông tin người dùng đã được xác thực");
                        return result;
                    }
                }
                else
                {
                    if (!customerDb.IsVerified)
                    {
                        result = BuildAppActionResultError(result, $"Thông tin người dùng đã chưa được xác thực");
                        return result;
                    }
                }

                var otpRepository = Resolve<IGenericRepository<OTP>>();
                var utility = Resolve<Utility>();
                var availableOTPDb = await otpRepository.GetAllDataByExpression(o => o.Account.PhoneNumber == phoneNumber && !o.IsUsed && o.ExpiredTime > utility.GetCurrentDateTimeInTimeZone() && o.Type == otpType, 0, 0, null, false, null);
                if (availableOTPDb.Items.Count > 1)
                {
                    result = BuildAppActionResultError(result, $"Xảy ra lỗi vì có nhiều hơn mã OTP hữu dụng tồn tại trong hệ thống. Vui lòng thử lại sau");
                    return result;
                }

                if (availableOTPDb.Items.Count == 1)
                {
                    result.Result = availableOTPDb.Items[0].Code;
                    return result;
                }

                var otp = await GenerateCustomerInfoOTP(customerDb, otpType);
                if (!otp.IsSuccess)
                {
                    return BuildAppActionResultError(result, $"Tạo OTP thất bại. Vui lòng thử lại");
                }

                customerDb.VerifyCode = otp.Result.ToString();
                await _accountRepository.Update(customerDb);
                await _unitOfWork.SaveChangesAsync();
                var smsService = Resolve<ISmsService>();
                var response = await smsService!.SendMessage($"Mã xác thực tại nhà hàng TP là: {customerDb.VerifyCode}",
                    customerDb.PhoneNumber);
                result.Result = customerDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> UpdateCustomerInfo(UpdateCustomerInforRequest customerInforRequest)
        {
            var result = new AppActionResult();
            try
            {
                //var accountDb = await _accountRepository.GetByExpression(p => p.Id == customerInforRequest.AccountId);
                //if (accountDb == null)
                //{
                //    result = BuildAppActionResultError(result, $"Tài khoản với số điện thoại {customerInforRequest.AccountId} không tồn tại!");
                //}
                var updateCustomerInfoList = await _accountRepository.GetAllDataByExpression(p => p.CustomerId == customerInforRequest.CustomerId.ToString() && p.PhoneNumber.Equals(customerInforRequest.PhoneNumber), 0, 0, null, false, null);

                if (updateCustomerInfoList.Items!.Count == 0)
                {
                    return BuildAppActionResultError(result, $"Thông tin khách hàng với id {customerInforRequest.CustomerId} không tồn tại!");
                }

                if (updateCustomerInfoList.Items.Count > 1)
                {
                    return BuildAppActionResultError(result, $"Xảy ra lỗi khi tìm kiếm thông tin khách hàng! Vui lòng kiểm tra lại thông tin");
                }

                var updateCustomerInfo = updateCustomerInfoList.Items[0];
                var customerInfoAddressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
                if (customerInforRequest.AddressId.HasValue)
                {
                    var customerInfoAddressDb = await customerInfoAddressRepository!.GetById(customerInforRequest.AddressId.Value);
                    if (customerInfoAddressDb == null)
                    {
                        return BuildAppActionResultError(result, $"Thông tin địa chỉ khách hàng với id {customerInforRequest.AddressId.Value} không tồn tại!");
                    }
                    if (!customerInfoAddressDb.IsCurrentUsed)
                    {
                        updateCustomerInfo.Address = customerInfoAddressDb.CustomerInfoAddressName;
                        customerInfoAddressDb.IsCurrentUsed = true;
                        var currentCustomerInfoAddressListDb = await customerInfoAddressRepository.GetAllDataByExpression(c => c.AccountId == updateCustomerInfo.CustomerId
                                                                                                                && c.IsCurrentUsed, 0, 0, null, false, null);
                        if (currentCustomerInfoAddressListDb.Items.Count > 1)
                        {
                            return BuildAppActionResultError(result, "Xảy ra lỗi khi cập nhật thông tin khách hàng!");
                        }

                        if (currentCustomerInfoAddressListDb.Items.Count == 0)
                        {
                            return BuildAppActionResultError(result, "Xảy ra lỗi khi cập nhật thông tin khách hàng vì không tìm được địa chỉ đang sử dụng!");
                        }

                        var currentCustomerInfoAddressDb = currentCustomerInfoAddressListDb.Items[0];
                        currentCustomerInfoAddressDb.IsCurrentUsed = false;
                        await customerInfoAddressRepository.Update(customerInfoAddressDb);
                        await customerInfoAddressRepository.Update(currentCustomerInfoAddressDb);
                    }
                }
                else
                {
                    await customerInfoAddressRepository!.Insert(new CustomerInfoAddress
                    {
                        CustomerInfoAddressId = Guid.NewGuid(),
                        CustomerInfoAddressName = customerInforRequest.Address!,
                        AccountId = updateCustomerInfo.CustomerId,
                        IsCurrentUsed = true
                    });

                    var currentCustomerInfoAddressListDb = await customerInfoAddressRepository.GetAllDataByExpression(c => c.AccountId == updateCustomerInfo.CustomerId
                                                                                                                && c.IsCurrentUsed, 0, 0, null, false, null);
                    if (currentCustomerInfoAddressListDb.Items.Count > 1)
                    {
                        return BuildAppActionResultError(result, "Xảy ra lỗi khi cập nhật thông tin khách hàng!");
                    }

                    if (currentCustomerInfoAddressListDb.Items.Count == 1)
                    {
                        var currentCustomerInfoAddressDb = currentCustomerInfoAddressListDb.Items[0];
                        currentCustomerInfoAddressDb.IsCurrentUsed = false;
                        await customerInfoAddressRepository.Update(currentCustomerInfoAddressDb);
                    }
                    updateCustomerInfo.Address = customerInforRequest.Address;
                }
                updateCustomerInfo.DOB = customerInforRequest.DOB;
                updateCustomerInfo.FirstName = customerInforRequest.FirstName;
                updateCustomerInfo.LastName = customerInforRequest.LastName;


                await _accountRepository.Update(updateCustomerInfo);
                await _unitOfWork.SaveChangesAsync();
                result.Result = updateCustomerInfo;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAllCustomerInfoByAccountId(string accountId, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            try
            {
                CustomerInfoResponse customerInfoResponse = new CustomerInfoResponse();
                var accountDb = await _accountRepository.GetByExpression(p => p.Id == accountId);
                if (accountDb == null)
                {
                    result = BuildAppActionResultError(result, $"Tài khoản với số điện thoại {accountId} không tồn tại!");
                }
                customerInfoResponse.Account = accountDb;
                result.Result = customerInfoResponse;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetCustomerInfo(Guid customerId)
        {
            var result = new AppActionResult();
            try
            {
                var customerInfoDb = await _accountRepository.GetByExpression(p => p.CustomerId == customerId.ToString());
                if (customerInfoDb == null)
                {
                    result = BuildAppActionResultError(result, $"Thông tin và địa chỉ của khách với id {customerId} không tồn tại");
                }
                result.Result = customerInfoDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> DeleteCustomerInfo(Guid customerId)
        {
            var result = new AppActionResult();
            try
            {
                var customerInfoDb = await _accountRepository.GetByExpression(p => p.CustomerId == customerId.ToString());
                if (customerInfoDb == null)
                {
                    result = BuildAppActionResultError(result, $"Thông tin và địa chỉ của khách với id {customerId} không tồn tại");
                }
                await _accountRepository.DeleteById(customerId);
                await _unitOfWork.SaveChangesAsync();
                result.IsSuccess = true;
                result.Messages.Add("Xóa thông tin địa chỉ khách hàng thành công");
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> VerifyForReservation(string phoneNumber, string code)
        {
            var result = new AppActionResult();
            try
            {
                var user = await _accountRepository.GetByExpression(p => p.PhoneNumber == phoneNumber && p.IsDeleted == false);
                if (user == null)
                {
                    result = BuildAppActionResultError(result, $"Số điện thoại này không tồn tại!");
                }

                var optUser = await _otpRepository.GetByExpression(p => p!.Code == code && p.Type == OTPType.VerifyForReservation, p => p.Account!);
                if (optUser == null)
                {
                    result = BuildAppActionResultError(result, $"Mã Otp không tồn tại!");
                }
                else if (optUser.IsUsed == true)
                {
                    result = BuildAppActionResultError(result, $"Mã Otp này đã được sử dụng!");
                }
                else if (optUser.Code != code && user.VerifyCode != code)
                {
                    result = BuildAppActionResultError(result, $"Mã Otp không đúng!");
                }

                if (!BuildAppActionResultIsError(result))
                {
                    user!.VerifyCode = null;
                    optUser.IsUsed = true;
                    await _otpRepository.Update(optUser);
                    await _accountRepository.Update(user);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> VerifyCustomerInfoOTP(string phoneNumber, string code, OTPType otpType)
        {
            var result = new AppActionResult();
            try
            {
                var user = await _accountRepository.GetByExpression(p => p.PhoneNumber == phoneNumber, null);
                if (user == null)
                {
                    result = BuildAppActionResultError(result, $"Số điện thoại này không tồn tại!");
                }

                var utility = Resolve<Utility>();

                var optUser = await _otpRepository.GetByExpression(p => p!.Code == code && p.Type == otpType && !p.IsUsed && p.ExpiredTime > utility.GetCurrentDateTimeInTimeZone(), p => p.Account!);
                if (optUser == null)
                {
                    result = BuildAppActionResultError(result, $"Mã Otp không tồn tại!");
                }
                else if (optUser.IsUsed == true)
                {
                    result = BuildAppActionResultError(result, $"Mã Otp này đã được sử dụng!");
                }
                else if (optUser.Type != OTPType.Register && !user.IsVerified)
                {
                    result = BuildAppActionResultError(result, $"Thông tin người dùng chưa được xác thực");
                }
                else if (optUser.Code != code && user.VerifyCode != code)
                {
                    result = BuildAppActionResultError(result, $"Mã Otp không đúng!");
                }

                if (!BuildAppActionResultIsError(result))
                {
                    user!.VerifyCode = null;
                    if (otpType == OTPType.Register)
                    {
                        user.IsVerified = true;
                    }
                    await _accountRepository.Update(user);

                    optUser.IsUsed = true;
                    await _otpRepository.Update(optUser);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetCustomerInfoByPhoneNumber(string phoneNumber)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var customerInfoListDb = await _accountRepository.GetAllDataByExpression(c => c.PhoneNumber.Equals(phoneNumber), 0, 0, null, false, null);
                if (customerInfoListDb.Items.Count > 1)
                {
                    return BuildAppActionResultError(result, $"Có nhiều hơn 1 thông tin người dùng với số điện thoại {phoneNumber}");
                }

                if (customerInfoListDb.Items.Count == 0)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy thông tin người dùng với số điện thoại {phoneNumber}");
                }

                var customerInfo = customerInfoListDb.Items[0];

                var customerInfoAddressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
                var customerInfoAddressDb = await customerInfoAddressRepository.GetAllDataByExpression(c => c.AccountId == customerInfo.CustomerId, 0, 0, null, false, null);

                var data = new CustomerInfoAddressResponse();
                data.CustomerInfo = customerInfo;
                data.CustomerAddresses = customerInfoAddressDb.Items;

                result.Result = data;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task DeleteOverdueOTP()
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var utility = Resolve<Utility>();
                var otpRepository = Resolve<IGenericRepository<OTP>>();
                var currentTime = utility!.GetCurrentDateTimeInTimeZone();
                var otpDb = await otpRepository!.GetAllDataByExpression(p => p.ExpiredTime < currentTime, 0, 0, null, false, null);
                if (otpDb!.Items!.Count > 0 && otpDb.Items != null)
                {
                    await otpRepository.DeleteRange(otpDb.Items);
                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
            Task.CompletedTask.Wait();
        }
    }
}
