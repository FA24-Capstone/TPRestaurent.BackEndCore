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

        public Task<AppActionResult> GetAllOrderByShipperId(string shipperId, Domain.Enums.OrderStatus? orderStatus, int pageNumber, int pageSize);

        public Task<AppActionResult> GetOrderByTime(double? minute, int pageNumber, int pageSize);

        public Task<AppActionResult> CalculateReservation(ReservationDto request);

        public Task<AppActionResult> CalculateDeliveryOrder(Guid customerInfoAddressId);

        public Task<AppActionResult> CreateOrder(OrderRequestDto orderRequestDto);

        public Task<AppActionResult> MakeDineInOrderBill(OrderPaymentRequestDto orderRequestDto);

        public Task<AppActionResult> ChangeOrderStatus(Guid orderId, bool IsSuccessful, OrderStatus? status, bool? asCustomer, bool? requireSignalR = true);
        public Task<AppActionResult> ChangeOrderStatusService(Guid orderId, bool IsSuccessful, OrderStatus? status, bool? asCustomer, bool? requireSignalR = true);
        public Task<AppActionResult> AddDishToOrder(AddDishToOrderRequestDto dto);

        //public Task<AppActionResult> SuggestTable(SuggestTableDto dto);
        //public Task<AppActionResult> GetOrderTotal(CalculateOrderRequest orderRequestDto);
        //public Task<AppActionResult> GetOrderJsonByTableSessionId(Guid TableSessionId);
        public Task<AppActionResult> GetTableReservationWithTime(Guid tableId, DateTime? time);

        public Task CancelOverReservation();

        public Task NotifyReservationDishToKitchen();

        public Task AccountDailyReservationDish();

        public Task<AppActionResult> GetUpdateCartComboDto(ComboChoice cartComboJson);

        public Task<AppActionResult> GetUpdateCartDishDto(List<CartDishItem> cartDishJson);

        public Task<AppActionResult> UpdateOrderDetailStatus(List<UpdateOrderDetailItemRequest> orderDetailIds, bool isSuccessful);

        public Task<AppActionResult> GetCurrentTableSession();

        public Task<AppActionResult> GetAllOrderDetail(Guid orderId);

        public Task<AppActionResult> GetAllTableDetails(OrderStatus orderStatus, int pageNumber, int pageSize);

        public Task<AppActionResult> AssignOrderForShipper(string shipperId, List<Guid> orderListId);

        public Task<AppActionResult> UploadConfirmedOrderImage(ConfirmedOrderRequest confirmedOrderRequest);

        public Task<AppActionResult> UpdateOrderStatus(Guid orderId, Domain.Enums.OrderStatus status);

        public Task<AppActionResult> UpdateOrderDetailStatusForce(List<OrderDetail> orderDetails, OrderDetailStatus status);

        public Task<AppActionResult> GetOrderWithFilter(ReservationTableRequest request);

        //public Task CancelReservation();
        public Task RemindOrderReservation();

        public Task<AppActionResult> GetNumberOfOrderByStatus(OrderFilterRequest request);

        public Task<AppActionResult> CancelDeliveringOrder(CancelDeliveringOrderRequest cancelDeliveringOrderRequest);

        public Task<AppActionResult> GetBestSellerDishesAndCombo(int topNumber, DateTime? startTime, DateTime? endTime);

        public Task<AppActionResult> GetAllOrderDetailByAccountId(string accountId, int feedbackStatus, int pageNumber, int pageSize);

        public Task<AppActionResult> UpdateCancelledOrderDishQuantity(Order order, List<DishSizeDetail> updateDishSizeDetailList, DateTime currentTime, bool refillAllow = true);
        public Task<AppActionResult> CancelOrderDetailBeforeCooking(List<Guid> orderDetailIds);

        public Task CancelOrder();
    }
}