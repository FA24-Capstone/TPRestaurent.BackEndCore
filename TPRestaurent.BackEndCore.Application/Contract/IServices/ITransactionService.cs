using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Common.DTO.Payment.PaymentRequest;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Contract.IServices
{
    public interface ITransactionService
    {
        Task<AppActionResult> CreatePayment(PaymentRequestDto paymentRequest);
        //Task<AppActionResult> GetAllPayment(int pageIndex, int pageSize, Domain.Enums.TransationStatus transationStatus);
        Task<AppActionResult> GetTransactionById(Guid paymentId);
        Task<AppActionResult> GetAllTransaction(Domain.Enums.TransationStatus? transactionStatus, int pageNumber, int pageSize);
        Task<AppActionResult> UpdateTransactionStatus(Guid transactionId, Domain.Enums.TransationStatus transactionStatus);
        Task<AppActionResult> GetTransactionHistory(Guid customerId, TransactionType? type);
        Task<AppActionResult> GetStoreCreditTransactionHistory(Guid customerId);
        Task<AppActionResult> GetLoyaltyPointHistory(Guid customerId);
        Task<AppActionResult> CreateRefund(Order order);
        Task<AppActionResult> CreateDepositRefund(DepositRefundRequest request);
        Task CancelPendingTransaction();
    }
}
