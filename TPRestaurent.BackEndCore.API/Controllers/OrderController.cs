using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
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
        public async Task<AppActionResult> GetAllOrderByStatus(Domain.Enums.OrderStatus? status, OrderType orderType, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllOrderByStatus(status, orderType, pageNumber, pageSize);
        }

        [HttpGet("get-order-by-account-id/{accountId}/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllOrderByAccountId(string accountId, Domain.Enums.OrderStatus? status, OrderType? orderType, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllOrderByCustomertId(accountId, status, orderType, 1, 10);
        }

        [HttpGet("get-all-order-by-phone-number/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllOrderByPhoneNumber(string phoneNumber, OrderStatus? status, OrderType? orderType, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllOrderByPhoneNumber(phoneNumber, status, orderType ,pageNumber, pageSize);
        }

        [HttpGet("get-order-by-time/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetOrderByTime(double? minute, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetOrderByTime(minute, pageNumber, pageSize);
        }

        [HttpGet("get-all-order-by-shipper-id/{shipperId}/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllOrderByShipperId(string shipperId, OrderStatus? status, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllOrderByShipperId(shipperId, status, pageNumber, pageSize);
        }

        [HttpGet("get-order-detail/{orderId}")]
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
        public async Task<AppActionResult> CalculateDeliveryOrder(Guid customerInfoAddressId)
        {
            return await _service.CalculateDeliveryOrder(customerInfoAddressId);        
        }

        [HttpPost("add-dish-to-order/{orderId}")]
        public async Task<AppActionResult> AddDishToOrder([FromBody] AddDishToOrderRequestDto dto)
        {
            return await _service.AddDishToOrder(dto);
        }

        [HttpPost("create-order")]
        public async Task<AppActionResult> CreateOrder([FromBody] OrderRequestDto dto)
        {
            return await _service.CreateOrder(dto);
        }

        [HttpPost("make-dine-in-order-bill")]
        public async Task<AppActionResult> MakeDineInOrderBill([FromBody] OrderPaymentRequestDto dto)
        {
            return await _service.MakeDineInOrderBill(dto);
        }

        [HttpGet("get-cart-combo-item")]
        public async Task<AppActionResult> GetCartItem(string cartItem)
        {
            return await _service.GetUpdateCartComboDto(cartItem);
        }

        [HttpGet("get-cart-dish-item")]
        public async Task<AppActionResult> GetUpdateCartDishDto(string cartItem)
        {
            return await _service.GetUpdateCartDishDto(cartItem);
        }

        [HttpPut("update-order-detail-status")]
        public async Task<AppActionResult> UpdatePrelistOrderStatus(List<Guid> list, bool? isSuccessful = true)
        {
            return await _service.UpdateOrderDetailStatus(list, !isSuccessful.HasValue || isSuccessful.HasValue && isSuccessful.Value);
        }

        [HttpGet("get-current-table-session")]
        public async Task<AppActionResult> GetCurrentTableSession()
        {
            return await _service.GetCurrentTableSession();
        }

        [HttpPost("suggest-table")]
        public async Task<AppActionResult> SuggestTable(SuggestTableDto dto)
        {
            return await _service.SuggestTable(dto);
        }

        [HttpGet("get-table-reservation-with-time")]
        public async Task<AppActionResult> GetTableReservationWithTime(Guid tableId, DateTime? time)
        {
            return await _service.GetTableReservationWithTime(tableId, time);
        }

        [HttpPut("update-order-status/{orderId}")]
        public async Task<AppActionResult> ChangeOrderStatus(Guid orderId, bool isSuccessful, OrderStatus? status)
        {
            return await _service.ChangeOrderStatus(orderId, isSuccessful, status);
        }

        [HttpGet("get-all-table-details/{pageNumber}/{pageSize}")]
        public async Task<AppActionResult> GetAllTableDetails(OrderStatus orderStatus, int pageNumber = 1, int pageSize = 10)
        {
            return await _service.GetAllTableDetails(orderStatus, pageNumber, pageSize);
        }

        [HttpPost("assign-order-for-shipper")]
        public async Task<AppActionResult> AssignOrderForShipper(string shipperId, List<Guid> orderListId)
        {
            return await _service.AssignOrderForShipper(shipperId, orderListId);      
        }

        [HttpPost("upload-confirmed-order-image")]
        public async Task<AppActionResult> UploadConfirmedOrderImage([FromForm] ConfirmedOrderRequest confirmedOrderRequest)
        {
            return await _service.UploadConfirmedOrderImage(confirmedOrderRequest);
        }

        [HttpPost("get-order-with-filter")]
        public async Task<AppActionResult> GetOrderWithFilter([FromBody] ReservationTableRequest request)
        {
            return await _service.GetOrderWithFilter(request);
        }
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
