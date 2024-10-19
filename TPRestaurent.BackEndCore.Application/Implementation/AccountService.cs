using AutoMapper;
using Castle.DynamicProxy.Generators;
using Firebase.Auth;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Hangfire.Common;
using Humanizer;
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
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
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
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using static System.Net.WebRequestMethods;
using static TPRestaurent.BackEndCore.Common.DTO.Response.EstimatedDeliveryTimeDto;
using static TPRestaurent.BackEndCore.Common.DTO.Response.MapInfo;
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
        private readonly IMapService _mapService;
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
            IGenericRepository<OTP> otpRepository,
            IMapService mapService
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
            _mapService = mapService;
        }

        public async Task<AppActionResult> Login(LoginRequestDto loginRequest, HttpContext httpContext)
        {
            var result = new AppActionResult();
            try
            {
                var utility = Resolve<Utility>();
                var tokenRepository = Resolve<IGenericRepository<Token>>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();

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

                var user = await _accountRepository.GetByExpression(u =>
                    u!.PhoneNumber!.ToLower() == loginRequest.PhoneNumber.ToLower() && u.IsDeleted == false);
                if (user == null)
                    result = BuildAppActionResultError(result, $" {loginRequest.PhoneNumber} này không tồn tại trong hệ thống");

                if (user.IsVerified == false)
                {
                    user.IsVerified = true;
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

                var headers = httpContext.Request.Headers;
                string userAgent = headers["User-Agent"].ToString();
                string deviceName = ParseDeviceNameFromUserAgent(userAgent);
                var deviceIp = GetClientIpAddress(httpContext);


                if (tokenDto != null)
                {
                    // Create Token object
                    var tokenDb = await tokenRepository!.GetByExpression(p => p.DeviceName == deviceName && p.DeviceIP == deviceIp && p.ExpiryTimeAccessToken > currentTime && p.AccountId == user.Id);
                    if (tokenDb != null)
                    {
                        _tokenDto.Token = tokenDb.AccessTokenValue;
                        _tokenDto.RefreshToken = tokenDb.RefreshTokenValue;
                    }
                    else
                    {
                        var token = new Token
                        {
                            TokenId = Guid.NewGuid(),
                            DeviceIP = GetClientIpAddress(httpContext),
                            AccountId = user.Id,
                            CreateDateAccessToken = utility.GetCurrentDateTimeInTimeZone(),
                            CreateRefreshToken = utility.GetCurrentDateTimeInTimeZone(),
                            ExpiryTimeAccessToken = utility.GetCurrentDateTimeInTimeZone().AddDays(30),
                            ExpiryTimeRefreshToken = utility.GetCurrentDateTimeInTimeZone().AddDays(30),
                            DeviceName = deviceName,
                            DeviceToken = string.Empty,
                            AccessTokenValue = tokenDto.Token!,
                            RefreshTokenValue = tokenDto.RefreshToken!,
                            IsActive = true,
                            LastLogin = utility.GetCurrentDateTimeInTimeZone(),
                        };

                        await tokenRepository!.Insert(token);

                    }

                    otpCodeDb!.IsUsed = true;
                    await _accountRepository.Update(user);
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


                if (!BuildAppActionResultIsError(result))
                {
                    result = await LoginDefault(email, user);
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
            var utility = Resolve<Utility>();
            var jwtService = Resolve<IJwtService>();
            var tokenRepository = Resolve<IGenericRepository<Token>>();
            var otpRepository = Resolve<IGenericRepository<OTP>>();
            var storeCreditRepository = Resolve<IGenericRepository<StoreCredit>>();
            try
            {
                if (await _accountRepository.GetByExpression(r => r!.PhoneNumber == signUpRequest.PhoneNumber) != null)
                    result = BuildAppActionResultError(result, "Số điện thoại đã tồn tại!");

                if (!BuildAppActionResultIsError(result))
                {
                    var emailService = Resolve<IEmailService>();
                    var smsService = Resolve<ISmsService>();
                    var currentTime = utility.GetCurrentDateTimeInTimeZone();

                    var user = new Account
                    {
                        Id = Guid.NewGuid().ToString(),
                        Email = signUpRequest.Email,
                        UserName = signUpRequest.PhoneNumber,
                        FirstName = signUpRequest.FirstName,
                        LastName = signUpRequest.LastName,
                        PhoneNumber = signUpRequest.PhoneNumber,
                        Gender = signUpRequest.Gender,
                        LoyaltyPoint = 0,
                        IsVerified = isGoogle ? true : false,
                        IsManuallyCreated = true,
                    };


                    var resultCreateUser = await _userManager.CreateAsync(user);
                    if (resultCreateUser.Succeeded)
                    {
                        result.Result = user;
                        if ((!string.IsNullOrEmpty(signUpRequest.Email) && !string.IsNullOrEmpty(signUpRequest.PhoneNumber) || (!string.IsNullOrEmpty(signUpRequest.Email))))
                        {
                            if (!isGoogle)
                            {
                                var random = new Random();
                                var verifyCode = string.Empty;
                                verifyCode = random.Next(100000, 999999).ToString();

                                emailService!.SendEmail(user.Email, SD.SubjectMail.VERIFY_ACCOUNT,
                                   TemplateMappingHelper.GetTemplateOTPEmail(
                                       TemplateMappingHelper.ContentEmailType.VERIFICATION_CODE, verifyCode,
                                       user.LastName));

                                var otpsDb = new OTP
                                {
                                    OTPId = Guid.NewGuid(),
                                    Type = OTPType.Register,
                                    AccountId = user.Id,
                                    Code = verifyCode,
                                    ExpiredTime = utility.GetCurrentDateTimeInTimeZone().AddMinutes(5),
                                    IsUsed = false,
                                };
                                await _otpRepository.Insert(otpsDb);
                                await _unitOfWork.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            var availableOtp = await _otpRepository.GetAllDataByExpression(o => o.Account.PhoneNumber.
                            Equals(user.PhoneNumber) && o.Type == OTPType.Register && o.ExpiredTime > utility.GetCurrentDateTimeInTimeZone() && !o.IsUsed, 0, 0, null, false, null);
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
                            if (!BuildAppActionResultIsError(result))
                            {
                                string code = await GenerateVerifyCodeSms(user.PhoneNumber, true); ;
                                var otpsDb = new OTP
                                {
                                    OTPId = Guid.NewGuid(),
                                    Type = OTPType.Register,
                                    AccountId = user.Id,
                                    Code = code,
                                    ExpiredTime = utility.GetCurrentDateTimeInTimeZone().AddMinutes(5),
                                    IsUsed = false,
                                };
                                await _otpRepository.Insert(otpsDb);
                                await _unitOfWork.SaveChangesAsync();
                            }
                        }
                    }
                    else
                    {
                        result = BuildAppActionResultError(result, $"Tạo tài khoản không thành công");
                    }

                    var resultCreateRole = await _userManager.AddToRoleAsync(user, "CUSTOMER");
                    if (!resultCreateRole.Succeeded) result = BuildAppActionResultError(result, $"Cấp quyền khách hàng không thành công");
                    //bool customerAdded = await AddCustomerInformation(user);
                    //if (!customerAdded)
                    //{
                    //    result = BuildAppActionResultError(result, $"Tạo thông tin khách hàng không thành công");
                    //}

                    var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                    var configurationDb = await configurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.EXPIRE_TIME_FOR_STORE_CREDIT), null);
                    if (configurationDb == null)
                    {
                        result = BuildAppActionResultError(result, $"Tạo ví không thành công");
                    }
                    var expireTimeInDay = double.Parse(configurationDb.CurrentValue);
                    var newStoreCreditDb = new StoreCredit
                    {
                        StoreCreditId = Guid.NewGuid(),
                        Amount = 0,
                        ExpiredDate = utility.GetCurrentDateInTimeZone().AddDays(expireTimeInDay),
                        AccountId = user.Id
                    };
                    await storeCreditRepository.Insert(newStoreCreditDb);

                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        private async Task<bool> AddAccountInfomation(Account user)
        {
            bool isSuccessful = false;
            try
            {
                var customer = new Account
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    DOB = user.DOB,
                    Gender = user.Gender,
                    IsVerified = false
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
                var otpRepository = Resolve<IGenericRepository<OTP>>();
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

                var user = await _accountRepository.GetAllDataByExpression(a =>
                  a!.PhoneNumber == phoneNumber && !a.IsDeleted, 0, 0, null, false, null);
                if (user.Items.Count() == 0)
                {
                    if (!(otp == OTPType.Login || otp == OTPType.Register))
                    {
                        return BuildAppActionResultError(result, $"Tài khoản với sđt {phoneNumber} không tồn tại. Vui lòng thử lại");
                    }
                }
                else
                {
                    if (user.Items.Count() == 1)
                    {
                        if (!(otp == OTPType.Login || otp == OTPType.Register) && !user.Items.FirstOrDefault().IsVerified)
                        {
                            return BuildAppActionResultError(result, $"Tài khoản với sđt {phoneNumber} chưa đuọc xác thực. Vui lòng thử lại");
                        }

                        if (otp == OTPType.Register)
                        {
                            return BuildAppActionResultError(result, $"Tài khoản với sđt {phoneNumber} đã tồn tại. Vui lòng dùng số điện thoại khác");
                        }
                    }
                    else
                    {
                        return BuildAppActionResultError(result, $"Nhiều hơn 1 tài khoản với sđt {phoneNumber} đã tồn tại. Vui lòng dùng số điện thoại khác");
                    }
                }


                if (!BuildAppActionResultIsError(result))
                {
                    var smsService = Resolve<ISmsService>();
                    string code = "";
                    var createdUser = user.Items.FirstOrDefault();
                    if (createdUser == null)
                    {
                        var createAccountResult = await RegisterAccountByPhoneNumber(phoneNumber);
                        if (!createAccountResult.IsSuccess)
                        {
                            result = BuildAppActionResultError(result, $"Đăng tài khoản cho số điện thoại {phoneNumber} thất bại. Vui lòng thử lại");
                            return result;
                        }
                        createdUser = (Account?)createAccountResult.Result;
                        if (createdUser == null)
                        {
                            result = BuildAppActionResultError(result, $"Đăng tài khoản cho số điện thoại {phoneNumber} thất bại. Vui lòng thử lại");
                            return result;
                        }
                        //var response = await smsService!.SendMessage($"Mã xác thực của bạn là là: {code}", phoneNumber);
                    }

                    code = await GenerateVerifyCodeSms(phoneNumber, true);
                    var otpsDb = new OTP
                    {
                        OTPId = Guid.NewGuid(),
                        Type = otp,
                        AccountId = createdUser.Id,
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
                var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var storeCreditRepository = Resolve<IGenericRepository<StoreCredit>>();
                var utility = Resolve<Utility>();

                var random = new Random();
                var verifyCode = string.Empty;
                verifyCode = random.Next(100000, 999999).ToString();
                int accountRandomNumber = random.Next(1000000, 9999999);
                var user = new Account
                {
                    Email = string.Empty,
                    UserName = phoneNumber,
                    FirstName = string.Empty,
                    LastName = string.Empty,
                    PhoneNumber = phoneNumber,
                    Gender = true,
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

                var configurationDb = await configurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.EXPIRE_TIME_FOR_STORE_CREDIT), null);
                if (configurationDb == null)
                {
                    result = BuildAppActionResultError(result, $"Tạo ví không thành công");
                }
                var expireTimeInDay = double.Parse(configurationDb.CurrentValue);
                var newStoreCreditDb = new StoreCredit
                {
                    StoreCreditId = Guid.NewGuid(),
                    Amount = 0,
                    ExpiredDate = utility.GetCurrentDateInTimeZone().AddDays(expireTimeInDay),
                    AccountId = user.Id
                };
                await storeCreditRepository.Insert(newStoreCreditDb);

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

                if (roleNameList.Contains("ADMIN"))
                {
                    item.MainRole = "ADMIN";
                }
                else if (roleNameList.Contains("SHIPPER"))
                {
                    item.MainRole = "SHIPPER";
                }
                else if (roleNameList.Contains("CHEF") && !roleNameList.Contains("ADMIN"))
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
            var tokenRepository = Resolve<IGenericRepository<Token>>();
            try
            {
                var user = await _accountRepository.GetById(userId);
                if (user == null)
                    result = BuildAppActionResultError(result, "Tài khoản không tồn tại");
                var token = await tokenRepository!.GetByExpression(p => p.AccountId == user.Id);
                if (token.RefreshTokenValue != refreshToken)
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
                var smsService = Resolve<ISmsService>();
                //var response = await smsService!.SendMessage($"Mã xác thực tại nhà hàng TP là: {code}",
                //    phoneNumber);
            }

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
            try
            {
                var tokenRepository = Resolve<IGenericRepository<Token>>();
                var storeCreditRepository = Resolve<IGenericRepository<StoreCredit>>();
                var customerInfoAddressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
                var jwtService = Resolve<IJwtService>();
                var utility = Resolve<Utility>();
                var token = await jwtService!.GenerateAccessToken(new LoginRequestDto { PhoneNumber = phoneNumber });
                var refreshToken = jwtService.GenerateRefreshToken();

                _tokenDto.Token = token;
                _tokenDto.RefreshToken = refreshToken;
                _tokenDto.Account = _mapper.Map<AccountResponse>(user);
                var customerInfoAddressDb = await customerInfoAddressRepository.GetAllDataByExpression(c => c.AccountId.Equals(user.Id) && !c.IsDeleted, 0, 0, null, false, null);
                if (customerInfoAddressDb.Items.Count() > 0)
                {
                    _tokenDto.Account.Addresses = customerInfoAddressDb.Items;
                }
                var creditStoreDb = await storeCreditRepository.GetAllDataByExpression(c => c.AccountId.Equals(user.Id), 0, 0, null, false, null);
                if (creditStoreDb.Items.Count() > 0)
                {
                    _tokenDto.Account.StoreCredit = creditStoreDb.Items[0].Amount;
                    _tokenDto.Account.StoreCreditExpireDay = creditStoreDb.Items[0].ExpiredDate;
                    _tokenDto.Account.StoreCreditId = creditStoreDb.Items[0].StoreCreditId;
                }

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
                if (roleNameList.Contains("ADMIN"))
                {
                    _tokenDto.MainRole = "ADMIN";
                }
                else if (roleNameList.Contains("SHIPPER"))
                {
                    _tokenDto.MainRole = "SHIPPER";
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

            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
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
                else if (optUser.Code != optCode)
                {
                    result = BuildAppActionResultError(result, $"Mã Otp không đúng!");
                }

                if (!BuildAppActionResultIsError(result))
                {
                    result = await LoginDefault(phoneNumber, user);
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



        public async Task<AppActionResult> GenerateCustomerOTP(Account customerDb, OTPType otpType)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                //Check Customermust be unverified
                if (customerDb == null)
                {
                    result = BuildAppActionResultError(result, $"Không tìm thấy thông tin người dùng với id {customerDb.Id}");
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
                    AccountId = customerDb.Id,
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

        //public async Task<AppActionResult> SendAccountOTP(string phoneNumber, OTPType otpType)
        //{
        //    AppActionResult result = new AppActionResult();
        //    try
        //    {
        //        var customerListDb = await _accountRepository.GetAllDataByExpression(c => c.PhoneNumber.Equals(phoneNumber), 0, 0, null, false, null);

        //        if (customerListDb.Items.Count > 1)
        //        {
        //            result = BuildAppActionResultError(result, $"Xảy ra lỗi khi tìm thông tin người dùng với sđt {phoneNumber}");
        //            return result;
        //        }

        //        if (customerListDb.Items.Count == 0)
        //        {
        //            result = BuildAppActionResultError(result, $"Không tìm thấy thông tin người dùng với sđt {phoneNumber}");
        //            return result;
        //        }
        //        var customerDb = customerListDb.Items[0];
        //        if (otpType == OTPType.Register)
        //        {
        //            if (customerDb.IsVerified)
        //            {
        //                result = BuildAppActionResultError(result, $"Thông tin người dùng đã được xác thực");
        //                return result;
        //            }
        //        }
        //        else
        //        {
        //            if (!customerDb.IsVerified)
        //            {
        //                result = BuildAppActionResultError(result, $"Thông tin người dùng đã chưa được xác thực");
        //                return result;
        //            }
        //        }

        //        var otpRepository = Resolve<IGenericRepository<OTP>>();
        //        var utility = Resolve<Utility>();
        //        var availableOTPDb = await otpRepository.GetAllDataByExpression(o => o.Account.PhoneNumber == phoneNumber && !o.IsUsed && o.ExpiredTime > utility.GetCurrentDateTimeInTimeZone() && o.Type == otpType, 0, 0, null, false, null);
        //        if (availableOTPDb.Items.Count > 1)
        //        {
        //            result = BuildAppActionResultError(result, $"Xảy ra lỗi vì có nhiều hơn mã OTP hữu dụng tồn tại trong hệ thống. Vui lòng thử lại sau");
        //            return result;
        //        }

        //        if (availableOTPDb.Items.Count == 1)
        //        {
        //            result.Result = availableOTPDb.Items[0].Code;
        //            return result;
        //        }

        //        var otp = await GenerateCustomerInfoOTP(customerDb, otpType);
        //        if (!otp.IsSuccess)
        //        {
        //            return BuildAppActionResultError(result, $"Tạo OTP thất bại. Vui lòng thử lại");
        //        }

        //        await _accountRepository.Update(customerDb);
        //        await _unitOfWork.SaveChangesAsync();
        //        var smsService = Resolve<ISmsService>();
        //        var response = await smsService!.SendMessage($"Mã xác thực tại nhà hàng TP là: {otp.Result.ToString()}",
        //            customerDb.PhoneNumber);
        //        result.Result = customerDb;
        //    }
        //    catch (Exception ex)
        //    {
        //        result = BuildAppActionResultError(result, ex.Message);
        //    }
        //    return result;
        //}

        public async Task<AppActionResult> UpdateAccount(UpdateAccountInfoRequest updateAccountRequest)
        {
            var result = new AppActionResult();
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var mapService = Resolve<IMapService>();
                    var firebaseService = Resolve<IFirebaseService>();
                    var accountDb = await _accountRepository.GetByExpression(p => p.Id == updateAccountRequest.AccountId);
                    if (accountDb == null)
                    {
                        result = BuildAppActionResultError(result, $"Tài khoản với số điện thoại {updateAccountRequest.AccountId} không tồn tại!");
                    }

                    accountDb.FirstName = updateAccountRequest.FirstName;
                    accountDb.LastName = updateAccountRequest.LastName;
                    accountDb.DOB = updateAccountRequest.DOB;
                    accountDb.IsManuallyCreated = false;
                    accountDb.Gender = updateAccountRequest.Gender;

                    if (updateAccountRequest.Image != null)
                    {

                        var pathName = SD.FirebasePathName.DISH_PREFIX + $"{updateAccountRequest.AccountId}{Guid.NewGuid()}.jpg";
                        var upload = await firebaseService!.UploadFileToFirebase(updateAccountRequest.Image!, pathName);

                        if (!upload.IsSuccess)
                        {
                            return BuildAppActionResultError(result, "Upload hình ảnh không thành công");
                        }
                        accountDb!.Avatar = upload.Result!.ToString()!;

                        if (!upload.IsSuccess)
                        {
                            return BuildAppActionResultError(result, "Upload hình ảnh không thành công");
                        }
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        await _accountRepository.Update(accountDb);
                        await _unitOfWork.SaveChangesAsync();
                        scope.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
            }
            return result;
        }



        public async Task<AppActionResult> DeleteAccount(string customerId)
        {
            var result = new AppActionResult();
            try
            {
                var customerInfoDb = await _accountRepository.GetByExpression(p => p.Id == customerId.ToString());
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
                else if (optUser.Code != code)
                {
                    result = BuildAppActionResultError(result, $"Mã Otp không đúng!");
                }

                if (!BuildAppActionResultIsError(result))
                {
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

        public async Task<AppActionResult> VerifyAccountOTP(string phoneNumber, string code, OTPType otpType)
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
                else if (optUser.Code != code)
                {
                    result = BuildAppActionResultError(result, $"Mã Otp không đúng!");
                }

                if (!BuildAppActionResultIsError(result))
                {
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

        public async Task<AppActionResult> GetAccountByPhoneNumber(string phoneNumber)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
                var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
                var listRole = await roleRepository!.GetAllDataByExpression(null, 1, 100, null, false, null);
                var customerInfoDb = await _accountRepository.GetByExpression(c => c.PhoneNumber.Equals(phoneNumber));
                var listMap = _mapper.Map<AccountResponse>(customerInfoDb);

                if (customerInfoDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy thông tin người dùng với số điện thoại {phoneNumber}");
                }

                var customerInfoAddressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
                var customerInfoAddressDb = await customerInfoAddressRepository!.GetAllDataByExpression(c => c.AccountId == customerInfoDb.Id && !c.IsDeleted, 0, 0, null, false, null);
                listMap.Addresses = customerInfoAddressDb.Items;

                var userRole = new List<IdentityRole>();
                var role = await userRoleRepository!.GetAllDataByExpression(a => a.UserId == customerInfoDb.Id, 1, 100, null, false, null);
                foreach (var itemRole in role.Items!)
                {
                    var roleUser = listRole.Items!.ToList().FirstOrDefault(a => a.Id == itemRole.RoleId);
                    if (roleUser != null) userRole.Add(roleUser);
                }
                listMap.Roles = userRole;
                var roleNameList = userRole.DistinctBy(i => i.Id).Select(i => i.Name).ToList();

                if (roleNameList.Contains("ADMIN"))
                {
                    listMap.MainRole = "ADMIN";
                }
                else if (roleNameList.Contains("SHIPPER"))
                {
                    listMap.MainRole = "SHIPPER";
                }
                else if (roleNameList.Contains("CHEF") && !roleNameList.Contains("ADMIN"))
                {
                    listMap.MainRole = "CHEF";
                }
                else if (roleNameList.Count > 1)
                {
                    listMap.MainRole = roleNameList.FirstOrDefault(n => !n.Equals("CUSTOMER"));
                }
                else
                {
                    listMap.MainRole = "CUSTOMER";
                }

                result.Result = listMap;
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

        public async Task<AppActionResult> CreateCustomerInfoAddress(CustomerInfoAddressRequest customerInfoAddressRequest)
        {
            var result = new AppActionResult();
            var customerInfoAddressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
            var accountRepository = Resolve<IGenericRepository<Account>>();
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {

                    var newCustomerInfoAddress = new CustomerInfoAddress
                    {
                        CustomerInfoAddressId = Guid.NewGuid(),
                        CustomerInfoAddressName = customerInfoAddressRequest.CustomerInfoAddressName,
                        IsCurrentUsed = customerInfoAddressRequest.IsCurrentUsed,
                        AccountId = customerInfoAddressRequest!.AccountId!,
                        Lat = customerInfoAddressRequest.Lat,
                        Lng = customerInfoAddressRequest.Lng,
                    };
                    if (customerInfoAddressRequest.IsCurrentUsed == true)
                    {
                        var mainAddressDb = await customerInfoAddressRepository!.GetByExpression(p => p.AccountId == customerInfoAddressRequest.AccountId && p.IsCurrentUsed == true);
                        if (mainAddressDb != null)
                        {
                            mainAddressDb.IsCurrentUsed = false;
                            await customerInfoAddressRepository.Update(mainAddressDb);
                        }
                        var accountDb = await accountRepository.GetByExpression(p => p.Id == customerInfoAddressRequest.AccountId);
                        accountDb.Address = newCustomerInfoAddress.CustomerInfoAddressName;
                        await accountRepository.Update(accountDb);
                    }
                    if (!BuildAppActionResultIsError(result))
                    {
                        await customerInfoAddressRepository!.Insert(newCustomerInfoAddress);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
            }
            return result;
        }

        public async Task<AppActionResult> UpdateCustomerInfoAddress(UpdateCustomerInforAddressRequest updateCustomerInforAddress)
        {
            var result = new AppActionResult();
            var customerInfoAddressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
            var accountRepository = Resolve<IGenericRepository<Account>>();
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var customerInfoDb = await customerInfoAddressRepository!.GetByExpression(p => p.CustomerInfoAddressId == updateCustomerInforAddress.CustomerInfoAddressId);
                    if (customerInfoDb == null)
                    {
                        return BuildAppActionResultError(result, $"Không tìm thấy địa chỉ với id {updateCustomerInforAddress.AccountId}");
                    }

                    customerInfoDb.CustomerInfoAddressName = updateCustomerInforAddress.CustomerInfoAddressName;
                    customerInfoDb.IsCurrentUsed = updateCustomerInforAddress.IsCurrentUsed;
                    customerInfoDb.AccountId = updateCustomerInforAddress.AccountId;
                    customerInfoDb.Lat = updateCustomerInforAddress.Lat;
                    customerInfoDb.Lng = updateCustomerInforAddress.Lng;

                    if (updateCustomerInforAddress.IsCurrentUsed == true)
                    {
                        var mainAddressDb = await customerInfoAddressRepository!.GetByExpression(p => p.AccountId == updateCustomerInforAddress.AccountId && p.IsCurrentUsed == true);
                        if (mainAddressDb != null)
                        {
                            var accountDb = await accountRepository.GetByExpression(p => p.Id == updateCustomerInforAddress.AccountId);
                            accountDb.Address = updateCustomerInforAddress.CustomerInfoAddressName;
                            mainAddressDb.IsCurrentUsed = false;
                            await customerInfoAddressRepository.Update(mainAddressDb);
                            await accountRepository.Update(accountDb);
                        }
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        await customerInfoAddressRepository!.Update(customerInfoDb);
                        await _unitOfWork.SaveChangesAsync();
                    }
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
                return result;
            }
        }


        public async Task<AppActionResult> DeleteCustomerInfoAddress(Guid customerInfoAddressId)
        {
            var result = new AppActionResult();
            var customerInfoAddressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
            try
            {
                var customerInfoAddressDb = await customerInfoAddressRepository!.GetByExpression(p => p.CustomerInfoAddressId == customerInfoAddressId);
                if (customerInfoAddressDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy địa chỉ khách hàng với id {customerInfoAddressId}");
                }
                if (customerInfoAddressDb.IsCurrentUsed == true)
                {
                    return BuildAppActionResultError(result, $"Không thể xóa địa chỉ đang sử dụng, hãy sử dụng địa chỉ khác");
                }
                else
                {
                    customerInfoAddressDb.IsDeleted = true;
                    await customerInfoAddressRepository.Update(customerInfoAddressDb);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
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

        public async Task<AppActionResult> LoadAvailableShipper()
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
                var roleRepository = Resolve<IGenericRepository<IdentityRole>>();

                var adminRoleDb = await roleRepository.GetByExpression(r => r.Name.Equals("ADMIN"));

                var shipperRoleDb = await roleRepository.GetByExpression(r => r.Name.Equals("SHIPPER"));
                if (shipperRoleDb == null)
                {
                    return BuildAppActionResultError(result, $"Không có thông tin về vai trò shipper");
                }
                var userWithShipperRoleDb = await userRoleRepository.GetAllDataByExpression(u => u.RoleId == shipperRoleDb.Id, 0, 0, null, false, null);
                var shipperIds = userWithShipperRoleDb.Items.DistinctBy(u => u.UserId).Select(u => u.UserId).ToList();
                var userWithAdminRoleDb = await userRoleRepository.GetAllDataByExpression(u => adminRoleDb != null && u.RoleId == adminRoleDb.Id, 0, 0, null, false, null);
                var adminIds = userWithAdminRoleDb.Items.DistinctBy(u => u.UserId).Select(u => u.UserId).ToList();
                shipperIds = shipperIds.Where(s => !adminIds.Contains(s)).ToList();
                var availableShipperDb = await _accountRepository.GetAllDataByExpression(a => a.IsVerified && !a.IsDeleted
                                                                                              && !a.IsDelivering && shipperIds.Contains(a.Id), 0, 0, null, false, null);

                result.Result = availableShipperDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> UpdateDeliveringStatus(string accountId)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
                var roleRepository = Resolve<IGenericRepository<IdentityRole>>();

                var shipperRoleDb = await roleRepository.GetByExpression(r => r.Name.Equals("CHEF"));
                if (shipperRoleDb == null)
                {
                    return BuildAppActionResultError(result, $"Không có thông tin về vai trò shipper");
                }
                var userRoleDb = await userRoleRepository.GetByExpression(u => u.RoleId == shipperRoleDb.Id && u.UserId.Equals(accountId));
                if (userRoleDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tồn tại shipper với id {accountId}");
                }
                var shipperDb = await _accountRepository.GetByExpression(a => a.IsVerified && !a.IsDeleted && userRoleDb.UserId.Equals(a.Id));
                if (shipperDb != null)
                {
                    shipperDb.IsDelivering = !shipperDb.IsDelivering;
                    await _accountRepository.Update(shipperDb);
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    return BuildAppActionResultError(result, $"Không tồn tại shipper với id {accountId}");
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> ChangeEmailRequest(string accountId, string newEmail)
        {
            var result = new AppActionResult();
            try
            {
                var accountDb = await _accountRepository.GetById(accountId);
                if (accountDb == null)
                {
                    return BuildAppActionResultError(result, $"Tài khoản với id {accountId} không tồn tại");
                }

                var verifyCode = await GenerateCustomerOTP(accountDb, OTPType.ConfirmEmail);
                _emailService!.SendEmail(newEmail, SD.SubjectMail.VERIFY_ACCOUNT,
                                 TemplateMappingHelper.GetTemplateOTPEmail(
                                     TemplateMappingHelper.ContentEmailType.VERIFICATION_CODE, verifyCode.Result.ToString(),
                                     accountDb.LastName));

            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> VerifyChangeEmail(string email, string accountId, string otpCode)
        {
            var result = new AppActionResult();
            try
            {
                var otpRepository = Resolve<IGenericRepository<OTP>>();
                var utility = Resolve<Utility>();
                var accountDb = await _accountRepository.GetById(accountId);
                if (accountDb == null)
                {
                    return BuildAppActionResultError(result, $"Tài khoản với id {accountId} không tồn tại");
                }


                var currentTime = utility!.GetCurrentDateTimeInTimeZone();
                var otpDb = await otpRepository!.GetAllDataByExpression(p => p.Code == otpCode && p.AccountId == accountId && p.Type == OTPType.ConfirmEmail && p.ExpiredTime > currentTime, 0, 0, p => p.ExpiredTime, false, null);
                if (otpDb.Items!.FirstOrDefault() == null)
                {
                    return BuildAppActionResultError(result, $"Mã OTP không tồn tại");
                }
                else
                {
                    accountDb.Email = email;
                    accountDb.NormalizedEmail = email.ToUpper();
                }


                await _accountRepository.Update(accountDb);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
    }
}
