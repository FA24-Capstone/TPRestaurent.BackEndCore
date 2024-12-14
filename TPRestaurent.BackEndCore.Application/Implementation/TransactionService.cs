using Firebase.Auth;
using Microsoft.AspNetCore.Identity;
using Firebase.Auth;
using Microsoft.AspNetCore.Identity;
using MailKit.Search;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.Record;
using RestSharp;
using System.Collections.Generic;
using System.Net;
using System.Text;
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

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class TransactionService : GenericBackendService, ITransactionService
    {
        private readonly IGenericRepository<Transaction> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private MomoConfiguration _momoConfiguration;
        private IHubServices.IHubServices _hubServices;

        public TransactionService(IConfiguration configuration,
            IGenericRepository<Transaction> repository, IUnitOfWork unitOfWork, IHubServices.IHubServices hubServices, IServiceProvider service)
            : base(service)
        {
            _configuration = configuration;
            _repository = repository;
            _unitOfWork = unitOfWork;
            _hubServices = hubServices;
            _momoConfiguration = Resolve<MomoConfiguration>();
        }

        public async Task<AppActionResult> CreatePayment(PaymentRequestDto paymentRequest, string returnUrl)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var paymentGatewayService = Resolve<IPaymentGatewayService>();
                var orderRepository = Resolve<IGenericRepository<Order>>();
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var orderService = Resolve<IOrderService>();
                var hashingService = Resolve<IHashingService>();
                var utility = Resolve<Utility>();
                var transaction = new Transaction();
             
                string key = _configuration["HashingKeys:PaymentLink"];
                string paymentUrl = "";
                double amount = 0;
                if (!paymentRequest.OrderId.HasValue && string.IsNullOrEmpty(paymentRequest.AccountId))
                {
                    return BuildAppActionResultError(result, $"Đơn hàng/Đặt bàn/Ví này không tồn tại");
                }

                if (!BuildAppActionResultIsError(result))
                {

                    switch (paymentRequest.PaymentMethod)
                    {
                        case Domain.Enums.PaymentMethod.VNPAY:
                            if (paymentRequest.OrderId.HasValue)
                            {
                                var orderDb =
                                    await orderRepository!.GetByExpression(p => p.OrderId == paymentRequest.OrderId,
                                        p => p.Account!);
                                if (orderDb.StatusId == OrderStatus.Completed ||
                                    orderDb.StatusId == OrderStatus.Cancelled)
                                {
                                 return BuildAppActionResultError(result,
                                        $"Đơn hàng đã hủy hoặc đã được thanh toán thành công");
                                }

                                //Has pending order payement
                                var existingTransaction = await _repository.GetByExpression(p =>
                                    p.OrderId == orderDb.OrderId && p.TransationStatusId == TransationStatus.SUCCESSFUL && p.TransactionTypeId == TransactionType.Order, null);
                                if(existingTransaction != null)
                                {
                                    return BuildAppActionResultError(result,
                                        $"Đơn hàng đang có giao dịch hoặc đã có giao dịch thanh toán thành công");
                                }

                                if ((orderDb.OrderTypeId != OrderType.Delivery &&
                                     (orderDb.StatusId == OrderStatus.TemporarilyCompleted ||
                                      orderDb.StatusId == OrderStatus.Processing))
                                    || orderDb.StatusId == OrderStatus.Pending)
                                {
                                    amount = Math.Ceiling(orderDb.TotalAmount / 1000) * 1000;
                                }
                                else
                                {
                                    if (orderDb.Deposit.HasValue)
                                    {
                                        amount = Math.Ceiling(orderDb.Deposit.Value / 1000) * 1000;
                                    }
                                    else
                                    {
                                        throw new Exception(
                                               $"Số tiền thanh toán không hợp lệ");
                                    }
                                }

                                transaction = new Transaction
                                {
                                    Id = Guid.NewGuid(),
                                    Amount = hashingService.Hashing("", orderDb.TotalAmount, false).Result.ToString(),
                                    PaymentMethodId = Domain.Enums.PaymentMethod.VNPAY,
                                    OrderId = orderDb.OrderId,
                                    Date = utility!.GetCurrentDateTimeInTimeZone(),
                                    TransationStatusId = TransationStatus.PENDING,
                                    TransactionTypeId = orderDb.StatusId == OrderStatus.TableAssigned
                                        ? TransactionType.Deposit
                                        : TransactionType.Order
                                };

                                var paymentInformationRequest = new PaymentInformationRequest
                                {
                                    TransactionID = transaction.Id.ToString(),
                                    PaymentMethod = paymentRequest.PaymentMethod,
                                    Amount = amount,
                                    CustomerName = orderDb!?.Account!?.LastName,
                                    AccountID = orderDb?.AccountId,
                                };

                                await _repository.Insert(transaction);
                                paymentUrl =
                                    await paymentGatewayService!.CreatePaymentUrlVnpay(paymentInformationRequest, returnUrl);

                                result.Result = paymentUrl;
                            }
                            else if (!string.IsNullOrEmpty(paymentRequest.AccountId))
                            {
                                if (paymentRequest.StoreCreditAmount.HasValue)
                                {
                                    var accountDb = await accountRepository!.GetById(paymentRequest.AccountId);
                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = hashingService.Hashing(paymentRequest.AccountId, paymentRequest.StoreCreditAmount.Value, false).Result.ToString(),
                                        PaymentMethodId = Domain.Enums.PaymentMethod.VNPAY,
                                        AccountId = paymentRequest.AccountId,
                                        Date = utility!.GetCurrentDateTimeInTimeZone(),
                                        TransationStatusId = TransationStatus.PENDING,
                                        TransactionTypeId = TransactionType.CreditStore,
                                    };

                                    var paymentInformationRequest = new PaymentInformationRequest
                                    {
                                        TransactionID = transaction.Id.ToString(),
                                        PaymentMethod = paymentRequest.PaymentMethod,
                                        Amount = (double)paymentRequest.StoreCreditAmount,
                                        CustomerName = accountDb!.LastName,
                                        AccountID = accountDb.Id,
                                    };

                                    await _repository.Insert(transaction);
                                    paymentUrl =
                                        await paymentGatewayService!.CreatePaymentUrlVnpay(
                                            paymentInformationRequest, returnUrl);

                                    result.Result = paymentUrl;
                                }
                            }

                            break;
                        case Domain.Enums.PaymentMethod.MOMO:
                            if (paymentRequest.OrderId.HasValue)
                            {
                                var orderDb =
                                    await orderRepository!.GetByExpression(p => p.OrderId == paymentRequest.OrderId,
                                        p => p.Account!);
                                if ((orderDb.OrderTypeId != OrderType.Delivery &&
                                     (orderDb.StatusId == OrderStatus.TemporarilyCompleted ||
                                      orderDb.StatusId == OrderStatus.Processing))
                                    || orderDb.StatusId == OrderStatus.Pending)
                                {
                                    amount = Math.Ceiling(orderDb.TotalAmount / 1000) * 1000;
                                }
                                else
                                {
                                    if (orderDb.Deposit.HasValue)
                                    {
                                        amount = Math.Ceiling(orderDb.Deposit.Value / 1000) * 1000;
                                    }
                                    else
                                    {
                                        throw new Exception(
                                               $"Số tiền thanh toán không hợp lệ");
                                    }
                                }

                                transaction = new Transaction
                                {
                                    Id = Guid.NewGuid(),
                                    Amount = hashingService.Hashing("", amount, false).Result.ToString(),
                                    PaymentMethodId = Domain.Enums.PaymentMethod.MOMO,
                                    OrderId = orderDb.OrderId,
                                    Date = utility!.GetCurrentDateTimeInTimeZone(),
                                    TransationStatusId = TransationStatus.PENDING,
                                    TransactionTypeId = orderDb.StatusId == OrderStatus.TableAssigned
                                        ? TransactionType.Deposit
                                        : TransactionType.Order
                                };

                                string endpoint = _momoConfiguration.Api;
                                string partnerCode = _momoConfiguration.PartnerCode;
                                string accessKey = _momoConfiguration.AccessKey;
                                string secretkey = _momoConfiguration.Secretkey;
                                string orderInfo = hashingService.Hashing("OR", key).Result.ToString();
                                string redirectUrl = $"{returnUrl}";
                                string ipnUrl = _momoConfiguration.IPNUrl;
                                string requestType = "payWithATM";

                                string requestId = Guid.NewGuid().ToString();
                                string extraData = transaction.Id.ToString();

                                var transactionAmountResult = hashingService.UnHashing(transaction.Amount, false);
                                var transactionAmount = double.Parse(transactionAmountResult.Result.ToString().Split('_')[1]);
                                string rawHash = "accessKey=" + accessKey +
                                                 "&amount=" + (Math.Ceiling(transactionAmount / 1000) * 1000)
                                                 .ToString() +
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
                            else if (!string.IsNullOrEmpty(paymentRequest.AccountId))
                            {
                                if (paymentRequest.StoreCreditAmount > 0)
                                {
                                    {
                                        var accountDb = await accountRepository!.GetById(paymentRequest.AccountId);
                                        transaction = new Transaction
                                        {
                                            Id = Guid.NewGuid(),
                                            Amount = hashingService.Hashing("", (double)(paymentRequest.StoreCreditAmount), false).Result.ToString(),
                                            PaymentMethodId = Domain.Enums.PaymentMethod.MOMO,
                                            AccountId = accountDb.Id,
                                            Date = utility!.GetCurrentDateTimeInTimeZone(),
                                            TransationStatusId = TransationStatus.PENDING,
                                            TransactionTypeId = TransactionType.CreditStore
                                        };
                                        amount = (double)paymentRequest.StoreCreditAmount;
                                        string endpoint = _momoConfiguration.Api;
                                        string partnerCode = _momoConfiguration.PartnerCode;
                                        string accessKey = _momoConfiguration.AccessKey;
                                        string secretkey = _momoConfiguration.Secretkey;
                                        string orderInfo = hashingService.Hashing("OR", key).Result.ToString();
                                        string redirectUrl = $"{_momoConfiguration.RedirectUrl}";
                                        string ipnUrl = _momoConfiguration.IPNUrl;
                                        string requestType = "payWithATM";

                                        string requestId = Guid.NewGuid().ToString();
                                        string extraData = transaction.OrderId.ToString();

                                        var transactionAmountResult = hashingService.UnHashing(transaction.Amount, false);
                                        var transactionAmount = double.Parse(transactionAmountResult.Result.ToString().Split('_')[1]);
                                        string rawHash = "accessKey=" + accessKey +
                                                         "&amount=" +
                                                         (Math.Ceiling(transactionAmount / 1000) * 1000)
                                                         .ToString() +
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

                                ////
                            }

                            break;
                        case Domain.Enums.PaymentMethod.STORE_CREDIT:
                            if (paymentRequest.OrderId.HasValue)
                            {
                                var orderDb =
                                    await orderRepository!.GetByExpression(p => p.OrderId == paymentRequest.OrderId,
                                        p => p.Account!);
                                Account accountDb = null;
                                if (!string.IsNullOrEmpty(orderDb.AccountId))
                                {
                                    accountDb = await accountRepository.GetById(orderDb.AccountId);
                                } else if (!string.IsNullOrEmpty(paymentRequest.AccountId))
                                {
                                    accountDb = await accountRepository.GetById(paymentRequest.AccountId);
                                }

                                if (accountDb == null)
                                {
                                    throw new Exception(
                                           $"Không tìm thấy số dư tài khoản khách hàng");
                                }

                                if (orderDb.Account == null)
                                {
                                    
                                }

                                if ((orderDb.OrderTypeId != OrderType.Delivery &&
                                     (orderDb.StatusId == OrderStatus.TemporarilyCompleted ||
                                      orderDb.StatusId == OrderStatus.Processing))
                                    || orderDb.StatusId == OrderStatus.Pending)
                                {
                                    amount = Math.Ceiling(orderDb.TotalAmount / 1000) * 1000;
                                }
                                else
                                {
                                    if (orderDb.Deposit.HasValue)
                                    {
                                        amount = Math.Ceiling(orderDb.Deposit.Value / 1000) * 1000;
                                    }
                                    else
                                    {
                                        throw new Exception(
                                               $"Số tiền thanh toán không hợp lệ");
                                    }
                                }


                                var storeCreditAmountResult = hashingService.UnHashing(accountDb.StoreCreditAmount, false);
                                int storeCreditAmount = 0;
                                if (storeCreditAmountResult.IsSuccess)
                                {
                                    storeCreditAmount = int.Parse(storeCreditAmountResult.Result.ToString().Split('_')[1]);
                                } else
                                {
                                    storeCreditAmount = int.Parse(accountDb.StoreCreditAmount);
                                }

                                if (storeCreditAmount < amount)
                                {
                                    throw new Exception(
                                           $"Số dư tài khoản của quý khách không đủ");
                                }

                                storeCreditAmount -= (int)amount;
                                accountDb.StoreCreditAmount = hashingService.Hashing(accountDb.Id, storeCreditAmount, false).Result.ToString();
                                transaction = new Transaction
                                {
                                    Id = Guid.NewGuid(),
                                    Amount = hashingService.Hashing(accountDb.Id, amount, false).Result.ToString(),
                                    PaymentMethodId = Domain.Enums.PaymentMethod.STORE_CREDIT,
                                    OrderId = orderDb.OrderId,
                                    Date = utility!.GetCurrentDateTimeInTimeZone(),
                                    PaidDate = utility!.GetCurrentDateTimeInTimeZone(),
                                    TransationStatusId = TransationStatus.SUCCESSFUL,
                                    TransactionTypeId = orderDb.StatusId == OrderStatus.TableAssigned
                                        ? TransactionType.Deposit
                                        : TransactionType.Order
                                };

                                await _repository.Insert(transaction);
                                await accountRepository.Update(accountDb);
                                //await orderService.ChangeOrderStatus(orderDb.OrderId, true);
                            }

                            break;
                        default:
                            if (paymentRequest.PaymentMethod == PaymentMethod.ZALOPAY)
                            {
                                throw new Exception(
                                       $"Hệ thống chưa hỗ trợ thanh toán với ZALOPAY");
                            }

                            if (paymentRequest.OrderId.HasValue)
                            {
                                var orderDb =
                                    await orderRepository!.GetByExpression(p => p.OrderId == paymentRequest.OrderId,
                                        p => p.Account!);
                                if ((orderDb.OrderTypeId != OrderType.Delivery &&
                                     (orderDb.StatusId == OrderStatus.TemporarilyCompleted ||
                                      orderDb.StatusId == OrderStatus.Processing))
                                    || orderDb.StatusId == OrderStatus.Pending)
                                {
                                    amount = Math.Ceiling(orderDb.TotalAmount / 1000) * 1000;
                                }
                                else
                                {
                                    if (orderDb.Deposit.HasValue)
                                    {
                                        amount = Math.Ceiling(orderDb.Deposit.Value / 1000) * 1000;
                                    }
                                    else
                                    {
                                        throw new Exception(
                                               $"Số tiền thanh toán không hợp lệ");
                                    }
                                }

                                transaction = new Transaction
                                {
                                    Id = Guid.NewGuid(),
                                    Amount = hashingService.Hashing("", amount, false).Result.ToString(),
                                    PaymentMethodId = Domain.Enums.PaymentMethod.Cash,
                                    OrderId = orderDb.OrderId,
                                    Date = utility!.GetCurrentDateTimeInTimeZone(),
                                    TransationStatusId = TransationStatus.PENDING,
                                    TransactionTypeId = orderDb.StatusId == OrderStatus.TableAssigned
                                        ? TransactionType.Deposit
                                        : TransactionType.Order
                                };
                                await _repository.Insert(transaction);
                                result.Result = transaction;
                            }
                            else if (!string.IsNullOrEmpty(paymentRequest.AccountId))
                            {
                                if (paymentRequest.StoreCreditAmount.HasValue)
                                {
                                    var storeCreditDb = await accountRepository!.GetById(paymentRequest.AccountId);

                                    if (storeCreditDb == null)
                                    {
                                        return BuildAppActionResultError(result,
                                            $"Khong tìm thấy thông tin ví với id {paymentRequest.AccountId}");
                                    }

                                    transaction = new Transaction
                                    {
                                        Id = Guid.NewGuid(),
                                        Amount = hashingService.Hashing(storeCreditDb.Id, paymentRequest.StoreCreditAmount.Value, false).Result.ToString(),
                                        PaymentMethodId = Domain.Enums.PaymentMethod.Cash,
                                        AccountId = storeCreditDb.Id,
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
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
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

        public async Task<AppActionResult> GetAllTransaction(Domain.Enums.TransationStatus? transactionStatus, string? phoneNumber, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var transactionDb = await _repository.GetAllDataByExpression(t => (!transactionStatus.HasValue || (transactionStatus.HasValue && transactionStatus.Value == t.TransationStatusId))
                                                                                  && (string.IsNullOrEmpty(phoneNumber)
                                                                                        || (!string.IsNullOrEmpty(phoneNumber)
                                                                                            && (t.Order.Account.PhoneNumber.Contains(phoneNumber)
                                                                                            || t.Account.PhoneNumber.Contains(phoneNumber))))
                                                                                   , pageNumber, pageSize, p => p.Date, false,
                    t => t.Account!,
                    t => t.Order!.Account!,
                    t => t.TransationStatus,
                    t => t.PaymentMethod!,
                    t => t.TransactionType
                    );
                result.Result = new PagedResult<Transaction>
                {
                    Items = GetDecodedTransactionList(transactionDb.Items),
                    TotalPages = transactionDb.TotalPages
                };
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
                                                                                        || (!string.IsNullOrEmpty(t.AccountId) && t.AccountId.Equals(customerId.ToString()))
                                                                                        )
                                                                                        , 0, 0, t => t.Date, false,
                                                                                    t => t.Order,
                                                                                    t => t.TransactionType,
                                                                                    t => t.TransationStatus);

                result.Result = new PagedResult<Transaction>
                {
                    Items = GetDecodedTransactionList(transactionDb.Items),
                    TotalPages = transactionDb.TotalPages,
                };
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetTransactionById(Guid paymentId)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var hashingService = Resolve<IHashingService>();
                var data = new TransactionReponse();
                var transactionDb = await _repository.GetByExpression(t => t.Id == paymentId, t => t.TransactionType, t => t.TransationStatus);
                var decodedTransactionAmountResult = hashingService.UnHashing(transactionDb.Amount, false).Result.ToString();
                transactionDb.Amount = decodedTransactionAmountResult.Split('_')[decodedTransactionAmountResult.Split('_').Length - 1];
                data.Transaction = transactionDb;
                if (transactionDb.OrderId.HasValue)
                {
                    var orderService = Resolve<IOrderService>();
                    var order = await orderService.GetAllOrderDetail(transactionDb.OrderId.Value);
                    var orderResult = order.Result as OrderWithDetailReponse;
                    await CalculateTotal(orderResult);
                    data.Order = orderResult;
                }
                result.Result = data;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> UpdateTransactionStatus(Guid transactionId, TransationStatus transactionStatus)
        {
            AppActionResult result = new AppActionResult();
            await _unitOfWork.ExecuteInTransaction(async () =>
            {
                try
                {
                    var storeCreditService = Resolve<IStoreCreditService>();
                    var orderRepository = Resolve<IGenericRepository<Order>>();
                    var orderService = Resolve<IOrderService>();
                    var transactionDb = await _repository.GetById(transactionId);
                    if (transactionDb.TransationStatusId == TransationStatus.PENDING && transactionStatus != TransationStatus.APPLIED)
                    {
                        transactionDb.TransationStatusId = transactionStatus;
                        if (transactionStatus == TransationStatus.SUCCESSFUL)
                        {
                            var utility = Resolve<Utility>();
                            transactionDb.PaidDate = utility.GetCurrentDateTimeInTimeZone();
                        }
                        await _repository.Update(transactionDb);
                        await _unitOfWork.SaveChangesAsync();

                        if (transactionStatus == TransationStatus.SUCCESSFUL)
                        {
                            if (transactionDb.OrderId.HasValue)
                            {
                                await orderService.ChangeOrderStatusService(transactionDb.OrderId.Value, transactionStatus == TransationStatus.SUCCESSFUL, null, false);

                                var orderDb = await orderRepository.GetById(transactionDb.OrderId);
                                if (orderDb != null && (orderDb.OrderTypeId == OrderType.Reservation || orderDb.OrderTypeId == OrderType.Delivery))
                                {
                                    await _hubServices.SendAsync(SD.SignalMessages.LOAD_ORDER_SESIONS);
                                    await _hubServices.SendAsync(SD.SignalMessages.LOAD_GROUPED_DISHES);
                                }
                            }
                            else
                            {
                                await storeCreditService.AddStoreCredit(transactionId);
                            }
                        }
                        await _hubServices.SendAsync(SD.SignalMessages.LOAD_USER_ORDER);
                    }
                    else
                    {
                        throw new Exception($"Để cập nhật, Trạng thanh toán phải chờ xử lí và trạng thái mong muốn phải khác chờ xử lí");
                    }
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
            });
            return result;
        }

        public async Task<AppActionResult> GetLoyaltyPointHistory(Guid customerId)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var loyaltyPointHistoryRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
                var loyaltyPointHistory = await loyaltyPointHistoryRepository.GetAllDataByExpression(l => l.Order.AccountId.Equals(customerId.ToString()), 0, 0, l => l.TransactionDate, false, l => l.Order);
                result.Result = new PagedResult<LoyalPointsHistory>
                {
                    Items = GetDecodedLoyaltyPointHistoryList(loyaltyPointHistory.Items),
                    TotalPages = loyaltyPointHistory.TotalPages
                };
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
                                                                                        || (!string.IsNullOrEmpty(t.AccountId) && t.AccountId.Equals(customerId.ToString()))
                                                                                        )
                                                                                        && (t.TransationStatusId == TransationStatus.SUCCESSFUL || t.TransationStatusId == TransationStatus.APPLIED)
                                                                                        , 0, 0, t => t.Date, false,
                                                                                    t => t.Order,
                                                                                    t => t.TransactionType,
                                                                                    t => t.TransationStatus,
                                                                                    t => t.PaymentMethod);
                result.Result = new PagedResult<Transaction>
                {
                    Items = GetDecodedTransactionList(transactionDb.Items),
                    TotalPages = transactionDb.TotalPages
                };
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public async Task<AppActionResult> CreateRefund(Domain.Models.Order order, bool asCustomer)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var hashingService = Resolve<IHashingService>();
                if (order == null)
                {
                    throw new Exception($"Không tìm thấy đơn hàng");
                }
                if (!order.CancelledTime.HasValue || order.StatusId != OrderStatus.Cancelled)
                {
                    throw new Exception($"Đơn hàng chưa được huỷ");
                }

                var utility = Resolve<Utility>();
                var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var emailService = Resolve<IEmailService>();
                var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();

                var paidDepositOrder = await _repository.GetAllDataByExpression(r => r.OrderId == order.OrderId
                                                                                     && (r.TransactionTypeId == TransactionType.Deposit && r.Order.OrderTypeId == OrderType.Reservation
                                                                                        || r.TransactionTypeId == TransactionType.Order && r.Order.OrderTypeId == OrderType.Delivery)
                                                                                    && r.TransationStatusId == TransationStatus.SUCCESSFUL, 0, 0, null, false, null);

                if (paidDepositOrder.Items.Count() == 0)
                {
                    return result;
                }

                var refundedOrderDb = await _repository.GetAllDataByExpression(r => r.OrderId == order.OrderId && r.TransactionTypeId == TransactionType.Refund
                                                                                    && r.TransationStatusId == TransationStatus.SUCCESSFUL, 0, 0, null, false, null);

                if (refundedOrderDb.Items.Count() > 0)
                {
                    throw new Exception($"Đơn hàng đã được hàng tiền");
                }

                var timeConfigurationDb = await configurationRepository.GetByExpression(t => t.Name.Equals(SD.DefaultValue.TIME_FOR_REFUND));
                if (timeConfigurationDb == null)
                {
                    throw new Exception($"không tìm thấy cấu hình tên {SD.DefaultValue.TIME_FOR_REFUND}");
                }

                if (asCustomer && (order.MealTime - order.CancelledTime).Value.Hours > double.Parse(timeConfigurationDb.CurrentValue))
                {
                    return result;
                }

                Configuration percentageConfigurationDb = null;
                if (asCustomer)
                {
                    percentageConfigurationDb = await configurationRepository.GetByExpression(t => t.Name.Equals(SD.DefaultValue.REFUND_PERCENTAGE_AS_CUSTOMER));
                }
                else
                {
                    percentageConfigurationDb = await configurationRepository.GetByExpression(t => t.Name.Equals(SD.DefaultValue.REFUND_PERCENTAGE_AS_ADMIN));
                }
                if (percentageConfigurationDb == null)
                {
                    throw new Exception($"không tìm thấy cấu hình tên {SD.DefaultValue.REFUND_PERCENTAGE_AS_ADMIN}");
                }

                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var refundAmount = order.Deposit.HasValue ? Math.Ceiling((double)(order.Deposit * double.Parse(percentageConfigurationDb.CurrentValue.ToString())) / 1000) * 1000
                                                          : Math.Ceiling((double)(order.TotalAmount * double.Parse(percentageConfigurationDb.CurrentValue.ToString())) / 1000) * 1000;
                var refundTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    TransactionTypeId = TransactionType.Refund,
                    Amount = hashingService.Hashing("", refundAmount, false).Result.ToString(),
                    AccountId = order.AccountId,
                    Date = currentTime,
                    PaidDate = currentTime,
                    OrderId = order.OrderId,
                    TransationStatusId = TransationStatus.SUCCESSFUL,
                    PaymentMethodId = PaymentMethod.STORE_CREDIT
                };

                var accountDb = await accountRepository.GetById(order.AccountId);

                var storeCreditAmountResult = hashingService.UnHashing(accountDb.StoreCreditAmount, false);
                var storeCreditAmount = int.Parse(storeCreditAmountResult.Result.ToString().Split('_')[1]);
                storeCreditAmount += (int)refundAmount;

                accountDb.StoreCreditAmount = hashingService.Hashing(accountDb.Id, storeCreditAmount, false).Result.ToString();
                var configurationDb = await configurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.EXPIRE_TIME_FOR_STORE_CREDIT), null);
                if (configurationDb == null)
                {
                    return BuildAppActionResultError(result, $"Xảy ra lỗi khi ghi lại thông tin nạp tiền. Vui lòng thử lại");
                }
                var expireTimeInDay = double.Parse(configurationDb.CurrentValue);

                accountDb.ExpiredDate = utility.GetCurrentDateInTimeZone().AddDays(expireTimeInDay);

                await _repository.Insert(refundTransaction);
                await accountRepository.Update(accountDb);
                await _unitOfWork.SaveChangesAsync();

                //var tableDetailDb = await tableDetailRepository.GetAllDataByExpression(p => p.OrderId == order.OrderId, 0, 0, null, false, p => p!.Table!, p => p.Table!.Room!, p => p.Table!.TableSize!);
                //var tableDetail = tableDetailDb!.Items!.FirstOrDefault();
                //emailService.SendEmail(accountDb.Email, "THÔNG BÁO HUỶ ĐẶT BÀN TẠI NHÀ HÀNG THIÊN PHÚ", TemplateMappingHelper.GetTemplateMailToCancelReservation(accountDb.FirstName, order, tableDetail));

                result.Result = refundTransaction;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> CreateDepositRefund(DepositRefundRequest request)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var utility = Resolve<Utility>();
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var hashingService = Resolve<IHashingService>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var refundTransaction = new Transaction
                {
                    OrderId = request.OrderId,
                    Amount = hashingService.Hashing("", request.RefundAmount, false).Result.ToString(),
                    Date = currentTime,
                    PaidDate = currentTime,
                    TransactionTypeId = TransactionType.Refund,
                    TransationStatusId = TransationStatus.SUCCESSFUL
                };

                if (request.PaymentMethod == PaymentMethod.Cash)
                {
                    refundTransaction.PaymentMethodId = PaymentMethod.Cash;
                }
                else
                {

                    refundTransaction.PaymentMethodId = PaymentMethod.STORE_CREDIT;
                    var storeCreditAmountResult = hashingService.UnHashing(request.Account.StoreCreditAmount, false);
                    var storeCreditAmount = int.Parse(storeCreditAmountResult.Result.ToString().Split('_')[1]);
                    storeCreditAmount += (int)request.RefundAmount;

                    request.Account.StoreCreditAmount = hashingService.Hashing(request.Account.Id, storeCreditAmount, false).Result.ToString();
                    await accountRepository.Update(request.Account);
                }

                await _repository.Insert(refundTransaction);
                await _unitOfWork.SaveChangesAsync();
                result.Result = refundTransaction;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        [Hangfire.Queue("cancel-pending-transaction")]
        public async Task CancelPendingTransaction()
        {
            try
            {
                var transactionDb = await _repository.GetAllDataByExpression(p => p.TransationStatusId == TransationStatus.PENDING, 0, 0, null, false, null);
                if (transactionDb!.Items!.Count > 0 && transactionDb.Items != null)
                {
                    await _repository.DeleteRange(transactionDb.Items);
                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
            Task.CompletedTask.Wait();
        }

        private async Task CalculateTotal(OrderWithDetailReponse order)
        {
            try
            {
                var couponRepository = Resolve<IGenericRepository<Coupon>>();
                var initialTotal = order.OrderDishes.Sum(o =>
                {
                    if (o.DishSizeDetail != null)
                    {
                        return Math.Ceiling(o.Quantity * o.DishSizeDetail.Price * (1 - o.DishSizeDetail.Discount) / 1000) * 1000;
                    }
                    return Math.Ceiling(o.Quantity * o.ComboDish.Combo.Price * (1 - o.ComboDish.Combo.Discount) / 1000) * 1000;
                });
                if (order.Order.Deposit.HasValue)
                {
                    initialTotal -= order.Order.Deposit.Value;
                }
                var couponDb = await couponRepository.GetAllDataByExpression(c => c.OrderId == order.Order.OrderId, 0, 0, null, false, c => c.CouponProgram);
                var discountPercent = ((double)couponDb.Items.Sum(c => c.CouponProgram.DiscountPercent) / 100);
                order.Order.CouponDiscount = Math.Ceiling(initialTotal * discountPercent / 1000) * 1000;
                order.Order.LoyaltyPointDiscount = initialTotal - order.Order.CouponDiscount - order.Order.TotalAmount;
            }
            catch (Exception ex)
            {

            }

        }

        public async Task<AppActionResult> CreateDuplicatedPaidOrderRefund(DuplicatedPaidOrderRefundRequest request)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                //varlidate order
                var orderRepository = Resolve<IGenericRepository<Order>>();
                var hashingService = Resolve<IHashingService>();
                var emailService = Resolve<IEmailService>();
                var utility = Resolve<Utility>();
                var orderDb = await orderRepository.GetByExpression(p => p.OrderId == request.OrderId & p.StatusId == OrderStatus.Completed, o => o.Account);

                if (orderDb == null)
                {
                    return BuildAppActionResultError(result, $"Đơn hàng không tồn tại hoặc chưa được thanh toán");
                }

                var transactionDb = await _repository.GetAllDataByExpression(p => p.OrderId == request.OrderId, 0, 0, null, false, null);

                var successfulTransaction = transactionDb.Items.FirstOrDefault(t => t.TransactionTypeId == TransactionType.Order && t.TransationStatusId == TransationStatus.SUCCESSFUL);
                if (successfulTransaction == null)
                {
                    return BuildAppActionResultError(result, $"Đơn hàng không có lịch sử thanh toán thành công");
                }
                //One failed after that as order is paid
                var failedAfterSuccessfulTransaction = transactionDb.Items.FirstOrDefault(t => t.TransactionTypeId == TransactionType.Order && t.TransationStatusId == TransationStatus.FAILED && t.PaidDate >= successfulTransaction.PaidDate);
                if (failedAfterSuccessfulTransaction == null)
                {
                    return BuildAppActionResultError(result, $"Đơn hàng không có giao dịch thất bại sau thanh toán thành công");
                }
                //Check if order has been refunded
                var existedRefundTransaction = transactionDb.Items.FirstOrDefault(t => t.TransactionTypeId == TransactionType.Refund && t.TransationStatusId == TransationStatus.SUCCESSFUL && t.PaidDate >= failedAfterSuccessfulTransaction.PaidDate);
                if (existedRefundTransaction != null)
                {
                    return BuildAppActionResultError(result, $"Đơn hàng đã được hoàn tiền");
                }

                if (orderDb.Account == null && request.PaymentMethod == PaymentMethod.STORE_CREDIT)
                {
                    return BuildAppActionResultError(result, $"Đơn hàng không có thông tin tài khoản dể dùng phương thức hoàn tiền vào số dư");
                }

                var refundTransaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    Amount = successfulTransaction.Amount,
                    Date = utility.GetCurrentDateTimeInTimeZone(),
                    PaidDate = utility.GetCurrentDateTimeInTimeZone(),
                    OrderId = request.OrderId,
                    TransationStatusId = TransationStatus.SUCCESSFUL,
                    TransactionTypeId = TransactionType.Refund,
                    PaymentMethodId = request.PaymentMethod,
                    AccountId = orderDb.AccountId
                };

                if(request.PaymentMethod == PaymentMethod.STORE_CREDIT)
                {
                    var accountRepository = Resolve < IGenericRepository<Account>>();
                    var storeCreditAmountResult = hashingService.UnHashing(orderDb.Account.StoreCreditAmount, false);
                    var storeCreditAmount = int.Parse(storeCreditAmountResult.Result.ToString().Split('_')[1]);
                    storeCreditAmount += (int)orderDb.TotalAmount;

                    orderDb.Account.StoreCreditAmount = hashingService.Hashing(orderDb.AccountId, storeCreditAmount, false).Result.ToString();
                    await accountRepository.Update(orderDb.Account);
                }
                await _repository.Insert(refundTransaction);
                await _unitOfWork.SaveChangesAsync();

                emailService.SendEmail(orderDb.Account.Email, SD.SubjectMail.NOTIFY_DUPLICATED_PAYMENT_OF_ORDER, TemplateMappingHelper.GetTemplateRefundDuplicatedPaymentForCustomer("", orderDb));
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> SendDuplicatedRefundEmail(Guid orderId)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var orderRepository = Resolve<IGenericRepository<Order>>();
                var emailService = Resolve<IEmailService>();
                var utility = Resolve<Utility>();
                var orderDb = await orderRepository.GetByExpression(p => p.OrderId == orderId & p.StatusId == OrderStatus.Completed, o => o.Account, o => o.CustomerInfoAddress);

                if (orderDb == null)
                {
                    return BuildAppActionResultError(result, $"Đơn hàng không tồn tại hoặc chưa được thanh toán");
                }

                var transactionDb = await _repository.GetAllDataByExpression(p => p.OrderId == orderId, 0, 0, null, false, null);

                //var successfulTransaction = transactionDb.Items.FirstOrDefault(t => t.TransactionTypeId == TransactionType.Order && t.TransationStatusId == TransationStatus.SUCCESSFUL);
                //if (successfulTransaction == null)
                //{
                //    return BuildAppActionResultError(result, $"Đơn hàng không có lịch sử thanh toán thành công");
                //}
                ////One failed after that as order is paid
                //var failedAfterSuccessfulTransaction = transactionDb.Items.FirstOrDefault(t => t.TransactionTypeId == TransactionType.Order && t.TransationStatusId == TransationStatus.FAILED && t.PaidDate >= successfulTransaction.PaidDate);
                //if (failedAfterSuccessfulTransaction == null)
                //{
                //    return BuildAppActionResultError(result, $"Đơn hàng không có giao dịch thất bại sau thanh toán thành công");
                //}
                ////Check if order has been refunded
                //var existedRefundTransaction = transactionDb.Items.FirstOrDefault(t => t.TransactionTypeId == TransactionType.Refund && t.TransationStatusId == TransationStatus.SUCCESSFUL && t.PaidDate >= failedAfterSuccessfulTransaction.PaidDate);
                //if (existedRefundTransaction != null)
                //{
                //    return BuildAppActionResultError(result, $"Đơn hàng đã được hoàn tiền");
                //}

                emailService.SendEmail(orderDb.Account.Email, SD.SubjectMail.NOTIFY_DUPLICATED_PAYMENT_OF_ORDER, TemplateMappingHelper.GetTemplateRefundDuplicatedPaymentForCustomer("", orderDb));
                var adminEmail = await GetAdminEmails();
                foreach(var email in adminEmail)
                {
                    emailService.SendEmail(email, SD.SubjectMail.NOTIFY_DUPLICATED_PAYMENT_OF_ORDER, TemplateMappingHelper.GetTemplateRefundDuplicatedPaymentForAdmin("", orderDb));
                }

            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        private async Task<List<string>> GetAdminEmails()
        {
            List<string> adminEmails = new List<string>();
            try
            {
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
                var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
                var customerRoleDb = await roleRepository.GetByExpression(r => r.Name.ToLower().Equals("admin"));
                var customerDb = await userRoleRepository.GetAllDataByExpression(null, 0, 0, null, false, null);
                var roleGroup = customerDb.Items.GroupBy(c => c.UserId).ToDictionary(c => c.Key, c => c.ToList());
                var adminIds = roleGroup.Where(c => c.Value.FirstOrDefault().RoleId.Equals(customerRoleDb.Id)).Select(c => c.Key).ToList();
                var adminDb = await accountRepository.GetAllDataByExpression(a => !a.IsDeleted && adminIds.Contains(a.Id), 0, 0, null, false, null);
                adminEmails = adminDb.Items.Select(a => a.Email).ToList();
            }
            catch (Exception ex)
            {
                adminEmails = new List<string>();
            }
            return adminEmails;
        }


     
        public List<Transaction> GetDecodedTransactionList(List<Transaction> transactions)
        {
            try
            {
                var hashingService = Resolve<IHashingService>();
                foreach (var transaction in transactions)
                {
                    var decodedTransaction = hashingService.UnHashing(transaction.Amount, false).Result.ToString();
                    transaction.Amount = decodedTransaction.Split('_')[decodedTransaction.Split('_').Length - 1];
                }
            }
            catch (Exception ex)
            {

            }
            return transactions;
        }

        public List<LoyalPointsHistory> GetDecodedLoyaltyPointHistoryList(List<LoyalPointsHistory> transactions)
        {
            try
            {
                var hashingService = Resolve<IHashingService>();
                foreach (var transaction in transactions)
                {
                    var decodedPointChanged = hashingService.UnHashing(transaction.PointChanged, true).Result.ToString();
                    transaction.PointChanged = decodedPointChanged.Split('_')[1];
                    var decodedNewBalance = hashingService.UnHashing(transaction.NewBalance, true).Result.ToString();
                    transaction.NewBalance = decodedNewBalance.Split('_')[1];
                }
            }
            catch (Exception ex)
            {

            }
            return transactions;
        }

        [Hangfire.Queue("log-money-information-hacked")]
        public async Task<string> LogMoneyInformationHacked()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var loyaltyPointRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
                var hashingService = Resolve<IHashingService>();
                var emailService = Resolve<IEmailService>();
                var notificationService = Resolve<INotificationMessageService>();
                var utility = Resolve<Utility>();
                var customerIds = await GetCustomerId();
                var accountDb = await accountRepository.GetAllDataByExpression(a => !a.IsDeleted && customerIds.Contains(a.Id), 0, 0, null, false, null);
                bool ShouldNoticeManager = false;
                if(accountDb.Items.Count > 0)
                {
                    foreach (var account in accountDb.Items)
                    {
                        var userName = account.FirstName + " " + account.LastName;  
                        string accountLogMessage = await CheckHacked(account, hashingService, loyaltyPointRepository);
                        if (!string.IsNullOrEmpty(accountLogMessage))
                        {
                            logMessage.Append($"Thông tin số dư và điểm thưởng của account với {account.Id} - {userName} - {account.PhoneNumber} có dấu hiệu bất thường. |");
                            logMessage.Append(accountLogMessage);
                            if (!ShouldNoticeManager)
                            {
                                ShouldNoticeManager = true;
                            }
                            //account.IsBanned = true;
                        }

                    }
                    Logger.WriteLog(new LogDto
                    {
                        Id = Guid.NewGuid(),
                        Timestamp = utility.GetCurrentDateTimeInTimeZone(),
                        Message = logMessage.ToString()
                    });

                    if (ShouldNoticeManager)
                    {
                        //await accountRepository.UpdateRange(accountDb.Items);
                        //await _unitOfWork.SaveChangesAsync();

                        //Send Email and FCM to All Admin
                        var adminEmail = await GetAdminEmails();
                        foreach (var email in adminEmail)
                        {
                            emailService.SendEmail(email, SD.SubjectMail.NOTIFY_ABNORMAL_DB_MODIFICATION, TemplateMappingHelper.GetTemplateForDatabaseNotification());
                        }
                        var message = "THÔNG BÁO CÓ SỬA ĐỔI BẤT THƯỜNG TRONG CƠ SỞ DỮ LIỆU";
                        await notificationService!.SendNotificationToRoleAsync(SD.RoleName.ROLE_ADMIN, message);

                    }
                }
            }
            catch (Exception ex)
            {

            }
            return logMessage.ToString();
        }

        private async Task<string> CheckHacked(Account account, IHashingService hashingService, IGenericRepository<LoyalPointsHistory> loyaltyPointRepository)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                int i = 1;
                var storeCredit = hashingService.UnHashing(account.StoreCreditAmount, false);
                var loyalPoint = hashingService.UnHashing(account.LoyaltyPoint, true);

                string accountInfo = "";
                if (account != null)
                {
                    accountInfo = $"của {account.FirstName} {account.LastName} với sđt (+84){account.PhoneNumber}";
                }
              
                if (!storeCredit.IsSuccess)
                {
                    logMessage.Append($"{i++}. Không thể giải mã số dư ví cho tài khoản. |");
                    return logMessage.ToString();
                }

                if (!storeCredit.Result.ToString().Contains(account.Id))
                {
                    logMessage.Append($"{i++}. Số dư không thuộc về tài khoản. |");
                }
                var storeCreditAmount = int.Parse(storeCredit.Result.ToString().Split('_')[1]);
                var storeCreditDb = await _repository.GetAllDataByExpression(t => (t.TransactionTypeId == TransactionType.CreditStore || t.TransactionTypeId == TransactionType.Refund || t.PaymentMethodId == PaymentMethod.STORE_CREDIT)
                                                                                    && (t.TransationStatusId == TransationStatus.SUCCESSFUL || t.TransationStatusId == TransationStatus.APPLIED)
                                                                                    && (t.AccountId.Equals(account.Id) || t.OrderId.HasValue && t.Order.AccountId.Equals(account.Id)), 0, 0, null, false, t => t.Account, t => t.Order.Account);
                string storeCreditRecord = "";
                foreach (var item in storeCreditDb.Items) 
                {
                    storeCreditRecord = hashingService.UnHashing(item.Amount, false).Result.ToString();
                    if (string.IsNullOrEmpty(storeCreditRecord))
                    {
                        logMessage.Append($"{i++}. Không thể giải mã lịch sử dùng/nạp ví cho tài khoản {accountInfo} (id record {item.Id}). |");
                        continue;
                    }
                    if (!storeCreditRecord.Contains(account.Id))
                    {
                        logMessage.Append($"{i++}. Lịch sử dùng/nạp ví không thuộc về tài khoản  {accountInfo} (id record {item.Id}). |");
                        continue;
                    }
                    if(item.TransactionTypeId == TransactionType.Refund || item.TransactionTypeId == TransactionType.CreditStore)
                    {
                        storeCreditAmount -= int.Parse(storeCreditRecord.Split('_')[1]);
                        Console.WriteLine(-int.Parse(storeCreditRecord.Split('_')[1]));
                    }
                    else
                    {
                        storeCreditAmount += int.Parse(storeCreditRecord.Split('_')[1]);
                        Console.WriteLine(int.Parse(storeCreditRecord.Split('_')[1]));
                    }

                }
                Console.WriteLine();
                if(storeCreditAmount != 0)
                {
                    logMessage.Append($"{i++}. Tổng số dư ví hiện tại không khớp với lịch sử. |");
                }

                if (!loyalPoint.IsSuccess)
                {
                    logMessage.Append($"{i++}Không thể giải mã điểm thành viên cho tài khoản {accountInfo}. |");
                    return logMessage.ToString();
                }

                if (!loyalPoint.Result.ToString().Contains(account.Id))
                {
                    logMessage.Append($"{i++}. Số điểm thành viên không thuộc về tài khoản {accountInfo}. |");

                }
                var loyaltyPointAmount = int.Parse(loyalPoint.Result.ToString().Split('_')[1]);
                var loyaltyPointDb = await loyaltyPointRepository.GetAllDataByExpression(t => t.Order.AccountId.Equals(account.Id)
                                                                                    , 0, 0, null, false, null);
                string loyaltyPointRecord = "";
                string newBalanceRecord = "";
                foreach (var item in loyaltyPointDb.Items.OrderBy(t => t.TransactionDate))
                {
                    loyaltyPointRecord = hashingService.UnHashing(item.PointChanged, true).Result.ToString();
                    newBalanceRecord = hashingService.UnHashing(item.NewBalance, true).Result.ToString();
                    if (string.IsNullOrEmpty(loyaltyPointRecord))
                    {
                        logMessage.Append($"{i++}. Không thể giải mã lịch sử dùng/cộng điểm thưởng cho tài khoản {accountInfo} (id record {item.LoyalPointsHistoryId}). |");
                        continue;
                    }

                    if (string.IsNullOrEmpty(newBalanceRecord))
                    {
                        logMessage.Append($"{i++}. Không thể giải mã lịch sử tổng điểm thưởng mới cho tài khoản {accountInfo} (id record {item.LoyalPointsHistoryId}). |");
                        continue;
                    }

                    if (!loyaltyPointRecord.Contains(account.Id))
                    {
                        logMessage.Append($"{i++}. Lịch sử dùng/cộng điểm thưởng không thuộc về tài khoản {accountInfo} (id record {item.LoyalPointsHistoryId}). |");
                        continue;
                    }

                    if (!newBalanceRecord.Contains(account.Id))
                    {
                        logMessage.Append($"{i++}. Lịch sử tổng điểm thưởng mới không thuộc về tài khoản {accountInfo} (id record {item.LoyalPointsHistoryId}). |");
                        continue;
                    }

                }

            }
            catch (Exception ex)
            {
                logMessage.Append($"Xảy ra lỗi khi kiểm tra thông tin tài khoản của {account.FirstName} {account.LastName} với sđt (+84){account.PhoneNumber} có id {account.Id}");
            }
            return logMessage.ToString();
        }

        private async Task<List<string>> GetCustomerId()
        {
            List<string> customerIds = new List<string>();
            try
            {
                var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
                var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
                var customerRoleDb = await roleRepository.GetByExpression(r => r.Name.ToLower().Equals("customer"));
                var customerDb = await userRoleRepository.GetAllDataByExpression(null, 0, 0, null, false, null);
                var roleGroup = customerDb.Items.GroupBy(c => c.UserId).ToDictionary(c => c.Key, c => c.ToList());
                customerIds = roleGroup.Where(c => c.Value.Count == 1 && c.Value.FirstOrDefault().RoleId.Equals(customerRoleDb.Id)).Select(c => c.Key).ToList();
            }
            catch (Exception ex)
            {
            }
            return customerIds;
        }

        public async Task<string> HashingData()
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var loyaltyPointRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
                var hashingService = Resolve<IHashingService>();
                var utility = Resolve<Utility>();
                var customerIds = await GetCustomerId();
                var accountDb = await accountRepository.GetAllDataByExpression(a => customerIds.Contains(a.Id), 0, 0, null, false, null);
                if (accountDb.Items.Count > 0)
                {
                    foreach (var account in accountDb.Items)
                    {
                        var storeCredit = hashingService.UnHashing(account.StoreCreditAmount, false);
                        var loyalPoint = hashingService.UnHashing(account.LoyaltyPoint, true);
                        if (!storeCredit.IsSuccess)
                        {
                            account.StoreCreditAmount = hashingService.Hashing(account.Id, double.Parse(account.StoreCreditAmount), false).Result.ToString();
                        }

                        if (!loyalPoint.IsSuccess)
                        {
                            account.LoyaltyPoint = hashingService.Hashing(account.Id, double.Parse(account.LoyaltyPoint), true).Result.ToString();
                        }
                    }
                    //var loyaltyPointDb = await loyaltyPointRepository.GetAllDataByExpression(l => !string.IsNullOrEmpty(l.Order.AccountId), 0, 0, null, false, l => l.Order);
                    //foreach (var loyaltyPoint in loyaltyPointDb.Items)
                    //{
                    //    var newBalanceHashed = hashingService.UnHashing(loyaltyPoint.NewBalance, true);
                    //    if (!newBalanceHashed.IsSuccess)
                    //    {
                    //        loyaltyPoint.NewBalance = hashingService.Hashing(loyaltyPoint.Order.AccountId, double.Parse(loyaltyPoint.NewBalance), true).Result.ToString();
                    //        loyaltyPoint.PointChanged = hashingService.Hashing(loyaltyPoint.Order.AccountId, double.Parse(loyaltyPoint.PointChanged), true).Result.ToString();
                    //    } else
                    //    {

                    //    }
                    //}

                    //var storeCreditDb = await _repository.GetAllDataByExpression(null, 0, 0, null, false, s => s.Order);
                    //foreach (var storeCredit in storeCreditDb.Items)
                    //{
                    //    var storeCreditHashed = hashingService.UnHashing(storeCredit.Amount, false);
                    //    if (!storeCreditHashed.IsSuccess)
                    //    {
                    //        storeCredit.Amount = hashingService.Hashing(storeCredit.Order?.AccountId, double.Parse(storeCredit.Amount), false).Result.ToString();
                    //    }
                    //}

                    await accountRepository.UpdateRange(accountDb.Items);
                    //await _repository.UpdateRange(storeCreditDb.Items);
                    //await loyaltyPointRepository.UpdateRange(loyaltyPointDb.Items);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {

            }
            return logMessage.ToString();
        }

        private async Task<string> HashingData(Account account, IHashingService? hashingService, IGenericRepository<LoyalPointsHistory>? loyaltyPointRepository)
        {
            StringBuilder logMessage = new StringBuilder();
            try
            {
                int i = 1;
                var storeCredit = hashingService.UnHashing(account.StoreCreditAmount, false);
                var loyalPoint = hashingService.UnHashing(account.LoyaltyPoint, true);
                if (!storeCredit.IsSuccess) {
                    account.StoreCreditAmount = hashingService.Hashing(account.Id, double.Parse(account.StoreCreditAmount), false).Result.ToString().Split('_')[1];
                }

                if (!loyalPoint.IsSuccess)
                {
                    account.LoyaltyPoint = hashingService.Hashing(account.Id, double.Parse(account.LoyaltyPoint), true).Result.ToString().Split('_')[1];
                }

            }
            catch (Exception ex)
            {
                logMessage.Append($"Xảy ra lỗi khi kiểm tra thông tin tài khoản với id {account.Id}");
            }
            return logMessage.ToString();
        }
    }
}