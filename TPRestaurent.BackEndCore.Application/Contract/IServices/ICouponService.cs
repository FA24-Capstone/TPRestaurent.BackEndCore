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
        public Task<AppActionResult> GetAllAvailableCoupon(DateTime startTime, DateTime endTime, int pageNumber, int pageSize);
        public Task<AppActionResult> GetCouponById(Guid couponId);
        public Task<AppActionResult> ApplyCoupon(Guid orderId);
        public Task<AppActionResult> CreateCoupon(CouponDto couponDto);
    }
}
