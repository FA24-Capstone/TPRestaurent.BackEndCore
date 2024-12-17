using Microsoft.AspNetCore.Mvc;
using TPRestaurent.BackEndCore.API.Middlewares;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Common.DTO.Payment.PaymentRespone;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;

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

        [HttpPost("create-payment")]
        [TokenValidationMiddleware(Permission.PAYMENT)]
        public async Task<AppActionResult> CreatePayment([FromBody] PaymentRequestDto paymentRequest, string? returnUrl = "https://thienphurestaurant.vercel.app/payment")
        {
            return await _service.CreatePayment(paymentRequest, returnUrl);
        }

        [HttpGet("get-all-payment/{pageIndex}/{pageSize}")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<AppActionResult> GetAllPayment(Domain.Enums.TransationStatus? transationStatus, string? phoneNumber, int pageIndex = 1, int pageSize = 10)
        {
            return await _service.GetAllTransaction(transationStatus, phoneNumber, pageIndex, pageSize);
        }

        [HttpGet("get-payment-by-id/{paymentId}")]
        [TokenValidationMiddleware(Permission.PAYMENT)]
        public async Task<AppActionResult> GetPaymentById(Guid paymentId)
        {
            return await _service.GetTransactionById(paymentId);
        }

        [HttpGet("get-loyalty-point-history-by-customer-id/{customerId}")]
        [TokenValidationMiddleware(Permission.PAYMENT)]
        public async Task<AppActionResult> GetLoyaltyPointHistory(Guid customerId)
        {
            return await _service.GetLoyaltyPointHistory(customerId);
        }

        [HttpGet("get-transaction-history-by-customer-id/{customerId}")]
        [TokenValidationMiddleware(Permission.PAYMENT)]
        public async Task<AppActionResult> GetTransactionHistory(Guid customerId, TransactionType? type)
        {
            return await _service.GetTransactionHistory(customerId, type);
        }

        [HttpGet("get-stored-credit-transaction-history-by-customer-id/{customerId}")]
        [TokenValidationMiddleware(Permission.PAYMENT)]
        public async Task<AppActionResult> GetStoreCreditTransactionHistory(Guid customerId)
        {
            return await _service.GetStoreCreditTransactionHistory(customerId);
        }

        [HttpPut("update-transaction-Status/{transactionId}/{transactionStatus}")]
        public async Task<AppActionResult> UpdateTransactionStatus(Guid transactionId, Domain.Enums.TransationStatus transactionStatus)
        {
            return await _service.UpdateTransactionStatus(transactionId, transactionStatus);
        }

        [HttpPut("create-duplicated-paid-order-refund")]
        [TokenValidationMiddleware(Permission.ADMIN)]
        public async Task<AppActionResult> CreateDuplicatedPaidOrderRefund(DuplicatedPaidOrderRefundRequest dto)
        {
            return await _service.CreateDuplicatedPaidOrderRefund(dto);
        }

        [HttpGet("send-duplicated-payment-email")]
        public async Task<AppActionResult> SendDuplicatedRefundEmail(Guid orderId)
        {
            return await _service.SendDuplicatedRefundEmail(orderId);
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