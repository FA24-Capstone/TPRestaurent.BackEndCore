using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("coupon")]
    [ApiController]
    public class CouponController : ControllerBase
    {
        private ICouponService _couponService;
        public CouponController(ICouponService couponService)
        {
            _couponService = couponService; 
        }

        [HttpGet("get-available-coupon/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllAvailableCoupon(DateTime startTime, DateTime endTime, int pageNumber = 1, int pageSize = 10)
        {
            return await _couponService.GetAllAvailableCoupon(startTime, endTime, pageNumber, pageSize);    
        }

        [HttpGet("get-combo-by-id/{comboId}")]
        public async Task<AppActionResult> GetComboById(Guid comboId)
        {
            return await _couponService.GetCouponById(comboId);     
        }

        [HttpPost("create-coupon")]
        public async Task<AppActionResult> CreateCoupon(CouponDto couponDto)
        {
            return await _couponService.CreateCoupon(couponDto);        
        }
    }
}
