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
        public Task<AppActionResult> GetAllOrderByCustomertId(string accountId, Domain.Enums.OrderStatus? status, OrderType? orderType, int pageNumber, int pageSize);
        public Task<AppActionResult> GetAllOrderByStatus(Domain.Enums.OrderStatus? status, OrderType? orderType, int pageNumber, int pageSize);
        public Task<AppActionResult> GetAllOrderByPhoneNumber(string phoneNumber, OrderStatus? status, OrderType? orderType, int pageNumber, int pageSize);
        public Task<AppActionResult> GetOrderDetail(Guid orderId);
        public Task<AppActionResult> GetOrderByTime(double? minute, int pageNumber, int pageSize);
        public Task<AppActionResult> CalculateReservation(ReservationDto request);
        public Task<AppActionResult> CalculateDeliveryOrder(Guid customerInfoAddressId);
        public Task<AppActionResult> CreateOrder(OrderRequestDto orderRequestDto);
        public Task<AppActionResult> MakeDineInOrderBill(OrderPaymentRequestDto orderRequestDto);
        public Task<AppActionResult> ChangeOrderStatus(Guid orderId, bool IsSuccessful);
        public Task<AppActionResult> AddDishToOrder(AddDishToOrderRequestDto dto);
        public Task<AppActionResult> SuggestTable(SuggestTableDto dto);
        //public Task<AppActionResult> GetOrderTotal(CalculateOrderRequest orderRequestDto);
        //public Task<AppActionResult> GetOrderJsonByTableSessionId(Guid TableSessionId);
        public Task<AppActionResult> GetTableReservationWithTime(Guid tableId, DateTime? time);
        public Task CancelOverReservation();
        public Task UpdateOrderStatusBeforeMealTime();
        public Task UpdateOrderDetailStatusBeforeDining();
        public Task<AppActionResult> GetUpdateCartComboDto(string cartComboJson);
        public Task<AppActionResult> GetUpdateCartDishDto(string cartDishJson);
        public Task<AppActionResult> UpdateOrderDetailStatus(List<Guid> orderDetailIds, bool isSuccessful);
        public Task<AppActionResult> GetCurrentTableSession();
        public Task<AppActionResult> GetAllReservationDetail(Guid orderId);
        public Task<AppActionResult> GetAllTableDetails(OrderStatus orderStatus, int pageNumber, int pageSize);
        public Task<AppActionResult> AssignOrderForShipper(string shipperId,  List<Guid> orderListId);
        public Task<AppActionResult> UploadConfirmedOrderImage(ConfirmedOrderRequest confirmedOrderRequest);
        public Task CancelReservation();
    }
}
