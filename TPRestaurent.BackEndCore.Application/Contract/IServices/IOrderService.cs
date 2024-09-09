using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface IOrderService
    {
        public Task<AppActionResult> GetAllOrderByAccountId(string accountId, Domain.Enums.OrderStatus? status, OrderType? orderType, int pageNumber, int pageSize);
        public Task<AppActionResult> GetAllOrderByStatus(Domain.Enums.OrderStatus? status, OrderType? orderType ,int pageNumber, int pageSize);
        public Task<AppActionResult> GetAllOrderByPhoneNumber(string phoneNumber, int pageNumber, int pageSize);
        public Task<AppActionResult> GetOrderDetail(Guid orderId);
        public Task<AppActionResult> GetOrderByTime(double? minute, int pageNumber, int pageSize);
        public Task<AppActionResult> CalculateReservation(ReservationDto request);
        public Task<AppActionResult> CreateOrder(OrderRequestDto orderRequestDto, HttpContext httpContext);
        //public Task<AppActionResult> CompleteOrder(OrderPaymentRequestDto orderRequestDto);
        public Task<AppActionResult> ChangeOrderStatus(Guid orderId, bool IsSuccessful);
        public Task<AppActionResult> AddDishToOrder(AddDishToOrderRequestDto dto);
        //public Task<AppActionResult> GetOrderTotal(CalculateOrderRequest orderRequestDto);
        //public Task<AppActionResult> GetOrderJsonByTableSessionId(Guid TableSessionId);

    }
}
