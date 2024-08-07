using Microsoft.AspNetCore.Http;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Payment.PaymentRequest;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class TransactionService : GenericBackendService, ITransactionService
    {
        private readonly IGenericRepository<Transaction> _repository;
        private readonly IUnitOfWork _unitOfWork;
        public TransactionService(IGenericRepository<Transaction> repository, IUnitOfWork unitOfWork, IServiceProvider service) : base(service)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;

        }

        public async Task<AppActionResult> CreateReservationDepositWithPayment(PaymentInformationRequest paymentInformationRequest, HttpContext context)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var paymentGatewayService = Resolve<IPaymentGatewayService>();
                var utility = Resolve<Utility>();
                var transaction = new Transaction();
                string paymentUrl = "";
                
                if(paymentInformationRequest.PaymentMethod == Domain.Enums.PaymentMethod.VNPAY)
                {
                    transaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        Amount = paymentInformationRequest.Amount,
                        PaymentMethodId = Domain.Enums.PaymentMethod.VNPAY,
                        ReservationId = Guid.Parse(paymentInformationRequest.ReservationID),
                        Date = utility.GetCurrentDateTimeInTimeZone()
                    };

                    await _repository.Insert(transaction);
                    paymentUrl = await paymentGatewayService!.CreatePaymentUrlVnpay(paymentInformationRequest, context);
                    await _unitOfWork.SaveChangesAsync();   

                    result.Result = paymentUrl;
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
    }
}
