using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.API.Middlewares;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("store-credit")]
    [ApiController]
    public class StoreCreditController : ControllerBase
    {
        private readonly IStoreCreditService _service;

        public StoreCreditController(IStoreCreditService service)
        {
            _service = service;
        }

        //[HttpGet("get-store-credit-by-account-id/{accountId}")]
        //public async Task<AppActionResult> Get(string accountId)
        //{
        //    return await _service.GetStoreCreditByAccountId(accountId);
        //}

        [HttpPost("add-store-credit")]
        public async Task<AppActionResult> AddStoreCredit(Guid transactionId)
        {
            return await _service.AddStoreCredit(transactionId);
        }

        [HttpPost("refund-reservation")]
        [TokenValidationMiddleware(Permission.PAYMENT)]
        public async Task<AppActionResult> RefundReservation(Guid reservationId)
        {
            return await _service.RefundReservation(reservationId);
        }
    }
}