using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IHubServices;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        public IOrderService _orderService;
        private IHubServices _hubServices;
        public TestController(IOrderService orderService, IHubServices hubServices)
        {
            _orderService = orderService;
            _hubServices = hubServices;
        }

        [HttpPut("update-order-status")]
        public async Task<AppActionResult> UpdateOrderStatus(Guid orderId, OrderStatus orderStatus)
        {
            return await _orderService.UpdateOrderStatus(orderId, orderStatus);
        }

        [HttpPost("trigger")]
        public async Task<IActionResult> TriggerAction()
        {
            await _hubServices.SendAsync("Test");
            return Ok();
        }
    }
}
