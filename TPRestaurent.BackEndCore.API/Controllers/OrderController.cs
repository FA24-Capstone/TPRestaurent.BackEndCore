using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.API.Middlewares;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;

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

        [HttpGet("get-all-order-by-Status/{pageNumber}/{pageSize}")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<AppActionResult> GetAllOrderByStatus(Domain.Enums.OrderStatus? status, OrderType orderType, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllOrderByStatus(status, orderType, pageNumber, pageSize);
        }

        [HttpGet("get-order-by-account-id/{accountId}/{pageNumber}/{pageSize}")]
        [TokenValidationMiddleware(Permission.PAYMENT)]
        public async Task<AppActionResult> GetAllOrderByAccountId(string accountId, Domain.Enums.OrderStatus? status, OrderType? orderType, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllOrderByCustomertId(accountId, status, orderType, 1, 10);
        }

        [HttpGet("get-all-order-by-phone-number/{pageNumber}/{pageSize}")]
        [TokenValidationMiddleware(Permission.ALL)]
        public async Task<AppActionResult> GetAllOrderByPhoneNumber(string phoneNumber, OrderStatus? status, OrderType? orderType, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllOrderByPhoneNumber(phoneNumber, status, orderType, pageNumber, pageSize);
        }

        [HttpGet("get-order-by-time/{pageNumber}/{pageSize}")]
        [TokenValidationMiddleware(Permission.DEVICE)]
        public async Task<AppActionResult> GetOrderByTime(double? minute, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetOrderByTime(minute, pageNumber, pageSize);
        }

        [HttpGet("get-all-order-by-shipper-id/{shipperId}/{pageNumber}/{pageSize}")]
        [TokenValidationMiddleware(Permission.DELIVERY)]
        public async Task<AppActionResult> GetAllOrderByShipperId(string shipperId, OrderStatus? status, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllOrderByShipperId(shipperId, status, pageNumber, pageSize);
        }

        [HttpGet("get-order-detail/{orderId}")]
        [TokenValidationMiddleware(Permission.ALL)]
        public async Task<AppActionResult> GetOrderDetail(Guid orderId)
        {
            return await _service.GetAllOrderDetail(orderId);
        }

        [HttpPost("calculate-reservation")]
        public async Task<AppActionResult> CalculateReservation([FromBody] ReservationDto request)
        {
            return await _service.CalculateReservation(request);
        }

        [HttpPost("calculate-deliver-order")]
        [TokenValidationMiddleware(Permission.DELIVERY_WITH_CUSTOMER)]
        public async Task<AppActionResult> CalculateDeliveryOrder(Guid customerInfoAddressId)
        {
            return await _service.CalculateDeliveryOrder(customerInfoAddressId);
        }

        [HttpPost("add-dish-to-order/{orderId}")]
        [TokenValidationMiddleware(Permission.DEVICE)]
        public async Task<AppActionResult> AddDishToOrder([FromBody] AddDishToOrderRequestDto dto)
        {
            return await _service.AddDishToOrder(dto);
        }

        [HttpPost("create-order")]
        [TokenValidationMiddleware(Permission.CREATE_ORDER)]
        public async Task<AppActionResult> CreateOrder([FromBody] OrderRequestDto dto)
        {
            return await _service.CreateOrder(dto);
        }

        [HttpPost("make-dine-in-order-bill")]
        [TokenValidationMiddleware(Permission.PAYMENT)]
        public async Task<AppActionResult> MakeDineInOrderBill([FromBody] OrderPaymentRequestDto dto)
        {
            return await _service.MakeDineInOrderBill(dto);
        }

        [HttpPost("get-cart-combo-item")]
        public async Task<AppActionResult> GetCartItem([FromBody] ComboChoice cartItem)
        {
            return await _service.GetUpdateCartComboDto(cartItem);
        }

        [HttpPost("get-cart-dish-item")]
        public async Task<AppActionResult> GetUpdateCartDishDto([FromBody] List<CartDishItem> cartItem)
        {
            return await _service.GetUpdateCartDishDto(cartItem);
        }

        [HttpPut("update-order-detail-status")]
        [TokenValidationMiddleware(Permission.KITCHEN)]
        public async Task<AppActionResult> UpdatePrelistOrderStatus(List<UpdateOrderDetailItemRequest> list, bool? isSuccessful = true)
        {
            return await _service.UpdateOrderDetailStatus(list, !isSuccessful.HasValue || isSuccessful.HasValue && isSuccessful.Value);
        }

        [HttpGet("get-current-table-session")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<AppActionResult> GetCurrentTableSession()
        {
            return await _service.GetCurrentTableSession();
        }

        //[HttpPost("suggest-table")]
        //public async Task<AppActionResult> SuggestTable(SuggestTableDto dto)
        //{
        //    return await _service.SuggestTable(dto);
        //}

        [HttpGet("get-table-reservation-with-time")]
        [TokenValidationMiddleware(Permission.RESERVATION_TIME_VIEW)]
        public async Task<AppActionResult> GetTableReservationWithTime(Guid tableId, DateTime? time)
        {
            return await _service.GetTableReservationWithTime(tableId, time);
        }

        [HttpPut("update-order-status/{orderId}")]
        //[TokenValidationMiddleware(Permission.ORDER_STATUS_MANAGEMENT)]
        public async Task<AppActionResult> ChangeOrderStatus(Guid orderId, bool isSuccessful, OrderStatus? status, bool? asCustomer)
        {
            return await _service.ChangeOrderStatus(orderId, isSuccessful, status, asCustomer, true);
        }

        [HttpGet("get-all-table-details/{pageNumber}/{pageSize}")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<AppActionResult> GetAllTableDetails(OrderStatus orderStatus, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllTableDetails(orderStatus, pageNumber, pageSize);
        }

        [HttpPost("assign-order-for-shipper")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<AppActionResult> AssignOrderForShipper(string shipperId, List<Guid> orderListId)
        {
            return await _service.AssignOrderForShipper(shipperId, orderListId);
        }

        [HttpPost("upload-confirmed-order-image")]
        //[TokenValidationMiddleware(Permission.SHIPPER)]
        public async Task<AppActionResult> UploadConfirmedOrderImage([FromForm] ConfirmedOrderRequest confirmedOrderRequest)
        {
            return await _service.UploadConfirmedOrderImage(confirmedOrderRequest);
        }

        [HttpPost("get-order-with-filter")]
        [TokenValidationMiddleware(Permission.ALL)]
        public async Task<AppActionResult> GetOrderWithFilter([FromBody] ReservationTableRequest request)
        {
            return await _service.GetOrderWithFilter(request);
        }

        [HttpPost("get-number-of-order-by-status")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<AppActionResult> GetNumberOfOrderByStatus([FromBody] OrderFilterRequest request)
        {
            return await _service.GetNumberOfOrderByStatus(request);
        }

        [HttpPost("cancel-delivering-order")]
        [TokenValidationMiddleware(Permission.DELIVERY_WITH_CUSTOMER)]
        public async Task<AppActionResult> CancelDeliveringOrder(CancelDeliveringOrderRequest cancelDeliveringOrderRequest)
        {
            return await _service.CancelDeliveringOrder(cancelDeliveringOrderRequest);
        }

        [HttpGet("get-best-seller-dishes-and-combo")]
        public async Task<AppActionResult> GetBestSellerDishesAndCombo(int topNumber, DateTime? startTime, DateTime? endTime)
        {
            return await _service.GetBestSellerDishesAndCombo(topNumber, startTime, endTime);
        }

        [HttpGet("get-all-order-detail-by-account-id/{accountId}/{feedbackStatus}/{pageNumber}/{pageSize}")]
        [TokenValidationMiddleware(Permission.ALL)]
        public async Task<AppActionResult> GetAllOrderDetailByAccountId(string accountId, int feedbackStatus, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllOrderDetailByAccountId(accountId, feedbackStatus, pageNumber, pageSize);
        }

        [HttpPut("cancel-order-detail-before-cooking")]
        [TokenValidationMiddleware(Permission.CANCEL_DISH)]
        public async Task<AppActionResult> CancelOrderDetailBeforeCooking(List<Guid> orderDetailIds)
        {
            return await _service.CancelOrderDetailBeforeCooking(orderDetailIds);
        }

        [HttpGet("get-all-orders-require-refund")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<AppActionResult> GetAllOrdersRequireRefund()
        {
            return await _service.GetAllOrdersRequireRefund();
        }
        //[HttpPut("over")]
        //public async Task CancelOverReservation()
        //{
        //    await _service.CancelOverReservation();
        //}
        //[HttpPost("calculate-order-total")]
        //public async Task<AppActionResult> GetOrderTotal([FromBody]CalculateOrderRequest dto)
        //{
        //    return await _service.GetOrderTotal(dto);
        //}

        //[HttpGet("get-order-json-by-table-session-id")]
        //public async Task<AppActionResult> GetOrderJsonByTableSessionId(Guid tableSessionId)
        //{
        //    return await _service.GetOrderJsonByTableSessionId(tableSessionId);
        //}
    }
}