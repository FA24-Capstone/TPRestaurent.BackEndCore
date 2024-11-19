using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface ICouponService
    {
        Task<AppActionResult> GetAllAvailableCoupon(int pageNumber, int pageSize);
        Task<AppActionResult> GetCouponById(Guid couponId);
        Task<AppActionResult> GetAvailableCouponByAccountId(string accountId, int pageNumber, int pageSize);
        Task<AppActionResult> GetApplicableCoupon(double total, string accountId);
        Task<AppActionResult> CreateCoupon(CouponDto couponDto);
        Task<AppActionResult> DeleteCouponProgram(Guid couponId);
        Task<AppActionResult> UpdateCoupon(UpdateCouponDto updateCouponDto);    
        Task RemoveExpiredCoupon();
    }
}
