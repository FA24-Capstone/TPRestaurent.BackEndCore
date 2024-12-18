using Aspose.Pdf;
using AutoMapper;
using Firebase.Auth;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.ConfigurationModel;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;
using static Aspose.Pdf.Artifacts.Pagination.PageNumber;
using Utility = TPRestaurent.BackEndCore.Common.Utils.Utility;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class AccountService : GenericBackendService, IAccountService
    {
        private readonly IGenericRepository<Account> _accountRepository;
        private readonly IGenericRepository<IdentityUserRole<string>> _userRoleRepository;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly SignInManager<Account> _signInManager;
        private readonly TokenDto _tokenDto;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<Account> _userManager;
        private readonly IEmailService _emailService;
        private readonly IExcelService _excelService;
        private readonly IMapService _mapService;
        private readonly IHashingService _hashingService;
        public AccountService(
            IGenericRepository<Account> accountRepository,
            UserManager<Account> userManager,
            IGenericRepository<IdentityUserRole<string>> userRoleRepository,
            SignInManager<Account> signInManager,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IExcelService excelService,
            IHashingService hashingService,
            IMapper mapper,
            IServiceProvider serviceProvider,
            IMapService mapService
        ) : base(serviceProvider)
        {
            _accountRepository = accountRepository;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _excelService = excelService;
            _hashingService = hashingService;
            _tokenDto = new TokenDto();
            _mapper = mapper;
            _userRoleRepository = userRoleRepository;
            _mapService = mapService;
        }

        public async Task<AppActionResult> Login(LoginRequestDto loginRequest, HttpContext httpContext)
        {
            var result = new AppActionResult();
            try
            {
                var utility = Resolve<Utility>();
                var tokenRepository = Resolve<IGenericRepository<Token>>();
                var hashingService = Resolve<IHashingService>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var loginResult = new AppActionResult();

                if (!utility.IsValidPhoneNumberInput(loginRequest.PhoneNumber))
                {
                    return BuildAppActionResultError(result, "Số điện thoại không hợp lệ. Vui lòng kiểm tra lại.");
                }

                //var otpCodeListDb = await _otpRepository.GetAllDataByExpression(p => p.Code == loginRequest.OTPCode && (p.Type == OTPType.Login || p.Type == OTPType.ConfirmPhone) && p.ExpiredTime > currentTime && !p.IsUsed, 0, 0, null, false, null);

                //if (otpCodeListDb.Items.Count > 1)
                //{
                //    return BuildAppActionResultError(result, "Xảy ra lỗi trong quá trình xử lí, có nhiều hơn 1 type khả dụng. Vui lòng thử lại sau ít phút");
                //    return result;
                //}

                //if (otpCodeListDb.Items.Count == 0)
                //{
                //    return BuildAppActionResultError(result, "Mã type đã nhập không khả dụng. Vui lòng thử lại");
                //    return result;
                //}
                //var otpCodeDb = otpCodeListDb.Items[0];

                var user = await _accountRepository.GetByExpression(u =>
                    u!.PhoneNumber!.ToLower() == loginRequest.PhoneNumber.ToLower() && u.IsDeleted == false);
                if (user == null)
                    return BuildAppActionResultError(result, $" {loginRequest.PhoneNumber} này không tồn tại trong hệ thống");

                if (user.IsVerified == false)
                {
                    user.IsVerified = true;
                }
                if (user.IsBanned == true)
                {
                    throw new Exception($"Tài khoản của bạn đã bị khóa bởi hệ thống. Vui lòng liên hệ với hệ thống của nhà hàng");
                }

                var otp = JsonConvert.DeserializeObject<OTP>(user.OTP);
                if (otp == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy mã OTP");
                }

                if (otp!.Type != OTPType.Login)
                {
                    return BuildAppActionResultError(result, $"Mã OTP hiện tại không thuộc loại Đăng nhập");
                }

                if (otp!.ExpiredTime < currentTime)
                {
                    return BuildAppActionResultError(result, $"Mã OTP đã hết hạn");
                }
                

                if (otp!.Code != loginRequest.OTPCode) return BuildAppActionResultError(result, "Đăng nhâp thất bại");
                
                if (!BuildAppActionResultIsError(result)) 
                    loginResult = await LoginDefault(loginRequest.PhoneNumber, user);

                var tokenDto = loginResult.Result as TokenDto;

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
                            DeviceToken = null,
                            AccessTokenValue = tokenDto.Token!,
                            RefreshTokenValue = tokenDto.RefreshToken!,
                            IsActive = true,
                            LastLogin = utility.GetCurrentDateTimeInTimeZone(),
                        };

                        await tokenRepository!.Insert(token);
                    }
                    user.OTP = "";
                    await _accountRepository.Update(user);
                }
                await _unitOfWork.SaveChangesAsync();
                user = hashingService.GetDecodedAccount(user);
                tokenDto.Account.LoyalPoint = user.LoyaltyPoint;
                tokenDto.Account.Amount = user.StoreCreditAmount;
                result.Result = tokenDto;
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
                    return BuildAppActionResultError(result, $"Email này không tồn tại");
                else if (user.IsVerified == false)
                    return BuildAppActionResultError(result, "Tài khoản này chưa xác thực !");

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
            var couponRepository = Resolve<IGenericRepository<Coupon>>();
            var couponProgramRepository = Resolve<IGenericRepository<CouponProgram>>();
            var emnailService = Resolve<IEmailService>();

            try
            {
                if (!utility.IsValidPhoneNumberInput(signUpRequest.PhoneNumber))
                {
                    return BuildAppActionResultError(result, "Số điện thoại không hợp lệ. Vui lòng kiểm tra lại.");
                }

                var existedAccount = await _accountRepository.GetByExpression(r => r!.PhoneNumber == signUpRequest.PhoneNumber && !r.IsDeleted && !r.IsBanned);
                if (existedAccount != null)
                    return BuildAppActionResultError(result, "Số điện thoại đã tồn tại!");

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
                        IsVerified = isGoogle ? true : false,
                        IsManuallyCreated = true,
                        RegisteredDate = utility.GetCurrentDateInTimeZone()
                    };

                    var loyaltyPoint = _hashingService.Hashing(user.Id, 0, true);
                    if (loyaltyPoint.IsSuccess)
                    {
                        user.LoyaltyPoint = loyaltyPoint.Result.ToString();
                    }

                    var storeCredit = _hashingService.Hashing(user.Id, 0, false);
                    if (storeCredit.IsSuccess)
                    {
                        user.StoreCreditAmount = storeCredit.Result.ToString();
                    }

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
                                Type = OTPType.Login,
                                Code = verifyCode,
                                ExpiredTime = utility.GetCurrentDateTimeInTimeZone().AddMinutes(5),
                            };
                            user.OTP = JsonConvert.SerializeObject(otpsDb);
                            //await _accountRepository.Insert(user);
                            //await _unitOfWork.SaveChangesAsync();
                            await _userManager.CreateAsync(user);
                        }
                    }

                    var resultCreateRole = await _userManager.AddToRoleAsync(user, "CUSTOMER");
                    if (!resultCreateRole.Succeeded) return BuildAppActionResultError(result, $"Cấp quyền khách hàng không thành công");
                    //bool customerAdded = await AddCustomerInformation(user);
                    //if (!customerAdded)
                    //{
                    //    return BuildAppActionResultError(result, $"Tạo thông tin khách hàng không thành công");
                    //}

                    var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                    var configurationDb = await configurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.EXPIRE_TIME_FOR_STORE_CREDIT), null);
                    if (configurationDb == null)
                    {
                        return BuildAppActionResultError(result, $"Tạo ví không thành công");
                    }
                    var expireTimeInDay = double.Parse(configurationDb.CurrentValue);
                    //var newStoreCreditDb = new StoreCredit
                    //{
                    //    StoreCreditId = Guid.NewGuid(),
                    //    Amount = 0,
                    //    ExpiredDate = utility.GetCurrentDateInTimeZone().AddDays(expireTimeInDay),
                    //    AccountId = user.Id
                    //};
                    //await storeCreditRepository.Insert(newStoreCreditDb);

                    var couponProgramDb = await couponProgramRepository!.GetByExpression(p => p.CouponProgramTypeId == CouponProgramType.NEWBIE);
                    var coupon = new Coupon
                    {
                        AccountId = user.Id,
                        CouponId = Guid.NewGuid(),
                        CouponProgramId = couponProgramDb.CouponProgramId,
                        IsUsedOrExpired = false
                    };

                    string username = user.FirstName + " " + user.LastName;
                    emailService.SendEmail(user.Email, SD.SubjectMail.NOTIFY_RESERVATION, TemplateMappingHelper.GetTemplateFirstRegistrationCoupon(username, couponProgramDb));
                    await couponRepository!.Insert(coupon);
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
                if (!utility.IsValidPhoneNumberInput(accountRequest.PhoneNumber))
                {
                    return BuildAppActionResultError(result, "Số điện thoại không hợp lệ. Vui lòng kiểm tra lại.");
                    return result;
                }
                var account =
                    await _accountRepository.GetByExpression(
                        a => a!.PhoneNumber == accountRequest.PhoneNumber!.ToLower() && !a.IsBanned && !a.IsDeleted);
                var otpCodeDb = JsonConvert.DeserializeObject<OTP>(account.OTP);

                if (otpCodeDb == null)
                {
                    return BuildAppActionResultError(result, $"Mã OTP không tồn tại");
                }

                if (otpCodeDb.Type != OTPType.ChangePhone)
                {
                    return BuildAppActionResultError(result, $"Mã OTP hiện tại không thuộc loại Cập nhật số điện thoại");
                }

                if (otpCodeDb!.ExpiredTime < currentTime)
                {
                    return BuildAppActionResultError(result, $"Mã OTP đã hết hạn");
                }
                
                if (!BuildAppActionResultIsError(result))
                {
                    account.PhoneNumber = accountRequest.PhoneNumber;
                    account.OTP = "";
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

        public async Task<AppActionResult> SendOTP(string phoneNumber, OTPType type)
        {
            var result = new AppActionResult();
            try
            {
                var utility = Resolve<Utility>();
                if (!utility.IsValidPhoneNumberInput(phoneNumber))
                {
                    return BuildAppActionResultError(result, "Số điện thoại không hợp lệ. Vui lòng kiểm tra lại.");
                }
                var accountDb = await _accountRepository.GetByExpression(o => o.PhoneNumber.Equals(phoneNumber) && !o.IsDeleted, null);
                if (accountDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy tài khoản khách hàng với số điện thoại {phoneNumber}.");
                }

                if (accountDb.IsBanned)
                {
                    return BuildAppActionResultError(result, $"Tài khoản khách hàng với số điện thoại {phoneNumber} đã bị cấm.");
                }

                if (!string.IsNullOrEmpty(accountDb.OTP))
                {
                    var otp = JsonConvert.DeserializeObject<OTP>(accountDb.OTP);
                    if(otp != null && otp.ExpiredTime > utility.GetCurrentDateTimeInTimeZone())
                    {
                        //Has valid OTP
                        if(otp.Type == type)
                        {
                            result.Result = otp;
                            return result;
                        } 
                        
                        return BuildAppActionResultError(result, $"Hiện tại đang có OTP khả dụng khác lo.");
                    }
                }

                if (!(type == OTPType.Login || type == OTPType.Register) && !accountDb.IsVerified)
                {
                    return BuildAppActionResultError(result, $"Tài khoản với sđt {phoneNumber} chưa đuọc xác thực. Vui lòng thử lại");
                }

                if (type == OTPType.Register)
                {
                    return BuildAppActionResultError(result, $"Tài khoản với sđt {phoneNumber} đã tồn tại. Vui lòng dùng số điện thoại khác");
                }

                if (!BuildAppActionResultIsError(result))
                {
                    var smsService = Resolve<ISmsService>();
                    string code = "";
                    var createdUser = accountDb;
                    if (createdUser == null)
                    {
                        var createAccountResult = await RegisterAccountByPhoneNumber(phoneNumber);
                        if (!createAccountResult.IsSuccess)
                        {
                            return BuildAppActionResultError(result, $"Đăng tài khoản cho số điện thoại {phoneNumber} thất bại. Vui lòng thử lại");
                            return result;
                        }
                        createdUser = (Account?)createAccountResult.Result;
                        if (createdUser == null)
                        {
                            return BuildAppActionResultError(result, $"Đăng tài khoản cho số điện thoại {phoneNumber} thất bại. Vui lòng thử lại");
                            return result;
                        }
                    }

                    code = await GenerateVerifyCodeSms(phoneNumber, true);
                    var otpsDb = new OTP
                    {
                        Type = type,
                        Code = code,
                        ExpiredTime = utility.GetCurrentDateTimeInTimeZone().AddMinutes(5),
                    };

                    createdUser.OTP = JsonConvert.SerializeObject(otpsDb);
                    await _accountRepository.Update(createdUser);
                    await _unitOfWork.SaveChangesAsync();
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
                var utility = Resolve<Utility>();
                var couponRepository = Resolve<IGenericRepository<Coupon>>();
                var couponProgramRepository = Resolve<IGenericRepository<CouponProgram>>();
                var random = new Random();
                var verifyCode = string.Empty;
                verifyCode = random.Next(100000, 999999).ToString();
                var emailService = Resolve<IEmailService>();
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
                    return BuildAppActionResultError(result, $"Tạo tài khoản không thành công");
                }

                var resultCreateRole = await _userManager.AddToRoleAsync(user, "CUSTOMER");
                if (!resultCreateRole.Succeeded) return BuildAppActionResultError(result, $"Cấp quyền khách hàng không thành công");

                //var configurationDb = await configurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.EXPIRE_TIME_FOR_STORE_CREDIT), null);
                //if (configurationDb == null)
                //{
                //    return BuildAppActionResultError(result, $"Tạo ví không thành công");
                //}
                //var expireTimeInDay = double.Parse(configurationDb.CurrentValue);
                //var newStoreCreditDb = new StoreCredit
                //{
                //    StoreCreditId = Guid.NewGuid(),
                //    Amount = 0,
                //    ExpiredDate = utility.GetCurrentDateInTimeZone().AddDays(expireTimeInDay),
                //    AccountId = user.Id
                //};
                //await storeCreditRepository.Insert(newStoreCreditDb);
                var couponProgramDb = await couponProgramRepository!.GetByExpression(p => p.CouponProgramTypeId == CouponProgramType.NEWBIE);
                var coupon = new Coupon
                {
                    AccountId = user.Id,
                    CouponId = Guid.NewGuid(),
                    CouponProgramId = couponProgramDb.CouponProgramId,
                    IsUsedOrExpired = false
                };

                string username = user.FirstName + " " + user.LastName;
                emailService.SendEmail(user.Email, SD.SubjectMail.NOTIFY_RESERVATION, TemplateMappingHelper.GetTemplateFirstRegistrationCoupon(username, couponProgramDb));
                await couponRepository!.Insert(coupon);
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
                if (account == null) return BuildAppActionResultError(result, $"Tài khoản với id {id} không tồn tại !");
                if (!BuildAppActionResultIsError(result)) result.Result = account;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }

        public async Task<AppActionResult> GetAllAccount(string? keyword, int pageIndex, int pageSize)
        {
            var result = new AppActionResult();
            var list = new PagedResult<Account>();
            var customerInfoAddressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
            if (keyword != null)
            {
                list = await _accountRepository.GetAllDataByExpression(p => p.PhoneNumber.Contains(keyword), pageIndex, pageSize, null, false, null);
            }
            else
            {
                list = await _accountRepository.GetAllDataByExpression(null, pageIndex, pageSize, null, false, null);
            }

            list.Items = DecodeStoreCreditAndLoyaltyPointOfAccount(list.Items);

            var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
            var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
            var listRole = await roleRepository!.GetAllDataByExpression(null, 1, 100, null, false, null);
            var listMap = _mapper.Map<List<AccountResponse>>(list.Items);
            foreach (var item in listMap)
            {
                var userRole = new List<IdentityRole>();

                var customerInfoAddressDb = await customerInfoAddressRepository!.GetAllDataByExpression(p => p.AccountId == item.Id, 0, 0, null, false, null);
                var role = await userRoleRepository!.GetAllDataByExpression(a => a.UserId == item.Id, 1, 100, null, false, null);
                foreach (var itemRole in role.Items!)
                {
                    var roleUser = listRole.Items!.ToList().FirstOrDefault(a => a.Id == itemRole.RoleId);
                    if (roleUser != null) userRole.Add(roleUser);
                }

                item.Roles = userRole;
                var roleNameList = userRole.DistinctBy(i => i.Id).Select(i => i.Name).ToList();
                item.Addresses = customerInfoAddressDb.Items;

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

        public async Task<AppActionResult> GetNewToken(string refreshToken, string userId)
        {
            var result = new AppActionResult();
            var tokenRepository = Resolve<IGenericRepository<Token>>();
            try
            {
                var user = await _accountRepository.GetById(userId);
                if (user == null)
                {
                    throw new Exception("Tài khoản không tồn tại");
                }
                var token = await tokenRepository!.GetByExpression(p => p.AccountId == user.Id);
                if (token.RefreshTokenValue != refreshToken)
                {
                    throw new Exception("Mã làm mới không chính xác");
                }

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

        public async Task<AppActionResult> SendEmailForActiveCode(string email)
        {
            var result = new AppActionResult();

            try
            {
                var user = await _accountRepository.GetByExpression(a =>
                    a!.Email == email && a.IsDeleted == false && a.IsVerified == false);
                if (user == null)
                {
                    throw new Exception("Tài khoản không tồn tại hoặc chưa xác thực");
                }
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
                var customerInfoAddressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
                var jwtService = Resolve<IJwtService>();
                var utility = Resolve<Utility>();
                var token = await jwtService!.GenerateAccessToken(new LoginRequestDto { PhoneNumber = phoneNumber });
                var refreshToken = jwtService.GenerateRefreshToken();

                _tokenDto.Token = token;
                _tokenDto.RefreshToken = refreshToken;
                //user = DecodeStoreCreditAndLoyaltyPointOfAccount(user);
                _tokenDto.Account = _mapper.Map<AccountResponse>(user);
                var customerInfoAddressDb = await customerInfoAddressRepository.GetAllDataByExpression(c => c.AccountId.Equals(user.Id) && !c.IsDeleted, 0, 0, null, false, null);
                if (customerInfoAddressDb.Items.Count() > 0)
                {
                    _tokenDto.Account.Addresses = customerInfoAddressDb.Items;
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
                    {
                        throw new Exception($"Vai trò với id {role} không tồn tại");
                    }

                if (!BuildAppActionResultIsError(result))
                    foreach (var role in roleId)
                    {
                        var roleDb = await identityRoleRepository!.GetById(role);
                        var resultCreateRole = await _userManager.AddToRoleAsync(user, roleDb.NormalizedName);
                        if (!resultCreateRole.Succeeded)
                        {
                            throw new Exception($"Cấp quyền với vai trò {role} không thành công");
                        }
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
                {
                    throw new Exception($"Người dùng với {userId} không tồn tại");
                }
                foreach (var role in roleId)
                    if (await identityRoleRepository.GetById(role) == null)
                    {
                        throw new Exception($"Vai trò với {role} không tồn tại");
                    }

                if (!BuildAppActionResultIsError(result))
                    foreach (var role in roleId)
                    {
                        var roleDb = await identityRoleRepository!.GetById(role);
                        var resultCreateRole = await _userManager.RemoveFromRoleAsync(user!, roleDb.NormalizedName);
                        if (!resultCreateRole.Succeeded)
                        {
                            throw new Exception($"Xóa quyền {role} thất bại");
                        }
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
                var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();

                var rolePriority = new[] { "ADMIN", "CHEF", "SHIPPER", "DEVICE", "CUSTOMER" };

                var normalizedRoleName = roleName.ToUpper();

                if (!rolePriority.Contains(normalizedRoleName))
                {
                    throw new Exception($"Invalid role: {roleName}");
                }

                var role = await roleRepository!.GetByExpression(r => r.NormalizedName == normalizedRoleName);
                if (role == null)
                {
                    throw new Exception($"Role not found: {roleName}");
                }

                var roleId = role.Id;
                List<string> accountIds = new List<string>();

                var usersWithRole = await userRoleRepository!.GetAllDataByExpression(ur => ur.RoleId == roleId, 0, 0, null, false, null);

                foreach (var userRole in usersWithRole.Items)
                {
                    bool hasHigherPriorityRole = false;

                    foreach (var higherRole in rolePriority.TakeWhile(r => r != normalizedRoleName))
                    {
                        var higherRoleEntity = await roleRepository.GetByExpression(r => r.NormalizedName == higherRole);
                        if (higherRoleEntity != null)
                        {
                            var hasHigherRole = await userRoleRepository.GetByExpression(ur => ur.UserId == userRole.UserId && ur.RoleId == higherRoleEntity.Id);
                            if (hasHigherRole != null)
                            {
                                hasHigherPriorityRole = true;
                                break;
                            }
                        }
                    }

                    if (!hasHigherPriorityRole)
                    {
                        accountIds.Add(userRole.UserId);
                    }
                }

                var accountDb = await _accountRepository.GetAllDataByExpression(a => accountIds.Contains(a.Id), pageNumber, pageSize, null, false, null);
                accountDb.Items = DecodeStoreCreditAndLoyaltyPointOfAccount(accountDb.Items);
                result.Result = accountDb;
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
                        accountDb.Items = DecodeStoreCreditAndLoyaltyPointOfAccount(accountDb.Items);
                        result.Result = accountDb;
                    }
                }
                else
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy vai trò với id {Id}");
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
                var utility = Resolve<Utility>();
                var hashingService = Resolve<IHashingService>();
                if (!utility.IsValidPhoneNumberInput(phoneNumber))
                {
                    return BuildAppActionResultError(result, "Số điện thoại không hợp lệ. Vui lòng kiểm tra lại.");
                    return result;
                }
                var user = await _accountRepository.GetByExpression(p => p.PhoneNumber == phoneNumber && p.IsDeleted == false);
                if (user == null)
                {
                    throw new Exception($"Số điện thoại này không tồn tại!");
                }

                var optUser = JsonConvert.DeserializeObject<OTP>(user.OTP);
                if (optUser == null)
                {
                    throw new Exception($"Mã Otp không tồn tại!");
                }
                if (optUser.Code != optCode)
                {
                    throw new Exception($"Mã Otp không đúng!");
                }

                if (optUser.Type != OTPType.ConfirmPhone)
                {
                    throw new Exception($"Mã Otp không thuộc loại Xác nhận số điện thoại!");
                }

                if (!BuildAppActionResultIsError(result))
                {
                    result = await LoginDefault(phoneNumber, user);
                    user.IsVerified = true;
                    user.OTP = "";
                    await _accountRepository.Update(user);
                    await _unitOfWork.SaveChangesAsync();
                }
                user = hashingService.GetDecodedAccount(user);
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
                    return BuildAppActionResultError(result, $"Không tìm thấy thông tin người dùng với id {customerDb.Id}");
                    return result;
                }

                if (customerDb.IsVerified && otpType == OTPType.Register)
                {
                    return BuildAppActionResultError(result, $"Thông tin người dùng đã được xác thực");
                    return result;
                }
                Random random = new Random();
                string code = random.Next(100000, 999999).ToString();
                var utility = Resolve<Utility>();

                var otpsDb = new OTP
                {
                    Type = otpType,
                    Code = code,
                    ExpiredTime = utility.GetCurrentDateTimeInTimeZone().AddMinutes(5),
                };
                customerDb.OTP = JsonConvert.SerializeObject(otpsDb);
                await _accountRepository.Update(customerDb);
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
        //            return BuildAppActionResultError(result, $"Xảy ra lỗi khi tìm thông tin người dùng với sđt {phoneNumber}");
        //            return result;
        //        }

        //        if (customerListDb.Items.Count == 0)
        //        {
        //            return BuildAppActionResultError(result, $"Không tìm thấy thông tin người dùng với sđt {phoneNumber}");
        //            return result;
        //        }
        //        var customerDb = customerListDb.Items[0];
        //        if (otpType == OTPType.Register)
        //        {
        //            if (customerDb.IsVerified)
        //            {
        //                return BuildAppActionResultError(result, $"Thông tin người dùng đã được xác thực");
        //                return result;
        //            }
        //        }
        //        else
        //        {
        //            if (!customerDb.IsVerified)
        //            {
        //                return BuildAppActionResultError(result, $"Thông tin người dùng đã chưa được xác thực");
        //                return result;
        //            }
        //        }

        //        var otpRepository = Resolve<IGenericRepository<OTP>>();
        //        var utility = Resolve<Utility>();
        //        var availableOTPDb = await otpRepository.GetAllDataByExpression(o => o.Account.PhoneNumber == phoneNumber && !o.IsUsed && o.ExpiredTime > utility.GetCurrentDateTimeInTimeZone() && o.Type == otpType, 0, 0, null, false, null);
        //        if (availableOTPDb.Items.Count > 1)
        //        {
        //            return BuildAppActionResultError(result, $"Xảy ra lỗi vì có nhiều hơn mã OTP hữu dụng tồn tại trong hệ thống. Vui lòng thử lại sau");
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
        //            throw new Exception($"Tạo OTP thất bại. Vui lòng thử lại");
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
            await _unitOfWork.ExecuteInTransaction(async () =>
            {
                try
                {
                    var mapService = Resolve<IMapService>();
                    var firebaseService = Resolve<IFirebaseService>();
                    var accountDb =
                        await _accountRepository.GetByExpression(p => p.Id == updateAccountRequest.AccountId);
                    if (accountDb == null)
                    {
                        throw new Exception(
                            $"Tài khoản với số điện thoại {updateAccountRequest.AccountId} không tồn tại!");
                    }

                    accountDb.FirstName = updateAccountRequest.FirstName;
                    accountDb.LastName = updateAccountRequest.LastName;
                    accountDb.DOB = updateAccountRequest.DOB;
                    accountDb.Gender = updateAccountRequest.Gender;
                    accountDb.IsManuallyCreated = false;

                    if (updateAccountRequest.Image != null)
                    {
                        var pathName = SD.FirebasePathName.ACCOUNT_PREFIX +
                                       $"{updateAccountRequest.AccountId}{Guid.NewGuid()}.jpg";
                        var upload = await firebaseService!.UploadFileToFirebase(updateAccountRequest.Image!, pathName);

                        if (!upload.IsSuccess)
                        {
                            throw new Exception("Upload hình ảnh không thành công");
                        }

                        accountDb!.Avatar = upload.Result!.ToString()!;

                        if (!upload.IsSuccess)
                        {
                            throw new Exception("Upload hình ảnh không thành công");
                        }
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        await _accountRepository.Update(accountDb);
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

        public async Task<AppActionResult> DeleteAccount(string customerId)
        {
            var result = new AppActionResult();
            try
            {
                var customerInfoDb = await _accountRepository.GetByExpression(p => p.Id == customerId.ToString());
                if (customerInfoDb == null)
                {
                    throw new Exception($"Thông tin và địa chỉ của khách với id {customerId} không tồn tại");
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
                var utility = Resolve<Utility>();
                if (!utility.IsValidPhoneNumberInput(phoneNumber))
                {
                    return BuildAppActionResultError(result, "Số điện thoại không hợp lệ. Vui lòng kiểm tra lại.");
                    return result;
                }
                var user = await _accountRepository.GetByExpression(p => p.PhoneNumber == phoneNumber && p.IsDeleted == false);
                if (user == null)
                {
                    throw new Exception($"Số điện thoại này không tồn tại!");
                }

                var optUser = JsonConvert.DeserializeObject<OTP>(user.OTP);
                if (optUser == null)
                {
                    throw new Exception($"Mã Otp không tồn tại!");
                }

                if (optUser.Type == OTPType.VerifyForReservation)
                {
                    throw new Exception($"Mã Otp không tồn tại!");
                }

                if (optUser.ExpiredTime < utility.GetCurrentDateTimeInTimeZone())
                {
                    throw new Exception($"Mã Otp không tồn tại!");
                }

                if (optUser.ExpiredTime < utility.GetCurrentDateTimeInTimeZone())
                {
                    throw new Exception($"Mã Otp này đã được sử dụng!");
                }

                if (optUser.Code != code)
                {
                    throw new Exception($"Mã Otp không đúng!");
                }

                if (!BuildAppActionResultIsError(result))
                {
                    user.OTP = "";
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
                var utility = Resolve<Utility>();
                if (!utility.IsValidPhoneNumberInput(phoneNumber))
                {
                    return BuildAppActionResultError(result, "Số điện thoại không hợp lệ. Vui lòng kiểm tra lại.");
                    return result;
                }
                var user = await _accountRepository.GetByExpression(p => p.PhoneNumber == phoneNumber, null);
                if (user == null)
                {
                    throw new Exception($"Số điện thoại này không tồn tại!");
                }

                var optUser = JsonConvert.DeserializeObject<OTP>(user.OTP);

                if (optUser == null)
                {
                    throw new Exception($"Mã Otp không tồn tại!");
                }

                if (optUser.Type != otpType)
                {
                    throw new Exception($"Loại OTP không phù hợp");
                }

                if (optUser.ExpiredTime < utility.GetCurrentDateTimeInTimeZone())
                {
                    throw new Exception($"Mã Otp này đã hết hạn");
                }


                 if (optUser.Type != OTPType.Register && !user.IsVerified)
                {
                    throw new Exception($"Mã Otp này đã hết hạn");
                }


                 if (optUser.Type != OTPType.Register && !user.IsVerified)
                {
                    throw new Exception($"Thông tin người dùng chưa được xác thực");
                }
                 if (optUser.Code != code)
                {
                    throw new Exception($"Mã Otp không đúng!");
                }

                if (!BuildAppActionResultIsError(result))
                {
                    if (otpType == OTPType.Register)
                    {
                        user.IsVerified = true;
                    }
                    user.OTP = "";
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

        public async Task<AppActionResult> GetAccountByPhoneNumber(string phoneNumber)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
                var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
                var listRole = await roleRepository!.GetAllDataByExpression(null, 1, 100, null, false, null);
                var customerInfoDb = await _accountRepository.GetByExpression(c => c.PhoneNumber.Equals(phoneNumber));
                customerInfoDb = DecodeStoreCreditAndLoyaltyPointOfAccount(customerInfoDb);
                var listMap = _mapper.Map<AccountResponse>(customerInfoDb);

                if (customerInfoDb == null)
                {
                    throw new Exception($"Không tìm thấy thông tin người dùng với số điện thoại {phoneNumber}");
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
                var currentTime = utility!.GetCurrentDateTimeInTimeZone();
                var accountDb = await _accountRepository!.GetAllDataByExpression(null, 0, 0, null, false, null);
                if (accountDb!.Items!.Count > 0)
                {
                    OTP accountOtp = null;
                    foreach(var account in accountDb.Items)
                    {
                        if (!string.IsNullOrEmpty(account.OTP))
                        {
                            accountOtp = JsonConvert.DeserializeObject<OTP>(account.OTP);
                            if(!string.IsNullOrEmpty(accountOtp.Code) && account.ExpiredDate > currentTime)
                            {
                                account.OTP = "";
                            }
                        }
                    }
                    await _accountRepository.UpdateRange(accountDb.Items);
                    await _unitOfWork.SaveChangesAsync();
                }
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
            await _unitOfWork.ExecuteInTransaction(async () =>
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
                        var mainAddressDb = await customerInfoAddressRepository!.GetAllDataByExpression(p =>
                            p.AccountId == customerInfoAddressRequest.AccountId && p.IsCurrentUsed == true, 0, 0, null, false, null);
                        if (mainAddressDb.Items.Count > 0)
                        {
                            
                            mainAddressDb.Items.ForEach(a => a.IsCurrentUsed =  false);
                            await customerInfoAddressRepository.UpdateRange(mainAddressDb.Items);
                        }

                        var accountDb =
                            await accountRepository.GetByExpression(p => p.Id == customerInfoAddressRequest.AccountId);
                        accountDb.Address = newCustomerInfoAddress.CustomerInfoAddressName;
                        await accountRepository.Update(accountDb);
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        await customerInfoAddressRepository!.Insert(newCustomerInfoAddress);
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

        public async Task<AppActionResult> UpdateCustomerInfoAddress(UpdateCustomerInforAddressRequest updateCustomerInforAddress)
        {
            var result = new AppActionResult();
            var customerInfoAddressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
            var accountRepository = Resolve<IGenericRepository<Account>>();
            await _unitOfWork.ExecuteInTransaction(async () =>
            {
                try
                {
                    var customerInfoDb = await customerInfoAddressRepository!.GetByExpression(p =>
                        p.CustomerInfoAddressId == updateCustomerInforAddress.CustomerInfoAddressId);
                    if (customerInfoDb == null)
                    {
                        throw new Exception($"Không tìm thấy địa chỉ với id {updateCustomerInforAddress.AccountId}");
                    }

                    customerInfoDb.CustomerInfoAddressName = updateCustomerInforAddress.CustomerInfoAddressName;
                    customerInfoDb.AccountId = updateCustomerInforAddress.AccountId;
                    customerInfoDb.IsCurrentUsed = updateCustomerInforAddress.IsCurrentUsed;
                    customerInfoDb.Lat = updateCustomerInforAddress.Lat;
                    customerInfoDb.Lng = updateCustomerInforAddress.Lng;

                    if (updateCustomerInforAddress.IsCurrentUsed == true)
                    {
                        var mainAddressDb = await customerInfoAddressRepository!.GetAllDataByExpression(p =>
                            p.AccountId == updateCustomerInforAddress.AccountId && p.IsCurrentUsed == true, 0,0, null, false, null);
                        if (mainAddressDb.Items.Count > 0)
                        {
                            var accountDb =
                                await accountRepository.GetByExpression(p =>
                                    p.Id == updateCustomerInforAddress.AccountId);
                            accountDb.Address = updateCustomerInforAddress.CustomerInfoAddressName;
                            mainAddressDb.Items.ForEach(o => o.IsCurrentUsed = false);
                            await customerInfoAddressRepository.UpdateRange(mainAddressDb.Items);
                            await accountRepository.Update(accountDb);
                        }
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        await customerInfoAddressRepository!.Update(customerInfoDb);
                        //await customerInfoAddressRepository.Insert(newCustomerAddress);
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

        public async Task<AppActionResult> DeleteCustomerInfoAddress(Guid customerInfoAddressId)
        {
            var result = new AppActionResult();
            var customerInfoAddressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
            try
            {
                var customerInfoAddressDb = await customerInfoAddressRepository!.GetByExpression(p => p.CustomerInfoAddressId == customerInfoAddressId);
                if (customerInfoAddressDb == null)
                {
                    throw new Exception($"Không tìm thấy địa chỉ khách hàng với id {customerInfoAddressId}");
                }
                if (customerInfoAddressDb.IsCurrentUsed == true)
                {
                    throw new Exception($"Không thể xóa địa chỉ đang sử dụng, hãy sử dụng địa chỉ khác");
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
                    throw new Exception($"Không có thông tin về vai trò shipper");
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

        public async Task<AppActionResult> UpdateDeliveringStatus(string accountId, bool isDelivering)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
                var roleRepository = Resolve<IGenericRepository<IdentityRole>>();

                var shipperRoleDb = await roleRepository.GetByExpression(r => r.Name.Equals("SHIPPER"));
                if (shipperRoleDb == null)
                {
                    throw new Exception($"Không có thông tin về vai trò shipper");
                }
                var userRoleDb = await userRoleRepository.GetByExpression(u => u.RoleId == shipperRoleDb.Id && u.UserId.Equals(accountId));
                if (userRoleDb == null)
                {
                    throw new Exception($"Không tồn tại shipper với id {accountId}");
                }
                var shipperDb = await _accountRepository.GetByExpression(a => a.IsVerified && !a.IsDeleted && userRoleDb.UserId.Equals(a.Id));
                if (shipperDb != null)
                {
                    shipperDb.IsDelivering = isDelivering;
                    await _accountRepository.Update(shipperDb);
                    await _unitOfWork.SaveChangesAsync();
                }
                else
                {
                    throw new Exception($"Không tồn tại shipper với id {accountId}");
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
                    throw new Exception($"Tài khoản với id {accountId} không tồn tại");
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
                var utility = Resolve<Utility>();
                var accountDb = await _accountRepository.GetById(accountId);
                if (accountDb == null)
                {
                    throw new Exception($"Tài khoản với id {accountId} không tồn tại");
                }

                var existedAccountWithEmail = await _accountRepository.GetByExpression(a => a.Email.Equals(email.ToLower()) && !a.IsDeleted && !a.IsBanned, null);
                if(existedAccountWithEmail != null)
                {
                    return BuildAppActionResultError(result, $"Tồn tại tài khoản với email {email}");
                }

                var currentTime = utility!.GetCurrentDateTimeInTimeZone();
                var otp = JsonConvert.DeserializeObject<OTP>(accountDb.OTP);
                if (otp == null)
                {
                    return BuildAppActionResultError(result, $"Mã OTP không tồn tại");
                }
                else
                {
                    if(otp.Code == otpCode && accountDb.Id.Equals(accountId) && otp.Type == OTPType.ConfirmEmail && otp.ExpiredTime > currentTime)
                    {
                        accountDb.Email = email;
                        accountDb.NormalizedEmail = email.ToUpper();
                        accountDb.UserName = email;
                        accountDb.NormalizedUserName = email.ToUpper();
                        accountDb.OTP = "";
                    } else 
                    {
                        return BuildAppActionResultError(result, $"Mã OTP không tồn tại");
                    }
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

        public async Task<AppActionResult> UpRole(string accountId, string roleName)
        {
            var result = new AppActionResult();
            var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
            try
            {
                if (roleName.Equals("CHEF") || roleName.Equals("SHIPPER"))
                {
                    var accountDb = await _userManager.FindByIdAsync(accountId);

                    var roleDb = await roleRepository!.GetByExpression(p => p.Name == roleName);
                    if (roleDb == null)
                    {
                        throw new Exception($"Không tìm thấy vai trò {roleName}");
                    }

                    var userRoleDb = await _userRoleRepository!.GetAllDataByExpression(p => p.UserId == accountDb.Id, 0, 0, null, false, null);
                    var userRole = userRoleDb.Items!.FirstOrDefault();
                    if (userRole == null)
                    {
                        throw new Exception($"Không tìm thấy tài khoản với id {userRole.UserId} vai trò");
                    }

                    var currentRoles = await _userManager.GetRolesAsync(accountDb);

                    await _userManager.RemoveFromRolesAsync(accountDb, currentRoles);
                    await _userManager.AddToRoleAsync(accountDb, roleName);
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> BanUser(string accountId)
        {
            var result = new AppActionResult();
            try
            {
                var accountDb = await _accountRepository.GetById(accountId);
                if (accountDb == null)
                {
                    throw new Exception($"Không tìm thấy tài khoản với id {accountId}");
                }

                if (accountDb.IsBanned == true)
                {
                    accountDb.IsBanned = false;
                }
                else
                {
                    accountDb.IsBanned = true;
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

        public async Task<AppActionResult> CreateAccountForRestaurantEmployees(EmployeeSignUpRequest signUpRequestDto, bool isGoogle)
        {
            var result = new AppActionResult();
            var utility = Resolve<Utility>();
            var jwtService = Resolve<IJwtService>();
            var tokenRepository = Resolve<IGenericRepository<Token>>();
            var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
            await _unitOfWork.ExecuteInTransaction(async () =>
            {
                try
                {
                    if (await _accountRepository.GetByExpression(r => r!.PhoneNumber == signUpRequestDto.PhoneNumber) !=
                        null)
                    {
                        throw new Exception("Số điện thoại đã tồn tại!");
                    }

                    if (signUpRequestDto.RoleName == SD.RoleName.ROLE_CUSTOMER ||
                        signUpRequestDto.RoleName == SD.RoleName.ROLE_ADMIN)
                    {
                        throw new Exception($"Bạn không có quyền hạn tạo tài khoản");
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        var emailService = Resolve<IEmailService>();
                        var smsService = Resolve<ISmsService>();
                        var currentTime = utility.GetCurrentDateTimeInTimeZone();

                        var user = new Account
                        {
                            Id = Guid.NewGuid().ToString(),
                            Email = signUpRequestDto.Email,
                            UserName = signUpRequestDto.PhoneNumber,
                            FirstName = signUpRequestDto.FirstName,
                            LastName = signUpRequestDto.LastName,
                            PhoneNumber = signUpRequestDto.PhoneNumber,
                            Gender = signUpRequestDto.Gender,
                            IsVerified = isGoogle ? true : false,
                            IsManuallyCreated = true,
                            RegisteredDate = utility.GetCurrentDateInTimeZone()
                            
                        };

                        var loyaltyPoint = _hashingService.Hashing(user.Id, 0, true);
                        if (loyaltyPoint.IsSuccess)
                        {
                            user.LoyaltyPoint = loyaltyPoint.Result.ToString();
                        } else
                        {
                            user.LoyaltyPoint = "";
                        }

                        var storeCredit = _hashingService.Hashing(user.Id, 0, false);
                        if (storeCredit.IsSuccess)
                        {
                            user.StoreCreditAmount = "";
                        }

                        if ((!string.IsNullOrEmpty(signUpRequestDto.Email) &&
                             !string.IsNullOrEmpty(signUpRequestDto.PhoneNumber) ||
                             (!string.IsNullOrEmpty(signUpRequestDto.Email))))
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
                                    Type = OTPType.Login,
                                    Code = verifyCode,
                                    ExpiredTime = utility.GetCurrentDateTimeInTimeZone().AddMinutes(5),
                                };

                                user.OTP = JsonConvert.SerializeObject(otpsDb);
                                await _userManager.CreateAsync(user);
                            }
                        }

                        var roleDb = await roleRepository.GetByExpression(p => p.Name == signUpRequestDto.RoleName);
                        if (roleDb == null)
                        {
                            throw new Exception($"Không tìm thấy vai trò với tên {signUpRequestDto.RoleName}");
                        }

                        var resultCreateRole = await _userManager.AddToRoleAsync(user, roleDb.Name);
                        if (!resultCreateRole.Succeeded)
                           throw new Exception($"Cấp quyền nhân viên không thành công");

                        if (!BuildAppActionResultIsError(result))
                        {
                            await _unitOfWork.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
            });

            return result;
        }

        public async Task<AppActionResult> GetAccountByPhoneNumberKeyword(string phoneNumber, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            try
            {
                var accountDb = await _accountRepository.GetAllDataByExpression(p => p.PhoneNumber.Contains(phoneNumber), pageNumber, pageSize, null, false, null);
                accountDb.Items = DecodeStoreCreditAndLoyaltyPointOfAccount(accountDb.Items);
                var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
                var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
                var listRole = await roleRepository!.GetAllDataByExpression(null, 1, 100, null, false, null);
                var listMap = _mapper.Map<List<AccountResponse>>(accountDb.Items);
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
                    { Items = listMap, TotalPages = accountDb.TotalPages };
                return result;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> IsExistAccount(string phoneNumber)
        {
            AppActionResult result = new AppActionResult();
            try { 
            
             result.Result = await _accountRepository.GetByExpression(p => p.PhoneNumber.Equals(phoneNumber.Trim()));
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        private List<Account> DecodeStoreCreditAndLoyaltyPointOfAccount(List<Account> accounts)
        {
            try
            {
                var storeCreditResult = new AppActionResult();
                var loyaltyPointResult = new AppActionResult();
                foreach (var account in accounts)
                {
                    storeCreditResult = _hashingService.UnHashing(account.StoreCreditAmount, false);
                    if (storeCreditResult.IsSuccess)
                    {
                        account.StoreCreditAmount = storeCreditResult.Result.ToString().Split('_')[1];
                    }

                    loyaltyPointResult = _hashingService.UnHashing(account.LoyaltyPoint, true);
                    if (loyaltyPointResult.IsSuccess)
                    {
                        account.LoyaltyPoint = loyaltyPointResult.Result.ToString().Split('_')[1];
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return accounts;
        }
        private Account DecodeStoreCreditAndLoyaltyPointOfAccount(Account account)
        {
            try
            {
                var storeCreditResult = new AppActionResult();
                var loyaltyPointResult = new AppActionResult();
                storeCreditResult = _hashingService.UnHashing(account.StoreCreditAmount, false);
                if (storeCreditResult.IsSuccess)
                {
                    account.StoreCreditAmount = storeCreditResult.Result.ToString().Split('_')[1];
                }

                loyaltyPointResult = _hashingService.UnHashing(account.LoyaltyPoint, true);
                if (loyaltyPointResult.IsSuccess)
                {
                    account.LoyaltyPoint = loyaltyPointResult.Result.ToString().Split('_')[1];
                }
            }
            catch (Exception ex)
            {
            }
            return account;
        }

        public async Task<AppActionResult> GetDemoAccountOTP()
        {
            var result = new AppActionResult();
            try
            {
                List<string> phones = new List<string>
                {
                    "945507865",
                    "366967957",
                    "984135344",
                    "389867608",
                    "966537537"
                };
                var list = new PagedResult<Account>();
                var customerInfoAddressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
                list = await _accountRepository.GetAllDataByExpression(p => phones.Contains(p.PhoneNumber), 0, 0, null, false, null);
                

                list.Items = DecodeStoreCreditAndLoyaltyPointOfAccount(list.Items);

                var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
                var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
                var listRole = await roleRepository!.GetAllDataByExpression(null, 1, 100, null, false, null);
                var listMap = _mapper.Map<List<AccountResponse>>(list.Items);
                foreach (var item in listMap)
                {
                    var userRole = new List<IdentityRole>();

                    var customerInfoAddressDb = await customerInfoAddressRepository!.GetAllDataByExpression(p => p.AccountId == item.Id, 0, 0, null, false, null);
                    var role = await userRoleRepository!.GetAllDataByExpression(a => a.UserId == item.Id, 1, 100, null, false, null);
                    foreach (var itemRole in role.Items!)
                    {
                        var roleUser = listRole.Items!.ToList().FirstOrDefault(a => a.Id == itemRole.RoleId);
                        if (roleUser != null) userRole.Add(roleUser);
                    }

                    item.Roles = userRole;
                    var roleNameList = userRole.DistinctBy(i => i.Id).Select(i => i.Name).ToList();
                    item.Addresses = customerInfoAddressDb.Items;

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
            catch (Exception ex)
            {
            }
            return result;
        }
    }
}