using Castle.Core.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.Style;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.ConfigurationModel;
using TPRestaurent.BackEndCore.Common.DTO.Payment.PaymentLibrary;
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
        private readonly IConfiguration _configuration;
        private MomoConfiguration _momoConfiguration;

        public TransactionService(IConfiguration configuration,
            IGenericRepository<Transaction> repository, IUnitOfWork unitOfWork, IServiceProvider service)
            : base(service)
        {
            _configuration = configuration;
            _repository = repository;
            _unitOfWork = unitOfWork;
            _momoConfiguration = Resolve<MomoConfiguration>();
        }


        public async Task<AppActionResult> CreatePayment(PaymentInformationRequest paymentInformationRequest, HttpContext context)
        {
            AppActionResult result = new AppActionResult();
            using (var scope = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var paymentGatewayService = Resolve<IPaymentGatewayService>();
                    var utility = Resolve<Utility>();
                    var transaction = new Transaction();
                    string paymentUrl = "";
                    if (paymentInformationRequest.OrderID == null || paymentInformationRequest.ReservationID == null)
                    {
                        result = BuildAppActionResultError(result, $"Hóa đơn với khách hàng tên {paymentInformationRequest.CustomerName}");
                    }
                    if (!BuildAppActionResultIsError(result))
                    {

                        switch (paymentInformationRequest.PaymentMethod)
                        {
                            case Domain.Enums.PaymentMethod.VNPAY:
                                if (!string.IsNullOrEmpty(paymentInformationRequest.ReservationID))
                                {
                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = paymentInformationRequest.Amount,
                                        PaymentMethodId = Domain.Enums.PaymentMethod.VNPAY,
                                        ReservationId = Guid.Parse(paymentInformationRequest.ReservationID),
                                        Date = utility!.GetCurrentDateTimeInTimeZone()
                                    };

                                    await _repository.Insert(transaction);
                                    paymentUrl = await paymentGatewayService!.CreatePaymentUrlVnpay(paymentInformationRequest, context);

                                    result.Result = paymentUrl;
                                }
                                else if (!string.IsNullOrEmpty(paymentInformationRequest.OrderID))
                                {
                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = paymentInformationRequest.Amount,
                                        PaymentMethodId = Domain.Enums.PaymentMethod.VNPAY,
                                        OrderId = Guid.Parse(paymentInformationRequest.OrderID),
                                        Date = utility!.GetCurrentDateTimeInTimeZone()
                                    };

                                    await _repository.Insert(transaction);
                                    paymentUrl = await paymentGatewayService!.CreatePaymentUrlVnpay(paymentInformationRequest, context);

                                    result.Result = paymentUrl;
                                }
                                break;

                            case Domain.Enums.PaymentMethod.MOMO:
                                if (!string.IsNullOrEmpty(paymentInformationRequest.ReservationID))
                                {
                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = paymentInformationRequest.Amount,
                                        PaymentMethodId = Domain.Enums.PaymentMethod.MOMO,
                                        ReservationId = Guid.Parse(paymentInformationRequest.ReservationID),
                                        Date = utility!.GetCurrentDateTimeInTimeZone()
                                    };

                                    string endpoint = _momoConfiguration.Api;
                                    string partnerCode = _momoConfiguration.PartnerCode;
                                    string accessKey = _momoConfiguration.AccessKey;
                                    string secretkey = _momoConfiguration.Secretkey;
                                    string orderInfo = $"Khach hang: {transaction.Reservation.CustomerInfo.Name} thanh toan hoa don {transaction.ReservationId}";
                                    string redirectUrl = $"{_momoConfiguration.RedirectUrl}/{transaction.ReservationId}";
                                    string ipnUrl = _momoConfiguration.IPNUrl;
                                    string requestType = "captureWallet";

                                    string amount = Math.Ceiling(transaction.Amount).ToString();
                                    string orderId = Guid.NewGuid().ToString();
                                    string requestId = Guid.NewGuid().ToString();
                                    string extraData = transaction.ReservationId.ToString();

                                    string rawHash = "accessKey=" + accessKey +
                                                     "&amount=" + amount +
                                                     "&extraData=" + extraData +
                                                     "&ipnUrl=" + ipnUrl +
                                                     "&orderId=" + orderId +
                                                     "&orderInfo=" + orderInfo +
                                                     "&partnerCode=" + partnerCode +
                                                     "&redirectUrl=" + redirectUrl +
                                                     "&requestId=" + requestId +
                                                     "&requestType=" + requestType;

                                    MomoSecurity crypto = new MomoSecurity();
                                    string signature = crypto.signSHA256(rawHash, secretkey);

                                    JObject message = new JObject
                                {
                                    { "partnerCode", partnerCode },
                                    { "partnerName", "Test" },
                                    { "storeId", "MomoTestStore" },
                                    { "requestId", requestId },
                                    { "amount", amount },
                                    { "orderId", orderId },
                                    { "orderInfo", orderInfo },
                                    { "redirectUrl", redirectUrl },
                                    { "ipnUrl", ipnUrl },
                                    { "lang", "en" },
                                    { "extraData", extraData },
                                    { "requestType", requestType },
                                    { "signature", signature }
                                };

                                    var client = new RestClient();
                                    var request = new RestRequest(endpoint, Method.Post);
                                    request.AddJsonBody(message.ToString());
                                    RestResponse response = await client.ExecuteAsync(request);
                                    if (response.StatusCode == HttpStatusCode.OK)
                                    {
                                        JObject jmessage = JObject.Parse(response.Content);
                                        await _repository.Insert(transaction);
                                        result.Result = jmessage.GetValue("payUrl").ToString();
                                    }
                                }
                                else if (!string.IsNullOrEmpty(paymentInformationRequest.OrderID))
                                {
                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = paymentInformationRequest.Amount,
                                        PaymentMethodId = Domain.Enums.PaymentMethod.MOMO,
                                        OrderId = Guid.Parse(paymentInformationRequest.OrderID),
                                        Date = utility!.GetCurrentDateTimeInTimeZone()
                                    };
                                    string endpoint = _momoConfiguration.Api;
                                    string partnerCode = _momoConfiguration.PartnerCode;
                                    string accessKey = _momoConfiguration.AccessKey;
                                    string secretkey = _momoConfiguration.Secretkey;
                                    string orderInfo = $"Khach hang: {transaction.Reservation.CustomerInfo.Name} thanh toan hoa don {transaction.ReservationId}";
                                    string redirectUrl = $"{_momoConfiguration.RedirectUrl}/{transaction.ReservationId}";
                                    string ipnUrl = _momoConfiguration.IPNUrl;
                                    string requestType = "captureWallet";

                                    string amount = Math.Ceiling(transaction.Amount).ToString();
                                    string orderId = Guid.NewGuid().ToString();
                                    string requestId = Guid.NewGuid().ToString();
                                    string extraData = transaction.ReservationId.ToString();

                                    string rawHash = "accessKey=" + accessKey +
                                                     "&amount=" + amount +
                                                     "&extraData=" + extraData +
                                                     "&ipnUrl=" + ipnUrl +
                                                     "&orderId=" + orderId +
                                                     "&orderInfo=" + orderInfo +
                                                     "&partnerCode=" + partnerCode +
                                                     "&redirectUrl=" + redirectUrl +
                                                     "&requestId=" + requestId +
                                                     "&requestType=" + requestType;

                                    MomoSecurity crypto = new MomoSecurity();
                                    string signature = crypto.signSHA256(rawHash, secretkey);

                                    JObject message = new JObject
                                {
                                    { "partnerCode", partnerCode },
                                    { "partnerName", "Test" },
                                    { "storeId", "MomoTestStore" },
                                    { "requestId", requestId },
                                    { "amount", amount },
                                    { "orderId", orderId },
                                    { "orderInfo", orderInfo },
                                    { "redirectUrl", redirectUrl },
                                    { "ipnUrl", ipnUrl },
                                    { "lang", "en" },
                                    { "extraData", extraData },
                                    { "requestType", requestType },
                                    { "signature", signature }
                                };

                                    var client = new RestClient();
                                    var request = new RestRequest(endpoint, Method.Post);
                                    request.AddJsonBody(message.ToString());
                                    RestResponse response = await client.ExecuteAsync(request);
                                    if (response.StatusCode == HttpStatusCode.OK)
                                    {
                                        JObject jmessage = JObject.Parse(response.Content);
                                        await _repository.Insert(transaction);
                                        result.Result = jmessage.GetValue("payUrl").ToString();
                                    }
                                }
                                break;

                            default:
                                // Handle other payment methods if needed
                                if (!string.IsNullOrEmpty(paymentInformationRequest.ReservationID))
                                {
                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = paymentInformationRequest.Amount,
                                        PaymentMethodId = Domain.Enums.PaymentMethod.MOMO,
                                        OrderId = Guid.Parse(paymentInformationRequest.OrderID),
                                        Date = utility!.GetCurrentDateTimeInTimeZone()
                                    };
                                    await _repository.Insert(transaction);
                                }
                                else if (!string.IsNullOrEmpty(paymentInformationRequest.OrderID))
                                {
                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = paymentInformationRequest.Amount,
                                        PaymentMethodId = Domain.Enums.PaymentMethod.MOMO,
                                        OrderId = Guid.Parse(paymentInformationRequest.OrderID),
                                        Date = utility!.GetCurrentDateTimeInTimeZone()
                                    };
                                    await _repository.Insert(transaction);
                                }
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
                if (!BuildAppActionResultIsError(result))
                {
                    await _unitOfWork.SaveChangesAsync();
                    scope.Complete();
                }
            }

            return result;
        }
    }
}
