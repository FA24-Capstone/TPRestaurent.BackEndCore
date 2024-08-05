using AutoMapper;
using Castle.DynamicProxy.Generators;
using Firebase.Auth;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.FormulaExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IGenericRepository<CustomerInfo> _customerInfoRepository;
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
            IGenericRepository<CustomerInfo> customerInfoRepository
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
            _customerInfoRepository = customerInfoRepository;   
        }

        public async Task<AppActionResult> Login(LoginRequestDto loginRequest)
        {
            var result = new AppActionResult();
            try
            {
                var currentTime = DateTime.Now; 
                var user = await _accountRepository.GetByExpression(u =>
                    u!.PhoneNumber!.ToLower() == loginRequest.PhoneNumber.ToLower() && u.IsDeleted == false);
                var otpCodeDb = await _otpRepository.GetByExpression(p => p.Code == loginRequest.OTPCode && p.Type == OTPType.Login);
                if (user == null)
                    result = BuildAppActionResultError(result, $" {loginRequest.PhoneNumber} này không tồn tại trong hệ thống");
                else if (user.IsVerified == false)
                    result = BuildAppActionResultError(result, "Tài khoản này chưa được xác thực !");
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
                var passwordSignIn =
                    await _signInManager.PasswordSignInAsync(loginRequest.PhoneNumber, loginRequest.Password, false, false);
                if (!passwordSignIn.Succeeded) result = BuildAppActionResultError(result, "Đăng nhâp thất bại");
                if (!BuildAppActionResultIsError(result)) result = await LoginDefault(loginRequest.PhoneNumber, user);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
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
                        IsVerified = isGoogle ? true : false
                    };
                    var resultCreateUser = await _userManager.CreateAsync(user, signUpRequest.Password);
                    if (resultCreateUser.Succeeded)
                    {
                        result.Result = user;
                        if (!isGoogle)
                            emailService!.SendEmail(user.Email, SD.SubjectMail.VERIFY_ACCOUNT,
                                TemplateMappingHelper.GetTemplateOTPEmail(
                                    TemplateMappingHelper.ContentEmailType.VERIFICATION_CODE, verifyCode,
                                    user.FirstName));
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
                var customer = new CustomerInfo
                {
                    CustomerId = Guid.NewGuid(),
                    Name = user.LastName + " " + user.FirstName,
                    PhoneNumber = user.PhoneNumber,
                    Address = "",
                    AccountId = user.Id,
                    LoyaltyPoint = 0
                };
                var customerRepository = Resolve<IGenericRepository<CustomerInfo>>();
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

        public async Task<AppActionResult> UpdateAccountPhoneNumber(UpdateAccountPhoneNumberRequestDto accountRequest)
        {
            var result = new AppActionResult();
            try
            {
                var currentTime = DateTime.Now; 
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
                var user = await _accountRepository.GetByExpression(a =>
                  a!.PhoneNumber == phoneNumber && a.IsDeleted == false );
                if (user == null) result = BuildAppActionResultError(result, "Tài khoản không tồn tại hoặc chưa được xác thực");
                if (!BuildAppActionResultIsError(result))
                {
                    var smsService = Resolve<ISmsService>();
                    var code = await GenerateVerifyCodeSms(user!.PhoneNumber, true);
                    var response = await smsService!.SendMessage($"Mã xác thực của bạn là là: {code}",
                    phoneNumber);
                    var optsDb = new OTP
                    {
                        OTPId = Guid.NewGuid(),
                        Type = otp,
                        AccountId = user.Id,
                        Code = code,
                        ExpiredTime = DateTime.UtcNow.AddMinutes(5),
                        IsUsed = false,
                    };
                    await _otpRepository.Insert(optsDb);
                    await _unitOfWork.SaveChangesAsync();
                }
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

                item.Role = userRole;
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
                var currentTime = DateTime.Now; 
                var otpCodeDb = await _otpRepository.GetByExpression(p => p.Code == changePasswordDto.OTPCode && p.Type == OTPType.ChangePassword);
                if (await _accountRepository.GetByExpression(c =>
                        c!.PhoneNumber == changePasswordDto.PhoneNumber && c.IsDeleted == false) == null)
                    result = BuildAppActionResultError(result,
                        $"Tài khoản có số điện thoại {changePasswordDto.PhoneNumber} không tồn tại!");
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
                    var user = await _accountRepository.GetByExpression(c =>
                        c!.PhoneNumber == changePasswordDto.PhoneNumber && c.IsDeleted == false);
                    var changePassword = await _userManager.ChangePasswordAsync(user!, changePasswordDto.OldPassword,
                        changePasswordDto.NewPassword);
                    if (!changePassword.Succeeded)
                        result = BuildAppActionResultError(result, "Thay đổi mật khẩu thất bại");
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
                var user = await _accountRepository.GetByExpression(a =>
                    a!.PhoneNumber == dto.PhoneNumber && a.IsDeleted == false && a.IsVerified == true);
                var userOtp = await _otpRepository.GetByExpression(p => p!.AccountId == user!.Id && p.Type == OTPType.ForgotPassword);
                DateTime currentTime = DateTime.Now;
                if (user == null)
                    result = BuildAppActionResultError(result, "Tài khoản không tồn tại hoặc chưa được xác thực!");
                else if (userOtp!.Code != dto.RecoveryCode)
                    result = BuildAppActionResultError(result, "Mã xác nhận sai");
                else if (userOtp.IsUsed == true)
                    result = BuildAppActionResultError(result, "Mã xác nhận đã sử dụng");
                else if (userOtp.ExpiredTime < currentTime)
                    result = BuildAppActionResultError(result, "Mã xác nhận đã hết hạn sử dụng");

                if (!BuildAppActionResultIsError(result))
                {
                    await _userManager.RemovePasswordAsync(user!);
                    var addPassword = await _userManager.AddPasswordAsync(user!, dto.NewPassword);
                    if (addPassword.Succeeded)
                        user!.VerifyCode = null;
                    else
                        result = BuildAppActionResultError(result, "Thay đổi mật khẩu thất bại. Vui lòng thử lại");
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
                            code, user.FirstName!));
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
                a!.PhoneNumber == phoneNumber && a.IsDeleted == false && a.IsVerified == isForForgettingPassword);

            if (user != null)
            {
                var random = new Random();
                code = random.Next(100000, 999999).ToString();
                user.VerifyCode = code;
                var smsService = Resolve<ISmsService>();
                var response = await smsService!.SendMessage($"Mã xác thực tại nhà hàng TP là: {code}",
                    phoneNumber);
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
            }else if (roleNameList.Contains("STAFF"))
            {
                _tokenDto.MainRole = "STAFF";
            }
            else if (roleNameList.Contains("CHEF"))
            {
                _tokenDto.MainRole = "CHEF";
            }
            else if (roleNameList.Count > 0)
            {
                _tokenDto.MainRole = roleNameList.FirstOrDefault(n => !n.Equals("CUSTOMER"));
            }
            else
            {
                _tokenDto.MainRole = "CUSTOMER";
            }

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
            var response = await smsService!.SendMessage($"Mã xác thực tại nhà hàng TP là: {code}",
                phoneNumber);

            if (response.IsSuccess)
            {
                result.Result = code;
            }
            else
            {
                foreach (var error in response.Messages)
                {
                    result.Messages.Add(error);
                }
            }
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

                var optUser = await _otpRepository.GetByExpression(p => p!.Code == optCode && p.Type == OTPType.Register, p => p.Account!);
                if (optUser == null) 
                {
                    result = BuildAppActionResultError(result, $"Mã Otp không tồn tại!");
                }
                else if (optUser.IsUsed == true)
                {
                    result = BuildAppActionResultError(result, $"Mã Otp này đã được sử dụng!");
                }else if (optUser.Code != optCode && user.VerifyCode != optCode)
                {
                    result = BuildAppActionResultError(result, $"Mã Otp không đúng!");
                }

                if (!BuildAppActionResultIsError(result))
                {
                    result = await LoginDefault(phoneNumber, user);
                    user!.VerifyCode = null;
                    user.IsVerified = true;
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
                var customerInforRepository = Resolve<IGenericRepository<CustomerInfo>>();
                var account =
                   await _accountRepository.GetByExpression(p => p.PhoneNumber == request.PhoneNumber, p => p.Customer!);
                if (account == null)
                {
                    result = BuildAppActionResultError(result, $"Tài khoản với số điện thoại {request.PhoneNumber} không tồn tại!");
                }
                if (!BuildAppActionResultIsError(result))
                {
                    account!.FirstName = request.FirstName;
                    account.LastName = request.LastName;
                    account.PhoneNumber = request.PhoneNumber;
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
                var accountDb = await _accountRepository.GetByExpression(p => p.Id == customerInforRequest.AccountId);
                if (accountDb == null)
                {
                    result = BuildAppActionResultError(result, $"Tài khoản với số điện thoại {customerInforRequest.AccountId} không tồn tại!");
                }
                var customerInfor = new CustomerInfo
                {
                    CustomerId = Guid.NewGuid(),
                    Name = customerInforRequest.Name,       
                    PhoneNumber = customerInforRequest.PhoneNumber,
                    AccountId = customerInforRequest.AccountId,
                    Address = customerInforRequest.Address,
                };

                await _customerInfoRepository.Insert(customerInfor);
                await _unitOfWork.SaveChangesAsync();   
                result.Result = customerInfor;  
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
                var accountDb = await _accountRepository.GetByExpression(p => p.Id == customerInforRequest.AccountId);
                if (accountDb == null)
                {
                    result = BuildAppActionResultError(result, $"Tài khoản với số điện thoại {customerInforRequest.AccountId} không tồn tại!");
                }
                var updateCustomerInfo = await _customerInfoRepository.GetByExpression(p => p.CustomerId == customerInforRequest.CustomerId);
                if (updateCustomerInfo == null)
                {
                    result = BuildAppActionResultError(result, $"Thông tin và địa chỉ của {customerInforRequest.CustomerId} không tồn tại!");
                }

                updateCustomerInfo.Address = customerInforRequest.Address;  
                updateCustomerInfo.PhoneNumber = customerInforRequest.PhoneNumber;  
                updateCustomerInfo.Name = customerInforRequest.Name;

                await _customerInfoRepository.Update(updateCustomerInfo);
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
                var customerInforList = await _customerInfoRepository.GetAllDataByExpression(p => p.AccountId == accountId, pageNumber, pageSize, null, false, null);
                customerInfoResponse.Account = accountDb;
                customerInfoResponse.CustomerInfo = customerInforList.Items!;
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
                var customerInfoDb = await _customerInfoRepository.GetByExpression(p => p.CustomerId == customerId, a => a.Account!);
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
                var customerInfoDb = await _customerInfoRepository.GetByExpression(p => p.CustomerId == customerId, a => a.Account!);
                if (customerInfoDb == null)
                {
                    result = BuildAppActionResultError(result, $"Thông tin và địa chỉ của khách với id {customerId} không tồn tại");
                }
                await _customerInfoRepository.DeleteById(customerId);
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

    }
}
