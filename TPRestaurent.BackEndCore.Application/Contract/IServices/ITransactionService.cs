using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Payment.PaymentRequest;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface ITransactionService
    {
        Task<AppActionResult> CreatePayment(PaymentRequestDto paymentRequest, HttpContext context);
        //Task<AppActionResult> GetAllPayment(int pageIndex, int pageSize, Domain.Enums.TransationStatus transationStatus);
        //Task<AppActionResult> GetPaymentById(Guid paymentId);
        Task<AppActionResult> GetAllTransaction(int pageNumber, int pageSize);
        Task<AppActionResult> UpdateTransactionStatus(Guid transactionId, Domain.Enums.TransationStatus transactionStatus);
    }
}
