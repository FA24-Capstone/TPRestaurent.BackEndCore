using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("order-session")]
    [ApiController]
    public class OrderSessionController : ControllerBase
    {
        private IOrderSessionService _service;

        public OrderSessionController(IOrderSessionService service)
        {
            _service = service;
        }

        [HttpGet("get-all-order-session")]
        public async Task<AppActionResult> GetAllOrderSession(OrderSessionStatus? status, int pageNumber, int pageSize)
        {
            return await _service.GetAllOrderSession(status, pageNumber, pageSize);
        }

        [HttpGet("get-order-session-by-id")]
        public async Task<AppActionResult> GetOrderSessionById(Guid id)
        {
            return await _service.GetOrderSessionById(id);
        }

        [HttpPut("update-order-session-status")]
        public async Task<AppActionResult> UpdateOrderSessionStatus(Guid id, OrderSessionStatus status)
        {
            return await _service.UpdateOrderSessionStatus(id, status, true);
        }

        [HttpGet("get-grouped-dish")]
        public async Task<AppActionResult> GetGroupedDish()
        {
            return await _service.GetGroupedDish(null);
        }
    }
}