using Castle.Core.Internal;
using MailKit.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using NPOI.SS.Formula.Functions;
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
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
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


        public async Task<AppActionResult> CreatePayment(PaymentRequestDto paymentRequest)
        {
            AppActionResult result = new AppActionResult();
            using (var scope = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var paymentGatewayService = Resolve<IPaymentGatewayService>();
                    var orderRepository = Resolve<IGenericRepository<Order>>();
                    var orderService = Resolve<IOrderService>();
                    var storeCreditRepository = Resolve<IGenericRepository<StoreCredit>>();
                    var hasingService = Resolve<IHashingService>();
                    var utility = Resolve<Utility>();
                    var transaction = new Transaction();
                    IConfiguration config = new ConfigurationBuilder()
                          .SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile("appsettings.json", true, true)
                          .Build();
                    string key = config["HashingKeys:PaymentLink"];
                    string paymentUrl = "";
                    double amount = 0;
                    if (!paymentRequest.OrderId.HasValue && !paymentRequest.StoreCreditId.HasValue)
                    {
                        result = BuildAppActionResultError(result, $"Đơn hàng/Đặt bàn/Ví này không tồn tại");
                        return result;
                    }
                    if (!BuildAppActionResultIsError(result))
                    {

                        switch (paymentRequest.PaymentMethod)
                        {
                            case Domain.Enums.PaymentMethod.VNPAY:
                                if (paymentRequest.OrderId.HasValue)
                                {
                                    var orderDb = await orderRepository!.GetByExpression(p => p.OrderId == paymentRequest.OrderId, p => p.Account!);
                                    if (orderDb.StatusId == OrderStatus.Dining || orderDb.StatusId == OrderStatus.Pending || orderDb.StatusId == OrderStatus.Delivering)
                                    {
                                        amount = orderDb.TotalAmount;
                                    }
                                    else
                                    {
                                        if (orderDb.Deposit.HasValue)
                                        {
                                            amount = orderDb.Deposit.Value;
                                        }
                                        else
                                        {
                                            return BuildAppActionResultError(result, $"Số tiền thanh toán không hợp lệ");
                                        }
                                    }
                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = amount,
                                        PaymentMethodId = Domain.Enums.PaymentMethod.VNPAY,
                                        OrderId = orderDb.OrderId,
                                        Date = utility!.GetCurrentDateTimeInTimeZone(),
                                        TransationStatusId = TransationStatus.PENDING,
                                        TransactionTypeId = orderDb.StatusId == OrderStatus.TableAssigned ? TransactionType.Deposit : TransactionType.Order
                                    };

                                    var paymentInformationRequest = new PaymentInformationRequest
                                    {
                                        TransactionID  = transaction.Id.ToString(),
                                        PaymentMethod = paymentRequest.PaymentMethod,
                                        Amount = amount,
                                        CustomerName = orderDb!.Account!.LastName,
                                        AccountID = orderDb.AccountId,
                                    };

                                    await _repository.Insert(transaction);
                                    paymentUrl = await paymentGatewayService!.CreatePaymentUrlVnpay(paymentInformationRequest);

                                    result.Result = paymentUrl;
                                }
                                else if (paymentRequest.StoreCreditId.HasValue)
                                {
                                    if (paymentRequest.StoreCreditAmount.HasValue)
                                    {
                                        var storeCreditDb = await storeCreditRepository!.GetByExpression(p => p.StoreCreditId == paymentRequest.StoreCreditId, p => p.Account!);
                                        transaction = new Transaction
                                        {
                                            Id = Guid.NewGuid(),
                                            Amount = (double)(paymentRequest.StoreCreditAmount),
                                            PaymentMethodId = Domain.Enums.PaymentMethod.VNPAY,
                                            StoreCreditId = paymentRequest.StoreCreditId,
                                            Date = utility!.GetCurrentDateTimeInTimeZone(),
                                            TransationStatusId = TransationStatus.PENDING,
                                            TransactionTypeId = TransactionType.CreditStore,
                                        };

                                        var paymentInformationRequest = new PaymentInformationRequest
                                        {
                                            TransactionID = transaction.Id.ToString(),
                                            PaymentMethod = paymentRequest.PaymentMethod,
                                            Amount = (double)paymentRequest.StoreCreditAmount,
                                            CustomerName = storeCreditDb!.Account!.LastName,
                                            AccountID = storeCreditDb.AccountId,
                                        };

                                        await _repository.Insert(transaction);
                                        paymentUrl = await paymentGatewayService!.CreatePaymentUrlVnpay(paymentInformationRequest);

                                        result.Result = paymentUrl;
                                    }
                                }
                                break;
                            case Domain.Enums.PaymentMethod.MOMO:
                                if (paymentRequest.OrderId.HasValue)
                                {
                                    var orderDb = await orderRepository!.GetByExpression(p => p.OrderId == paymentRequest.OrderId, p => p.Account!);
                                    if (orderDb.StatusId == OrderStatus.Dining || orderDb.StatusId == OrderStatus.Pending || orderDb.StatusId == OrderStatus.Delivering)
                                    {
                                        amount = orderDb.TotalAmount;
                                    }
                                    else
                                    {
                                        if (orderDb.Deposit.HasValue)
                                        {
                                            amount = orderDb.Deposit.Value;
                                        }
                                        else
                                        {
                                            return BuildAppActionResultError(result, $"Số tiền thanh toán không hợp lệ");
                                        }
                                    }
                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = amount,
                                        PaymentMethodId = Domain.Enums.PaymentMethod.MOMO,
                                        OrderId = orderDb.OrderId,
                                        Date = utility!.GetCurrentDateTimeInTimeZone(),
                                        TransationStatusId = TransationStatus.PENDING,
                                        TransactionTypeId = orderDb.StatusId == OrderStatus.TableAssigned ? TransactionType.Deposit : TransactionType.Order
                                    };

                                    string endpoint = _momoConfiguration.Api;
                                    string partnerCode = _momoConfiguration.PartnerCode;
                                    string accessKey = _momoConfiguration.AccessKey;
                                    string secretkey = _momoConfiguration.Secretkey;
                                    string orderInfo = hasingService.Hashing("OR", key);
                                    string redirectUrl = $"{_momoConfiguration.RedirectUrl}";
                                    string ipnUrl = _momoConfiguration.IPNUrl;
                                    string requestType = "payWithATM";

                                    string requestId = Guid.NewGuid().ToString();
                                    string extraData = transaction.OrderId.ToString();

                                    string rawHash = "accessKey=" + accessKey +
                                                     "&amount=" + Math.Ceiling(transaction.Amount).ToString() +
                                                     "&extraData=" + extraData +
                                                     "&ipnUrl=" + ipnUrl +
                                                     "&orderId=" + transaction.Id +
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
                                    { "orderId", transaction.Id },
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
                                    if(paymentRequest.StoreCreditAmount > 0)
                                    {
                                        {
                                            var storCreditDb = await storeCreditRepository!.GetByExpression(p => p.StoreCreditId == paymentRequest.StoreCreditId, p => p.Account!);
                                            transaction = new Transaction
                                            {
                                                Id = Guid.NewGuid(),
                                                Amount = (double)(paymentRequest.StoreCreditAmount),
                                                PaymentMethodId = Domain.Enums.PaymentMethod.MOMO,
                                                StoreCreditId = storCreditDb.StoreCreditId,
                                                Date = utility!.GetCurrentDateTimeInTimeZone(),
                                                TransationStatusId = TransationStatus.PENDING,
                                                TransactionTypeId = TransactionType.CreditStore
                                            };
                                            string endpoint = _momoConfiguration.Api;
                                            string partnerCode = _momoConfiguration.PartnerCode;
                                            string accessKey = _momoConfiguration.AccessKey;
                                            string secretkey = _momoConfiguration.Secretkey;
                                            string orderInfo = hasingService.Hashing("OR", key);
                                            string redirectUrl = $"{_momoConfiguration.RedirectUrl}";
                                            string ipnUrl = _momoConfiguration.IPNUrl;
                                            string requestType = "payWithATM";

                                            string requestId = Guid.NewGuid().ToString();
                                            string extraData = transaction.OrderId.ToString();

                                            string rawHash = "accessKey=" + accessKey +
                                                             "&amount=" + Math.Ceiling(transaction.Amount).ToString() +
                                                             "&extraData=" + extraData +
                                                             "&ipnUrl=" + ipnUrl +
                                                             "&orderId=" + transaction.Id +
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
                                    { "orderId", transaction.Id },
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
                                }
                                break;
                            case Domain.Enums.PaymentMethod.STORE_CREDIT:
                                if (paymentRequest.OrderId.HasValue)
                                {
                                    var orderDb = await orderRepository!.GetByExpression(p => p.OrderId == paymentRequest.OrderId, p => p.Account!);

                                    if(orderDb.Account == null)
                                    {
                                        return BuildAppActionResultError(result, $"Không tìm thấy tài khoản đặt hàng. Khôn thể thực hiện thanh toán với phương thức: Thanh toán với số dư tài khoản.");
                                    }

                                    if (orderDb.StatusId == OrderStatus.Dining || orderDb.StatusId == OrderStatus.Pending || orderDb.StatusId == OrderStatus.Delivering)
                                    {
                                        amount = orderDb.TotalAmount;
                                    }
                                    else
                                    {
                                        if (orderDb.Deposit.HasValue)
                                        {
                                            amount = orderDb.Deposit.Value;
                                        }
                                        else
                                        {
                                            return BuildAppActionResultError(result, $"Số tiền thanh toán không hợp lệ");
                                        }
                                    }

                                    var storeCreditDb = await storeCreditRepository.GetByExpression(s => s.AccountId == orderDb.AccountId, null);
                                    if(storeCreditDb == null)
                                    {
                                        return BuildAppActionResultError(result, $"Không tìm thấy số dư tài khoản khách hàng");
                                    }

                                    if(storeCreditDb.Amount < amount)
                                    {
                                        return BuildAppActionResultError(result, $"Số dư tài khoản của quý khách không đủ");
                                    }

                                    storeCreditDb.Amount -= amount;
                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = amount,
                                        PaymentMethodId = Domain.Enums.PaymentMethod.STORE_CREDIT,
                                        OrderId = orderDb.OrderId,
                                        Date = utility!.GetCurrentDateTimeInTimeZone(),
                                        PaidDate = utility!.GetCurrentDateTimeInTimeZone(),
                                        TransationStatusId = TransationStatus.SUCCESSFUL,
                                        TransactionTypeId = orderDb.StatusId == OrderStatus.TableAssigned ? TransactionType.Deposit : TransactionType.Order
                                    };

                                    await _repository.Insert(transaction);
                                    await orderService.ChangeOrderStatus(orderDb.OrderId, true);
                                    await storeCreditRepository.Update(storeCreditDb);
                                }
                                break;
                            default:
                                if(paymentRequest.PaymentMethod == PaymentMethod.ZALOPAY)
                                {
                                    return BuildAppActionResultError(result, $"Hệ thống chưa hỗ trợ thanh toán với ZALOPAY");
                                }

                                if (paymentRequest.OrderId.HasValue)
                                {
                                    var orderDb = await orderRepository!.GetByExpression(p => p.OrderId == paymentRequest.OrderId, p => p.Account!);
                                    if (orderDb.StatusId == OrderStatus.Dining || orderDb.StatusId == OrderStatus.Pending || orderDb.StatusId == OrderStatus.TableAssigned )
                                    {
                                        amount = orderDb.TotalAmount;
                                    } 
                                    else
                                    {
                                        if (orderDb.Deposit.HasValue)
                                        {
                                            amount = orderDb.Deposit.Value;
                                        }
                                        else
                                        {
                                            return BuildAppActionResultError(result, $"Số tiền thanh toán không hợp lệ");
                                        }
                                    }

                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = amount,
                                        PaymentMethodId = Domain.Enums.PaymentMethod.Cash,
                                        OrderId = orderDb.OrderId,
                                        Date = utility!.GetCurrentDateTimeInTimeZone(),
                                        TransationStatusId = TransationStatus.PENDING,
                                        TransactionTypeId = orderDb.StatusId == OrderStatus.TableAssigned ? TransactionType.Deposit : TransactionType.Order
                                    };
                                    await _repository.Insert(transaction);
                                    result.Result = transaction;
                                }
                                else if (paymentRequest.StoreCreditId.HasValue)
                                {
                                    if (paymentRequest.StoreCreditAmount.HasValue)
                                    {
                                        var storeCreditDb = await storeCreditRepository!.GetByExpression(p => p.StoreCreditId == paymentRequest.StoreCreditId, p => p.Account!);

                                        if (storeCreditDb == null)
                                        {
                                            result = BuildAppActionResultError(result, $"Khong tìm thấy thông tin ví với id {paymentRequest.StoreCreditId}");
                                            return result;
                                        }

                                        transaction = new Transaction
                                        {
                                            Id = Guid.NewGuid(),
                                            Amount = paymentRequest.StoreCreditAmount.Value,
                                            PaymentMethodId = Domain.Enums.PaymentMethod.Cash,
                                            StoreCreditId = storeCreditDb.StoreCreditId,
                                            Date = utility!.GetCurrentDateTimeInTimeZone(),
                                            TransationStatusId = TransationStatus.PENDING,
                                            TransactionTypeId = TransactionType.CreditStore

                                        };
                                        await _repository.Insert(transaction);
                                    }
                                }
                                break;
                        }
                    }
                    if (!BuildAppActionResultIsError(result))
                    {
                        await _unitOfWork.SaveChangesAsync();
                        scope.Complete();
                    }
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
                
            }

            return result;
        }

        //public async Task<AppActionResult> GetAllPayment(int pageIndex, int pageSize, Domain.Enums.TransationStatus transationStatus)
        //{
        //    var result = new AppActionResult();
        //    try
        //    {
        //        var transactionListDb = await _repository.GetAllDataByExpression(p => p.TransationStatusId == transationStatus, pageIndex, pageSize, null, false, p => p.Order!, p => p.Reservation!,
        //            p => p.PaymentMethod!,
        //            p => p.TransationStatus!,
        //            p => p.StoreCreditHistory!.StoreCredit!.Account!,
        //            p => p.Reservation!.CustomerInfo!,
        //            p => p.Reservation!.ReservationStatus!,
        //            p => p.Order!.Status!,
        //            p => p.Order!.LoyalPointsHistory!,
        //            p => p.Order!.CustomerSavedCoupon!.Coupon!
        //            );
        //        result.Result = transactionListDb;
        //    }
        //    catch (Exception ex)
        //    {
        //        result = BuildAppActionResultError(result, ex.Message);

        //    }
        //    return result;
        //}

        public async Task<AppActionResult> GetAllTransaction(Domain.Enums.TransationStatus? transactionStatus, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var transactionDb = await _repository.GetAllDataByExpression(t => !transactionStatus.HasValue || (transactionStatus.HasValue && transactionStatus.Value == t.TransationStatusId), pageNumber, pageSize, p => p.Date, false , 
                    t => t.StoreCredit!.Account!, 
                    t => t.Order!.Account!,
                    t => t.TransationStatus,
                    t => t.PaymentMethod!,
                    t => t.TransactionType
                    );
                result.Result = transactionDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetTransactionHistory(Guid customerId, TransactionType? type)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var transactionDb = await _repository.GetAllDataByExpression(t => (!type.HasValue || type.Value == 0 || (type.HasValue && type.Value == t.TransactionTypeId)) &&
                                                                                        (t.OrderId.HasValue && t.Order.AccountId.Equals(customerId.ToString())
                                                                                        || (t.StoreCreditId.HasValue && t.StoreCredit.AccountId.Equals(customerId.ToString()))
                                                                                        )
                                                                                        , 0, 0, null, false,
                                                                                    t => t.StoreCredit,
                                                                                    t => t.Order,
                                                                                    t => t.TransactionType,
                                                                                    t => t.TransationStatus);

                result.Result = transactionDb;
            }
            catch (Exception ex)
            {
                result.Result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }


        public async Task<AppActionResult> GetTransactionById(Guid paymentId)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var data = new TransactionReponse();
                var transactionDb = await _repository.GetByExpression(t => t.Id == paymentId, t => t.StoreCredit);
                data.Transaction = transactionDb;
                if (transactionDb.OrderId.HasValue)
                {
                    var orderService = Resolve<IOrderService>();
                    var order = await orderService.GetAllReservationDetail(transactionDb.OrderId.Value);
                    data.Order = order.Result as ReservationReponse;
                }
                result.Result = data;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        //public async Task<AppActionResult> GetPaymentById(Guid paymentId)
        //{
        //    var result = new AppActionResult();
        //    try
        //    {
        //        var transactionDb = await _repository.GetByExpression(p => p.Id == paymentId, p => p.Order!, p => p.Reservation!, p => p.PaymentMethod!);
        //        if (transactionDb == null)
        //        {
        //            result = BuildAppActionResultError(result, $"Hóa đơn với id {paymentId} không tồn tại");
        //        }
        //        result.Result = transactionDb;      
        //    }
        //    catch (Exception ex)
        //    {
        //        result = BuildAppActionResultError(result, ex.Message);
        //    }
        //    return result;
        //}

        public async Task<AppActionResult> UpdateTransactionStatus(Guid transactionId, TransationStatus transactionStatus)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var transactionDb = await _repository.GetById(transactionId);
                if(transactionDb.TransationStatusId == TransationStatus.PENDING && transactionStatus != TransationStatus.APPLIED)
                {
                    transactionDb.TransationStatusId = transactionStatus;
                    if(transactionStatus == TransationStatus.SUCCESSFUL)
                    {
                        var utility = Resolve<Utility>();
                        transactionDb.PaidDate = utility.GetCurrentDateTimeInTimeZone();
                    }
                    await _repository.Update(transactionDb);
                    await _unitOfWork.SaveChangesAsync();
                    if (transactionDb.OrderId.HasValue)
                    {
                        var orderService = Resolve<IOrderService>();
                        await orderService.ChangeOrderStatus(transactionDb.OrderId.Value, transactionStatus == TransationStatus.SUCCESSFUL);
                    }
                } else
                {
                    result = BuildAppActionResultError(result, $"Để cập nhật, Trạn thanh toán phải chờ xử lí và trạng thái mong muốn phải khác chờ xử lí");
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetLoyaltyPointHistory(Guid customerId)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var loyaltyPointHistoryRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
                var loyaltyPointHistory = await loyaltyPointHistoryRepository.GetAllDataByExpression(l => l.Order.AccountId.Equals(customerId.ToString()), 0, 0, l => l.TransactionDate, false, l => l.Order);
                result.Result = loyaltyPointHistory;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetStoreCreditTransactionHistory(Guid customerId)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var transactionDb = await _repository.GetAllDataByExpression(t => (t.PaymentMethodId == PaymentMethod.STORE_CREDIT || t.TransactionTypeId == TransactionType.CreditStore || t.TransactionTypeId == TransactionType.Refund) &&
                                                                                        (t.OrderId.HasValue && t.Order.AccountId.Equals(customerId.ToString())
                                                                                        || (t.StoreCreditId.HasValue && t.StoreCredit.AccountId.Equals(customerId.ToString()))
                                                                                        )
                                                                                        , 0, 0, null, false,
                                                                                    t => t.StoreCredit,
                                                                                    t => t.Order,
                                                                                    t => t.TransactionType,
                                                                                    t => t.TransationStatus,
                                                                                    t => t.PaymentMethod);
                result.Result = transactionDb;
            }
            catch (Exception ex)
            {

            }
            return result;
        }
    }
}
