using Microsoft.AspNetCore.Identity;
using NPOI.SS.Formula.Functions;
using NPOI.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class CouponService : GenericBackendService, ICouponService
    {
        private IGenericRepository<CouponProgram> _couponProgramRepository;
        private IGenericRepository<Coupon> _couponRepository;
        private IUnitOfWork _unitOfWork;

        public CouponService(IServiceProvider serviceProvider, IGenericRepository<CouponProgram> couponProgramRepository, IGenericRepository<Coupon> couponRepository, IUnitOfWork unitOfWork) : base(serviceProvider)
        {
            _couponProgramRepository = couponProgramRepository;
            _couponRepository = couponRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AppActionResult> AssignCoupon(AssignCouponRequestDto couponDto)
        {
            var result = new AppActionResult();
            try
            {
                var utility = Resolve<Utility>();
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var couponProgramDb = await _couponProgramRepository.GetByExpression(p => p.CouponProgramId == couponDto.CouponProgramId);
                if (couponProgramDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy mã giảm giá với id {couponDto.CouponProgramId}");
                }

                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                if (currentTime > couponProgramDb.ExpiryDate)
                {
                    return BuildAppActionResultError(result, $"Mã giảm giá {couponProgramDb.Code} đã hết hạn");
                }

                if (couponProgramDb.Quantity < couponDto.CustomerIds.Count)
                {
                    return BuildAppActionResultError(result, $"Số lượng mã giảm giá còn lại không đủ");
                }

                List<Coupon> coupons = new List<Coupon>();
                Account accountDb = null;
                foreach (var customerId in couponDto.CustomerIds)
                {
                    accountDb = await accountRepository.GetById(customerId);
                    if (accountDb == null)
                    {
                        return BuildAppActionResultError(result, $"Không tìm thấy khách hàng với id {accountDb}");
                    }

                    coupons.Add(new Coupon
                    {
                        AccountId = customerId,
                        CouponId = Guid.NewGuid(),
                        CouponProgramId = couponDto.CouponProgramId,
                        IsUsedOrExpired = false
                    });
                }

                couponProgramDb.Quantity -= couponDto.CustomerIds.Count;

                await _couponProgramRepository.Update(couponProgramDb);
                await _couponRepository.InsertRange(coupons);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> AssignCouponToUserWithRank(AssignCouponToRankRequest dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var utility = Resolve<Utility>();
                var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                List<Coupon> couponList = new List<Coupon>();
                if (dto.BronzeCouponProgramIds != null && dto.BronzeCouponProgramIds.Count > 0)
                {
                    var bronzeCouponList = await GetCouponListForCreation(dto.BronzeCouponProgramIds, UserRank.BRONZE, currentTime);
                    if(bronzeCouponList.Count == 0)
                    {
                        return BuildAppActionResultError(result, $"Có các chương trình giảm giá không hợp lệ cho hạng Đồng");
                    }
                    couponList.AddRange(bronzeCouponList);
                }

                if (dto.SilverCouponProgramIds != null && dto.SilverCouponProgramIds.Count > 0)
                {
                    var silverCouponList = await GetCouponListForCreation(dto.SilverCouponProgramIds, UserRank.SILVER, currentTime);
                    if (silverCouponList.Count == 0)
                    {
                        return BuildAppActionResultError(result, $"Có các chương trình giảm giá không hợp lệ cho hạng Bạc");
                    }
                    couponList.AddRange(silverCouponList);

                }

                if (dto.GoldCouponProgramIds != null && dto.GoldCouponProgramIds.Count > 0)
                {
                    var goldCouponList = await GetCouponListForCreation(dto.GoldCouponProgramIds, UserRank.GOLD, currentTime);
                    if (goldCouponList.Count == 0)
                    {
                        return BuildAppActionResultError(result, $"Có các chương trình giảm giá không hợp lệ cho hạng Bạc");
                    }
                    couponList.AddRange(goldCouponList);
                }

                if (dto.DiamondCouponProgramIds != null && dto.DiamondCouponProgramIds.Count > 0)
                {
                    var diamondCouponList = await GetCouponListForCreation(dto.DiamondCouponProgramIds, UserRank.DIAMOND, currentTime);
                    if (diamondCouponList.Count == 0)
                    {
                        return BuildAppActionResultError(result, $"Có các chương trình giảm giá không hợp lệ cho hạng Bạc");
                    }
                    couponList.AddRange(diamondCouponList);
                }

                await _couponRepository.InsertRange(couponList);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        private async Task<List<Coupon>> GetCouponListForCreation(List<Guid> couponProgramIds, UserRank rank, DateTime currentTime)
        {
            List<Coupon> result = new List<Coupon>();
            try
            {
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var couponProgramDb = await _couponProgramRepository.GetAllDataByExpression(c => couponProgramIds.Contains(c.CouponProgramId)
                                                                                                           && c.UserRankId == UserRank.BRONZE
                                                                                                           && !c.IsDeleted && c.ExpiryDate > currentTime
                                                                                                           , 0, 0, null, false, null);
                if (couponProgramDb.Items.Count != couponProgramIds.Count)
                {
                    return result;
                }

                var customerIds = await GetCustomerId();
                var accountDb = await accountRepository.GetAllDataByExpression(a => customerIds.Contains(a.Id) && a.UserRankId == UserRank.BRONZE && !a.IsBanned && !a.IsDeleted, 0, 0, null, false, null);
                if (accountDb.Items.Count > 0)
                {
                    foreach (var couponProgram in couponProgramDb.Items)
                    {
                        accountDb.Items.ForEach(c => result.Add(new Coupon
                        {
                            CouponId = Guid.NewGuid(),
                            CouponProgramId = couponProgram.CouponProgramId,
                            AccountId = c.Id,
                            IsUsedOrExpired = false,
                            OrderId = null
                        }));
                        couponProgram.Quantity -= accountDb.Items.Count;
                    }
                    await _couponProgramRepository.UpdateRange(couponProgramDb.Items);
                }
            }
            catch (Exception ex)
            {
                result = new List<Coupon>();
            }
            return result;
        }

        public async Task<AppActionResult> CreateCouponProgram(CouponProgramDto couponDto)
        {
            var result = new AppActionResult();
            try
            {
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var couponDb = await _couponProgramRepository.GetByExpression(p => p.Code == couponDto.Code);
                var firebaseService = Resolve<IFirebaseService>();
                var staticFileRepository = Resolve<IGenericRepository<Image>>();
                if (couponDb != null)
                {
                    return BuildAppActionResultError(result, $"Mã giảm giá với code {couponDto.Code} đã tồn tại");
                }

                var accountDb = await accountRepository.GetById(couponDto.AccountId);
                if (accountDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy tài khoản với id {couponDto.AccountId}");
                }

                var coupon = new CouponProgram
                {
                    CouponProgramId = Guid.NewGuid(),
                    Code = couponDto.Code,
                    Title = couponDto.Title,
                    DiscountPercent = couponDto.DiscountPercent,
                    CouponProgramTypeId = couponDto.CouponProgramType,
                    ExpiryDate = couponDto.ExpiryDate,
                    MinimumAmount = couponDto.MinimumAmount,
                    Quantity = couponDto.Quantity,
                    StartDate = couponDto.StartDate,
                    CreateBy = couponDto.AccountId,
                    IsDeleted = false,
                    Img = couponDto.File,
                    UserRankId = couponDto.UserRank
                };


                await _couponProgramRepository.Insert(coupon);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> DeleteCouponProgram(Guid couponId)
        {
            var result = new AppActionResult();
            try
            {
                var couponDb = await _couponProgramRepository.GetById(couponId);
                if (couponDb == null)
                {
                    return BuildAppActionResultError(result, $"Mã khuyến mãi với id {couponId} không tồn tại");
                }

                couponDb.IsDeleted = true;
                await _couponProgramRepository.Update(couponDb);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAllAvailableCouponProgram(int pageNumber, int pageSize)
        {
            var utility = Resolve<Utility>();
            var result = new AppActionResult();
            var currentTime = utility!.GetCurrentDateTimeInTimeZone();
            try
            {
                var couponDb = await _couponProgramRepository.GetAllDataByExpression(p => p.ExpiryDate >= currentTime && p.Quantity > 0 && p.IsDeleted == false, pageNumber, pageSize, p => p.ExpiryDate, false, p => p.CreateByAccount, p => p.UserRank, p => p.CouponProgramType);
                result.Result = couponDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAvailableCouponByAccountId(string accountId, double? total, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            var utility = Resolve<Utility>();
            var currentTime = utility!.GetCurrentDateTimeInTimeZone();
            try
            {
                var couponDb = await _couponRepository.GetAllDataByExpression(p => p.AccountId == accountId && p.CouponProgram.ExpiryDate >= currentTime
                                                                                    && (!total.HasValue || total.Value >= p.CouponProgram.MinimumAmount)
                                                                                    && !p.IsUsedOrExpired && p.CouponProgram.IsDeleted == false,
                                                                                    pageNumber, pageSize, p => p.CouponProgram.DiscountPercent, false, p => p.Account!, p => p.CouponProgram, p => p.CouponProgram.UserRank, p => p.CouponProgram.CouponProgramType);
                result.Result = couponDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        [Hangfire.Queue("get-birthday-user-for-coupon")]
        public async Task GetBirthdayUserForCoupon()
        {
            var utility = Resolve<Utility>();
            var accountRepository = Resolve<IGenericRepository<Account>>();
            var currentTime = utility!.GetCurrentDateTimeInTimeZone();
            var listCouponBirthday = new List<Coupon>();
            var emailService = Resolve<IEmailService>();
            try
            {
                var couponDb = await _couponProgramRepository!.GetByExpression(p => p.CouponProgramTypeId == CouponProgramType.BIRTHDAY);
                var accountDb = await accountRepository!.GetAllDataByExpression(p => p.DOB.Value.Month == currentTime.Month, 0, 0, null, false, null);
                if (accountDb!.Items!.Count > 0 && accountDb.Items != null)
                {
                    foreach (var account in accountDb.Items)
                    {
                        var coupon = new Coupon
                        {
                            CouponId = Guid.NewGuid(),
                            AccountId = account.Id,
                            CouponProgramId = couponDb.CouponProgramId,
                            IsUsedOrExpired = false
                        };
                        listCouponBirthday.Add(coupon);
                        
                        var username = account.FirstName + " " + account.LastName;
                        emailService.SendEmail(account.Email, SD.SubjectMail.NOTIFY_RESERVATION,
                             TemplateMappingHelper.GetTemplateBirthdayCoupon(username, couponDb)
                           );
                    }

                    await _couponRepository.InsertRange(listCouponBirthday);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
            }
            Task.CompletedTask.Wait();
        }

        public async Task<AppActionResult> GetCouponProgramById(Guid couponId)
        {
            var result = new AppActionResult();
            try
            {
                var couponDb = await _couponProgramRepository.GetById(couponId);
                result.Result = couponDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetRanks()
        {
            var result = new AppActionResult();
            var configurationRepository = Resolve<IGenericRepository<Configuration>>();
            var listRank = new List<Configuration>();
            try
            {
                var bronzeRankDb = await configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.BRONZE_RANK);
                var silverRankDb = await configurationRepository.GetByExpression(p => p.Name == SD.DefaultValue.SILVER_RANK);
                var goldRankDb = await configurationRepository.GetByExpression(p => p.Name == SD.DefaultValue.GOLD_RANK);
                var diamondRankDb = await configurationRepository.GetByExpression(p => p.Name == SD.DefaultValue.DIAMOND_RANK);

                listRank.Add(bronzeRankDb);
                listRank.Add(silverRankDb);
                listRank.Add(goldRankDb);
                listRank.Add(diamondRankDb);

                result.Result = listRank;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetUserByRank(UserRank userRank)
        {
            var result = new AppActionResult();
            var accountRepository = Resolve<IGenericRepository<Account>>();
            try
            {
                var userByRankDb = await accountRepository!.GetAllDataByExpression(p => p.UserRankId == userRank, 0, 0, null, false, null);
                if (userByRankDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy người dùng với rank {userRank}");
                }
                result.Result = userByRankDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        [Hangfire.Queue("remove-expired-coupon")]
        public async Task RemoveExpiredCoupon()
        {
            var utility = Resolve<Utility>();
            var currentTime = utility.GetCurrentDateTimeInTimeZone();
            try
            {
                var expiredCouponProgramDb = await _couponProgramRepository.GetAllDataByExpression(p => p.ExpiryDate < currentTime, 0, 0, null, false, null);
                if (expiredCouponProgramDb.Items.Count > 0 && expiredCouponProgramDb.Items != null)
                {
                    expiredCouponProgramDb.Items.ForEach(e => e.IsDeleted = true);
                    var couponDb = await _couponRepository.GetAllDataByExpression(c => expiredCouponProgramDb.Items.Select(e => e.CouponProgramId).Contains(c.CouponProgramId), 0, 0, null, false, null);
                    couponDb.Items.ForEach(c => c.IsUsedOrExpired = true);
                    await _couponProgramRepository.UpdateRange(expiredCouponProgramDb.Items);
                    await _couponRepository.UpdateRange(couponDb.Items);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
            }
            Task.CompletedTask.Wait();
        }

        [Hangfire.Queue("reset-user-rank")]
        public async Task ResetUserRank()
        {
            var acocuntRepository = Resolve<IGenericRepository<Account>>();
            var orderRepository = Resolve<IGenericRepository<Order>>();
            var configurationRepository = Resolve<IGenericRepository<Configuration>>();
            try
            {
                var accountDb = await acocuntRepository!.GetAllDataByExpression(null, 0, 0, null, false, null);
                var bronzeRankDb = await configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.BRONZE_RANK);
                var silverRankDb = await configurationRepository.GetByExpression(p => p.Name == SD.DefaultValue.SILVER_RANK);
                var goldRankDb = await configurationRepository.GetByExpression(p => p.Name == SD.DefaultValue.GOLD_RANK);
                var diamondRankDb = await configurationRepository.GetByExpression(p => p.Name == SD.DefaultValue.DIAMOND_RANK);
                if (accountDb!.Items!.Count > 0 && accountDb.Items != null)
                {
                    foreach (var account in accountDb.Items)
                    {
                        var orderDb = await orderRepository!.GetAllDataByExpression(p => p.AccountId == account.Id, 0, 0, null, false, null);
                        var totalAmount = orderDb.Items!.Sum(p => p.TotalAmount);
                        if (account.UserRankId == UserRank.DIAMOND)
                        {
                            if (totalAmount <= double.Parse(diamondRankDb.CurrentValue))
                            {
                                account.UserRankId = UserRank.GOLD;
                            }
                        }
                        else if (account.UserRankId == UserRank.GOLD)
                        {
                            if (totalAmount <= double.Parse(goldRankDb.CurrentValue))
                            {
                                account.UserRankId = UserRank.SILVER;
                            }
                        }
                        else if (account.UserRankId == UserRank.SILVER)
                        {
                            if (totalAmount <= double.Parse(silverRankDb.CurrentValue))
                            {
                                account.UserRankId = UserRank.BRONZE;
                            }
                        }
                    }

                    await acocuntRepository.UpdateRange(accountDb.Items);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
            }
            Task.CompletedTask.Wait();
        }

        public async Task<AppActionResult> UpdateCouponProgram(UpdateCouponProgramDto updateCouponDto)
        {
            var result = new AppActionResult();
            var firebaseService = Resolve<IFirebaseService>();
            try
            {
                var couponDb = await _couponProgramRepository.GetById(updateCouponDto.CouponProgramId);
                if (couponDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy coupon với id {updateCouponDto.CouponProgramId} không tồn tại");
                }

                if (updateCouponDto.Quantity.HasValue)
                {
                    couponDb.Quantity = updateCouponDto.Quantity.Value;
                }

                if (!string.IsNullOrEmpty(updateCouponDto.Code))
                {
                    couponDb.Code = updateCouponDto.Code;
                }

                if (!string.IsNullOrEmpty(updateCouponDto.Title))
                {
                    couponDb.Title = updateCouponDto.Title;
                }

                if (updateCouponDto.DiscountPercent.HasValue)
                {
                    couponDb.DiscountPercent = updateCouponDto.DiscountPercent.Value;
                }

                if (updateCouponDto.StartDate.HasValue)
                {
                    couponDb.StartDate = updateCouponDto.StartDate.Value;
                }

                if (updateCouponDto.ExpiryDate.HasValue)
                {
                    couponDb.ExpiryDate = updateCouponDto.ExpiryDate.Value;
                }

                if (updateCouponDto.MinimumAmount.HasValue)
                {
                    couponDb.MinimumAmount = updateCouponDto.MinimumAmount.Value;
                }

                if (updateCouponDto.CouponProgramType != null)
                {
                    couponDb.CouponProgramTypeId = updateCouponDto.CouponProgramType.Value;
                }
                if (updateCouponDto.UserRank.HasValue)
                {
                    couponDb.UserRankId = updateCouponDto.UserRank;
                }

                if (updateCouponDto.ImageFile != null)
                {
                    couponDb.Img = updateCouponDto.ImageFile;
                }
                if (!string.IsNullOrEmpty(updateCouponDto.AccountId))
                {
                    couponDb.UpdateBy =  updateCouponDto.AccountId;     
                }

                await _couponProgramRepository.Update(couponDb);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task AssignCouponToUserWithRank()
        {
            try
            {
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var emailService = Resolve<IEmailService>();
                var customerIds = await GetCustomerId(); 
                var couponList = new List<Coupon>();    
                var accountDb = await accountRepository.GetAllDataByExpression(a => customerIds.Contains(a.Id) && !a.IsBanned && !a.IsDeleted && a.IsVerified, 0, 0, null, false, null);
                var bronzeCoupons = await GetCouponListForCreation(accountDb.Items.Where(a => a.UserRankId == UserRank.BRONZE).Select(a => a.Id).ToList(), UserRank.BRONZE, currentTime, emailService);
                if(bronzeCoupons.Count > 0)
                {
                    couponList.AddRange(bronzeCoupons);
                }

                var silverCoupons = await GetCouponListForCreation(accountDb.Items.Where(a => a.UserRankId == UserRank.SILVER).Select(a => a.Id).ToList(), UserRank.SILVER, currentTime, emailService);
                if (silverCoupons.Count > 0)
                {
                    couponList.AddRange(silverCoupons);
                }

                var goldCoupons = await GetCouponListForCreation(accountDb.Items.Where(a => a.UserRankId == UserRank.GOLD).Select(a => a.Id).ToList(), UserRank.GOLD, currentTime, emailService);
                if (goldCoupons.Count > 0)
                {
                    couponList.AddRange(goldCoupons);
                }

                var diamondCoupons = await GetCouponListForCreation(accountDb.Items.Where(a => a.UserRankId == UserRank.DIAMOND).Select(a => a.Id).ToList(), UserRank.DIAMOND, currentTime, emailService);
                if (diamondCoupons.Count > 0)
                {
                    couponList.AddRange(diamondCoupons);
                }

                await _couponRepository.InsertRange(couponList);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
        }

        private async Task<List<Coupon>> GetCouponListForCreation(List<string> accountIds, UserRank rank, DateTime currentTime, IEmailService emailService)
        {
            List<Coupon> result = new List<Coupon>();
            try
            {
                var couponProgramDb = await _couponProgramRepository.GetAllDataByExpression(c => c.ExpiryDate.Date.Month == currentTime.Month 
                                                                                                 && c.ExpiryDate.Date.Year == currentTime.Year
                                                                                                 && !c.IsDeleted && c.UserRankId == rank
                                                                                                 , 0, 0, null, false, c => c.CreateByAccount);
                var insufficientCouponProgramList = couponProgramDb.Items.Where(c => c.Quantity < accountIds.Count).ToList();
                if(insufficientCouponProgramList.Count != couponProgramDb.Items.Count)
                {

                    var html = GetInSufficientCouponProgramHtml(rank.ToString(), accountIds.Count, insufficientCouponProgramList);
                    emailService!.SendEmail(insufficientCouponProgramList.FirstOrDefault().CreateByAccount.Email, SD.SubjectMail.INSUFFICIENT_COUPON_QUANTITY,
                                         TemplateMappingHelper.GetTemplateOTPEmail(
                                             TemplateMappingHelper.ContentEmailType.INSUFFICIENT_COUPON_QUANTITY, html,
                                             insufficientCouponProgramList.FirstOrDefault().CreateByAccount.LastName));
                    return result;
                }

                foreach (var couponProgram in couponProgramDb.Items)
                {
                    accountIds.ForEach(a => result.Add(
                        new Coupon
                        {
                            CouponId = Guid.NewGuid(),
                            AccountId = a,
                            CouponProgramId = couponProgram.CouponProgramId,
                            IsUsedOrExpired = false,
                            OrderId = null
                        }
                    ));
                    couponProgram.Quantity -= accountIds.Count;
                }
                await _couponProgramRepository.UpdateRange(couponProgramDb.Items);
            }
            catch (Exception ex)
            {
                result = new List<Coupon>();
            }
            return result;
        }

        private async Task<List<string>> GetCustomerId()
        {
            List<string> customerIds = new List<string>();
            try
            {
                var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
                var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
                var customerRoleDb = await roleRepository.GetByExpression(r => r.Name.ToLower().Equals("customer"));
                var customerDb = await userRoleRepository.GetAllDataByExpression(null, 0, 0, null, false, null);
                var roleGroup = customerDb.Items.GroupBy(c => c.UserId).ToDictionary(c => c.Key, c => c.ToList());
                customerIds = roleGroup.Where(c => c.Value.Count == 1 && c.Value.FirstOrDefault().RoleId.Equals(customerRoleDb.Id)).Select(c => c.Key).ToList();
            }
            catch (Exception ex)
            {

            }
            return customerIds;
        }

        private string GetInSufficientCouponProgramHtml(string rank, int quantity, List<CouponProgram> couponPrograms)
        {
            var sb = new StringBuilder();
            sb.AppendLine($@"<p>Hạng {rank}</p>");
            sb.AppendLine($@"<p>Số lượng yêu cầu: {quantity}</p>");
            sb.AppendLine(@"<ul class=""couponList"">");

            foreach (var coupon in couponPrograms)
            {
                var quantityStatus = coupon.Quantity > 0
                    ? $"Số lượng hiện tại: {coupon.Quantity}"
                    : "Số lượng hiện tại: 0";

                sb.AppendLine($@"  <li>{coupon.Code}: {quantityStatus}</li>");
            }

            sb.AppendLine("</ul>");
            return sb.ToString();
        }
    }
}
