using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.API.Middlewares;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("invoice")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private IInvoiceService _service;

        public InvoiceController(IInvoiceService service)
        {
            _service = service;
        }

        [HttpGet("get-by-id/{id}")]
        [TokenValidationMiddleware(Permission.PAYMENT)]
        public async Task<AppActionResult> GetById(Guid id)
        {
            return await _service.GetInvoiceById(id);
        }

        [HttpPost("get-all-invoice")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<AppActionResult> GetAllInvoice([FromBody] InvoiceFilterRequest dto)
        {
            return await _service.GetAllInvoice(dto);
        }

        [HttpPost("generate-general-invoice")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<AppActionResult> GenerateGeneralInvoice([FromBody] InvoiceFilterRequest dto)
        {
            return await _service.GenerateGeneralInvoice(dto);
        }


        [HttpPost("get-invoice-by-order-id/{orderId}")]
        //[TokenValidationMiddleware(Permission.PAYMENT)]
        public async Task<AppActionResult> GetInvoicebyOrderId(Guid orderId)
        {
            return await _service.GetInvoicebyOrderId(orderId);
        }
    }
}