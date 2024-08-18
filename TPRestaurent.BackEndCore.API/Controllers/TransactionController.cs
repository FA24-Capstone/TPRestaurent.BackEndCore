using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Payment.PaymentRequest;
using TPRestaurent.BackEndCore.Common.DTO.Payment.PaymentRespone;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.API.Controllers
{
    [Route("transaction")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private ITransactionService _service;
        public TransactionController(ITransactionService service)
        {
            _service = service;
        }

        [HttpPost("create-vpn-payment")]
        public async Task<AppActionResult> CreateReservationDepositWithPayment([FromBody] PaymentRequestDto paymentRequest)
        {
            return await _service.CreatePayment(paymentRequest, HttpContext);
        }

        [HttpGet("get-all-payment/{pageIndex}/{pageSize}")]
        public async Task<AppActionResult> GetAllPayment(int pageIndex, int pageSize)
        {
            return await _service.GetAllPayment(pageIndex, pageSize);       
        }

        [HttpGet("get-payment-by-id/{paymentId}")]
        public async Task<AppActionResult> GetPaymentById(Guid paymentId)
        {
            return await _service.GetPaymentById(paymentId);    
        }

        [HttpGet("VNPayIpn")]
        public async Task<IActionResult> VNPayIPN()
        {
            try
            {
                var response = new VNPayResponseDto
                {
                    PaymentMethod = Request.Query["vnp_BankCode"],
                    OrderDescription = Request.Query["vnp_OrderInfo"],
                    OrderId = Request.Query["vnp_TxnRef"],
                    PaymentId = Request.Query["vnp_TransactionNo"],
                    TransactionId = Request.Query["vnp_TransactionNo"],
                    Token = Request.Query["vnp_SecureHash"],
                    VnPayResponseCode = Request.Query["vnp_ResponseCode"],
                    PayDate = Request.Query["vnp_PayDate"],
                    Amount = Request.Query["vnp_Amount"],
                    Success = true
                };

              
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost("MomoIpn")]
        public async Task<IActionResult> MomoIPN(MomoResponseDto momo)
        {
            try
            {
                //if (momo.resultCode == 0)
                //{
                //    await _paymentService.UpdatePaymentStatus(momo.extraData, true, 1);
                //}
                //else
                //{
                //    await _paymentService.UpdatePaymentStatus(momo.extraData, false, 1);
                //}

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
