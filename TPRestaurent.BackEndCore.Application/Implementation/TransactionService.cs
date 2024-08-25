using Castle.Core.Internal;
using MailKit.Search;
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
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;
using Twilio.Rest.Verify.V2.Service.Entity;

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


        public async Task<AppActionResult> CreatePayment(PaymentRequestDto paymentRequest, HttpContext context)
        {
            AppActionResult result = new AppActionResult();
            using (var scope = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var paymentGatewayService = Resolve<IPaymentGatewayService>();
                    var reservationRepository = Resolve<IGenericRepository<Reservation>>();
                    var orderRepository = Resolve<IGenericRepository<Order>>();     
                    var storeCreditRepository = Resolve<IGenericRepository<StoreCredit>>();
                    var utility = Resolve<Utility>();
                    var transaction = new Transaction();
                    string paymentUrl = "";
                    if (!paymentRequest.OrderId.HasValue && !paymentRequest.ReservationId.HasValue && !paymentRequest.StoreCreditId.HasValue)
                    {
                        result = BuildAppActionResultError(result, $"Đơn hàng/Đặt bàn/Ví này không tồn tại");
                    }
                    if (!BuildAppActionResultIsError(result))
                    {

                        switch (paymentRequest.PaymentMethod)
                        {
                            case Domain.Enums.PaymentMethod.VNPAY:
                                if (paymentRequest.ReservationId.HasValue)
                                {
                                    var reservationDb = await reservationRepository!.GetByExpression(p => p.ReservationId == paymentRequest.ReservationId, p => p.CustomerInfo!.Account!);

                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = reservationDb.Deposit,
                                        PaymentMethodId = Domain.Enums.PaymentMethod.VNPAY,
                                        ReservationId =  reservationDb.ReservationId,   
                                        Date = utility!.GetCurrentDateTimeInTimeZone()
                                    };

                                    PaymentInformationRequest paymentInformationRequest = new Common.DTO.Payment.PaymentRequest.PaymentInformationRequest
                                    {
                                        ReservationID = reservationDb.ReservationId.ToString(),
                                        Amount = reservationDb.Deposit,
                                        CustomerName = reservationDb.CustomerInfo.Name,
                                        AccountID = reservationDb.CustomerInfo.AccountId,   
                                        PaymentMethod = paymentRequest.PaymentMethod,   
                                    };

                                    await _repository.Insert(transaction);
                                    paymentUrl = await paymentGatewayService!.CreatePaymentUrlVnpay(paymentInformationRequest, context);

                                    result.Result = paymentUrl;
                                }
                                else if (paymentRequest.OrderId.HasValue)
                                {
                                    var orderDb = await orderRepository!.GetByExpression(p => p.OrderId == paymentRequest.OrderId, p => p.CustomerInfo!.Account!);       
                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = orderDb.TotalAmount,
                                        PaymentMethodId = Domain.Enums.PaymentMethod.VNPAY,
                                        OrderId = orderDb.OrderId,
                                        Date = utility!.GetCurrentDateTimeInTimeZone()
                                    };

                                    var paymentInformationRequest = new PaymentInformationRequest
                                    {
                                        OrderID = orderDb.OrderId.ToString(),
                                        PaymentMethod = paymentRequest.PaymentMethod,
                                        Amount = orderDb.TotalAmount,
                                        CustomerName = orderDb!.CustomerInfo!.Name,
                                        AccountID = orderDb.CustomerInfo.AccountId,
                                    };


                                    await _repository.Insert(transaction);
                                    paymentUrl = await paymentGatewayService!.CreatePaymentUrlVnpay(paymentInformationRequest, context);

                                    result.Result = paymentUrl;
                                } 
                                else if (paymentRequest.StoreCreditId.HasValue)
                                {
                                    if (paymentRequest.StoreCreditAmount.HasValue)
                                    {
                                        var storeCreditDb = await storeCreditRepository!.GetByExpression(p => p.StoreCreditId == paymentRequest.StoreCreditId, p => p!.Account!);
                                        transaction = new Transaction
                                        {
                                            Id = Guid.NewGuid(),
                                            Amount = paymentRequest.StoreCreditAmount.Value,
                                            PaymentMethodId = Domain.Enums.PaymentMethod.VNPAY,
                                            StoreCreditId = paymentRequest.StoreCreditId.Value,
                                            Date = utility!.GetCurrentDateTimeInTimeZone()
                                        };

                                        var paymentInformationRequest = new PaymentInformationRequest
                                        {
                                            TransactionID = transaction.Id.ToString(),
                                            PaymentMethod = paymentRequest.PaymentMethod,
                                            Amount = paymentRequest.StoreCreditAmount.Value,
                                            CustomerName = "",
                                            AccountID = storeCreditDb.AccountId,
                                        };


                                        await _repository.Insert(transaction);
                                        paymentUrl = await paymentGatewayService!.CreatePaymentUrlVnpay(paymentInformationRequest, context);

                                        result.Result = paymentUrl;
                                    }
                                }
                                break;

                            case Domain.Enums.PaymentMethod.MOMO:
                                if (paymentRequest.ReservationId.HasValue)
                                {
                                    var reservationDb = await reservationRepository!.GetByExpression(p => p.ReservationId == paymentRequest.ReservationId, p => p.CustomerInfo!.Account!);

                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = reservationDb.Deposit,
                                        PaymentMethodId = Domain.Enums.PaymentMethod.MOMO,
                                        ReservationId = reservationDb.ReservationId,    
                                        Date = utility!.GetCurrentDateTimeInTimeZone()
                                    };

                                    var paymentInformationRequest = new PaymentInformationRequest
                                    {
                                        ReservationID = reservationDb.ReservationId.ToString(),     
                                        PaymentMethod = paymentRequest.PaymentMethod,
                                        Amount = reservationDb.Deposit,
                                        CustomerName = reservationDb.CustomerInfo!.Name,
                                        AccountID = reservationDb.CustomerInfo.AccountId,
                                    };

                                    string endpoint = _momoConfiguration.Api;
                                    string partnerCode = _momoConfiguration.PartnerCode;
                                    string accessKey = _momoConfiguration.AccessKey;
                                    string secretkey = _momoConfiguration.Secretkey;
                                    string orderInfo = $"RE";
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
                                else if (paymentRequest.OrderId.HasValue)
                                {
                                    var orderDb = await orderRepository!.GetByExpression(p => p.OrderId == paymentRequest.OrderId, p => p.CustomerInfo!.Account!);

                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = orderDb.TotalAmount,
                                        PaymentMethodId = Domain.Enums.PaymentMethod.MOMO,
                                        OrderId = orderDb.OrderId,
                                        Date = utility!.GetCurrentDateTimeInTimeZone()
                                    };
                                    string endpoint = _momoConfiguration.Api;
                                    string partnerCode = _momoConfiguration.PartnerCode;
                                    string accessKey = _momoConfiguration.AccessKey;
                                    string secretkey = _momoConfiguration.Secretkey;
                                    string orderInfo = $"OR";
                                    string redirectUrl = $"{_momoConfiguration.RedirectUrl}/{transaction.OrderId}";
                                    string ipnUrl = _momoConfiguration.IPNUrl;
                                    string requestType = "captureWallet";

                                    string amount = Math.Ceiling(transaction.Amount).ToString();
                                    string orderId = Guid.NewGuid().ToString();
                                    string requestId = Guid.NewGuid().ToString();
                                    string extraData = transaction.OrderId.ToString();

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
                                else if (paymentRequest.StoreCreditId.HasValue)
                                {
                                    if (paymentRequest.StoreCreditAmount.HasValue)
                                    {
                                        var storeCreditDb = await storeCreditRepository!.GetByExpression(p => p.StoreCreditId == paymentRequest.StoreCreditId, p => p!.Account!);
                                        transaction = new Transaction
                                        {
                                            Id = Guid.NewGuid(),
                                            Amount = paymentRequest.StoreCreditAmount.Value,
                                            PaymentMethodId = Domain.Enums.PaymentMethod.MOMO,
                                            StoreCreditId = storeCreditDb.StoreCreditId,
                                            Date = utility!.GetCurrentDateTimeInTimeZone()
                                        };
                                        string endpoint = _momoConfiguration.Api;
                                        string partnerCode = _momoConfiguration.PartnerCode;
                                        string accessKey = _momoConfiguration.AccessKey;
                                        string secretkey = _momoConfiguration.Secretkey;
                                        string orderInfo = $"CR_{transaction.Id}";
                                        string redirectUrl = $"{_momoConfiguration.RedirectUrl}/{transaction.OrderId}";
                                        string ipnUrl = _momoConfiguration.IPNUrl;
                                        string requestType = "captureWallet";

                                        string amount = Math.Ceiling(transaction.Amount).ToString();
                                        string orderId = Guid.NewGuid().ToString();
                                        string requestId = Guid.NewGuid().ToString();
                                        string extraData = transaction.OrderId.ToString();

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
                                }
                                break;

                            default:
                                // Handle other payment methods if needed
                                if (paymentRequest.ReservationId.HasValue)
                                {
                                    var reservationDb = await reservationRepository!.GetByExpression(p => p.ReservationId == paymentRequest.ReservationId, p => p.CustomerInfo!.Account!);

                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = reservationDb.Deposit,
                                        PaymentMethodId = Domain.Enums.PaymentMethod.Cash,
                                        ReservationId = reservationDb.ReservationId,
                                        Date = utility!.GetCurrentDateTimeInTimeZone()
                                    };
                                    await _repository.Insert(transaction);
                                    result.Result = transaction;
                                }
                                else if (paymentRequest.OrderId.HasValue)
                                {
                                    var orderDb = await orderRepository!.GetByExpression(p => p.OrderId == paymentRequest.OrderId, p => p.CustomerInfo!.Account!);

                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = orderDb.TotalAmount,
                                        PaymentMethodId = Domain.Enums.PaymentMethod.Cash,
                                        OrderId = orderDb.OrderId,
                                        Date = utility!.GetCurrentDateTimeInTimeZone()
                                    };
                                    await _repository.Insert(transaction);
                                    result.Result = transaction;
                                }
                                else if (paymentRequest.StoreCreditId.HasValue)
                                {
                                    if (paymentRequest.StoreCreditAmount.HasValue)
                                    {
                                        var storeCreditDb = await storeCreditRepository!.GetByExpression(p => p.StoreCreditId == paymentRequest.StoreCreditId, p => p!.Account!);
                                        transaction = new Transaction
                                        {
                                            Id = Guid.NewGuid(),
                                            Amount = paymentRequest.StoreCreditAmount.Value,
                                            PaymentMethodId = Domain.Enums.PaymentMethod.Cash,
                                            StoreCreditId = storeCreditDb.StoreCreditId,
                                            Date = utility!.GetCurrentDateTimeInTimeZone()
                                        };
                                        await _repository.Insert(transaction);
                                        result.Result = transaction;
                                    }
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

        public async Task<AppActionResult> GetAllPayment(int pageIndex, int pageSize)
        {
            var result = new AppActionResult();
            try
            {
                var transactionListDb = await _repository.GetAllDataByExpression(null, pageIndex, pageSize, null, false, p => p.Order!, p => p.Reservation!, p => p.PaymentMethod!);
                result.Result = transactionListDb;  
            } 
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);

            }
            return result;
        }

        public async Task<AppActionResult> GetPaymentById(Guid paymentId)
        {
            var result = new AppActionResult();
            try
            {
                var transactionDb = await _repository.GetByExpression(p => p.Id == paymentId, p => p.Order!, p => p.Reservation!, p => p.PaymentMethod!);
                if (transactionDb == null)
                {
                    result = BuildAppActionResultError(result, $"Hóa đơn với id {paymentId} không tồn tại");
                }
                result.Result = transactionDb;      
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
    }
}
