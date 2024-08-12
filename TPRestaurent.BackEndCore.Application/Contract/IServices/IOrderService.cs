using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IOrderService
    {
        public Task<AppActionResult> GetAllOrderByAccountId(string accountId, Domain.Enums.OrderStatus? status, int pageNumber, int paeSize);
        public Task<AppActionResult> GetOrderDetail(Guid orderId);
        public Task<AppActionResult> CreateOrder(OrderRequestDto orderRequestDto);
        public Task<AppActionResult> CompleteOrder(OrderPaymentRequestDto orderRequestDto);
        public Task<AppActionResult> ChangeOrderStatus(Guid orderId, bool IsSuccessful);
        public Task<AppActionResult> AddDishToOrder(AddDishToOrderRequestDto dto);
        public Task<AppActionResult> GetOrderTotal(CalculateOrderRequest orderRequestDto);
    }
}
