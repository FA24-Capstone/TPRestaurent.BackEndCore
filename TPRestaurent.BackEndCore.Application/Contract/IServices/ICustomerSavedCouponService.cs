using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface ICustomerSavedCouponService
    {
        public Task<AppActionResult> GetAllCustomerCoupon(string accountId, int pageNumber, int pageSize);
        public Task<AppActionResult> TakeCoupon(string accountId, Guid couponId);
        public Task UpdateExpiredCouponStatus();
    }
}
