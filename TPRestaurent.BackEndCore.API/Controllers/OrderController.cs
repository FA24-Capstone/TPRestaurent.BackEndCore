using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;
        public OrderController(IOrderService service)
        {
            _service = service;
        }

        [HttpGet("get-order-by-account-id/{accountId}")]
        public async Task<AppActionResult> GetAllOrderByAccountId(string accountId, Domain.Enums.OrderStatus? status, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllOrderByAccountId(accountId, status, 1, 10);
        }

        [HttpGet("get-order-detail/{orderId}")]
        public async Task<AppActionResult> GetOrderDetail(Guid orderId)
        {
            return await _service.GetOrderDetail(orderId);
        }

        [HttpPost("add-dish-to-order/{orderId}")]
        public async Task<AppActionResult> AddDishToOrder(AddDishToOrderRequestDto dto)
        {
            return await _service.AddDishToOrder(dto);
        }

    }
}
