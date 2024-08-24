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
        private IGenericRepository<Coupon> _couponRepository;
        private IUnitOfWork _unitOfWork;

        public CouponService(IServiceProvider serviceProvider, IGenericRepository<Coupon> couponRepository, IUnitOfWork unitOfWork) : base(serviceProvider)
        {
            _couponRepository = couponRepository;   
            _unitOfWork = unitOfWork;   
        }

        public async Task<AppActionResult> ApplyCoupon(Guid orderId)
        {
            var result = new AppActionResult();
            var orderRepository = Resolve<IGenericRepository<Order>>();
            var customerSavedCouponRepository = Resolve<IGenericRepository<CustomerSavedCoupon>>();
            var utility = Resolve<Utility>();
            try
            {
                var orderDb = await orderRepository!.GetAllDataByExpression(
                    p => p.OrderId == orderId, 0, 0, null, false, 
                    p => p.CustomerInfo!.Account!,
                    p => p.PaymentMethod!,
                    p => p.CustomerSavedCoupon!,
                    p => p.Reservation!,
                    p => p.Reservation!,
                    p => p.LoyalPointsHistory!
                    );
                var couponId = orderDb.Items!.FirstOrDefault()!.CustomerSavedCoupon!.CouponId;
                var couponDb = await _couponRepository.GetById(couponId);
                if (couponDb.ExpiryDate > utility!.GetCurrentDateTimeInTimeZone())
                {
                    result = BuildAppActionResultError(result, $"Mã giảm giá với id {couponId} đã hết hạn");
                }
                var discount = (orderDb.Items!.FirstOrDefault()!.TotalAmount * couponDb.DiscountPercent) / 100;
                orderDb.Items!.FirstOrDefault()!.TotalAmount -= discount;
                var customerSavedCouponDb = orderDb.Items!.FirstOrDefault()!.CustomerSavedCoupon;
                customerSavedCouponDb!.IsUsedOrExpired = true;

                await customerSavedCouponRepository!.Update(customerSavedCouponDb);
                await orderRepository.Update(orderDb.Items!.FirstOrDefault()!);

                result.Result = orderDb;
                result.Messages.Add("Áp dụng mã giảm giá thành công");
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> CreateCoupon(CouponDto couponDto)
        {
            var result = new AppActionResult(); 
            try
            {
                var couponDb = await _couponRepository.GetByExpression(p => p.Code == couponDto.Code);
                var firebaseService = Resolve<IFirebaseService>();
                var staticFileRepository = Resolve<IGenericRepository<StaticFile>>();
                if (couponDb != null)
                {
                    result = BuildAppActionResultError(result, $"Mã giảm giá với code {couponDto.Code} đã tồn tại");
                }
                var coupon = new Coupon
                {
                    CouponId = Guid.NewGuid(),
                    Code = couponDto.Code,  
                    DiscountPercent = couponDto.DiscountPercent,    
                    ExpiryDate = couponDto.ExpiryDate,  
                    MinimumAmount = couponDto.MinimumAmount,    
                    Quantity = couponDto.Quantity,  
                    StartDate = couponDto.StartDate,    
                };

                var pathName = SD.FirebasePathName.COUPON_PREFIX + $"{coupon.CouponId}{Guid.NewGuid()}.jpg";
                var upload = await firebaseService!.UploadFileToFirebase(couponDto.File, pathName);
                couponDto.Img = pathName;
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

        public async Task<AppActionResult> GetAllAvailableCoupon(DateTime startTime, DateTime endTime,int pageNumber, int pageSize)
        { 
            var result = new AppActionResult();     
            try
            {
                var couponDb = await _couponRepository.GetAllDataByExpression(p => p.StartDate < startTime && p.ExpiryDate > endTime, pageNumber, pageSize, p => p.ExpiryDate, false, null);
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
    }
}
