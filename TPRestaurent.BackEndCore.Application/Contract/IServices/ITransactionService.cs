using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Payment.PaymentRequest;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface ITransactionService
    {
        Task<AppActionResult> CreatePayment(PaymentInformationRequest paymentInformationRequest, HttpContext context);
        Task<AppActionResult> GetAllPayment(int pageIndex, int pageSize);
        Task<AppActionResult> GetPaymentById(Guid paymentId);
    }
}
