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
        public IInvoiceService _invoiceService;
        public IGroupedDishCraftService _groupedDishCraftService;
        public IOrderSessionService _orderSessionService;
        public ITransactionService _transactionService;
        private IHubServices _hubServices;

        public TestController(IOrderService orderService, IInvoiceService invoiceService,
                              IHubServices hubServices, IGroupedDishCraftService groupedDishCraftService, 
                              IOrderSessionService orderSessionService, ITransactionService transactionService)
        {
            _orderService = orderService;
            _invoiceService = invoiceService;
            _hubServices = hubServices;
            _groupedDishCraftService = groupedDishCraftService;
            _orderSessionService = orderSessionService;
            _transactionService = transactionService;
        }

        [HttpPut("update-order-status")]
        public async Task<AppActionResult> UpdateOrderStatus(Guid orderId, OrderStatus orderStatus)
        {
            return await _orderService.UpdateOrderStatus(orderId, orderStatus);
        }

        [HttpPut("cancel-order")]
        public async Task CancelOrder()
        {
            await _orderService.CancelOrder();
        }

        [HttpPut("account-daily-reservation-dish")]
        public async Task AccountDailyReservationDish()
        {
            await _orderService.AccountDailyReservationDish();
        }

        [HttpPost("trigger")]
        public async Task<IActionResult> TriggerAction()
        {
            await _hubServices.SendAsync("Test");
            return Ok();
        }

        [HttpPost("test-pdf")]
        public async Task Pdf()
        {
            await _invoiceService.GenerateInvoice();
        }

        [HttpGet("test-signalr")]
        public async Task TestSignalR(string method)
        {
            await _hubServices.SendAsync(method);
        }

        [HttpPost("test-update-late-grouped")]
        public async Task UpdateLateWarningGroupedDish()
        {
            await _groupedDishCraftService.UpdateLateWarningGroupedDish();
        }

        [HttpPost("test-update-late-session")]
        public async Task UpdateLateOrderSession()
        {
            await _orderSessionService.UpdateLateOrderSession();
        }

        [HttpDelete("delete-session-daily")]
        public async Task ClearOrderSessionDaily()
        {
            await _orderSessionService.ClearOrderSessionDaily();
        }


        [HttpPut("nofitfy-dishes-to-kitchen")]
        public async Task NotifyReservationDishToKitchen()
        {
            await _orderService.NotifyReservationDishToKitchen();
        }

        [HttpPost("add-check-hacked")]
        public async Task<string> LogMoneyInformationHacked()
        {
           return await _transactionService.LogMoneyInformationHacked();
        }

        [HttpPut("hashed-all-data")]
        public async Task<string> HashingData()
        {
            return await _transactionService.HashingData();
        }
    }
}