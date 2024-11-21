﻿using NPOI.Util;
using System;
using System.Collections.Generic;
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
                if(currentTime > couponProgramDb.ExpiryDate)
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
                    Img = couponDto.File
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
                var couponDb = await _couponProgramRepository.GetAllDataByExpression(p => p.ExpiryDate >= currentTime && p.Quantity > 0 && p.IsDeleted == false, pageNumber, pageSize, p => p.ExpiryDate, false, p => p.CreateByAccount);
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
                                                                                    pageNumber, pageSize, p => p.CouponProgram.DiscountPercent, false, p => p.Account!, p => p.CouponProgram);
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
            try
            {
                var accountDb = await acocuntRepository!.GetAllDataByExpression(null, 0, 0, null, false, null);
                if (accountDb!.Items!.Count > 0 && accountDb.Items != null)
                {
                    foreach (var account in accountDb.Items)
                    {
                        if (account.UserRankId == UserRank.DIAMOND)
                        {
                            account.UserRankId = UserRank.GOLD;
                        }
                        else if (account.UserRankId == UserRank.GOLD)
                        {
                            account.UserRankId = UserRank.SILVER;
                        }
                        else if (account.UserRankId == UserRank.SILVER)
                        {
                            account.UserRankId = UserRank.BRONZE;
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

                if (!string.IsNullOrEmpty(updateCouponDto.Tittle))
                {
                    couponDb.Title = updateCouponDto.Tittle;
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

                if (updateCouponDto.ImageFile != null)
                {
                   couponDb.Img = updateCouponDto.ImageFile;    
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
    }
}
