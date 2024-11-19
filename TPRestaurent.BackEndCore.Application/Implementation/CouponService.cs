using NPOI.Util;
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
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class CouponService :  GenericBackendService, ICouponService
    {
        private IGenericRepository<CouponProgram> _couponRepository;
        private IUnitOfWork _unitOfWork;

        public CouponService(IServiceProvider serviceProvider, IGenericRepository<CouponProgram> couponRepository, IUnitOfWork unitOfWork) : base(serviceProvider)
        {
            _couponRepository = couponRepository;   
            _unitOfWork = unitOfWork;   
        }


        public async Task<AppActionResult> CreateCoupon(CouponDto couponDto)
        {
            var result = new AppActionResult(); 
            try
            {
                var couponDb = await _couponRepository.GetByExpression(p => p.Code == couponDto.Code);
                var firebaseService = Resolve<IFirebaseService>();
                var staticFileRepository = Resolve<IGenericRepository<Image>>();
                if (couponDb != null)
                {
                    result = BuildAppActionResultError(result, $"Mã giảm giá với code {couponDto.Code} đã tồn tại");
                }
                var coupon = new CouponProgram
                {
                    CouponProgramId = Guid.NewGuid(),
                    Code = couponDto.Code,  
                    DiscountPercent = couponDto.DiscountPercent,    
                    ExpiryDate = couponDto.ExpiryDate,  
                    MinimumAmount = couponDto.MinimumAmount,    
                    Quantity = couponDto.Quantity,  
                    StartDate = couponDto.StartDate,    
                };

                var pathName = SD.FirebasePathName.COUPON_PREFIX + $"{coupon.CouponProgramId}{Guid.NewGuid()}.jpg";
                var upload = await firebaseService!.UploadFileToFirebase(couponDto.File, pathName);
                coupon.Img = pathName;
                if (!upload.IsSuccess)
                {
                    return BuildAppActionResultError(result, "Upload hình ảnh không thành công");
                }
                await _couponRepository.Insert(coupon);
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
                var couponDb = await _couponRepository.GetById(couponId);
                if (couponDb == null)
                {
                    return BuildAppActionResultError(result, $"Mã khuyến mãi với id {couponId} không tồn tại");
                }

                couponDb.IsDeleted = true;
                await _couponRepository.Update(couponDb);   
                await _unitOfWork.SaveChangesAsync();       
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAllAvailableCoupon(int pageNumber, int pageSize)
        {
            var utility = Resolve<Utility>();
            var result = new AppActionResult();
            var currentTime = utility!.GetCurrentDateTimeInTimeZone();
            try
            {
                var couponDb = await _couponRepository.GetAllDataByExpression(p => p.ExpiryDate >= currentTime && p.Quantity > 0 && p.IsDeleted == false, pageNumber, pageSize, p => p.ExpiryDate, false, p => p.Account);
                 result.Result = couponDb;      
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;  
        }

        public async Task<AppActionResult> GetApplicableCoupon(double total, string accountId)
        {
            var utility = Resolve<Utility>();
            var currentTime = utility!.GetCurrentDateTimeInTimeZone();
            var result = new AppActionResult(); 
            try
            {
                var couponDb = await _couponRepository.GetAllDataByExpression(p => p.MinimumAmount <= total && p.AccountId == accountId && p.Quantity > 0 && p.IsDeleted == false && p.ExpiryDate >= currentTime, 0, 0, p => p.ExpiryDate, false, p => p.Account!);
                result.Result = couponDb;   
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAvailableCouponByAccountId(string accountId, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            var utility = Resolve<Utility>();
            var currentTime = utility!.GetCurrentDateTimeInTimeZone();
            try
            {
                var couponDb = await _couponRepository.GetAllDataByExpression(p => p.AccountId == accountId && p.ExpiryDate >= currentTime && p.Quantity > 0 && p.IsDeleted == false, pageNumber, pageSize, p => p.ExpiryDate, false, p => p.Account!);
                result.Result = couponDb;   
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetCouponById(Guid couponId)
        {
            var result = new AppActionResult();
            try
            {
                var couponDb = await _couponRepository.GetById(couponId);
                result.Result = couponDb;   
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task RemoveExpiredCoupon()
        {
            var utility = Resolve<Utility>();
            var currentTime = utility.GetCurrentDateTimeInTimeZone();
            try
            {
                var expiredCouponDb = await _couponRepository.GetAllDataByExpression(p => p.ExpiryDate < currentTime, 0, 0, null, false, null);
                if (expiredCouponDb.Items.Count > 0 && expiredCouponDb.Items != null)
                {
                    await _couponRepository.DeleteRange(expiredCouponDb.Items); 
                    await _unitOfWork.SaveChangesAsync();   
                }
            }
            catch (Exception ex)
            {
            }
            Task.CompletedTask.Wait();
        }

        public async Task<AppActionResult> UpdateCoupon(UpdateCouponDto updateCouponDto)
        {
            var result = new AppActionResult();
            var firebaseService = Resolve<IFirebaseService>();
            try
            {
                var couponDb = await _couponRepository.GetById(updateCouponDto.CouponProgramId);
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

                if (updateCouponDto.ImageFile != null)
                {
                    await firebaseService!.DeleteFileFromFirebase(couponDb.Img);
                    var pathName = SD.FirebasePathName.COUPON_PREFIX + $"{couponDb.CouponProgramId}{Guid.NewGuid()}.jpg";
                    var upload = await firebaseService!.UploadFileToFirebase(updateCouponDto.ImageFile, pathName);
                    couponDb.Img = pathName;
                    if (!upload.IsSuccess)
                    {
                        return BuildAppActionResultError(result, "Upload hình ảnh không thành công");
                    }
                }

                await _couponRepository.Update(couponDb);   
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
