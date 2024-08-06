using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("customer-loved-dish")]
    [ApiController]
    public class CustomerLovedDishController : ControllerBase
    {
        private ICustomerLovedDishService _service;
        public CustomerLovedDishController(ICustomerLovedDishService service)
        {
            _service = service;
        }

        [HttpGet("get-customer-loved-dish/{accountId}/{pageNumber:int}/{pageSize:int}")]
        public async Task<AppActionResult> GetAllCustomerLovedDish(string accountId, int pageNumber = 1, int pageSize = 10) 
        { 
            return await _service.GetAllCustomerLovedDish(accountId, pageNumber, pageSize);
        }
    }
}
