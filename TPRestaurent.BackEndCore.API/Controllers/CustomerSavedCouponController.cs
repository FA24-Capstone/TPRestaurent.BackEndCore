using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("customer-saved-coupon")]
    [ApiController]
    public class CustomerSavedCouponController : ControllerBase
    {
        private ICustomerSavedCouponService _service;
        public CustomerSavedCouponController(ICustomerSavedCouponService service)
        {
            _service = service;
        }

        [HttpGet("get-customer-coupon/{accountId}/{pageNumber:int}/{pageSize:int}")]
        public async Task<AppActionResult> GetAllCustomerCoupon(string accountId, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllCustomerCoupon(accountId, pageNumber, pageSize);
        }
    }
}
