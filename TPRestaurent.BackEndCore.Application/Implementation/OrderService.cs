using AutoMapper;
using Castle.Core.Internal;
using Castle.Core.Logging;
using Humanizer;
using MailKit;
using MailKit.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;
using static TPRestaurent.BackEndCore.Common.DTO.Response.MapInfo;
using Transaction = TPRestaurent.BackEndCore.Domain.Models.Transaction;
using Utility = TPRestaurent.BackEndCore.Common.Utils.Utility;
using MathNet.Numerics.LinearAlgebra.Storage;
using TPRestaurent.BackEndCore.Application.IHubServices;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class OrderService : GenericBackendService, IOrderService
    {
        private readonly IGenericRepository<Order> _repository;
        private readonly IGenericRepository<OrderDetail> _detailRepository;
        private readonly IGenericRepository<TableDetail> _tableDetailRepository;
        private readonly IGenericRepository<OrderSession> _sessionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private BackEndLogger _logger;
        private IHubServices.IHubServices _hubServices;

        public OrderService(IGenericRepository<Order> repository, IGenericRepository<OrderDetail> detailRepository, IGenericRepository<TableDetail> tableDetailRepository, IGenericRepository<OrderSession> sessionRepository, IUnitOfWork unitOfWork, IMapper mapper, IServiceProvider service, BackEndLogger logger, IHubServices.IHubServices hubServices
        ) : base(service)
        {
            _repository = repository;
            _detailRepository = detailRepository;
            _tableDetailRepository = tableDetailRepository;
            _sessionRepository = sessionRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _hubServices = hubServices;
        }

        public async Task<AppActionResult> AddDishToOrder(AddDishToOrderRequestDto dto)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                AppActionResult result = new AppActionResult();
                try
                {
                    var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                    var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                    var comboRepository = Resolve<IGenericRepository<Combo>>();
                    var dishComboRepository = Resolve<IGenericRepository<DishCombo>>();
                    var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                    var orderSessionRepository = Resolve<IGenericRepository<OrderSession>>();
                    var notificationService = Resolve<INotificationMessageService>();
                    var dishManagementService = Resolve<IDishManagementService>();
                    var hubService = Resolve<IHubServices.IHubServices>();

                    var orderDb = await _repository.GetById(dto.OrderId);
                    var utility = Resolve<Utility>();
                    var orderCombo = false;
                    if (orderDb == null)
                    {
                        result = BuildAppActionResultError(result, $"Không tìm thấy đơn hàng với id {dto.OrderId}");
                        return result;
                    }
                    var orderSessionDb = await orderSessionRepository!.GetAllDataByExpression(null, 0, 0, null, false, null);
                    var latestOrderSession = orderSessionDb.Items!.Count() + 1;
                    var orderSession = new OrderSession()
                    {
                        OrderSessionId = Guid.NewGuid(),
                        OrderSessionTime = utility!.GetCurrentDateTimeInTimeZone(),
                        OrderSessionStatusId = OrderSessionStatus.Confirmed,
                        OrderSessionNumber = latestOrderSession
                    };

                    var orderDetailDb = await orderDetailRepository!.GetAllDataByExpression(o => o.OrderId == dto.OrderId, 0, 0, null, false, null);
                    var orderDetails = new List<OrderDetail>();

                    List<ComboOrderDetail> comboOrderDetails = new List<ComboOrderDetail>();
                    List<CalculatePreparationTime> estimatedPreparationTime = new List<CalculatePreparationTime>();
                    List<DishSizeDetail> dishSizeDetails = new List<DishSizeDetail>();
                    foreach (var o in dto.OrderDetailsDtos)
                    {
                        var orderDetail = new OrderDetail
                        {
                            OrderDetailId = Guid.NewGuid(),
                            OrderId = dto.OrderId,
                            OrderSessionId = orderSession.OrderSessionId
                        };

                        if (o.Combo != null)
                        {
                            var combo = await comboRepository!.GetById(o.Combo.ComboId);
                            orderCombo = true;
                            orderDetail.Price = combo.Price;
                            orderDetail.ComboId = combo.ComboId;

                            foreach (var dishComboId in o.Combo.DishComboIds)
                            {
                                var dishCombo = await dishComboRepository.GetByExpression(d => d.DishComboId == dishComboId, null);
                                var dishSizeDetail = dishSizeDetails.FirstOrDefault(d => d.DishSizeDetailId == dishCombo.DishSizeDetailId);
                                if (dishSizeDetail == null)
                                {
                                    dishSizeDetail = await dishSizeDetailRepository.GetByExpression(d => d.DishSizeDetailId == dishCombo.DishSizeDetailId, d => d.Dish);
                                    dishSizeDetails.Add(dishSizeDetail);
                                }

                                comboOrderDetails.Add(new ComboOrderDetail
                                {
                                    ComboOrderDetailId = Guid.NewGuid(),
                                    DishComboId = dishComboId,
                                    OrderDetailId = orderDetail.OrderDetailId,
                                    PreparationTime = await dishManagementService.CalculatePreparationTime(new List<CalculatePreparationTime>
                                                        {
                                                            new CalculatePreparationTime
                                                            {
                                                                PreparationTime = dishSizeDetail.Dish.PreparationTime.Value,
                                                                Quantity = orderDetail.Quantity
                                                            }
                                                        })
                                });
                                if (dishSizeDetail.QuantityLeft < o.Quantity * dishCombo.Quantity)
                                {
                                    return BuildAppActionResultError(result, $"Món ăn {dishSizeDetail.Dish.Name} chỉ còn x{dishSizeDetail.QuantityLeft}");
                                }
                                dishSizeDetail.QuantityLeft -= o.Quantity * dishCombo.Quantity;
                                if (dishSizeDetail.QuantityLeft == 0)
                                {
                                    dishSizeDetail.IsAvailable = false;
                                }
                                if (dishSizeDetail.QuantityLeft <= 5)
                                {
                                    string message = $"{dishSizeDetail.Dish.Name} chỉ còn x{dishSizeDetail.QuantityLeft} món";
                                    await hubService!.SendAsync(SD.SignalMessages.LOAD_NOTIFICATION);
                                    await notificationService!.SendNotificationToRoleAsync(SD.RoleName.ROLE_ADMIN, message);
                                }

                            }

                            orderDetail.OrderDetailStatusId = OrderDetailStatus.Unchecked;
                            orderDetail.OrderTime = utility!.GetCurrentDateTimeInTimeZone();
                            orderDetail.Quantity = o.Quantity;
                            orderDetail.Note = o.Note;
                            estimatedPreparationTime.Add(new CalculatePreparationTime
                            {
                                PreparationTime = combo.PreparationTime.Value,
                                Quantity = orderDetail.Quantity
                            });
                        }
                        else
                        {
                            var dishSizeDetail = dishSizeDetails.FirstOrDefault(d => d.DishSizeDetailId == o.DishSizeDetailId);
                            if (dishSizeDetail == null)
                            {
                                dishSizeDetail = await dishSizeDetailRepository.GetByExpression(d => d.DishSizeDetailId == o.DishSizeDetailId, d => d.Dish);
                                dishSizeDetails.Add(dishSizeDetail);
                            }
                            orderDetail.Price = dishSizeDetail.Price;
                            orderDetail.DishSizeDetailId = o.DishSizeDetailId;
                            orderDetail.Quantity = o.Quantity;
                            orderDetail.OrderDetailStatusId = OrderDetailStatus.Unchecked;
                            orderDetail.OrderTime = utility!.GetCurrentDateTimeInTimeZone();
                            orderDetail.Note = o.Note;
                            orderDetail.PreparationTime = await dishManagementService.CalculatePreparationTime(new List<CalculatePreparationTime>
                            {
                                new CalculatePreparationTime
                                {
                                    PreparationTime = dishSizeDetail.Dish.PreparationTime.Value,
                                    Quantity = orderDetail.Quantity
                                }
                            });

                            if (dishSizeDetail.QuantityLeft < o.Quantity)
                            {
                                return BuildAppActionResultError(result, $"Món ăn {dishSizeDetail.Dish.Name} chỉ còn x{dishSizeDetail.QuantityLeft}");
                            }

                            dishSizeDetail.QuantityLeft -= o.Quantity;
                            if (dishSizeDetail.QuantityLeft == 0)
                            {
                                dishSizeDetail.IsAvailable = false;
                            }
                            if (dishSizeDetail.QuantityLeft <= 5)
                            {
                                string message = $"{dishSizeDetail.Dish.Name} chỉ còn x{dishSizeDetail.QuantityLeft} món";
                                await hubService!.SendAsync(SD.SignalMessages.LOAD_NOTIFICATION);
                                await notificationService!.SendNotificationToRoleAsync(SD.RoleName.ROLE_ADMIN, message);
                            }

                            estimatedPreparationTime.Add(new CalculatePreparationTime
                            {
                                PreparationTime = dishSizeDetail.Dish.PreparationTime.Value,
                                Quantity = orderDetail.Quantity
                            });
                        }

                        orderDb.TotalAmount += orderDetail.Price * orderDetail.Quantity;
                        orderDetails.Add(orderDetail);
                    }

                    orderSession.PreparationTime = await dishManagementService.CalculatePreparationTime(estimatedPreparationTime);
                    orderDb.TotalAmount = Math.Ceiling(orderDb.TotalAmount / 1000) * 1000;
                    await _repository.Update(orderDb);
                    await orderSessionRepository.Insert(orderSession);
                    await orderDetailRepository.InsertRange(orderDetails);
                    await comboOrderDetailRepository!.InsertRange(comboOrderDetails);
                    await dishSizeDetailRepository!.UpdateRange(dishSizeDetails);
                    await _unitOfWork.SaveChangesAsync();
                    if (orderCombo)
                    {
                        await dishManagementService.UpdateComboAvailability();
                        await dishManagementService.UpdateDishAvailability();
                    }
                    scope.Complete();
                    await _hubServices.SendAsync(SD.SignalMessages.LOAD_ORDER_SESIONS);
                    await _hubServices.SendAsync(SD.SignalMessages.LOAD_GROUPED_DISHES);
                    //AddOrderMessageToChef
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
                return result;
            }
        }

        public async Task<AppActionResult> ChangeOrderStatus(Guid orderId, bool IsSuccessful, OrderStatus? status, bool? requireSignalR = true, bool? asCustomer = false)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var result = new AppActionResult();
                try
                {
                    var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                    var transactionService = Resolve<ITransactionService>();
                    var dishManagementService = Resolve<IDishManagementService>();
                    var orderDb = await _repository.GetById(orderId);
                    var updateDishSizeDetailList = new List<DishSizeDetail>();
                    var utility = Resolve<Utility>();
                    var currentTime = utility.GetCurrentDateTimeInTimeZone();
                    if (orderDb == null)
                    {
                        result = BuildAppActionResultError(result, $"Đơn hàng với id {orderId} không tồn tại");
                    }
                    else
                    {
                        if (orderDb.OrderTypeId == OrderType.Reservation)
                        {
                            if (orderDb.StatusId == OrderStatus.TableAssigned)
                            {
                                if (IsSuccessful)
                                {
                                    var transactionRepository = Resolve<IGenericRepository<Domain.Models.Transaction>>();
                                    var reservationTransactionDb = await transactionRepository.GetByExpression(t => t.OrderId == orderId && t.TransationStatusId == TransationStatus.SUCCESSFUL && t.TransactionTypeId == TransactionType.Deposit, null);
                                    if (reservationTransactionDb == null)
                                    {
                                        result = BuildAppActionResultError(result, $"Không tìm thấy giao dịch thành công cho đơn hàng với id {orderId}");
                                        return result;
                                    }
                                    orderDb.StatusId = OrderStatus.DepositPaid;
                                }
                                else
                                {
                                    orderDb.StatusId = OrderStatus.Cancelled;
                                    orderDb.CancelledTime = utility.GetCurrentDateTimeInTimeZone();
                                    await UpdateCancelledOrderDishQuantity(orderDb, updateDishSizeDetailList, currentTime, false);
                                }
                            }
                            else if (orderDb.StatusId == OrderStatus.DepositPaid)
                            {
                                if (status.HasValue && status.Value == OrderStatus.Processing)
                                {
                                    orderDb.StatusId = OrderStatus.Processing;
                                }
                                else if (!IsSuccessful)
                                {
                                    orderDb.StatusId = OrderStatus.Cancelled;
                                    orderDb.CancelledTime = utility.GetCurrentDateTimeInTimeZone();
                                    await UpdateCancelledOrderDishQuantity(orderDb, updateDishSizeDetailList, currentTime);
                                }
                            }
                            else if (orderDb.StatusId == OrderStatus.TemporarilyCompleted || orderDb.StatusId == OrderStatus.Processing)
                            {
                                if (status.HasValue)
                                {
                                    if (status.Value == OrderStatus.TemporarilyCompleted) orderDb.StatusId = OrderStatus.TemporarilyCompleted;
                                    else if (status.Value == OrderStatus.Processing) orderDb.StatusId = OrderStatus.Processing;
                                }
                                else
                                {
                                    if (IsSuccessful)
                                    {
                                        //Trong DB có transaction có status là successful rồiva2 transaction đó status phải là Order
                                        var transactionRepository = Resolve<IGenericRepository<Domain.Models.Transaction>>();
                                        var reservationTransactionDb = await transactionRepository.GetByExpression(t => t.OrderId == orderId && t.TransationStatusId == TransationStatus.SUCCESSFUL && t.TransactionTypeId == TransactionType.Order, null);
                                        if (reservationTransactionDb == null)
                                        {
                                            result = BuildAppActionResultError(result, $"Không tìm thấy giao dịch thành công cho đơn hàng với id {orderId}");
                                            return result;
                                        }

                                        orderDb.StatusId = OrderStatus.Completed;
                                    }
                                    else if (!asCustomer.Value)
                                    {
                                        orderDb.StatusId = OrderStatus.Cancelled;
                                        orderDb.CancelledTime = utility.GetCurrentDateTimeInTimeZone();
                                        await UpdateCancelledOrderDishQuantity(orderDb, updateDishSizeDetailList, currentTime);
                                    }
                                }
                            }
                            else
                            {
                                result = BuildAppActionResultError(result, $"Đơn hàng không thể cập nhật những trạng thái khác");
                            }
                        }
                        else if (orderDb.OrderTypeId == OrderType.Delivery)
                        {
                            if (orderDb.StatusId == OrderStatus.Pending)
                            {
                                if (IsSuccessful)
                                {
                                    //Trong DB có transaction có status là successful rồiva2 transaction đó status phải là Order
                                    var transactionRepository = Resolve<IGenericRepository<Domain.Models.Transaction>>();
                                    var reservationTransactionDb = await transactionRepository.GetByExpression(t => t.OrderId == orderId && t.TransationStatusId == TransationStatus.SUCCESSFUL && t.TransactionTypeId == TransactionType.Order, null);
                                    if (reservationTransactionDb == null)
                                    {
                                        result = BuildAppActionResultError(result, $"Không tìm thấy giao dịch thành công cho đơn hàng với id {orderId}");
                                        return result;
                                    }
                                    orderDb.StatusId = OrderStatus.Processing;
                                    var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(o => o.OrderId == orderId, 0, 0, null, false, null);
                                    if (orderDetailDb.Items.Count() > 0)
                                    {
                                        await ChangeOrderDetailStatusAfterPayment(orderDetailDb.Items.Where(o => o.OrderDetailStatusId == OrderDetailStatus.Reserved).ToList());
                                    }
                                }
                                else
                                {
                                    orderDb.StatusId = OrderStatus.Cancelled;
                                    orderDb.CancelledTime = utility.GetCurrentDateTimeInTimeZone();
                                    await UpdateCancelledOrderDishQuantity(orderDb, updateDishSizeDetailList, currentTime, false);
                                }
                            }
                            else if (orderDb.StatusId == OrderStatus.Processing && !asCustomer.Value)
                            {
                                if (IsSuccessful)
                                {
                                    orderDb.StatusId = OrderStatus.ReadyForDelivery;
                                }
                                else if (!asCustomer.Value)
                                {
                                    orderDb.StatusId = OrderStatus.Cancelled;
                                    orderDb.CancelledTime = utility.GetCurrentDateTimeInTimeZone();
                                    await UpdateCancelledOrderDishQuantity(orderDb, updateDishSizeDetailList, currentTime);
                                }
                            }
                            else if (orderDb.StatusId == OrderStatus.ReadyForDelivery && !asCustomer.Value)
                            {
                                orderDb.StatusId = IsSuccessful ? OrderStatus.AssignedToShipper : OrderStatus.Cancelled;
                                if (IsSuccessful)
                                {
                                    orderDb.AssignedTime = utility.GetCurrentDateTimeInTimeZone();
                                }
                                else
                                {
                                    orderDb.CancelledTime = utility.GetCurrentDateTimeInTimeZone();
                                }
                            }
                            else if (orderDb.StatusId == OrderStatus.AssignedToShipper && !asCustomer.Value)
                            {
                                orderDb.StatusId = IsSuccessful ? OrderStatus.Delivering : OrderStatus.Cancelled;
                                if (IsSuccessful)
                                {
                                    orderDb.StartDeliveringTime = utility.GetCurrentDateTimeInTimeZone();
                                }
                                else
                                {
                                    orderDb.CancelledTime = utility.GetCurrentDateTimeInTimeZone();
                                }
                            }
                            else if (orderDb.StatusId == OrderStatus.Delivering && !asCustomer.Value)
                            {
                                orderDb.StatusId = IsSuccessful ? OrderStatus.Completed : OrderStatus.Cancelled;
                                if (IsSuccessful)
                                {
                                    orderDb.DeliveredTime = utility.GetCurrentDateTimeInTimeZone();
                                }
                                else
                                {
                                    orderDb.CancelledTime = utility.GetCurrentDateTimeInTimeZone();
                                }
                            }
                            else
                            {
                                result = BuildAppActionResultError(result, $"Đơn hàng không thể cập nhật những trạng thái khác");
                            }
                        }
                        else
                        {
                            if (orderDb.StatusId == OrderStatus.TemporarilyCompleted || orderDb.StatusId == OrderStatus.Processing)
                            {
                                if (status.HasValue)
                                {
                                    orderDb.StatusId = status.Value;
                                }
                                else
                                {
                                    if (IsSuccessful)
                                    {
                                        var transactionRepository = Resolve<IGenericRepository<Domain.Models.Transaction>>();
                                        var reservationTransactionDb = await transactionRepository.GetByExpression(t => t.OrderId == orderId && t.TransationStatusId == TransationStatus.SUCCESSFUL && t.TransactionTypeId == TransactionType.Order, null);
                                        if (reservationTransactionDb == null)
                                        {
                                            result = BuildAppActionResultError(result, $"Không tìm thấy giao dịch thành công cho đơn hàng với id {orderId}");
                                            return result;
                                        }
                                        orderDb.StatusId = OrderStatus.Completed;
                                    }
                                    else if (!asCustomer.Value)
                                    {
                                        orderDb.CancelledTime = utility.GetCurrentDateTimeInTimeZone();
                                        orderDb.StatusId = OrderStatus.Cancelled;
                                        await UpdateCancelledOrderDishQuantity(orderDb, updateDishSizeDetailList, currentTime, false);
                                    }
                                }
                            }
                            else
                            {
                                result = BuildAppActionResultError(result, $"Đơn hàng không thể cập nhật những trạng thái khác");
                            }
                        }


                        await _repository.Update(orderDb);
                        await _unitOfWork.SaveChangesAsync();

                        if ((orderDb.OrderTypeId == OrderType.Reservation || orderDb.OrderTypeId == OrderType.Delivery && !asCustomer.Value) && orderDb.StatusId == OrderStatus.Cancelled)
                        {
                            var refund = await transactionService.CreateRefund(orderDb, asCustomer.HasValue && asCustomer.Value);
                            if (!refund.IsSuccess)
                            {
                                return BuildAppActionResultError(result, $"Thực hiện hoàn tiền thất bại");
                            }
                        }
                        await dishManagementService.UpdateComboAvailability();
                        await dishManagementService.UpdateDishAvailability();
                        scope.Complete();
                        if ((!requireSignalR.HasValue || requireSignalR.Value) && (orderDb.StatusId == OrderStatus.Processing || orderDb.StatusId == OrderStatus.ReadyForDelivery))
                        {
                            await _hubServices.SendAsync(SD.SignalMessages.LOAD_ORDER_SESIONS);
                            await _hubServices.SendAsync(SD.SignalMessages.LOAD_GROUPED_DISHES);
                        }

                    }
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
                return result;
            }
        }

        private async Task ApplyLoyalTyPoint(Order order)
        {
            try
            {
                var loyaltyPointRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var accountDb = order.Account;
                var loyaltyPointDb = await loyaltyPointRepository.GetAllDataByExpression(l => l.OrderId == order.OrderId, 0, 0, l => l.PointChanged, true, null);
                foreach (var loyaltyPoint in loyaltyPointDb.Items)
                {
                    accountDb.LoyaltyPoint += loyaltyPoint.PointChanged;
                    loyaltyPoint.NewBalance = accountDb.LoyaltyPoint;
                }
                await accountRepository.Update(accountDb);
                await loyaltyPointRepository.UpdateRange(loyaltyPointDb.Items);
                await _unitOfWork.SaveChangesAsync();

            }
            catch (Exception ex)
            {

            }
        }

        public async Task<AppActionResult> CreateOrder(OrderRequestDto orderRequestDto)
        {
            var result = new AppActionResult();
                try
                {
                var utility = Resolve<Utility>();
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var notificationService = Resolve<INotificationMessageService>();
                var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                var loyalPointsHistoryRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
                var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                var comboRepository = Resolve<IGenericRepository<Combo>>();
                var dishComboRepository = Resolve<IGenericRepository<DishCombo>>();
                var couponProgramRepository = Resolve<IGenericRepository<CouponProgram>>();
                var fireBaseService = Resolve<IFirebaseService>();
                var dishRepository = Resolve<IGenericRepository<Dish>>();
                var orderAppliedCouponRepository = Resolve<IGenericRepository<OrderAppliedCoupon>>();
                var tableRepository = Resolve<IGenericRepository<Table>>();
                var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                var transcationService = Resolve<ITransactionService>();
                var orderSessionRepository = Resolve<IGenericRepository<OrderSession>>();
                var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var customerInfoAddressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
                var notificationMessageRepository = Resolve<IGenericRepository<NotificationMessage>>();
                var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
                var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
                var tokenRepostiory = Resolve<IGenericRepository<Token>>();
                var hubService = Resolve<IHubServices.IHubServices>();
                var tableService = Resolve<ITableService>();
                var dishManagementService = Resolve<IDishManagementService>();
                var mapService = Resolve<IMapService>();
                var smsService = Resolve<ISmsService>();
                var emailService = Resolve<IEmailService>();
                var createdOrderId = new Guid();
                var combo = new Combo();
                var orderWithPayment = new OrderWithPaymentResponse();
                var orderCombo = false;
                List<OrderDetail> orderDetails = new List<OrderDetail>();
                List<ComboOrderDetail> comboOrderDetails = new List<ComboOrderDetail>();
                double money = 0;
                var order = new Order();
                Account accountDb = null;
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        order = new Order()
                        {
                            OrderId = Guid.NewGuid(),
                            OrderTypeId = orderRequestDto.OrderType,
                            Note = orderRequestDto.Note,
                        };

                        if (orderRequestDto.CustomerId.HasValue)
                        {
                            accountDb = await accountRepository.GetByExpression(c => c.Id == orderRequestDto.CustomerId.Value.ToString(), null);
                            if (accountDb == null)
                            {
                                return BuildAppActionResultError(result, $"Xảy ra lỗi");
                            }

                            order.AccountId = orderRequestDto.CustomerId.ToString();

                            if (orderRequestDto.OrderType == OrderType.Reservation)
                            {
                                if (accountDb.Email.Contains("TPCustomer") || accountDb.LastName.ToLower().Contains("lastname") || accountDb.LastName.ToLower().Contains("firstname") || accountDb.DOB == new DateTime(1, 1, 1))
                                {
                                    return BuildAppActionResultError(result, $"Quý khách chưa cập nhật thông tin tài khoản nên không thể tạo đặt bàn");
                                }
                            }

                        }

                        if ((orderRequestDto.OrderType < OrderType.Reservation && orderRequestDto.OrderType > OrderType.Delivery)
                            || (orderRequestDto.OrderType == OrderType.Reservation && orderRequestDto.ReservationOrder == null)
                            || (orderRequestDto.OrderType == OrderType.MealWithoutReservation && orderRequestDto.MealWithoutReservation == null)
                            || (orderRequestDto.OrderType == OrderType.Delivery && orderRequestDto.DeliveryOrder == null))
                        {
                            return BuildAppActionResultError(result, $"Loại đơn hàng và dữ liệu không trùng khớp");
                        }


                        if (orderRequestDto.OrderType != OrderType.MealWithoutReservation && ((orderRequestDto!.ReservationOrder != null && orderRequestDto.ReservationOrder.PaymentMethod == 0)
                            || (orderRequestDto.DeliveryOrder != null && orderRequestDto.DeliveryOrder.PaymentMethod == 0)))
                        {
                            return BuildAppActionResultError(result, $"Yêu cầu phương thức thanh toán");
                        }

                        if (orderRequestDto.OrderDetailsDtos != null && orderRequestDto.OrderDetailsDtos.Count > 0)
                        {
                            List<DishSizeDetail> dishSizeDetails = new List<DishSizeDetail>();
                            var orderSessionDb = await orderSessionRepository!.GetAllDataByExpression(null, 0, 0, null, false, null);
                            var latestOrderSession = orderSessionDb.Items!.Count() + 1;
                            var estimatedPreparationTime = new List<CalculatePreparationTime>();
                            var orderSession = new OrderSession()
                            {
                                OrderSessionId = Guid.NewGuid(),
                                OrderSessionTime = utility!.GetCurrentDateTimeInTimeZone(),
                                OrderSessionStatusId = orderRequestDto.OrderType == OrderType.Reservation ? OrderSessionStatus.PreOrder : OrderSessionStatus.Confirmed,
                                OrderSessionNumber = latestOrderSession
                            };
                            DateTime orderTime = utility.GetCurrentDateTimeInTimeZone();
                            if (orderRequestDto.OrderType != OrderType.Reservation && orderRequestDto.ReservationOrder != null && orderRequestDto?.ReservationOrder?.MealTime > orderTime)
                            {
                                orderTime = (DateTime)(orderRequestDto?.ReservationOrder?.MealTime);
                            }

                            foreach (var item in orderRequestDto.OrderDetailsDtos)
                            {
                                var orderDetail = new OrderDetail()
                                {
                                    OrderDetailId = Guid.NewGuid(),
                                    Quantity = item.Quantity,
                                    Note = item.Note,
                                    OrderId = order.OrderId,
                                    OrderTime = orderTime,
                                    OrderSessionId = orderSession.OrderSessionId
                                };

                                if (item.DishSizeDetailId.HasValue)
                                {
                                    var dishSizeDetail = dishSizeDetails.FirstOrDefault(d => d.DishSizeDetailId == item.DishSizeDetailId);
                                    if (dishSizeDetail == null)
                                    {
                                        dishSizeDetail = await dishSizeDetailRepository.GetByExpression(o => o.DishSizeDetailId == item.DishSizeDetailId, o => o.Dish);
                                        dishSizeDetails.Add(dishSizeDetail);
                                    }


                                    if (dishSizeDetail == null)
                                    {
                                        return BuildAppActionResultError(result, $"Không tìm thấy món ăn có size với id {item.DishSizeDetailId.Value}");
                                    }

                                    orderDetail.DishSizeDetailId = item.DishSizeDetailId.Value;
                                    orderDetail.Price = dishSizeDetail.Price;
                                    orderDetail.PreparationTime = await dishManagementService.CalculatePreparationTime(new List<CalculatePreparationTime>
            {
                new CalculatePreparationTime
                {
                    PreparationTime = dishSizeDetail.Dish.PreparationTime.Value,
                    Quantity = orderDetail.Quantity
                }
            });
                                    estimatedPreparationTime.Add(new CalculatePreparationTime
                                    {
                                        PreparationTime = dishSizeDetail.Dish.PreparationTime.Value,
                                        Quantity = orderDetail.Quantity
                                    });
                                    if (dishSizeDetail.QuantityLeft < item.Quantity)
                                    {
                                        return BuildAppActionResultError(result, $"Món ăn {dishSizeDetail.Dish.Name} chỉ còn x{dishSizeDetail.QuantityLeft}");
                                    }

                                    if (orderRequestDto.OrderType != OrderType.Reservation || orderRequestDto.ReservationOrder.MealTime.Date == orderTime.Date)
                                    {
                                        dishSizeDetail.QuantityLeft -= item.Quantity;
                                        if (dishSizeDetail.QuantityLeft == 0)
                                        {
                                            dishSizeDetail.IsAvailable = false;
                                        }
                                        if (dishSizeDetail.QuantityLeft <= 5)
                                        {
                                            string message = $"{dishSizeDetail.Dish.Name} chỉ còn x{dishSizeDetail.QuantityLeft} món";
                                            await hubService!.SendAsync(SD.SignalMessages.LOAD_NOTIFICATION);
                                            await notificationService!.SendNotificationToRoleAsync(SD.RoleName.ROLE_ADMIN, message);
                                        }
                                    }
                                    estimatedPreparationTime.Add(new CalculatePreparationTime
                                    {
                                        PreparationTime = dishSizeDetail.Dish.PreparationTime.Value,
                                        Quantity = orderDetail.Quantity
                                    });
                                }
                                else if (item.Combo != null)
                                {
                                    orderCombo = true;
                                    combo = await comboRepository.GetById(item.Combo.ComboId);

                                    if (combo == null)
                                    {
                                        return BuildAppActionResultError(result, $"Không tìm thấy combo với id {item.Combo.ComboId}");
                                    }

                                    orderDetail.ComboId = item.Combo.ComboId;
                                    orderDetail.Price = combo.Price;

                                    if (item.Combo.DishComboIds.Count == 0)
                                    {
                                        return BuildAppActionResultError(result, $"Không tìm thấy chi tiết cho combo {combo.Name}");
                                    }

                                    foreach (var dishComboId in item.Combo.DishComboIds)
                                    {
                                        var comboDetail = await dishComboRepository.GetByExpression(d => d.DishComboId == dishComboId, d => d.DishSizeDetail.Dish);
                                        if (comboDetail == null)
                                        {
                                            return BuildAppActionResultError(result, $"Không tìm thấy chi tiết combo với id {dishComboId}");
                                        }

                                        if (comboDetail.DishSizeDetail.QuantityLeft < item.Quantity * comboDetail.Quantity)
                                        {
                                            return BuildAppActionResultError(result, $"Món ăn {comboDetail.DishSizeDetail.Dish.Name} chỉ còn x{comboDetail.DishSizeDetail.QuantityLeft}");
                                        }

                                        if (orderRequestDto.OrderType != OrderType.Reservation || orderRequestDto.ReservationOrder.MealTime.Date == orderTime.Date)
                                        {
                                            var dishSizeDetail = dishSizeDetails.FirstOrDefault(d => d.DishSizeDetailId == comboDetail.DishSizeDetailId);
                                            if (dishSizeDetail == null)
                                            {
                                                dishSizeDetail = await dishSizeDetailRepository.GetByExpression(o => o.DishSizeDetailId == comboDetail.DishSizeDetailId, o => o.Dish);
                                                dishSizeDetails.Add(dishSizeDetail);
                                            }
                                            dishSizeDetail.QuantityLeft -= item.Quantity * comboDetail.Quantity;
                                            if (dishSizeDetail.QuantityLeft == 0)
                                            {
                                                dishSizeDetail.IsAvailable = false;
                                            }
                                            if (dishSizeDetail.QuantityLeft <= 5)
                                            {
                                                string message = $"{dishSizeDetail.Dish.Name} chỉ còn x{dishSizeDetail.QuantityLeft} món";
                                                await hubService!.SendAsync(SD.SignalMessages.LOAD_NOTIFICATION);
                                                await notificationService!.SendNotificationToRoleAsync(SD.RoleName.ROLE_ADMIN, message);
                                            }
                                        }

                                        comboOrderDetails.Add(new ComboOrderDetail
                                        {
                                            ComboOrderDetailId = Guid.NewGuid(),
                                            OrderDetailId = orderDetail.OrderDetailId,
                                            DishComboId = dishComboId,
                                            PreparationTime = await dishManagementService.CalculatePreparationTime(new List<CalculatePreparationTime>
                        {
                            new CalculatePreparationTime
                            {
                                PreparationTime = comboDetail.DishSizeDetail.Dish.PreparationTime.Value,
                                Quantity = orderDetail.Quantity
                            }
                        })
                                        });

                                    }
                                    estimatedPreparationTime.Add(new CalculatePreparationTime
                                    {
                                        PreparationTime = combo.PreparationTime == null ? comboOrderDetails.Sum(c => c.PreparationTime) : combo.PreparationTime.Value,
                                        Quantity = orderDetail.Quantity
                                    });
                                }
                                else
                                {
                                    return BuildAppActionResultError(result, $"Không tìm thấy thông tin món ăn");
                                }

                                orderDetails.Add(orderDetail);
                            }
                            money = orderDetails.Sum(o => o.Quantity * o.Price);
                            orderSession.PreparationTime = await dishManagementService.CalculatePreparationTime(estimatedPreparationTime);
                            await orderSessionRepository.Insert(orderSession);
                            await dishSizeDetailRepository.UpdateRange(dishSizeDetails);
                        }


                        if (orderDetails.Count > 0)
                        {
                            order.TotalAmount = Math.Ceiling(money / 1000) * 1000;
                            await orderDetailRepository.InsertRange(orderDetails);
                            if (comboOrderDetails.Count > 0)
                            {
                                if (orderRequestDto.OrderType != OrderType.Reservation)
                                {
                                    comboOrderDetails.ForEach(c => c.StatusId = DishComboDetailStatus.Unchecked);
                                }
                                await comboOrderDetailRepository.InsertRange(comboOrderDetails);
                            }
                        }


                        List<TableDetail> tableDetails = new List<TableDetail>();

                        if (orderRequestDto.OrderType == OrderType.Reservation)
                        {
                            if (orderRequestDto.ReservationOrder == null)
                            {
                                return BuildAppActionResultError(result, $"Thiếu thông tin để tạo đặt bàn");
                            }


                            if (orderRequestDto.ReservationOrder.Deposit < 0)
                            {
                                result = BuildAppActionResultError(result, $"Số tiền cọc không hợp lệ");
                                return result;
                            }

                            if (orderRequestDto.ReservationOrder == null)
                            {
                                return BuildAppActionResultError(result, $"Thiếu thông tin để tạo đặt bàn");
                            }

                            if (orderRequestDto.ReservationOrder == null)
                            {
                                return BuildAppActionResultError(result, $"Thiếu thông tin để tạo đặt bàn");
                            }
                            if (orderRequestDto.ReservationOrder.MealTime < utility!.GetCurrentDateTimeInTimeZone())
                            {
                                result = BuildAppActionResultError(result, "Thời gian đặt bàn không hợp lệ");
                                return result;
                            }

                            order.StatusId = OrderStatus.TableAssigned;
                            order.OrderTypeId = OrderType.Reservation;
                            order.ReservationDate = utility.GetCurrentDateTimeInTimeZone();
                            order.NumOfPeople = orderRequestDto.ReservationOrder.NumberOfPeople;
                            order.MealTime = orderRequestDto.ReservationOrder.MealTime;
                            order.EndTime = orderRequestDto.ReservationOrder.EndTime;
                            order.IsPrivate = orderRequestDto.ReservationOrder.IsPrivate;
                            order.Deposit = Math.Ceiling((double)orderRequestDto.ReservationOrder.Deposit / 1000) * 1000;

                            var suggestTableDto = new FindTableDto
                            {
                                StartTime = orderRequestDto.ReservationOrder.MealTime,
                                EndTime = orderRequestDto.ReservationOrder.EndTime,
                                IsPrivate = orderRequestDto.ReservationOrder.IsPrivate,
                                NumOfPeople = orderRequestDto.ReservationOrder.NumberOfPeople,
                            };

                            var suggestedTables = await tableService.FindTable(suggestTableDto);

                            if (!suggestedTables.IsSuccess)
                            {
                                return BuildAppActionResultError(result, $"Xảy ra lỗi khi xếp bàn. Vui lòng thử lại");
                            }

                            if (suggestedTables.Result == null)
                            {
                                return BuildAppActionResultError(result, $"Không có bàn trống cho {orderRequestDto.ReservationOrder.NumberOfPeople} người " +
                                                                           $"vào lúc {orderRequestDto.ReservationOrder.MealTime.Hour}h{orderRequestDto.ReservationOrder.MealTime.Minute}p " +
                                                                           $"ngày {orderRequestDto.ReservationOrder.MealTime.Date}");
                            }

                            if (suggestedTables.Messages.Count > 0)
                            {
                                result.Messages.AddRange(suggestedTables.Messages);
                            }

                            //Add busniness rule for reservation time(if needed)
                            List<TableDetail> reservationTableDetails = new List<TableDetail>();

                            foreach (var suggestedTable in suggestedTables.Result as List<TableArrangementResponseItem>)
                            {
                                reservationTableDetails.Add(new TableDetail
                                {
                                    TableDetailId = Guid.NewGuid(),
                                    OrderId = order.OrderId,
                                    TableId = suggestedTable.Id
                                });
                            }

                            await tableDetailRepository.InsertRange(reservationTableDetails);

                            if (orderDetails.Count > 0)
                            {
                                orderDetails.ForEach(o => o.OrderDetailStatusId = OrderDetailStatus.Reserved);
                            }
                            orderWithPayment.Order = order;
                        }
                        else if (orderRequestDto.OrderType == OrderType.MealWithoutReservation)
                        {
                            order.OrderTypeId = OrderType.MealWithoutReservation;
                            order.StatusId = OrderStatus.Processing;
                            order.MealTime = utility.GetCurrentDateTimeInTimeZone();
                            order.NumOfPeople = 0;
                            order.TotalAmount = Math.Ceiling(money / 1000) * 1000;
                            if (orderDetails.Count > 0)
                            {
                                orderDetails.ForEach(o => o.OrderDetailStatusId = OrderDetailStatus.Unchecked);
                            }
                            else
                            {
                                return BuildAppActionResultError(result, "Bàn không thực hiện gọi món.");
                            }

                            orderWithPayment.Order = order;

                            if (orderRequestDto.MealWithoutReservation != null && orderRequestDto.MealWithoutReservation.TableIds.Count > 0)
                            {
                                var collidedTable = await GetCollidedTable(orderRequestDto.MealWithoutReservation.TableIds, (DateTime)order.MealTime, order.NumOfPeople);

                                if (collidedTable.Count > 0)
                                {
                                    // Tạo danh sách các tên bàn thành chuỗi phân cách bằng dấu phẩy
                                    var tableNames = string.Join(", ", collidedTable.Select(ct => ct.TableName));

                                    result = BuildAppActionResultError(result, $"Sắp xếp bàn chưa hợp lí. Trong danh sách nhập vào có các bàn đang hoặc đã được đặt từ trước, gồm: {tableNames}");
                                    return result;
                                }

                                foreach (var tableId in orderRequestDto.MealWithoutReservation.TableIds)
                                {
                                    var reservationTableDetail = new TableDetail
                                    {
                                        TableDetailId = Guid.NewGuid(),
                                        OrderId = order.OrderId,
                                        TableId = tableId
                                    };
                                    tableDetails.Add(reservationTableDetail);
                                }

                                await tableDetailRepository.InsertRange(tableDetails);
                                orderWithPayment.Order = order;


                                if (orderRequestDto.CustomerId.HasValue)
                                {
                                    if (!string.IsNullOrWhiteSpace(accountDb.Email))
                                    {
                                        var username = accountDb.FirstName + " " + accountDb.LastName;
                                        emailService.SendEmail(accountDb.Email, SD.SubjectMail.NOTIFY_RESERVATION,
                                                                     TemplateMappingHelper.GetTemplateOrderConfirmation(
                                                                     username, order)
                                            );
                                    }
                                    else
                                    {
                                        var smsMessage = $"[NHÀ HÀNG THIÊN PHÚ] Đơn đặt bàn của bạn vào lúc {order.ReservationDate} đã thành công. " +
                                                         $"Vui lòng thanh toán {order.TotalAmount} VND" +
                                                         $"Xin chân trọng cảm ơn quý khách.";
                                        await smsService.SendMessage(smsMessage, accountDb.PhoneNumber);
                                    }
                                }
                            }
                            else
                            {
                                return BuildAppActionResultError(result, "Không có thông tin bàn");
                            }
                        }
                        else if (orderRequestDto.OrderType == OrderType.Delivery)
                        {
                            order.OrderTypeId = OrderType.Delivery;
                            order.StatusId = OrderStatus.Pending;
                            order.TotalAmount = Math.Ceiling(money / 1000) * 1000;
                            order.OrderDate = utility.GetCurrentDateTimeInTimeZone();
                            if (accountDb == null)
                            {
                                return BuildAppActionResultError(result, $"Không tìm thấy thông tin khách hàng. Đặt hàng thất bại");
                            }

                            if (orderDetails.Count == 0)
                            {
                                return BuildAppActionResultError(result, "Đơn hàng không thực hiện đặt món.");
                            }

                            order.AccountId = accountDb.Id;

                            if (string.IsNullOrEmpty(accountDb.Address))
                            {
                                return BuildAppActionResultError(result, $"Không tìm thấy địa chỉ của bạn. Vui lòng cập nhật địa chỉ");
                            }

                            var restaurantLatConfig = await configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.RESTAURANT_LATITUDE);
                            var restaurantLngConfig = await configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.RESTAURANT_LNG);
                            var restaurantMaxDistanceToOrderConfig = await configurationRepository.GetByExpression(p => p.Name == SD.DefaultValue.DISTANCE_ORDER);

                            var customerInfoAddressListDb = await customerInfoAddressRepository!.GetAllDataByExpression(p => p.CustomerInfoAddressName == accountDb.Address, 0, 0, null, false, null);
                            if (customerInfoAddressListDb.Items.Count == 0)
                            {
                                return BuildAppActionResultError(result, $"Không tìm thấy địa chỉ {accountDb.Address}");
                            }

                            var customerAddressDb = customerInfoAddressListDb.Items.FirstOrDefault(c => c.IsCurrentUsed && !c.IsDeleted);

                            order.AddressId = customerAddressDb.CustomerInfoAddressId;

                            var restaurantLat = Double.Parse(restaurantLatConfig.CurrentValue);
                            var restaurantLng = Double.Parse(restaurantLngConfig.CurrentValue);
                            var maxDistanceToOrder = double.Parse(restaurantMaxDistanceToOrderConfig!.CurrentValue);

                            double[] restaurantAddress = new double[]
                            {
            restaurantLat, restaurantLng
                            };

                            double[] customerAddress = new double[]
                            {
            customerAddressDb.Lat, customerAddressDb.Lng
                            };

                            var distanceResponse = await mapService!.GetEstimateDeliveryResponse(restaurantAddress, customerAddress);
                            var element = distanceResponse.Result as EstimatedDeliveryTimeDto.Response;
                            var distance = element!.TotalDistance;
                            if (distance > maxDistanceToOrder)
                            {
                                return BuildAppActionResultError(result, $"Nhà hàng chỉ hỗ trợ cho đơn giao hàng trong bán kính 10km");
                            }

                            var shippingCost = await CalculateDeliveryOrder(customerAddressDb.CustomerInfoAddressId);
                            if (shippingCost.Result == null)
                            {
                                return BuildAppActionResultError(result, $"Xảy ra lỗi khi tính phí giao hàng. Vui lòng kiểm tra lại thông tin địa chỉ");
                            }
                            money += double.Parse(shippingCost.Result.ToString());

                            var currentTime = utility.GetCurrentDateTimeInTimeZone();
                            var couponDb = await couponProgramRepository!.GetAllDataByExpression(c => currentTime > c.StartDate && currentTime < c.ExpiryDate && c.MinimumAmount <= money && c.Quantity > 0, 0, 0, null, false, null);
                            if (couponDb.Items!.Count > 0 && couponDb.Items != null
                                && orderRequestDto.DeliveryOrder != null && orderRequestDto.DeliveryOrder.CouponIds.Count > 0)
                            {
                                foreach (var couponId in orderRequestDto.DeliveryOrder.CouponIds)
                                {
                                    if (money <= 0)
                                    {
                                        break;
                                    }

                                    var coupon = couponDb.Items.FirstOrDefault(c => c.CouponProgramId == couponId);

                                    if (coupon == null)
                                    {
                                        return BuildAppActionResultError(result, $"Không tìm thấy coupon với id {couponId}");
                                    }

                                    //var orderAppliedCoupon = new OrderAppliedCoupon
                                    //{
                                    //    OrderAppliedCouponId = Guid.NewGuid(),
                                    //    CouponProgramId = couponId,
                                    //    OrderId = order.OrderId
                                    //};

                                    double discountMoney = money * (coupon.DiscountPercent * 0.01);
                                    money -= discountMoney;
                                    money = Math.Max(0, money);

                                    //await orderAppliedCouponRepository.Insert(orderAppliedCoupon);
                                }
                            }

                            if (orderRequestDto.DeliveryOrder.LoyalPointToUse.HasValue && orderRequestDto.DeliveryOrder.LoyalPointToUse > 0)
                            {
                                // Check if the user has enough points
                                if (accountDb!.LoyaltyPoint >= orderRequestDto.DeliveryOrder.LoyalPointToUse)
                                {
                                    // Calculate the discount (assuming 1 point = 1 currency unit)
                                    double loyaltyDiscount = Math.Min(orderRequestDto.DeliveryOrder.LoyalPointToUse.Value, money);
                                    money -= loyaltyDiscount;

                                    // Ensure the total doesn't go below zero
                                    money = Math.Max(0, money);

                                    // Update the customer's loyalty points
                                    accountDb.LoyaltyPoint -= (int)loyaltyDiscount;

                                    // Create a new loyalty point history entry for the point usage
                                    var loyalPointUsageHistory = new LoyalPointsHistory
                                    {
                                        LoyalPointsHistoryId = Guid.NewGuid(),
                                        TransactionDate = utility!.GetCurrentDateTimeInTimeZone(),
                                        OrderId = order.OrderId,
                                        PointChanged = -(int)loyaltyDiscount,
                                        NewBalance = accountDb.LoyaltyPoint
                                    };

                                    await loyalPointsHistoryRepository!.Insert(loyalPointUsageHistory);
                                }
                                else
                                {
                                    // Handle the case where the user doesn't have enough points
                                    return BuildAppActionResultError(result, "Không đủ điểm tích lũy để sử dụng.");
                                }
                            }

                            var newLoyalPointHistory = new LoyalPointsHistory
                            {
                                LoyalPointsHistoryId = Guid.NewGuid(),
                                TransactionDate = utility!.GetCurrentDateTimeInTimeZone(),
                                OrderId = order.OrderId,
                                PointChanged = (int)money / 100,
                                NewBalance = accountDb.LoyaltyPoint + (int)money / 100
                            };

                            await loyalPointsHistoryRepository!.Insert(newLoyalPointHistory);

                            accountDb.LoyaltyPoint = newLoyalPointHistory.NewBalance;

                            orderWithPayment.Order = order;

                            orderDetails.ForEach(o => o.OrderDetailStatusId = OrderDetailStatus.Unchecked);
                            order.TotalAmount = Math.Ceiling(money / 1000) * 1000;

                            await orderDetailRepository.InsertRange(orderDetails);

                            if (orderRequestDto.CustomerId.HasValue)
                            {
                                if (!string.IsNullOrEmpty(accountDb.Email))
                                {
                                    var username = accountDb.FirstName + " " + accountDb.LastName;
                                    emailService.SendEmail(accountDb.Email, SD.SubjectMail.NOTIFY_RESERVATION,
                                                                 TemplateMappingHelper.GetTemplateOrderConfirmation(
                                                                 username, order)
                                        );
                                    string notificationEmailMessage = "Nhà hàng đã gửi thông báo mới tới email của bạn";
                                    await notificationService!.SendNotificationToAccountAsync(accountDb.Id, notificationEmailMessage, true);

                                }
                                else
                                {
                                    var smsMessage = $"[NHÀ HÀNG THIÊN PHÚ] Đơn hàng của bạn vào lúc {order.OrderDate} đã thành công. " +
                                               $"Vui lòng thanh toán {order.TotalAmount} VND" +
                                               $"Xin chân trọng cảm ơn quý khách.";
                                    //await smsService.SendMessage(smsMessage, accountDb.PhoneNumber);
                                    string notificationSmsMessage = "Nhà hàng đã gửi thông báo mới tới số điện thoại của bạn";
                                    await notificationService!.SendNotificationToAccountAsync(accountDb.Id, notificationSmsMessage, true);

                                }
                            }


                            if (!BuildAppActionResultIsError(result))
                            {
                                await accountRepository.Update(accountDb);
                            }
                        }
                        else
                        {
                            return BuildAppActionResultError(result, $"Thiếu loại đơn hàng (orderType) trong yêu cầu tạo");
                        }

                        if (!BuildAppActionResultIsError(result))
                        {
                            await _repository.Insert(order);
                            await _unitOfWork.SaveChangesAsync();
                            if (orderCombo)
                            {
                                await dishManagementService.UpdateComboAvailability();
                                await dishManagementService.UpdateDishAvailability();
                            }


                            if (orderRequestDto.DeliveryOrder != null || orderRequestDto.ReservationOrder != null)
                            {
                                var paymentRequest = new PaymentRequestDto
                                {
                                    OrderId = order.OrderId,
                                    PaymentMethod = orderRequestDto.DeliveryOrder != null ? orderRequestDto.DeliveryOrder.PaymentMethod : orderRequestDto.ReservationOrder.PaymentMethod,
                                };
                                var linkPaymentDb = await transcationService!.CreatePayment(paymentRequest);
                                if (!linkPaymentDb.IsSuccess)
                                {
                                    return BuildAppActionResultError(result, String.Join(", ", linkPaymentDb.Messages));
                                }

                                if ((orderRequestDto.DeliveryOrder?.PaymentMethod != null && orderRequestDto.DeliveryOrder?.PaymentMethod == PaymentMethod.STORE_CREDIT)
                                    || (orderRequestDto.ReservationOrder?.PaymentMethod != null && orderRequestDto.ReservationOrder?.PaymentMethod == PaymentMethod.STORE_CREDIT))
                                {
                                    await ChangeOrderStatus(order.OrderId, true, null);
                                }

                                if (linkPaymentDb.Result != null && !string.IsNullOrEmpty(linkPaymentDb.Result.ToString()))
                                {
                                    orderWithPayment.PaymentLink = linkPaymentDb!.Result!.ToString();
                                }
                            }

                        }
                        result.Result = orderWithPayment;
                        scope.Complete();

                    }
                if (orderRequestDto.DeliveryOrder != null || orderRequestDto.ReservationOrder != null)
                {
                    StringBuilder messageBody = new StringBuilder();
                    if (orderDetails != null && orderDetails.Count > 0)
                    {
                        foreach (var orderDetail in orderDetails)
                        {
                            if (orderDetail.DishSizeDetailId.HasValue)
                            {
                                var dishSizeDb = await dishSizeDetailRepository.GetByExpression(d => d.DishSizeDetailId == orderDetail.DishSizeDetailId.Value, d => d.Dish, d => d.DishSize);
                                messageBody.Append($"{dishSizeDb.Dish.Name}: {dishSizeDb.DishSize.VietnameseName} x {orderDetail.Quantity}, ");
                            }
                            else
                            {
                                var comboDishDetailDb = await comboOrderDetailRepository.GetAllDataByExpression(c => c.OrderDetailId == orderDetail.OrderDetailId, 0, 0, null, false, c => c.DishCombo.DishSizeDetail.Dish, c => c.DishCombo.DishSizeDetail.DishSize);
                                messageBody.Append($"{orderDetail.Combo.Name}: {orderDetail.Quantity} x [");
                                comboDishDetailDb.Items.ForEach(c =>
                                    messageBody.Append($"{c.DishCombo.DishSizeDetail.Dish.Name}: {c.DishCombo.DishSizeDetail.DishSize.VietnameseName} x {c.DishCombo.Quantity}, ")
                                );
                                messageBody.Length -= 2;
                                messageBody.Append("], ");
                            }
                        }

                        messageBody.Length -= 2;
                    }
                    var createSuccessfulMessage = $"Đơn của bạn đã được đặt thành công";
                    if (orderDetails != null && orderDetails.Count > 0)
                    {
                        await notificationService!.SendNotificationToRoleAsync(SD.RoleName.ROLE_CHEF, messageBody.ToString());

                    }
                    await notificationService!.SendNotificationToAccountAsync(accountDb.Id, createSuccessfulMessage);
                    if (order.OrderTypeId != OrderType.MealWithoutReservation)
                    {
                        await _hubServices.SendAsync(SD.SignalMessages.LOAD_ORDER_SESIONS);
                        await _hubServices.SendAsync(SD.SignalMessages.LOAD_GROUPED_DISHES);
                        await _hubServices.SendAsync(SD.SignalMessages.LOAD_ORDER_DETAIL_STATUS);
                    }
                    await hubService!.SendAsync(SD.SignalMessages.LOAD_USER_ORDER);
                }

            }
            catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
                return result;
            
        }

        public async Task<AppActionResult> GetAllOrderByCustomertId(string customerId, OrderStatus? status, OrderType? orderType, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                PagedResult<Order> data = new PagedResult<Order>();

                if (status.HasValue)
                {
                    data = await _repository.GetAllDataByExpression((o => o.Account.Id.Equals(customerId) && (
                    o.StatusId == status && o.OrderTypeId == orderType) ||
                    (o.StatusId == status) ||
                    (o.OrderTypeId == orderType)), pageNumber, pageSize, o => o.OrderTypeId == OrderType.Delivery ? o.OrderDate : o.MealTime, false,
                     p => p.Status!,
                     p => p.Account!,
                     p => p.LoyalPointsHistory!,
                     p => p.OrderType!
                    );
                }
                else
                {
                    data = await _repository.GetAllDataByExpression(o => o.Account.Id.Equals(customerId), pageNumber, pageSize, o => o.OrderTypeId == OrderType.Delivery ? o.OrderDate : o.MealTime, false,
                        p => p.Status!,
                        p => p.Account!,
                        p => p.LoyalPointsHistory!,
                        p => p.OrderType!,
                        p => p.Shipper
                        );
                }
                data.Items = data.Items.OrderByDescending(o => o.MealTime).ThenByDescending(o => o.OrderDate).ToList();
                var mappedData = _mapper.Map<List<OrderWithFirstDetailResponse>>(data.Items);
                foreach (var order in mappedData)
                {
                    var orderDetailDb = await _detailRepository.GetAllDataByExpression(o => o.OrderId == order.OrderId, 0, 0, null, false, o => o.DishSizeDetail.Dish, o => o.Combo);
                    if (orderDetailDb.Items.Count > 0)
                    {
                        order.OrderDetail = orderDetailDb.Items.FirstOrDefault();
                        order.ItemLeft = orderDetailDb.Items.Count() - 1;
                    }
                }
                result.Result = new PagedResult<OrderWithFirstDetailResponse>
                {
                    Items = mappedData.OrderByDescending(m => m.OrderTypeId == OrderType.Delivery ? m.OrderDate : m.MealTime).ToList(),
                    TotalPages = data.TotalPages,
                };
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetOrderDetail(Guid orderId)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var orderDb = await _repository.GetAllDataByExpression(p => p.OrderId == orderId, 0, 0, p => p.OrderDate, false, p => p.Account!,
                        p => p.Status!,
                        p => p.Account!,
                        p => p.LoyalPointsHistory!,
                        p => p.OrderType!,
                        p => p.CustomerInfoAddress

                    );
                if (orderDb.Items! == null && orderDb.Items.Count == 0)
                {
                    result = BuildAppActionResultError(result, $"Không tìm thấy đơn hàng với id {orderId}");
                    return result;
                }

                var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                var orderDetailDb = await orderDetailRepository!.GetAllDataByExpression(o => o.OrderId == orderId, 0, 0, null, false, o => o.DishSizeDetail!.Dish!, o => o.Combo!);
                var orderDetailReponseList = new List<OrderDetailResponse>();
                foreach (var o in orderDetailDb!.Items!)
                {
                    var comboOrderDetailsDb = await comboOrderDetailRepository!.GetAllDataByExpression(
                        c => c.OrderDetailId == o.OrderDetailId,
                        0,
                        0,
                        null,
                        false,
                        c => c.DishCombo!.DishSizeDetail!.Dish!
                    );
                    orderDetailReponseList.Add(new OrderDetailResponse
                    {
                        OrderDetail = o,
                        ComboOrderDetails = comboOrderDetailsDb.Items!
                    });
                }
                result.Result = new OrderReponse
                {
                    Order = orderDb.Items!.FirstOrDefault()!,
                    OrderDetails = orderDetailReponseList
                };
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> MakeDineInOrderBill(OrderPaymentRequestDto orderRequestDto)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                AppActionResult result = new AppActionResult();
                try
                {
                    var accountRepository = Resolve<IGenericRepository<Account>>();
                    var couponRepository = Resolve<IGenericRepository<CouponProgram>>();
                    var orderAppliedCouponRepository = Resolve<IGenericRepository<OrderAppliedCoupon>>();
                    var customerInfoRepository = Resolve<IGenericRepository<Account>>();
                    var loyalPointsHistoryRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
                    var transactionService = Resolve<ITransactionService>();
                    var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                    var utility = Resolve<Utility>();
                    var orderDb = await _repository.GetByExpression(o => o.OrderId == orderRequestDto.OrderId, null);
                    Transaction refundTransaction = null;

                    if (orderRequestDto.LoyalPointsToUse.HasValue && orderRequestDto.LoyalPointsToUse.Value < 0)
                    {
                        return BuildAppActionResultError(result, $"Số điểm thành viên sử dụng không được âm");
                    }

                    if (orderDb == null)
                    {
                        return BuildAppActionResultError(result, $"Không tìm thấy đơn với id {orderRequestDto.OrderId}");
                    }

                    if (orderDb.OrderTypeId == OrderType.Delivery)
                    {
                        return BuildAppActionResultError(result, $"Đơn hàng giao tận nơi đã có giao dịch");
                    }
                    Account accountDb = null;
                    if (!string.IsNullOrEmpty(orderRequestDto.AccountId))
                    {
                        accountDb = await accountRepository.GetById(orderRequestDto.AccountId);
                    }
                    else if (!string.IsNullOrEmpty(orderDb.AccountId))
                    {
                        accountDb = await accountRepository.GetById(orderDb.AccountId);
                    }
                    if(accountDb == null && orderRequestDto.CouponIds.Count > 0)
                    {
                        return BuildAppActionResultError(result, $"Coupon chỉ áp dụng được cho khách hàng có tài khoản trong hệ thống");
                    }

                    var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(o => o.OrderId == orderRequestDto.OrderId && o.OrderDetailStatusId != OrderDetailStatus.Cancelled, 0, 0, p => p.OrderTime, false, null);
                    double money = 0;
                    orderDetailDb.Items.ForEach(o => money += o.Price * o.Quantity);

                    money -= ((orderDb.Deposit.HasValue && orderDb.Deposit.Value > 0) ? Math.Ceiling(orderDb.Deposit.Value / 1000) * 1000 : 0);

                    if (money < 0)
                    {
                        if (!orderRequestDto.ChooseCashRefund.Value && accountDb == null)
                        {
                            return BuildAppActionResultError(result, $"Không tìm thấy thông tin khách hàng nên không thể thực hiện hoàn tiền vào ví");
                        }
                        var depositRefundRequest = new DepositRefundRequest
                        {
                            OrderId = orderDb.OrderId,
                            Account = accountDb,
                            RefundAmount = -Math.Ceiling(-money / 1000) * 1000,
                            PaymentMethod = orderRequestDto.ChooseCashRefund.Value ? PaymentMethod.Cash : PaymentMethod.STORE_CREDIT
                        };
                        var refundResult = await transactionService.CreateDepositRefund(depositRefundRequest);
                        if (refundResult.IsSuccess)
                        {
                            refundTransaction = refundResult.Result as Transaction;
                        }
                    }
                    else if (accountDb != null)
                    {
                        var currentTime = utility.GetCurrentDateTimeInTimeZone();
                        var customerSavedCouponDb = await couponRepository!.GetAllDataByExpression(c => currentTime > c.StartDate && currentTime < c.ExpiryDate && c.MinimumAmount <= money && c.Quantity > 0, 0, 0, null, false, null);
                        if (customerSavedCouponDb.Items!.Count > 0 && customerSavedCouponDb.Items != null
                            && orderRequestDto.CouponIds.Count > 0)
                        {
                            foreach (var couponId in orderRequestDto.CouponIds)
                            {
                                if (money <= 0)
                                {
                                    break;
                                }

                                var coupon = customerSavedCouponDb.Items.FirstOrDefault(c => c.CouponProgramId == couponId);

                                if (coupon == null)
                                {
                                    return BuildAppActionResultError(result, $"Không tìm thấy coupon với id {couponId}");
                                }



                                double discountMoney = money * (coupon.DiscountPercent * 0.01);
                                money -= discountMoney;
                                money = Math.Max(0, money);

                            }
                        }

                        if (orderRequestDto.LoyalPointsToUse.HasValue && orderRequestDto.LoyalPointsToUse > 0)
                        {
                            // Check if the user has enough points
                            if (accountDb!.LoyaltyPoint >= orderRequestDto.LoyalPointsToUse)
                            {
                                // Calculate the discount (assuming 1 point = 1 currency unit)
                                double loyaltyDiscount = Math.Min(orderRequestDto.LoyalPointsToUse.Value, money);
                                money -= loyaltyDiscount;

                                // Ensure the total doesn't go below zero
                                money = Math.Max(0, money);

                                // Update the customer's loyalty points
                                accountDb.LoyaltyPoint -= (int)loyaltyDiscount;

                                // Create a new loyalty point history entry for the point usage
                                var loyalPointUsageHistory = new LoyalPointsHistory
                                {
                                    LoyalPointsHistoryId = Guid.NewGuid(),
                                    TransactionDate = utility!.GetCurrentDateTimeInTimeZone(),
                                    OrderId = orderDb.OrderId,
                                    PointChanged = -(int)loyaltyDiscount,
                                    NewBalance = accountDb.LoyaltyPoint,
                                    IsApplied = false
                                };

                                await loyalPointsHistoryRepository!.Insert(loyalPointUsageHistory);
                            }
                            else
                            {
                                // Handle the case where the user doesn't have enough points
                                return BuildAppActionResultError(result, "Không đủ điểm tích lũy để sử dụng.");
                            }
                        }

                        var newLoyalPointHistory = new LoyalPointsHistory
                        {
                            LoyalPointsHistoryId = Guid.NewGuid(),
                            TransactionDate = utility!.GetCurrentDateTimeInTimeZone(),
                            OrderId = orderDb.OrderId,
                            PointChanged = (int)money / 100,
                            NewBalance = accountDb.LoyaltyPoint + (int)money / 100,
                            IsApplied = false
                        };

                        await loyalPointsHistoryRepository!.Insert(newLoyalPointHistory);

                        //accountDb.LoyaltyPoint = newLoyalPointHistory.NewBalance;
                        //await customerInfoRepository.Update(accountDb);
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        orderDb.TotalAmount = Math.Max(Math.Ceiling(money / 1000) * 1000, 0);
                        var orderWithPayment = new OrderWithPaymentResponse();
                        orderWithPayment.Order = orderDb;
                        result.Result = orderWithPayment;
                        if (refundTransaction == null)
                        {
                            if (orderRequestDto.PaymentMethod == PaymentMethod.Cash)
                            {
                                if (orderRequestDto.CashReceived.HasValue && orderRequestDto.ChangeReturned.HasValue)
                                {
                                    orderDb.CashReceived = orderRequestDto.CashReceived.Value;
                                    orderDb.ChangeReturned = orderRequestDto.ChangeReturned.Value;
                                }
                                else
                                {
                                    orderDb.CashReceived = Math.Ceiling(orderDb.TotalAmount / 1000) * 1000;
                                    orderDb.ChangeReturned = 0;
                                }
                                orderDb.StatusId = OrderStatus.Completed;
                                await _repository.Update(orderDb);
                                await _unitOfWork.SaveChangesAsync();
                            }
                            else
                            {
                                await _repository.Update(orderDb);
                                await _unitOfWork.SaveChangesAsync();
                                var paymentRequest = new PaymentRequestDto
                                {
                                    OrderId = orderDb.OrderId,
                                    PaymentMethod = orderRequestDto.PaymentMethod,
                                    AccountId = accountDb?.Id
                                };
                                var linkPaymentDb = await transactionService!.CreatePayment(paymentRequest);
                                if (!linkPaymentDb.IsSuccess)
                                {
                                    return BuildAppActionResultError(result, "Tạo thanh toán thất bại");
                                }
                                orderWithPayment.PaymentLink = linkPaymentDb.Result.ToString();
                                result.Result = orderWithPayment;
                            }
                        }
                        else
                        {
                            await _repository.Update(orderDb);
                            await _unitOfWork.SaveChangesAsync();
                        }
                        scope.Complete();
                    }

                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
                return result;


            }
        }


        //public async Task<AppActionResult> GetOrderTotal(CalculateOrderRequest orderRequestDto)
        //{
        //    AppActionResult result = new AppActionResult();
        //    try
        //    {
        //        var utility = Resolve<Utility>();
        //        var customerInfoRepository = Resolve<IGenericRepository<Account>>();
        //        if (orderRequestDto.CustomerId.HasValue)
        //        {
        //            var data = new CalculateOrderResponse();
        //            data.Amount = orderRequestDto.Total;
        //            var customerInfoDb = await customerInfoRepository!.GetById(orderRequestDto.CustomerId);
        //            if (customerInfoDb == null)
        //            {
        //                return BuildAppActionResultError(result, $"Không tìm thấy thông tin khách hàng với id {orderRequestDto.CustomerId}");
        //            }
        //            var customerSavedCouponRepository = Resolve<IGenericRepository<CustomerSavedCoupon>>();
        //            var reservationRepository = Resolve<IGenericRepository<Reservation>>();
        //            var loyalPointsHistoryRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
        //            var reservationDb = await reservationRepository.GetById(orderRequestDto.ReservationId);
        //            if (reservationDb != null)
        //            {
        //                data.PaidDeposit = reservationDb.Deposit;
        //            }
        //            var customerSavedCouponDb = await customerSavedCouponRepository!.GetAllDataByExpression(p => !string.IsNullOrEmpty(customerInfoDb.CustomerId) && p.CustomerId == customerInfoDb.CustomerId, 0, 0, null, false, p => p.Coupon!);
        //            if (customerSavedCouponDb.Items!.Count > 0 && customerSavedCouponDb.Items != null)
        //            {
        //                if (orderRequestDto.CouponId.HasValue)
        //                {
        //                    var customerSavedCoupon = customerSavedCouponDb.Items.FirstOrDefault(c => c.CouponId == orderRequestDto.CouponId);
        //                    if (customerSavedCoupon != null && customerSavedCoupon.IsUsedOrExpired == false)
        //                    {
        //                        if (customerSavedCoupon.Coupon != null && customerSavedCoupon.Coupon.MinimumAmount < orderRequestDto.Total && customerSavedCoupon.Coupon.ExpiryDate > utility!.GetCurrentDateTimeInTimeZone())
        //                        {
        //                            data.CouponDiscount = orderRequestDto.Total * (customerSavedCoupon.Coupon.DiscountPercent * 0.01);
        //                        }
        //                        else if (customerSavedCoupon.Coupon.ExpiryDate < utility!.GetCurrentDateTimeInTimeZone())
        //                        {
        //                            return BuildAppActionResultError(result, $"Mã giảm giá của bạn đã hết hạn");
        //                        }
        //                        else
        //                        {
        //                            result.Messages.Add($"Coupon {customerSavedCoupon.Coupon.Code} yều cầu hoá đơn phải trên {customerSavedCoupon.Coupon.MinimumAmount}");
        //                        }

        //                    }
        //                }
        //            }
        //            if (customerInfoDb.Account != null && orderRequestDto.LoyalPointsToUse.HasValue && orderRequestDto.LoyalPointsToUse > 0)
        //            {
        //                // Check if the user has enough points
        //                if (customerInfoDb.Account.LoyaltyPoint >= orderRequestDto.LoyalPointsToUse)
        //                {
        //                    // Calculate the discount (assuming 1 point = 1 currency unit)
        //                    data.LoyalPointUsed = (double)orderRequestDto.LoyalPointsToUse;
        //                }
        //                else
        //                {
        //                    // Handle the case where the user doesn't have enough points
        //                    return BuildAppActionResultError(result, $"Không đủ điểm tích lũy để sử dụng. Bạn còn {customerInfoDb.Account.LoyaltyPoint} điểm");
        //                }
        //            }

        //            data.FinalPrice = Math.Max(0, data.Amount - data.PaidDeposit - data.CouponDiscount - data.LoyalPointUsed);
        //            data.LoyalPointAdded = Math.Floor(data.FinalPrice / 100);
        //            result.Result = data;
        //        }
        //        else
        //        {
        //            result.Result = new CalculateOrderResponse
        //            {
        //                Amount = orderRequestDto.Total,
        //                FinalPrice = orderRequestDto.Total
        //            };
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        result = BuildAppActionResultError(result, ex.Message);
        //    }
        //    return result;
        //}

        public async Task<AppActionResult> GetAllOrderByPhoneNumber(string phoneNumber, OrderStatus? status, OrderType? orderType, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            try
            {
                PagedResult<OrderWithFirstDetailResponse> orderList = new PagedResult<OrderWithFirstDetailResponse>();

                if (status.HasValue && status > 0 && orderType.HasValue && orderType > 0)
                {
                    var orderListDb = await _repository.GetAllDataByExpression(o => o.StatusId == status && o.OrderTypeId == orderType && o.Account.PhoneNumber.Equals(phoneNumber), pageNumber, pageSize, o => o.OrderTypeId == OrderType.Delivery ? o.OrderDate : o.MealTime, false, p => p.Account!,
                                                                     p => p.Status!,
                                                                     p => p.Account!,
                                                                     p => p.LoyalPointsHistory!,
                                                                     p => p.OrderType!,
                                                                     p => p.Shipper,
                                                                     p => p.CustomerInfoAddress
                     );
                    orderListDb.Items = orderListDb.Items.OrderByDescending(o => o.MealTime).ThenByDescending(o => o.OrderDate).ToList();
                    var mappedData = _mapper.Map<List<OrderWithFirstDetailResponse>>(orderListDb.Items);
                    foreach (var order in mappedData)
                    {
                        var orderDetailDb = await _detailRepository.GetAllDataByExpression(o => o.OrderId == order.OrderId, 0, 0, null, false, o => o.DishSizeDetail.Dish, o => o.Combo, o => o.OrderDetailStatus);
                        if (orderDetailDb.Items.Count > 0)
                        {
                            order.OrderDetail = orderDetailDb.Items.FirstOrDefault();
                        }
                    }
                    orderList.Items = mappedData;
                    orderList.TotalPages = orderListDb.TotalPages;
                }
                else if (status.HasValue && status > 0)
                {
                    var orderListDb = await _repository.GetAllDataByExpression(o => o.StatusId == status && o.Account.PhoneNumber.Equals(phoneNumber), pageNumber, pageSize, o => o.OrderTypeId == OrderType.Delivery ? o.OrderDate : o.MealTime, false, p => p.Account!,
                                                                     p => p.Status!,
                                                                     p => p.Account!,
                                                                     p => p.LoyalPointsHistory!,
                                                                     p => p.OrderType!,
                                                                     p => p.Shipper,
                                                                     p => p.CustomerInfoAddress
                     );
                    orderListDb.Items = orderListDb.Items.OrderByDescending(o => o.MealTime).ThenByDescending(o => o.OrderDate).ToList();
                    var mappedData = _mapper.Map<List<OrderWithFirstDetailResponse>>(orderListDb.Items);
                    foreach (var order in mappedData)
                    {
                        var orderDetailDb = await _detailRepository.GetAllDataByExpression(o => o.OrderId == order.OrderId, 0, 0, null, false, o => o.DishSizeDetail.Dish, o => o.Combo, o => o.OrderDetailStatus);
                        if (orderDetailDb.Items.Count > 0)
                        {
                            order.OrderDetail = orderDetailDb.Items.FirstOrDefault();
                            order.ItemLeft = orderDetailDb.Items.Count() - 1;
                        }
                    }
                    orderList.Items = mappedData;
                    orderList.TotalPages = orderListDb.TotalPages;
                }
                else if (orderType.HasValue && orderType > 0)
                {
                    var orderListDb = await _repository.GetAllDataByExpression(o => o.OrderTypeId == orderType && o.Account.PhoneNumber.Equals(phoneNumber), pageNumber, pageSize, o => o.OrderTypeId == OrderType.Delivery ? o.OrderDate : o.MealTime, false, p => p.Account!,
                                                                     p => p.Status!,
                                                                     p => p.Account!,
                                                                     p => p.LoyalPointsHistory!,
                                                                     p => p.OrderType!,
                                                                     p => p.Shipper,
                                                                     p => p.CustomerInfoAddress
                     );
                    orderListDb.Items = orderListDb.Items.OrderByDescending(o => o.MealTime).ThenByDescending(o => o.OrderDate).ToList();
                    var mappedData = _mapper.Map<List<OrderWithFirstDetailResponse>>(orderListDb.Items);
                    foreach (var order in mappedData)
                    {
                        var orderDetailDb = await _detailRepository.GetAllDataByExpression(o => o.OrderId == order.OrderId, 0, 0, null, false, o => o.DishSizeDetail.Dish, o => o.Combo, o => o.OrderDetailStatus);
                        if (orderDetailDb.Items.Count > 0)
                        {
                            order.OrderDetail = orderDetailDb.Items.FirstOrDefault();
                        }
                    }
                    orderList.Items = mappedData;
                    orderList.TotalPages = orderListDb.TotalPages;
                }
                else
                {

                    var orderListDb = await
                        _repository.GetAllDataByExpression(p => p.Account!.PhoneNumber == phoneNumber, pageNumber, pageSize, o => o.OrderTypeId == OrderType.Delivery ? o.OrderDate : o.MealTime, false,
                            p => p.Status!,
                            p => p.Account!,
                            p => p.LoyalPointsHistory!,
                            p => p.OrderType!,
                            p => p.Shipper,
                            p => p.CustomerInfoAddress
                        );

                    orderListDb.Items = orderListDb.Items.OrderByDescending(o => o.MealTime).ThenByDescending(o => o.OrderDate).ToList();
                    var mappedData = _mapper.Map<List<OrderWithFirstDetailResponse>>(orderListDb.Items);
                    foreach (var order in mappedData)
                    {
                        var orderDetailDb = await _detailRepository.GetAllDataByExpression(o => o.OrderId == order.OrderId, 0, 0, null, false, o => o.DishSizeDetail.Dish, o => o.Combo, o => o.OrderDetailStatus);
                        if (orderDetailDb.Items.Count > 0)
                        {
                            order.OrderDetail = orderDetailDb.Items.FirstOrDefault();
                        }
                    }
                    orderList.Items = mappedData;
                    orderList.TotalPages = orderListDb.TotalPages;
                }
                orderList.Items = orderList.Items.OrderByDescending(m => m.OrderTypeId == OrderType.Delivery ? m.OrderDate : m.MealTime).ToList();
                result.Result = orderList;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAllOrderByStatus(OrderStatus? status, OrderType? orderType, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                PagedResult<Order> data = new PagedResult<Order>();


                if (status.HasValue && status > 0 && orderType.HasValue && orderType > 0)
                {
                    data = await _repository.GetAllDataByExpression(o => o.StatusId == status && o.OrderTypeId == orderType, pageNumber, pageSize, o => o.OrderTypeId == OrderType.Delivery ? o.OrderDate : o.MealTime
                    , false, p => p.Account!,
                       p => p.Status!,
                       p => p.Account!,
                       p => p.LoyalPointsHistory!,
                       p => p.OrderType!,
                       p => p.Shipper!,
                       p => p.CustomerInfoAddress
                       );
                }
                else if (status.HasValue && status > 0)
                {
                    data = await _repository.GetAllDataByExpression(o => o.StatusId == status, pageNumber, pageSize, o => o.OrderTypeId == OrderType.Delivery ? o.OrderDate : o.MealTime, false, p => p.Account!,
                       p => p.Status!,
                       p => p.Account!,
                       p => p.LoyalPointsHistory!,
                       p => p.OrderType!,
                       p => p.Shipper!,
                       p => p.CustomerInfoAddress
                       );
                }
                else if (orderType.HasValue && orderType > 0)
                {
                    data = await _repository.GetAllDataByExpression(o => o.OrderTypeId == orderType, pageNumber, pageSize, o => o.OrderTypeId == OrderType.Delivery ? o.OrderDate : o.MealTime, false, p => p.Account!,
                       p => p.Status!,
                       p => p.Account!,
                       p => p.LoyalPointsHistory!,
                       p => p.OrderType!,
                       p => p.Shipper!,
                       p => p.CustomerInfoAddress
                       );
                }
                else
                {
                    data = await _repository.GetAllDataByExpression(null, pageNumber, pageSize, o => o.OrderTypeId == OrderType.Delivery ? o.OrderDate : o.MealTime, false, p => p.Account!,
                      p => p.Status!,
                      p => p.Account!,
                      p => p.LoyalPointsHistory!,
                      p => p.OrderType!,
                      p => p.Shipper!,
                      p => p.CustomerInfoAddress
                      );
                }

                data.Items = data.Items.OrderByDescending(o => o.MealTime).ThenByDescending(o => o.OrderDate).ToList();
                var mappedData = _mapper.Map<List<OrderWithFirstDetailResponse>>(data.Items);
                foreach (var order in mappedData)
                {
                    var orderDetailDb = await _detailRepository.GetAllDataByExpression(o => o.OrderId == order.OrderId, 0, 0, null, false, o => o.DishSizeDetail.Dish, o => o.Combo, o => o.OrderDetailStatus);
                    if (orderDetailDb.Items.Count > 0)
                    {
                        order.OrderDetail = orderDetailDb.Items.FirstOrDefault();
                        order.ItemLeft = orderDetailDb.Items.Count() - 1;
                    }
                }
                result.Result = new PagedResult<OrderWithFirstDetailResponse>
                {
                    Items = mappedData.OrderByDescending(m => m.OrderTypeId == OrderType.Delivery ? m.OrderDate : m.MealTime).ToList(),
                    TotalPages = data.TotalPages,
                };
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetOrderByTime(double? minute, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone().AddMinutes(-minute.GetValueOrDefault(0));
                var orderDb = await _repository.GetAllDataByExpression(p => p.MealTime <= currentTime && (p.StatusId == OrderStatus.Processing || p.StatusId == OrderStatus.TemporarilyCompleted),
                    pageNumber, pageSize, p => p.MealTime, false, p => p.Account!,
                        p => p.Status!,
                        p => p.Account!,
                        p => p.LoyalPointsHistory!,
                        p => p.OrderType!,
                        p => p.CustomerInfoAddress
                    );
                result.Result = orderDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> CalculateReservation(ReservationDto request)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                double total = 0;
                var dishRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                var comboRepository = Resolve<IGenericRepository<Combo>>();
                var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var utility = Resolve<Utility>();
                if (request.ReservationDishDtos.Count() > 0)
                {

                    DishSizeDetail dishSizeDetailDb = null;
                    Combo comboDb = null;

                    foreach (var reservationDish in request.ReservationDishDtos!)
                    {
                        if (reservationDish.Combo != null)
                        {
                            comboDb = await comboRepository!.GetByExpression(c => c.ComboId == reservationDish.Combo.ComboId && c.EndDate > utility.GetCurrentDateTimeInTimeZone(), null);
                            if (comboDb == null)
                            {
                                result = BuildAppActionResultError(result, $"Không tìm thấy combo với id {reservationDish.Combo.ComboId}");
                                return result;
                            }
                            total += reservationDish.Quantity * comboDb.Price;
                        }
                        else
                        {

                            dishSizeDetailDb = await dishRepository!.GetByExpression(c => c.DishSizeDetailId == reservationDish.DishSizeDetailId.Value && c.IsAvailable, null);
                            if (dishSizeDetailDb == null)
                            {
                                result = BuildAppActionResultError(result, $"Không tìm thấy món ăn với id {reservationDish.DishSizeDetailId}");
                                return result;
                            }
                            total += reservationDish.Quantity * dishSizeDetailDb.Price;
                        }
                    }
                }

                var configurationDb = await configurationRepository.GetAllDataByExpression(c => c.Name.Equals(SD.DefaultValue.DEPOSIT_PERCENT), 0, 0, null, false, null);
                if (configurationDb.Items.Count == 0 || configurationDb.Items.Count > 1)
                {
                    return BuildAppActionResultError(result, $"Xảy ra lỗi khi lấy thông số cấu hình {SD.DefaultValue.DEPOSIT_PERCENT}");
                }

                double deposit = total * double.Parse(configurationDb.Items[0].CurrentValue);
                string tableTypeDeposit = SD.DefaultValue.DEPOSIT_FOR_NORMAL_TABLE;
                if (request.IsPrivate)
                {
                    tableTypeDeposit = SD.DefaultValue.DEPOSIT_FOR_PRIVATE_TABLE;
                }
                else
                {
                    tableTypeDeposit = SD.DefaultValue.DEPOSIT_FOR_NORMAL_TABLE;
                }
                var tableConfigurationDb = await configurationRepository.GetAllDataByExpression(c => c.Name.Equals(tableTypeDeposit), 0, 0, null, false, null);
                if (tableConfigurationDb.Items.Count == 0 || tableConfigurationDb.Items.Count > 1)
                {
                    return BuildAppActionResultError(result, $"Xảy ra lỗi khi lấy thông số cấu hình {tableTypeDeposit}");
                }
                deposit += double.Parse(tableConfigurationDb.Items[0].CurrentValue);
                result.Result = deposit;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        //public async Task<AppActionResult> GetOrderJsonByTableSessionId(Guid tableSessionId)
        //{
        //    AppActionResult result = new AppActionResult();
        //    try
        //    {
        //        //public List<OrderDetailsDto> OrderDetailsDtos { get; set; } = new List<OrderDetailsDto>();

        //        //public Guid? DishSizeDetailId { get; set; }
        //        //public ComboOrderDto? Combo { get; set; }
        //        //public int Quantity { get; set; }
        //        //public string? Note { get; set; }

        //        //public Guid ComboId { get; set; }
        //        //public List<Guid> DishComboIds { get; set; } = new List<Guid>();
        //        var data = new OrderRequestDto();
        //        var tableSessionRepository = Resolve<IGenericRepository<TableSession>>();
        //        var tableSessionService = Resolve<ITableSessionService>();
        //        var tableSessionDb = await tableSessionRepository.GetById(tableSessionId);
        //        if(tableSessionDb == null)
        //        {
        //            return BuildAppActionResultError(result, $"Không tìm thấy phiên dùng bữa với id {tableSessionId}");
        //        }

        //        data.ReservationId = tableSessionDb.ReservationId;
        //        data.TableId = tableSessionDb.TableId;

        //        var tableSessionResponse = await tableSessionService.GetTableSessionById(tableSessionId);
        //        if (!tableSessionResponse.IsSuccess) {
        //            return BuildAppActionResultError(result, $"Xảy ra lỗi khi lấy thông tin của phiên đặt bàn với id {tableSessionId}");
        //        }
        //        var tableSessionResponseResult = (TableSessionResponse) tableSessionResponse.Result;
        //        var orderList = new List<PrelistOrderDetails>();

        //        orderList.AddRange(tableSessionResponseResult.UncheckedPrelistOrderDetails);
        //        orderList.AddRange(tableSessionResponseResult.ReadPrelistOrderDetails);
        //        orderList.AddRange(tableSessionResponseResult.ReadyToServePrelistOrderDetails);

        //        var orderDetailsDtoList = new List<OrderDetailsDto>();

        //        foreach (var orderItem in orderList)
        //        {
        //            var orderDetailDto = new OrderDetailsDto
        //            {
        //                DishSizeDetailId = orderItem.PrelistOrder.DishSizeDetailId,
        //                Quantity = orderItem.PrelistOrder.Quantity,
        //                Note = orderItem.PrelistOrder.Note
        //            };

        //            if(orderItem.ComboOrderDetails.Count > 0)
        //            {
        //                var comboDto = new ComboOrderDto
        //                {
        //                    ComboId = (Guid)orderItem.PrelistOrder.ComboId,
        //                    DishComboIds = orderItem.ComboOrderDetails.Select(c => c.DishComboId).ToList()
        //                };
        //                orderDetailDto.Combo = comboDto;
        //            }
        //            orderDetailsDtoList.Add(orderDetailDto);
        //        }

        //        data.OrderDetailsDtos = orderDetailsDtoList;

        //        result.Result = data;

        //    } catch (Exception ex)
        //    {
        //        result = BuildAppActionResultError(result, ex.Message);
        //    }
        //    return result;
        //}

        public async Task<AppActionResult> GetAvailableTable(DateTime startTime, DateTime? endTime, int? numOfPeople, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var _configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var conditions = new List<Func<Expression<Func<Order, bool>>>>();

                // !(endTime < r.ReservationDate || r.EndTime < startTime)
                var configurationDb = await _configurationRepository.GetAllDataByExpression(c => c.Name.Equals(SD.DefaultValue.AVERAGE_MEAL_DURATION), 0, 0, null, false, null);
                if (configurationDb.Items.Count == 0 || configurationDb.Items.Count > 1)
                {
                    return BuildAppActionResultError(result, $"Xảy ra lỗi khi lấy thông số cấu hình {SD.DefaultValue.AVERAGE_MEAL_DURATION}");
                }
                if (!endTime.HasValue)
                {

                    endTime = startTime.AddHours(double.Parse(configurationDb.Items[0].CurrentValue));
                }

                conditions.Add(() => r => !(endTime < r.ReservationDate || (r.EndTime.HasValue && r.EndTime.Value < startTime || !r.EndTime.HasValue && r.ReservationDate.Value.AddHours(double.Parse(configurationDb.Items[0].CurrentValue)) < startTime))
                                          && r.StatusId != OrderStatus.Cancelled);

                Expression<Func<Order, bool>> expression = r => true; // Default expression to match all

                if (conditions.Count > 0)
                {
                    expression = DynamicLinqBuilder<Order>.BuildExpression(conditions);
                }

                // Get all collided reservations
                var unavailableReservation = await _repository.GetAllDataByExpression(expression, pageNumber, pageSize, null, false, null);
                var tableRepository = Resolve<IGenericRepository<Table>>();
                if (unavailableReservation!.Items.Count > 0)
                {
                    var unavailableReservationIds = unavailableReservation.Items.Select(x => x.OrderId);
                    var reservationTableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                    var reservedTableDb = await reservationTableDetailRepository!.GetAllDataByExpression(r => unavailableReservationIds.Contains(r.OrderId), 0, 0, null, false, r => r.Table.Room);
                    var reservedTableIds = reservedTableDb.Items!.Select(x => x.TableId);
                    var availableTableDb = await tableRepository!.GetAllDataByExpression(t => !reservedTableIds.Contains(t.TableId), 0, 0, null, false, t => t.Room);
                    result.Result = availableTableDb;
                }
                else
                {
                    result.Result = await tableRepository!.GetAllDataByExpression(null, 0, 0, null, false, r => r.Room);
                }
                //result.Result = availableReservation.Items.Select(x => x.Table);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        //public async Task<AppActionResult> SuggestTable(SuggestTableDto dto)
        //{
        //    AppActionResult result = new AppActionResult();
        //    try
        //    {
        //        if (dto.NumOfPeople <= 0)
        //        {
        //            return null;
        //        }

        //        //Get All Available Table
        //        var availableTableResult = await GetAvailableTable(dto.StartTime, dto.EndTime, dto.NumOfPeople, 0, 0);
        //        if (availableTableResult.IsSuccess)
        //        {
        //            var availableTable = (PagedResult<Table>)availableTableResult.Result!;
        //            if (availableTable.Items!.Count > 0)
        //            {
        //                var suitableTables = await GetTables(availableTable.Items, dto.NumOfPeople, dto.IsPrivate);
        //                result.Result = suitableTables.Count == 0 ? new List<Table>() : suitableTables;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        result = BuildAppActionResultError(result, ex.Message);
        //    }

        //    return result;
        //}
        public async Task<List<Table>> GetTables(List<Table> allAvailableTables, int quantity, bool isPrivate)
        {
            List<Table> result = new List<Table>();
            try
            {
                string tableCode = "T";
                if (isPrivate)
                {
                    tableCode = "V";
                }

                var tableType = allAvailableTables.Where(t => t.TableName.Contains(tableCode)).GroupBy(t => t.TableSizeId)
                                                  .ToDictionary(k => k.Key, k => k.ToList());
                if (tableType.Count == 0)
                {
                    return result;
                }

                if (!tableCode.Equals("V"))
                {
                    if (quantity <= 2)
                    {
                        if (tableType.ContainsKey(TableSize.TWO) && tableType[TableSize.TWO].Count > 0)
                        {
                            result.AddRange(tableType[TableSize.TWO].ToList());
                            return result;
                        }
                    }

                    if (quantity <= 4)
                    {
                        if (tableType.ContainsKey(TableSize.FOUR) && tableType[TableSize.SIX].Count > 0)
                        {
                            result.AddRange(tableType[TableSize.FOUR]);
                            return result;
                        }
                    }

                    if (quantity <= 6)
                    {
                        if (tableType.ContainsKey(TableSize.SIX) && tableType[TableSize.SIX].Count > 0)
                        {
                            result.AddRange(tableType[TableSize.SIX]);
                            return result;
                        }
                    }
                }

                if (quantity <= 8)
                {
                    if (tableType.ContainsKey(TableSize.EIGHT) && tableType[TableSize.EIGHT].Count > 0)
                    {
                        result.AddRange(tableType[TableSize.EIGHT]);
                        return result;
                    }
                }

                if (quantity <= 10)
                {
                    if (tableType.ContainsKey(TableSize.TEN) && tableType[TableSize.TEN].Count > 0)
                    {
                        result.AddRange(tableType[TableSize.TEN]);
                        return result;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                result = new List<Table>();
            }
            return result;


        }
        public async Task<List<Table>> GetSuitableTable(SuggestTableDto dto)
        {
            List<Table> result = null;
            try
            {
                if (dto.NumOfPeople <= 0)
                {
                    return null;
                }
                //Get All Available Table
                var availableTableResult = await GetAvailableTable(dto.StartTime, dto.EndTime, dto.NumOfPeople, 0, 0);
                if (availableTableResult.IsSuccess)
                {
                    var availableTable = (PagedResult<Table>)availableTableResult.Result!;
                    if (availableTable.Items!.Count > 0)
                    {
                        var suitableTables = await GetTables(availableTable.Items, dto.NumOfPeople, dto.IsPrivate);
                        result = suitableTables.Count > 0 ? suitableTables : new List<Table>();
                    }
                }
            }
            catch (Exception ex)
            {
                result = new List<Table>();
            }
            return result;
        }
        private async Task<List<Table>> GetCollidedTable(List<Guid> tableIds, DateTime startTime, int? numOfPeople)
        {
            List<Table> collidedTables = new List<Table>();
            try
            {
                AppActionResult availableTableResult = await GetAvailableTable(startTime, null, numOfPeople, 0, 100);
                if (!availableTableResult.IsSuccess)
                {
                    return collidedTables;
                }
                var availableTable = (List<Table>)availableTableResult.Result!;
                if (availableTable.Count > 0)
                {
                    collidedTables = availableTable.Where(at => tableIds.Contains(at.TableId)).ToList();
                }
            }
            catch (Exception ex)
            {
                return new List<Table>();
            }
            return collidedTables;
        }


        [Hangfire.Queue("notify-reservation-dish-to-kitchen")]
        public async Task NotifyReservationDishToKitchen()
        {
            try
            {
                var utility = Resolve<Utility>();
                var currentTime = utility!.GetCurrentDateTimeInTimeZone();
                var orderSessionRepository = Resolve<IGenericRepository<OrderSession>>();
                var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                var orderListDb = await _repository.GetAllDataByExpression(p => p.MealTime!.Value.AddHours(-1) <= currentTime && p.StatusId == OrderStatus.DepositPaid, 0, 0, null, false, null);
                if (orderListDb!.Items!.Count > 0 && orderListDb.Items != null)
                {
                    var orderDetailDb = await _detailRepository.GetAllDataByExpression(o => orderListDb.Items.Select(or => or.OrderId).ToList().Contains(o.OrderId), 0, 0, null, false, o => o.OrderSession);
                    if (orderDetailDb.Items.Count > 0)
                    {
                        orderDetailDb.Items.ForEach(o => o.OrderDetailStatusId = OrderDetailStatus.Unchecked);
                        var orderSessionDb = orderDetailDb.Items.Select(o => o.OrderSession).ToList();
                        orderSessionDb.ForEach(o => o.OrderSessionStatusId = OrderSessionStatus.Confirmed);
                        var orderDetailIds = orderDetailDb.Items.Select(o => o.OrderDetailId).ToList();
                        var comboOrderDetailDb = await comboOrderDetailRepository.GetAllDataByExpression(c => c.OrderDetailId.HasValue && orderDetailIds.Contains(c.OrderDetailId.Value), 0, 0, null, false, null);
                        comboOrderDetailDb.Items.ForEach(c => c.StatusId = DishComboDetailStatus.Unchecked);

                        await _detailRepository.UpdateRange(orderDetailDb.Items);
                        await comboOrderDetailRepository.UpdateRange(comboOrderDetailDb.Items);
                        await orderSessionRepository.UpdateRange(orderSessionDb);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, this);
            }
            Task.CompletedTask.Wait();
        }

        [Hangfire.Queue("account-daily-reservation-dish")]
        public async Task AccountDailyReservationDish()
        {
            try
            {
                var utility = Resolve<Utility>();
                var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                var emailService = Resolve<IEmailService>();
                var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                var orderSessionRepository = Resolve<IGenericRepository<OrderSession>>();
                var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var currentTime = utility!.GetCurrentDateTimeInTimeZone();
                //Order thats has mealdate > reservationDate And Deposit Paid && MealDate is today && deposit paid
                var orderListDb = await _repository.GetAllDataByExpression(p => p.OrderTypeId == OrderType.Reservation
                                                                                && p.MealTime!.Value.Date > p.ReservationDate.Value.Date
                                                                                && p.MealTime.Value.Date == currentTime.Date
                                                                                && (p.StatusId == OrderStatus.DepositPaid), 0, 0, null, false, null);
                List<DishSizeDetail> updateDishSizeDetailList = new List<DishSizeDetail>();

                foreach (var order in orderListDb.Items)
                {
                    var orderDetailDb = await _detailRepository.GetAllDataByExpression(o => o.OrderId == order.OrderId, 0, 0, null, false, o => o.DishSizeDetail);
                    foreach (var orderDetail in orderDetailDb.Items.Where(o => o.DishSizeDetailId.HasValue))
                    {
                        var existedDishSizeDetail = updateDishSizeDetailList.FirstOrDefault(u => u.DishSizeDetailId == orderDetail.DishSizeDetailId);
                        if (existedDishSizeDetail != null)
                        {
                            existedDishSizeDetail.QuantityLeft -= orderDetail.Quantity;
                            if (existedDishSizeDetail.QuantityLeft <= 0)
                            {
                                existedDishSizeDetail.IsAvailable = false;
                            }
                        }
                        else
                        {
                            var dishSizeDetailDb = await dishSizeDetailRepository.GetById(orderDetail.DishSizeDetailId);
                            dishSizeDetailDb.QuantityLeft -= orderDetail.Quantity;
                            if (dishSizeDetailDb.QuantityLeft <= 0)
                            {
                                dishSizeDetailDb.IsAvailable = false;
                            }
                            updateDishSizeDetailList.Add(dishSizeDetailDb);
                        }
                    }

                    foreach (var orderDetail in orderDetailDb.Items.Where(o => o.ComboId.HasValue))
                    {
                        var comboOrderDetailDb = await comboOrderDetailRepository.GetAllDataByExpression(c => c.OrderDetailId == orderDetail.OrderDetailId, 0, 0, null, false, o => o.OrderDetail, o => o.DishCombo);
                        foreach (var comboOrderDetail in comboOrderDetailDb.Items)
                        {
                            var existedDishSizeDetail = updateDishSizeDetailList.FirstOrDefault(u => u.DishSizeDetailId == comboOrderDetail.DishCombo.DishSizeDetailId);
                            if (existedDishSizeDetail != null)
                            {
                                existedDishSizeDetail.QuantityLeft -= orderDetail.Quantity * comboOrderDetail.DishCombo.Quantity;
                                if (existedDishSizeDetail.QuantityLeft <= 0)
                                {
                                    existedDishSizeDetail.IsAvailable = false;
                                }
                            }
                            else
                            {
                                var dishSizeDetailDb = await dishSizeDetailRepository.GetById(comboOrderDetail.DishCombo.DishSizeDetailId);
                                dishSizeDetailDb.QuantityLeft -= orderDetail.Quantity * comboOrderDetail.DishCombo.Quantity;
                                if (dishSizeDetailDb.QuantityLeft <= 0)
                                {
                                    dishSizeDetailDb.IsAvailable = false;
                                }
                                updateDishSizeDetailList.Add(dishSizeDetailDb);
                            }
                        }
                    }
                }

                await dishSizeDetailRepository.UpdateRange(updateDishSizeDetailList);
                await _repository.UpdateRange(orderListDb.Items);
                await _unitOfWork.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, this);
            }
            Task.CompletedTask.Wait();
        }
        public async Task<AppActionResult> GetUpdateCartComboDto(ComboChoice cart)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                //check combo Available
                var comboRepository = Resolve<IGenericRepository<Combo>>();
                var dishComboRepository = Resolve<IGenericRepository<DishCombo>>();
                var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                var dishRepository = Resolve<IGenericRepository<Dish>>();
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                Combo comboDb = null;
                DishCombo dishComboDb = null;
                DishSizeDetail dishSizeDetailDb = null;
                Dish dishDb = null;
                double total = 0;
                bool isRemoved = false;
                List<Item> cartItemstoRemove = new List<Item>();
                foreach (var cartItem in cart.items)
                {
                    isRemoved = false;
                    comboDb = await comboRepository.GetByExpression(c => c.ComboId == cartItem.combo.ComboId && !c.IsDeleted, null);
                    if (comboDb == null)
                    {
                        cartItemstoRemove.Add(cartItem);
                        continue;
                    }

                    if (comboDb.EndDate <= currentTime)
                    {
                        cartItemstoRemove.Add(cartItem);
                        continue;
                    }

                    cartItem.combo.Price = comboDb.Price;
                    foreach (var dishDetail in cartItem.selectedDishes)
                    {
                        dishComboDb = await dishComboRepository.GetById(dishDetail.DishComboId);
                        if (dishComboDb == null)
                        {
                            cartItemstoRemove.Add(cartItem);
                            isRemoved = true;
                            break;
                        }

                        dishSizeDetailDb = await dishSizeDetailRepository.GetByExpression(d => d.DishSizeDetailId == dishDetail.DishSizeDetailId && !d.Dish.IsDeleted);
                        if (dishSizeDetailDb == null)
                        {
                            cartItemstoRemove.Add(cartItem);
                            isRemoved = true;
                            break;
                        }

                        if (!dishSizeDetailDb.IsAvailable)
                        {
                            cartItemstoRemove.Add(cartItem);
                            isRemoved = true;
                            break;
                        }

                        dishDetail.DishSizeDetail.Price = dishSizeDetailDb.Price;
                        dishDetail.DishSizeDetail.Discount = dishSizeDetailDb.Discount;

                        dishDb = await dishRepository.GetByExpression(d => d.DishId == dishDetail.DishSizeDetail.DishId, null);
                        if (dishDb == null)
                        {
                            cartItemstoRemove.Add(cartItem);
                            isRemoved = true;
                            break;
                        }

                        if (!dishDb.isAvailable)
                        {
                            cartItemstoRemove.Add(cartItem);
                            isRemoved = true;
                            break;
                        }

                        dishDetail.DishSizeDetail.Dish.Name = dishDb.Name;
                        dishDetail.DishSizeDetail.Dish.Description = dishDb.Description;
                        dishDetail.DishSizeDetail.Dish.Image = dishDb.Image;
                    }
                    if (!isRemoved)
                    {
                        total += cartItem.combo.Price * (1 - cartItem.combo.Discount) * cartItem.quantity;
                    }
                }

                cartItemstoRemove.ForEach(c => cart.items.Remove(c));

                cart.total = total;
                //string unProcessedJson = JsonConvert.SerializeObject(cart);
                //string formattedJson = unProcessedJson.Replace("\\", "");
                result.Result = cart;
                //for each check dish dishSizeDetailId Price Isavailable, dishId Hin2h anh3 is Available
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
        public async Task<AppActionResult> GetUpdateCartDishDto(List<CartDishItem> cart)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var ratingRepository = Resolve<IGenericRepository<Rating>>();
                var dishComboRepository = Resolve<IGenericRepository<DishCombo>>();
                var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                var dishRepository = Resolve<IGenericRepository<Dish>>();
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                Combo comboDb = null;
                DishCombo dishComboDb = null;
                DishSizeDetail dishSizeDetailDb = null;
                Dish dishDb = null;
                List<CartDishItem> dishToRemove = new List<CartDishItem>();
                foreach (var dish in cart)
                {
                    dishDb = await dishRepository.GetById(Guid.Parse(dish.dish.dishId));
                    if (dishDb == null)
                    {
                        dishToRemove.Add(dish);
                        continue;
                    }

                    if (dishDb.IsDeleted)
                    {
                        dishToRemove.Add(dish);
                        continue;
                    }

                    dish.dish.name = dishDb.Name;
                    dish.dish.description = dishDb.Description;
                    dish.dish.image = dishDb.Image;
                    dish.dish.isAvailable = dishDb.isAvailable;
                    dish.dish.isAvailable = dishDb.IsDeleted;
                    dish.dish.isMainItem = dishDb.IsMainItem;

                    var ratingDb = await ratingRepository.GetAllDataByExpression(
                        o => o.OrderDetailId.HasValue && o.OrderDetail.DishSizeDetailId.HasValue && dishDb.DishId == o.OrderDetail.DishSizeDetail.DishId,
                        0, 0, null, false, null
                    );
                    if (ratingDb.Items.Count > 0)
                    {
                        dish.dish.averageRating = ratingDb.Items.Average(r => int.Parse(r.PointId.ToString()));
                        dish.dish.numberOfRating = ratingDb.Items.Count();
                    }

                    dishSizeDetailDb = await dishSizeDetailRepository.GetById(Guid.Parse(dish.size.dishSizeDetailId));
                    if (dishSizeDetailDb == null)
                    {
                        dishToRemove.Add(dish);
                        continue;
                    }

                    if (!dishSizeDetailDb.IsAvailable)
                    {
                        dishToRemove.Add(dish);
                        continue;
                    }

                    dish.size.price = dishSizeDetailDb.Price;
                    dish.size.discount = dishSizeDetailDb.Discount;
                    //dish.size.isAvailable = dishSizeDetailDb.IsAvailable;

                    //string unProcessedJson = JsonConvert.SerializeObject(cart);
                    //string formattedJson = unProcessedJson.Replace("\\\"", "\"");
                    result.Result = cart;
                }
                result.Result = cart;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
        public async Task<AppActionResult> UpdateOrderDetailStatus(List<UpdateOrderDetailItemRequest> orderDetailItems, bool isSuccessful)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                AppActionResult result = new AppActionResult();
                try
                {
                    var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                    var orderSessionRepository = Resolve<IGenericRepository<OrderSession>>();
                    var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                    var orderSessionService = Resolve<IOrderSessionService>();
                    var groupedDishCraftService = Resolve<IGroupedDishCraftService>();
                    var orderDetailIds = orderDetailItems.Select(o => o.OrderDetailId).ToList();
                    var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(p => orderDetailIds.Contains(p.OrderDetailId) && !(p.OrderDetailStatusId == OrderDetailStatus.Reserved || p.OrderDetailStatusId == OrderDetailStatus.ReadyToServe || p.OrderDetailStatusId == OrderDetailStatus.Cancelled), 0, 0, null, false, null);
                    //if (orderDetailDb.Items.Count != orderDetailIds.Count)
                    //{
                    //    return BuildAppActionResultError(result, $"Tồn tại id gọi món không nằm trong hệ thống hoặc không thể ập nhập trạng thái được");
                    //}

                    var utility = Resolve<Utility>();
                    var time = utility.GetCurrentDateTimeInTimeZone();
                    bool orderSessionUpdated = false;
                    bool hasFinishedDish = false;
                    foreach (var orderDetail in orderDetailDb.Items.ToList())
                    {
                        if (orderDetail.ComboId.HasValue)
                        {
                            var orderComboDetailDb = await comboOrderDetailRepository.GetAllDataByExpression(c => c.OrderDetailId == orderDetail.OrderDetailId

                                                                                                                    && (c.StatusId != DishComboDetailStatus.Reserved
                                                                                                                        && c.StatusId != DishComboDetailStatus.ReadyToServe
                                                                                                                        && c.StatusId != DishComboDetailStatus.Cancelled),
                                                                                                                  0, 0, null, false, c => c.DishCombo.DishSizeDetail);

                            if (orderComboDetailDb.Items.Count() > 0)
                            {
                                var orderComboDetail = orderComboDetailDb.Items.FirstOrDefault(o => o.DishCombo.DishSizeDetail.DishId == orderDetailItems.FirstOrDefault(od => od.OrderDetailId == orderDetail.OrderDetailId
                                                                                                                                                        && od.DishId == o.DishCombo.DishSizeDetail.DishId)?.DishId);
                                if (orderComboDetail != null)
                                {
                                    if (orderComboDetail.StatusId == DishComboDetailStatus.Unchecked)
                                    {
                                        if (isSuccessful)
                                        {
                                            orderComboDetail.StatusId = DishComboDetailStatus.Processing;
                                        }
                                        else
                                        {
                                            orderComboDetail.StatusId = DishComboDetailStatus.Cancelled;
                                        }

                                    }
                                    else if (orderComboDetail.StatusId == DishComboDetailStatus.Processing)
                                    {
                                        if (isSuccessful)
                                        {
                                            orderComboDetail.StatusId = DishComboDetailStatus.ReadyToServe;
                                            if (!hasFinishedDish) hasFinishedDish = true;
                                        }
                                        else
                                        {
                                            return BuildAppActionResultError(result, $"Chi tiết đơn hàng đang ở trạng thái dang xử lí, không thể huỷ");
                                        }
                                    }
                                }

                                if (orderComboDetail.StatusId == DishComboDetailStatus.Processing)
                                {
                                    if (orderComboDetailDb.Items.Where(o => o.ComboOrderDetailId != orderComboDetail.ComboOrderDetailId)
                                                               .All(o => o.StatusId == DishComboDetailStatus.Unchecked))
                                    {
                                        orderDetail.OrderDetailStatusId = OrderDetailStatus.Processing;
                                    }
                                }
                                else if (orderComboDetail.StatusId == DishComboDetailStatus.ReadyToServe)
                                {
                                    if (orderComboDetailDb.Items.Where(o => o.ComboOrderDetailId != orderComboDetail.ComboOrderDetailId)
                                                              .All(o => o.StatusId == DishComboDetailStatus.ReadyToServe))
                                    {
                                        orderDetail.OrderDetailStatusId = OrderDetailStatus.ReadyToServe;
                                        if (!hasFinishedDish) hasFinishedDish = true;
                                    }
                                }
                                await comboOrderDetailRepository.Update(orderComboDetail);
                            }
                        }
                        else
                        {
                            if (orderDetail.OrderDetailStatusId == OrderDetailStatus.Unchecked)
                            {
                                if (isSuccessful)
                                {
                                    orderDetail.OrderDetailStatusId = OrderDetailStatus.Processing;
                                }
                                else
                                {
                                    orderDetail.OrderDetailStatusId = OrderDetailStatus.Cancelled;
                                }

                            }
                            else if (orderDetail.OrderDetailStatusId == OrderDetailStatus.Processing)
                            {
                                if (isSuccessful)
                                {
                                    orderDetail.OrderDetailStatusId = OrderDetailStatus.ReadyToServe;
                                    if (!hasFinishedDish) hasFinishedDish = true;
                                }
                                else
                                {
                                    return BuildAppActionResultError(result, $"Chi tiết đơn hàng đang ở trạng thái dang xử lí, không thể huỷ");
                                }
                            }
                        }


                    }

                    await orderDetailRepository.UpdateRange(orderDetailDb.Items);
                    var orderSessionIds = orderDetailDb.Items.DistinctBy(o => o.OrderSessionId).Select(o => o.OrderSessionId).ToList();
                    var orderSessionDb = await orderSessionRepository.GetAllDataByExpression(o => orderSessionIds.Contains(o.OrderSessionId), 0, 0, null, false, null);
                    var orderSessionSet = new HashSet<Guid>();
                    foreach (var session in orderSessionDb.Items)
                    {
                        if (orderSessionSet.Contains(session.OrderSessionId))
                        {
                            continue;
                        }

                        var sessionOrderDetailDb = await _detailRepository.GetAllDataByExpression(o => o.OrderSessionId == session.OrderSessionId && !orderDetailIds.Contains(o.OrderDetailId), 0, 0, null, false, null);

                        if (session.OrderSessionStatusId == OrderSessionStatus.Confirmed)
                        {
                            session.OrderSessionStatusId = OrderSessionStatus.Processing;
                            orderSessionUpdated = true;
                        }
                        else if (orderDetailDb.Items.Where(o => o.OrderSessionId == session.OrderSessionId).All(o => o.OrderDetailStatusId == OrderDetailStatus.Cancelled)
                                 && sessionOrderDetailDb.Items.All(o => o.OrderDetailStatusId == OrderDetailStatus.Cancelled))
                        {
                            session.OrderSessionStatusId = OrderSessionStatus.Cancelled;
                            orderSessionUpdated = true;
                        }
                        else if (orderDetailDb.Items.Where(o => o.OrderSessionId == session.OrderSessionId).All(o => o.OrderDetailStatusId == OrderDetailStatus.ReadyToServe)
                                 && sessionOrderDetailDb.Items.All(o => o.OrderDetailStatusId == OrderDetailStatus.ReadyToServe))
                        {
                            session.OrderSessionStatusId = OrderSessionStatus.Completed;
                            orderSessionUpdated = true;
                            if (orderDetailDb.Items.FirstOrDefault().Order.OrderTypeId == OrderType.Delivery)
                            {
                                await ChangeOrderStatus(orderDetailDb.Items.FirstOrDefault().OrderId, true, null, false);
                            }
                            else
                            {
                                if (orderDetailDb.Items.FirstOrDefault().Order.StatusId == OrderStatus.Processing)
                                {
                                    //All OrderDetail in DB is ready to serve
                                    var allOrderDetailDb = await _detailRepository.GetAllDataByExpression(o => o.OrderId == orderDetailDb.Items.FirstOrDefault().OrderId, 0, 0, null, false, null);
                                    if (allOrderDetailDb.Items.All(a => a.OrderDetailStatusId == OrderDetailStatus.ReadyToServe || a.OrderDetailStatusId == OrderDetailStatus.Cancelled))
                                    {
                                        await ChangeOrderStatus(orderDetailDb.Items.FirstOrDefault().OrderId, true, OrderStatus.TemporarilyCompleted, false);
                                    }
                                }
                            }
                        }
                        orderSessionSet.Add(session.OrderSessionId);
                    }

                    await orderSessionRepository.UpdateRange(orderSessionDb.Items);

                    await _unitOfWork.SaveChangesAsync();

                    await groupedDishCraftService.UpdateGroupedDish(orderDetailDb.Items.Where(o => o.OrderDetailStatusId == OrderDetailStatus.Unchecked
                                                                                                || o.OrderDetailStatusId == OrderDetailStatus.Processing
                                                                                                || o.OrderDetailStatusId == OrderDetailStatus.ReadyToServe)
                                                                                       .Select(o => o.OrderDetailId).ToList());

                    await _hubServices.SendAsync(SD.SignalMessages.LOAD_ORDER_DETAIL_STATUS);
                    if (orderSessionUpdated)
                    {
                        await _hubServices.SendAsync(SD.SignalMessages.LOAD_ORDER);
                        await _hubServices.SendAsync(SD.SignalMessages.LOAD_GROUPED_DISHES);
                        await _hubServices.SendAsync(SD.SignalMessages.LOAD_ORDER_SESIONS);
                    }

                    if (hasFinishedDish)
                    {
                        await _hubServices.SendAsync(SD.SignalMessages.LOAD_FINISHED_DISHES);
                    }
                    result.Result = orderDetailDb;
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
                return result;
            }

        }

        public async Task<AppActionResult> GetCurrentTableSession()
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                var tableDetailDb = await tableDetailRepository.GetAllDataByExpression(t => t.Order.StatusId == OrderStatus.Processing || t.Order.StatusId == OrderStatus.TemporarilyCompleted, 0, 0, null, false, t => t.Table, t => t.Order);
                if (tableDetailDb.Items.Count > 0)
                {
                    var latestSessionByTable = tableDetailDb.Items.GroupBy(t => t.TableId).Select(t => t.OrderByDescending(ta => ta.StartTime).FirstOrDefault());
                    List<KitchenTableSimpleResponse> data = new List<KitchenTableSimpleResponse>();
                    foreach (var item in latestSessionByTable)
                    {
                        var uncheckedPreorderList = await orderDetailRepository.GetAllDataByExpression(p => p.OrderId == item.OrderId && p.OrderDetailStatusId == OrderDetailStatus.Unchecked, 0, 0, p => p.OrderTime, false, null);
                        data.Add(new KitchenTableSimpleResponse
                        {
                            TableId = item.TableId,
                            OrderId = item.OrderId,
                            TableName = item.Table.TableName,
                            UnCheckedNumberOfDishes = uncheckedPreorderList.Items.Count
                        });
                    }
                    result.Result = data;
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetTableReservationWithTime(Guid tableId, DateTime? time)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var tableRepository = Resolve<IGenericRepository<Table>>();
                var reservationTableRepository = Resolve<IGenericRepository<TableDetail>>();
                var tableDb = await tableRepository!.GetById(tableId);
                if (tableDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy bàn với id {tableId}");
                }
                var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var configDb = await configurationRepository!.GetByExpression(c => c.Name.Equals(SD.DefaultValue.TIME_TO_LOOK_UP_FOR_RESERVATION), null);
                if (configDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy cấu hình với tên {SD.DefaultValue.TIME_TO_LOOK_UP_FOR_RESERVATION}");
                }

                if (!time.HasValue)
                {
                    var utility = Resolve<Utility>();
                    time = utility!.GetCurrentDateTimeInTimeZone();
                }
                var nearReservationDb = await reservationTableRepository.GetAllDataByExpression(r => r.TableId == tableId
                                                                && r.Order!.MealTime <= time.Value.AddHours(double.Parse(configDb.CurrentValue))
                                                                && r.Order.MealTime.Value.AddHours(double.Parse(configDb.CurrentValue)) >= time
                                                                && r.Order.OrderTypeId == OrderType.Reservation
                                                                && (r.Order.StatusId == OrderStatus.DepositPaid || r.Order.StatusId == OrderStatus.Processing || r.Order.StatusId == OrderStatus.TemporarilyCompleted), 0, 0, r => r.Order!.ReservationDate, true, o => o.Order);
                if (nearReservationDb.Items.Count > 0)
                {
                    result = await GetAllOrderDetail(nearReservationDb.Items.OrderBy(o => Math.Abs(o.Order.MealTime.Value.Ticks - time.Value.Ticks)).FirstOrDefault().OrderId);
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAllReservationDetail2(Guid orderId)
        {
            try
            {
                var order = await _repository.GetByExpression(r => r.OrderId == orderId, r => r.Account!, r => r.Shipper);
                if (order == null)
                {
                    return BuildAppActionResultError(new AppActionResult(), $"Không tìm thấy thông tin đặt bàn với id {orderId}");
                }

                var transactionRepository = Resolve<IGenericRepository<Transaction>>();
                var orderTransactionDb = await transactionRepository.GetAllDataByExpression(o => o.OrderId.HasValue && o.OrderId == orderId && o.TransationStatusId == TransationStatus.SUCCESSFUL, 0, 0, p => p.PaidDate, false, null);
                var reservationTableDetails = await GetReservationTableDetails(orderId);
                var reservationDishes = await GetReservationDishes(orderId);
                var orderResponse = _mapper.Map<OrderResponse>(order);
                var successfulDepositTransaction = orderTransactionDb.Items.Where(o => o.TransactionTypeId == TransactionType.Deposit).ToList();
                if (successfulDepositTransaction.Count() == 1)
                {
                    orderResponse.DepositPaidDate = successfulDepositTransaction[0].PaidDate;
                }

                var successfulOrderTransaction = orderTransactionDb.Items.Where(o => o.TransactionTypeId == TransactionType.Order).ToList();
                if (successfulOrderTransaction.Count() == 1)
                {
                    orderResponse.OrderPaidDate = successfulOrderTransaction[0].PaidDate;
                }

                var data = new ReservationReponse
                {
                    Order = orderResponse,
                    OrderTables = reservationTableDetails,
                    OrderDishes = reservationDishes
                };

                return new AppActionResult { Result = data };
            }
            catch (Exception ex)
            {
                return BuildAppActionResultError(new AppActionResult(), ex.Message);
            }
        }

        private async Task<List<TableDetail>> GetReservationTableDetails(Guid orderId)
        {
            var reservationTableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
            var result = await reservationTableDetailRepository!.GetAllDataByExpression(
                o => o.OrderId == orderId,
                0, 0, null, false,
                o => o.Table!.Room,
                o => o.Order!
            );
            return result.Items!;
        }

        private async Task<List<Common.DTO.Response.OrderDishDto>> GetReservationDishes(Guid orderId)
        {
            var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
            var reservationDishDb = await orderDetailRepository!.GetAllDataByExpression(
                o => o.OrderId == orderId,
                0, 0, null, false,
                o => o.Combo!,
                o => o.DishSizeDetail!.Dish!,
                o => o.DishSizeDetail!.DishSize!,
                o => o.OrderDetailStatus
            );

            var reservationDishes = new List<Common.DTO.Response.OrderDishDto>();
            var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();

            foreach (var r in reservationDishDb.Items)
            {
                if (r.Combo != null)
                {
                    var comboDishDto = await CreateComboDishDto(r, comboOrderDetailRepository);
                    reservationDishes.Add(new Common.DTO.Response.OrderDishDto
                    {
                        OrderDetailsId = r.OrderDetailId,
                        ComboDish = comboDishDto
                    });
                }
                else
                {
                    reservationDishes.Add(new Common.DTO.Response.OrderDishDto
                    {
                        OrderDetailsId = r.OrderDetailId,
                        DishSizeDetailId = r.DishSizeDetailId,
                        DishSizeDetail = r.DishSizeDetail
                    });
                }
            }

            return reservationDishes;
        }

        private async Task<ComboDishDto> CreateComboDishDto(OrderDetail r, IGenericRepository<ComboOrderDetail> comboOrderDetailRepository)
        {
            var comboOrderDetails = await comboOrderDetailRepository!.GetAllDataByExpression(
                d => d.OrderDetailId == r.OrderDetailId,
                0, 0, null, false,
                d => d.DishCombo.DishSizeDetail!.Dish,
                d => d.DishCombo.DishSizeDetail.DishSize,
                d => d.DishCombo.ComboOptionSet
            );

            return new ComboDishDto
            {
                ComboId = r.Combo.ComboId,
                Combo = r.Combo,
                DishCombos = comboOrderDetails.Items.Select(d => d.DishCombo).ToList()
            };
        }

        public async Task<AppActionResult> GetAllOrderDetail(Guid reservationId)
        {
            try
            {
                var order = await _repository.GetByExpression(r => r.OrderId == reservationId, r => r.Account, r => r.Shipper, r => r.Status, r => r.OrderType, r => r.LoyalPointsHistory, r => r.CustomerInfoAddress);
                if (order == null)
                {
                    return BuildAppActionResultError(new AppActionResult(), $"Không tìm thấy thông tin đặt bàn với id {reservationId}");
                }
                var transactionRepository = Resolve<IGenericRepository<Transaction>>();
                var orderResponse = _mapper.Map<OrderResponse>(order);
                var orderTransactionDb = await transactionRepository.GetAllDataByExpression(o => o.OrderId.HasValue && o.OrderId == reservationId, 0, 0, null, false, o => o.TransactionType);
                if (orderTransactionDb.Items.Count() > 0)
                {
                    orderResponse.Transaction = orderTransactionDb.Items.OrderByDescending(o => o.PaidDate).OrderByDescending(o => o.Date).FirstOrDefault();
                    var successfulDepositTransaction = orderTransactionDb.Items.Where(o => o.TransationStatusId == TransationStatus.SUCCESSFUL && o.TransactionTypeId == TransactionType.Deposit).ToList();
                    if (successfulDepositTransaction.Count() == 1)
                    {
                        orderResponse.DepositPaidDate = successfulDepositTransaction.OrderByDescending(o => o.PaidDate).OrderByDescending(o => o.Date).First().PaidDate;
                    }

                    var successfulOrderTransaction = orderTransactionDb.Items.Where(o => o.TransationStatusId == TransationStatus.SUCCESSFUL && o.TransactionTypeId == TransactionType.Order).ToList();
                    if (successfulOrderTransaction.Count() == 1)
                    {
                        orderResponse.OrderPaidDate = successfulOrderTransaction.OrderByDescending(o => o.PaidDate).OrderByDescending(o => o.Date).First().PaidDate;
                    }
                }

                var reservationTableDetails = await GetReservationTableDetails(reservationId);
                var reservationDishes = await GetReservationDishes2(reservationId);

                var data = new ReservationReponse
                {
                    Order = await AssignEstimatedTimeToOrder(orderResponse),
                    OrderTables = reservationTableDetails,
                    OrderDishes = reservationDishes
                };

                return new AppActionResult { Result = data };
            }
            catch (Exception ex)
            {
                return BuildAppActionResultError(new AppActionResult(), ex.Message);
            }
        }
        private async Task<List<Common.DTO.Response.OrderDishDto>> GetReservationDishes2(Guid reservationId, List<OrderDetail> orderDetails = null)
        {
            var reservationDishDb = new PagedResult<OrderDetail>();
            if (orderDetails != null && orderDetails.Count > 0)
            {
                reservationDishDb = new PagedResult<OrderDetail>
                {
                    Items = orderDetails
                };
            }
            else
            {
                reservationDishDb = await _detailRepository.GetAllDataByExpression(
                    o => o.OrderId == reservationId,
                    0, 0, null, false,
                    o => o.DishSizeDetail.Dish,
                    o => o.DishSizeDetail.DishSize,
                    o => o.Combo,
                    o => o.OrderDetailStatus
                );
            }

            var reservationDishes = new List<Common.DTO.Response.OrderDishDto>();
            var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();

            foreach (var r in reservationDishDb.Items)
            {
                if (r.Combo != null)
                {
                    var comboDishDto = await CreateComboDishDto(r, comboOrderDetailRepository);
                    reservationDishes.Add(new Common.DTO.Response.OrderDishDto
                    {
                        OrderDetailsId = r.OrderDetailId,
                        ComboDish = comboDishDto,
                        Quantity = r.Quantity,
                        StatusId = r.OrderDetailStatusId,
                        Status = r.OrderDetailStatus,
                        OrderTime = r.OrderTime,
                        Note = r.Note,
                        IsRated = r.IsRated,
                    });
                }
                else
                {
                    reservationDishes.Add(new Common.DTO.Response.OrderDishDto
                    {
                        OrderDetailsId = r.OrderDetailId,
                        DishSizeDetailId = r.DishSizeDetailId,
                        DishSizeDetail = r.DishSizeDetail,
                        Quantity = r.Quantity,
                        StatusId = r.OrderDetailStatusId,
                        Status = r.OrderDetailStatus,
                        OrderTime = r.OrderTime,
                        Note = r.Note,
                        IsRated = r.IsRated
                    });
                }
            }

            return reservationDishes;
        }
        public async Task<AppActionResult> CalculateDeliveryOrder(Guid customerInfoAddressId)
        {
            var result = new AppActionResult();
            var configurationRepository = Resolve<IGenericRepository<Configuration>>();
            var customerInfoAddressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
            var mapService = Resolve<IMapService>();
            try
            {
                var restaurantLatConfig = await configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.RESTAURANT_LATITUDE);
                var restaurantLngConfig = await configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.RESTAURANT_LNG);
                var restaurantMaxDistanceToOrderConfig = await configurationRepository.GetByExpression(p => p.Name == SD.DefaultValue.DISTANCE_ORDER);
                var distanceStepConfig = await configurationRepository.GetByExpression(p => p.Name == SD.DefaultValue.DISTANCE_STEP);
                var flatCostDistance = await configurationRepository.GetByExpression(p => p.Name == SD.DefaultValue.FLAT_COST_DISTANCE);
                var distanceStepFeeConfig = await configurationRepository.GetByExpression(p => p.Name == SD.DefaultValue.DISTANCE_STEP_FEE);

                double total = 0;
                var customerInfoAddressDb = await customerInfoAddressRepository!.GetByExpression(p => p.CustomerInfoAddressId == customerInfoAddressId && p.IsCurrentUsed);
                if (customerInfoAddressDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy địa chỉ với id {customerInfoAddressId}");
                }

                var restaurantLat = Double.Parse(restaurantLatConfig.CurrentValue);
                var restaurantLng = Double.Parse(restaurantLngConfig.CurrentValue);

                double[] restaurantAddress = new double[]
                {
                    restaurantLat, restaurantLng
                };

                double[] customerAddress = new double[]
                {
                    customerInfoAddressDb.Lat, customerInfoAddressDb.Lng
                };

                var distanceResponse = await mapService!.GetEstimateDeliveryResponse(restaurantAddress, customerAddress);
                var eletement = distanceResponse.Result as EstimatedDeliveryTimeDto.Response;

                var distance = eletement!.TotalDistance;
                var maxDistanceToOrder = double.Parse(restaurantMaxDistanceToOrderConfig!.CurrentValue);
                var distanceStep = int.Parse(distanceStepConfig!.CurrentValue);
                var distanceStepFee = double.Parse(distanceStepFeeConfig!.CurrentValue);
                if (distance > maxDistanceToOrder)
                {
                    return BuildAppActionResultError(result, $"Nhà hàng chỉ hỗ trợ cho đơn giao hàng trong bán kính 10km");
                }
                else
                {
                    int step = (int)Math.Ceiling(distance / distanceStep);
                    total = Math.Ceiling(distanceStepFee * step / 1000) * 1000;
                }
                result.Result = total;
            }
            catch (Exception ex)
            {
                return BuildAppActionResultError(new AppActionResult(), ex.Message);
            }
            return result;
        }
        public async Task<AppActionResult> GetAllTableDetails(OrderStatus orderStatus, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
            try
            {
                var tableDetailList = new List<TableDetail>();
                var orderDb = await _repository.GetAllDataByExpression(p => p.StatusId == orderStatus && p.OrderTypeId == OrderType.MealWithoutReservation, pageNumber, pageSize, p => p.OrderDate, false, null);

                if (orderDb.Items == null || !orderDb.Items.Any())
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy đơn đặt hàng với status {orderStatus}");
                }

                foreach (var order in orderDb.Items)
                {
                    var tableDetailDb = await tableDetailRepository!.GetAllDataByExpression(
                        p => p.OrderId == order.OrderId,
                        0,
                        0,
                        p => p.StartTime,
                        false,
                        p => p.Order!,
                        p => p.Order!.Status!,
                        p => p.Order!.Account,
                        p => p.Order!.LoyalPointsHistory!,
                        p => p.Order!.OrderType!,
                        p => p.Table!.Room!,
                        p => p.Table!.TableSize!
                    );

                    var tableDetail = tableDetailDb.Items.FirstOrDefault();
                    if (tableDetail != null)
                    {
                        tableDetailList.Add(tableDetail);
                    }
                }

                result.Result = new PagedResult<TableDetail>
                {
                    Items = tableDetailList,
                    TotalPages = orderDb.TotalPages,
                };
            }
            catch (Exception ex)
            {
                return BuildAppActionResultError(new AppActionResult(), ex.Message);
            }
            return result;
        }
        public async Task<AppActionResult> AssignOrderForShipper(string shipperId, List<Guid> orderListId)
        {
            var result = new AppActionResult();
            var notificationService = Resolve<INotificationMessageService>();
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var accountRepository = Resolve<IGenericRepository<Account>>();
                try
                {
                    var orderList = new List<Order>();
                    var customerAddressRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
                    var shipperAccountDb = await accountRepository!.GetByExpression(p => p.Id == shipperId);
                    if (shipperAccountDb == null)
                    {
                        return BuildAppActionResultError(result, $"Không tìm thấy tài khoản của shipper với id {shipperId}");
                    }
                    foreach (var orderId in orderListId)
                    {
                        var orderDb = await _repository.GetByExpression(p => p.OrderId == orderId && p.OrderTypeId == OrderType.Delivery && p.StatusId == OrderStatus.ReadyForDelivery);
                        if (orderDb == null)
                        {
                            return BuildAppActionResultError(result, $"Không tìm thấy đơn hàng với id {orderId}");
                        }
                        orderDb.ShipperId = shipperId;
                        await ChangeOrderStatus(orderDb.OrderId, true, null);
                        orderList.Add(orderDb);

                        var addressDb = await customerAddressRepository!.GetById(orderDb.AddressId);
                        string message = $"Bạn có đơn cần giao tại {addressDb.CustomerInfoAddressName}";
                        await notificationService!.SendNotificationToShipperAsync(shipperAccountDb.Id, message);
                        await _hubServices.SendAsync(SD.SignalMessages.LOAD_ASSIGNED_ORDER);
                    }


                    if (!BuildAppActionResultIsError(result))
                    {
                        await _repository.UpdateRange(orderList);
                        await _unitOfWork.SaveChangesAsync();
                        scope.Complete();
                    }
                }
                catch (Exception ex)
                {
                    return BuildAppActionResultError(new AppActionResult(), ex.Message);
                }
                return result;
            }
        }

        public async Task<AppActionResult> UploadConfirmedOrderImage(ConfirmedOrderRequest confirmedOrderRequest)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var result = new AppActionResult();
                try
                {
                    var firebaseService = Resolve<IFirebaseService>();
                    var utility = Resolve<Utility>();
                    var orderDb = await _repository.GetByExpression(p => p.OrderId == confirmedOrderRequest.OrderId && p.StatusId == OrderStatus.Delivering);
                    if (orderDb == null)
                    {
                        return BuildAppActionResultError(result, $"Không tìm thấy đơn hàng với id {confirmedOrderRequest.OrderId}");
                    }

                    if (!confirmedOrderRequest.IsSuccessful.HasValue || confirmedOrderRequest.IsSuccessful.Value)
                    {
                        var pathName = SD.FirebasePathName.ORDER_PREFIX + $"{confirmedOrderRequest.OrderId}{Guid.NewGuid()}.jpg";
                        var upload = await firebaseService!.UploadFileToFirebase(confirmedOrderRequest.Image, pathName);

                        if (!upload.IsSuccess)
                        {
                            return BuildAppActionResultError(result, "Upload hình ảnh không thành công");
                        }
                        orderDb.ValidatingImg = upload.Result!.ToString();

                        orderDb.DeliveredTime = utility.GetCurrentDateTimeInTimeZone();
                        orderDb.StatusId = OrderStatus.Completed;
                    }
                    else
                    {
                        orderDb.CancelledTime = utility.GetCurrentDateTimeInTimeZone();
                        orderDb.CancelDeliveryReason = confirmedOrderRequest.CancelReason;
                        orderDb.StatusId = OrderStatus.Cancelled;
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        await _repository.Update(orderDb);
                        await _unitOfWork.SaveChangesAsync();
                        scope.Complete();
                    }
                }
                catch (Exception ex)
                {
                    return BuildAppActionResultError(new AppActionResult(), ex.Message);
                }
                return result;
            }
        }

        public async Task<AppActionResult> GetAllOrderByShipperId(string shipperId, OrderStatus? orderStatus, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            var accountRepository = Resolve<IGenericRepository<Account>>();
            var customerInfoRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
            var configurationRepository = Resolve<IGenericRepository<Configuration>>();
            var mapService = Resolve<IMapService>();
            try
            {
                var shipperDb = await accountRepository!.GetByExpression(p => p.Id == shipperId);
                if (shipperDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy shipper với id {shipperId}");
                }

                var startLatConfig = await configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.RESTAURANT_LATITUDE);
                var startLngConfig = await configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.RESTAURANT_LNG);

                var startLat = Double.Parse(startLatConfig.CurrentValue);
                var startLng = Double.Parse(startLngConfig.CurrentValue);


                PagedResult<Order> data = new PagedResult<Order>();
                if (orderStatus.HasValue && orderStatus > 0)
                {
                    data = await _repository.GetAllDataByExpression(o => o.StatusId == orderStatus && o.OrderTypeId == OrderType.Delivery && o.ShipperId == shipperId, pageNumber, pageSize, o => o.OrderDate, false, p => p.Account!,
                       p => p.Status!,
                       p => p.Account!,
                       p => p.LoyalPointsHistory!,
                       p => p.OrderType!,
                       p => p.CustomerInfoAddress
                       );
                }
                else
                {
                    data = await _repository.GetAllDataByExpression(o => o.OrderTypeId == OrderType.Delivery && o.ShipperId == shipperId, pageNumber, pageSize, o => o.OrderDate, false, p => p.Account!,
                      p => p.Status!,
                      p => p.Account!,
                      p => p.LoyalPointsHistory!,
                      p => p.OrderType!,
                      p => p.Shipper,
                      p => p.CustomerInfoAddress
                      );
                }

                double[] startDestination = new double[2];
                startDestination[0] = startLat;
                startDestination[1] = startLng;


                data.Items = data.Items.OrderByDescending(o => o.MealTime).ThenByDescending(o => o.OrderDate).ToList();
                var mappedData = _mapper.Map<List<OrderWithFirstDetailResponse>>(data.Items);
                foreach (var order in mappedData)
                {
                    var orderDetailDb = await _detailRepository.GetAllDataByExpression(o => o.OrderId == order.OrderId, 0, 0, null, false, o => o.DishSizeDetail.Dish, o => o.Combo, o => o.OrderDetailStatus);
                    if (orderDetailDb.Items.Count > 0)
                    {
                        order.OrderDetail = orderDetailDb.Items.FirstOrDefault();
                        order.ItemLeft = orderDetailDb.Items.Count() - 1;

                        var customerAddressDb = await customerInfoRepository!.GetByExpression(p => p.CustomerInfoAddressId == order.AddressId);
                        if (customerAddressDb == null)
                        {
                            return BuildAppActionResultError(result, $"Không tìm thấy địa chỉ với id {order.AccountId}");
                        }

                        double[] endDestination = new double[2];
                        endDestination[0] = customerAddressDb.Lat;
                        endDestination[1] = customerAddressDb.Lng;
                        Task.Delay(350);
                        var estimateDelivery = await mapService!.GetEstimateDeliveryResponse(startDestination, endDestination);
                        if (estimateDelivery.IsSuccess && estimateDelivery.Result is EstimatedDeliveryTimeDto.Response response)
                        {
                            order.TotalDistance = response.Elements.FirstOrDefault().Distance.Text;
                            order.TotalDuration = response.Elements.FirstOrDefault().Duration.Text;
                        }
                    }
                }
                result.Result = new PagedResult<OrderWithFirstDetailResponse>
                {
                    Items = mappedData,
                    TotalPages = data.TotalPages,
                };
            }
            catch (Exception ex)
            {
                return BuildAppActionResultError(new AppActionResult(), ex.Message);
            }
            return result;
        }


        public async Task<AppActionResult> UpdateOrderStatus(Guid orderId, OrderStatus status)
        {
            var result = new AppActionResult();
            try
            {
                var orderDb = await _repository.GetByExpression(p => p.OrderId == orderId);
                if (orderDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy đơn hàng với id {orderId}");
                }

                orderDb.StatusId = status;
                await _repository.Update(orderDb);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BuildAppActionResultError(new AppActionResult(), ex.Message);
            }
            return result;

        }

        private async Task<OrderResponse> AssignEstimatedTimeToOrder(OrderResponse order)
        {
            try
            {
                var customerInfoRepository = Resolve<IGenericRepository<CustomerInfoAddress>>();
                var customerAddressDb = await customerInfoRepository!.GetByExpression(p => p.CustomerInfoAddressId == order.AddressId);
                if (customerAddressDb == null)
                {
                    return order;
                }

                var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var mapService = Resolve<IMapService>();
                var startLatConfig = await configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.RESTAURANT_LATITUDE);
                var startLngConfig = await configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.RESTAURANT_LNG);

                var startLat = Double.Parse(startLatConfig.CurrentValue);
                var startLng = Double.Parse(startLngConfig.CurrentValue);

                double[] startDestination = new double[2];
                startDestination[0] = startLat;
                startDestination[1] = startLng;


                double[] endDestination = new double[2];
                endDestination[0] = customerAddressDb.Lat;
                endDestination[1] = customerAddressDb.Lng;

                var estimateDelivery = await mapService!.GetEstimateDeliveryResponse(startDestination, endDestination);
                if (estimateDelivery.IsSuccess && estimateDelivery.Result is EstimatedDeliveryTimeDto.Response response)
                {
                    order.TotalDistance = response.Elements.FirstOrDefault().Distance.Text;
                    order.TotalDuration = response.Elements.FirstOrDefault().Duration.Text;
                }
            }
            catch (Exception ex)
            {
            }
            return order;
        }

        public async Task<AppActionResult> UpdateOrderDetailStatusForce(List<Guid> orderDetailIds, OrderDetailStatus status)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(p => orderDetailIds.Contains(p.OrderDetailId), 0, 0, null, false, null);
                if (orderDetailDb.Items.Count != orderDetailIds.Count)
                {
                    return BuildAppActionResultError(result, $"Tồn tại id gọi món hông nằm trong hệ thống");
                }

                if (orderDetailDb.Items.All(o => o.OrderDetailStatusId == OrderDetailStatus.Reserved || o.OrderDetailStatusId == OrderDetailStatus.Cancelled))
                {
                    return BuildAppActionResultError(result, $"Các chi tiết đơn hàng không thể cập nhật trạng thái vì đều không ở trạn thái chờ hay đang xừ lí");
                }

                orderDetailDb.Items.ForEach(p => p.OrderDetailStatusId = OrderDetailStatus.ReadyToServe);
                await orderDetailRepository.UpdateRange(orderDetailDb.Items);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        private async Task<bool> ChangeOrderDetailStatusAfterPayment(List<OrderDetail> orderDetails)
        {
            try
            {
                var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                orderDetails.ForEach(o => o.OrderDetailStatusId = OrderDetailStatus.Unchecked);
                var orderDetailIds = orderDetails.Where(or => or.ComboId.HasValue).Select(or => or.OrderDetailId).ToList();
                var comboOrderDetailDb = await comboOrderDetailRepository.GetAllDataByExpression(o => orderDetailIds.Contains(o.OrderDetailId.Value),
                                                                                                                  0, 0, null, false, null);
                comboOrderDetailDb.Items.ForEach(c => c.StatusId = DishComboDetailStatus.Unchecked);
                await comboOrderDetailRepository.UpdateRange(comboOrderDetailDb.Items);
                await _detailRepository.UpdateRange(orderDetails);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<AppActionResult> GetOrderWithFilter(ReservationTableRequest request)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                List<Order> orderDb = null;
                int totalPage = 0;
                if (request.Type != OrderType.Delivery)
                {
                    var orderDiningTableDb = await _tableDetailRepository.GetAllDataByExpression(
                                                                                      o => (
                                                                                            o.Order.MealTime.Value.Date >= request.StartDate.Date
                                                                                            && o.Order.MealTime.Value.Date <= request.EndDate.Date
                                                                                            ||
                                                                                            o.Order.OrderDate.Date >= request.StartDate.Date
                                                                                            && o.Order.OrderDate.Date <= request.EndDate.Date
                                                                                            )
                                                                                            && (
                                                                                                request.Type == 0
                                                                                                || o.Order.OrderTypeId == request.Type
                                                                                            )
                                                                                            &&
                                                                                            (
                                                                                                !request.Status.HasValue
                                                                                                || (request.Status.HasValue && o.Order.StatusId == request.Status.Value)
                                                                                            )
                                                                                            &&
                                                                                            (
                                                                                                !request.TableId.HasValue
                                                                                                || (request.TableId.HasValue && o.TableId == request.TableId.Value)
                                                                                            )
                                                                                            &&
                                                                                            (
                                                                                                request.PhoneNumber!.Equals(o.Order.Account!.PhoneNumber)
                                                                                            )
                                                                                            ,
                                                                                            0, 0, null, false, null);

                    if (orderDiningTableDb.Items.Count == 0)
                    {
                        return result;
                    }

                    var orderIds = orderDiningTableDb.Items.DistinctBy(o => o.OrderId).Select(o => o.OrderId).ToList();
                    var orderDiningDb = await _repository.GetAllDataByExpression(o => orderIds.Contains(o.OrderId), request.pageNumber, request.pageSize, null, false, o => o.Status,
                                                                                                                                        o => o.OrderType,
                                                                                                                                        o => o.Account);
                    if (orderDiningDb.Items!.Count > 0)
                    {
                        orderDb = orderDiningDb.Items;
                        totalPage = orderDiningDb.TotalPages;
                    }

                }
                else
                {
                    var orderDeliveryDb = (await _repository.GetAllDataByExpression(o => o.OrderDate.Date >= request.StartDate.Date
                                                                                            && o.OrderDate.Date <= request.EndDate.Date
                                                                                            && o.OrderTypeId == request.Type
                                                                                            &&
                                                                                            (
                                                                                                !request.Status.HasValue
                                                                                                || (request.Status.HasValue && o.StatusId == request.Status.Value)
                                                                                            )
                                                                                            ,
                                                                                            request.pageNumber, request.pageSize, null, false,
                                                                                            o => o.Status!,
                                                                                            o => o.OrderType!,
                                                                                            o => o.Account!));
                    if (orderDeliveryDb.Items!.Count > 0)
                    {
                        orderDb = orderDeliveryDb.Items;
                        totalPage = orderDeliveryDb.TotalPages;
                    }
                }


                List<ReservationTableItemResponse> data = new List<ReservationTableItemResponse>();
                if (orderDb != null && orderDb.Count > 0)
                {
                    if (orderDb.FirstOrDefault()!.OrderTypeId != OrderType.Delivery)
                    {
                        orderDb = orderDb.OrderByDescending(o => o.MealTime).ToList();
                    }
                    else
                    {
                        orderDb = orderDb.OrderByDescending(o => o.OrderDate).ToList();
                    }

                    foreach (var item in orderDb)
                    {
                        if (item == null)
                        {
                            continue;
                        }
                        var reservation = await GetReservationDetailByOrder(item);
                        if (reservation == null)
                        {
                            result.Messages.Add($"Xảy ra lỗi khi truy vấn đặt bàn có id {item.OrderId}");
                        }
                        data.Add(reservation);
                    }
                }
                result.Result = new PagedResult<ReservationTableItemResponse>
                {
                    Items = data,
                    TotalPages = totalPage
                };
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        private async Task<List<ReservationTableItemResponse>> GetReservationListDetailByOrder(List<Order> orders)
        {
            List<ReservationTableItemResponse> result = new List<ReservationTableItemResponse>();
            try
            {
                if (orders.Count == 0)
                {
                    return result;
                }

                foreach (var reservation in orders)
                {
                    var reservationResponse = await GetReservationDetailByOrder(reservation);
                    if (reservationResponse != null)
                    {
                        result.Add(reservationResponse);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return result;
        }
        private async Task<ReservationTableItemResponse> GetReservationDetailByOrder(Order order)
        {
            ReservationTableItemResponse result = null;
            try
            {
                result = _mapper.Map<ReservationTableItemResponse>(order);
                result.Tables = (await _tableDetailRepository.GetAllDataByExpression(t => t.OrderId == order.OrderId, 0, 0, t => t.Table.TableName, false, t => t.Table.TableSize, t => t.Table.Room)).Items;
            }
            catch (Exception ex)
            {
            }
            return result;
        }


        public async Task<AppActionResult> GetNumberOfOrderByStatus(OrderFilterRequest request)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                int numberOfOrder = 0;
                int totalPage = 0;
                if (request.Type != OrderType.Delivery)
                {
                    var orderDiningTableDb = await _tableDetailRepository.GetAllDataByExpression(
                                                                                      o => (
                                                                                            o.Order.MealTime.Value.Date >= request.StartDate.Date
                                                                                            && o.Order.MealTime.Value.Date <= request.EndDate.Date)
                                                                                            && o.Order.OrderTypeId == request.Type
                                                                                            &&
                                                                                            (
                                                                                                !request.Status.HasValue
                                                                                                || (request.Status.HasValue && o.Order.StatusId == request.Status.Value)
                                                                                            )
                                                                                            ,
                                                                                            0, 0, null, false, null);

                    if (orderDiningTableDb.Items.Count == 0)
                    {
                        return result;
                    }

                    var orderIds = orderDiningTableDb.Items.DistinctBy(o => o.OrderId).Select(o => o.OrderId).ToList();
                    var orderDiningDb = await _repository.GetAllDataByExpression(o => orderIds.Contains(o.OrderId), request.pageNumber, request.pageSize, null, false, o => o.Status,
                                                                                                                                        o => o.OrderType,
                                                                                                                                        o => o.Account);
                    if (orderDiningDb.Items!.Count > 0)
                    {
                        numberOfOrder = orderDiningDb.Items!.Count;
                    }

                }
                else
                {
                    var orderDeliveryDb = (await _repository.GetAllDataByExpression(o => o.OrderDate.Date >= request.StartDate.Date
                                                                                            && o.OrderDate.Date <= request.EndDate.Date
                                                                                            && o.OrderTypeId == request.Type
                                                                                            &&
                                                                                            (
                                                                                                !request.Status.HasValue
                                                                                                || (request.Status.HasValue && o.StatusId == request.Status.Value)
                                                                                            )
                                                                                            ,
                                                                                            request.pageNumber, request.pageSize, null, false,
                                                                                            o => o.Status!,
                                                                                            o => o.OrderType!,
                                                                                            o => o.Account!));
                    if (orderDeliveryDb.Items!.Count > 0)
                    {
                        numberOfOrder = orderDeliveryDb.Items.Count();
                    }
                }

                result.Result = numberOfOrder;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
        public async Task<AppActionResult> CancelDeliveringOrder(CancelDeliveringOrderRequest cancelDeliveringOrderRequest)
        {

            var result = new AppActionResult();

            var utility = Resolve<Utility>();
            var currentTime = utility!.GetCurrentDateTimeInTimeZone();
            var transactionRepository = Resolve<IGenericRepository<Transaction>>();
            var accountRepository = Resolve<IGenericRepository<Account>>();
            var loyalPointsHistoryRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
            var notificationService = Resolve<INotificationMessageService>();
            var orderAssignedRequestRepository = Resolve<IGenericRepository<OrderAssignedRequest>>();

            try
            {
                var orderDb = await _repository.GetAllDataByExpression(
                    p => p.OrderId == cancelDeliveringOrderRequest.OrderId,
                    0, 0, null, false,
                    p => p.Status!, p => p.OrderType!,
                    p => p.Account!, p => p.Shipper!,
                    p => p.LoyalPointsHistory!
                );

                if (orderDb!.Items!.Count <= 0)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy đơn hàng với id {cancelDeliveringOrderRequest.OrderId}");
                }

                var order = orderDb.Items!.FirstOrDefault();
                if (order == null)
                {
                    return BuildAppActionResultError(result, "Order is null.");
                }

                var accountDb = await accountRepository!.GetById(order.AccountId);
                if (accountDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy khách hàng với id {order.AccountId}");
                }

                if (cancelDeliveringOrderRequest.isCancelledByAdmin == true)
                {
                    order.CancelDeliveryReason = SD.CancelledReason.CANCELLED_BY_SYSTEM;

                    var newTransaction = new Transaction
                    {
                        Id = Guid.NewGuid(),
                        AccountId = order.AccountId,
                        PaymentMethodId = PaymentMethod.STORE_CREDIT,
                        TransactionTypeId = TransactionType.Refund,
                        Date = currentTime,
                        Amount = order.TotalAmount,
                        OrderId = order.OrderId,
                        TransationStatusId = TransationStatus.SUCCESSFUL
                    };

                    await transactionRepository!.Insert(newTransaction);

                    accountDb.LoyaltyPoint += (int)order.TotalAmount;
                    var newLoyaltyPointHistory = new LoyalPointsHistory
                    {
                        LoyalPointsHistoryId = Guid.NewGuid(),
                        OrderId = order.OrderId,
                        TransactionDate = currentTime,
                        PointChanged = (int)order.TotalAmount,
                        NewBalance = accountDb.LoyaltyPoint
                    };

                    await loyalPointsHistoryRepository!.Insert(newLoyaltyPointHistory);

                    string message = $"Đơn hàng ID {order.OrderId} đã bị hủy và chúng tôi đã hoàn tiền {order.TotalAmount} cho bạn.";
                    await notificationService!.SendNotificationToAccountAsync(accountDb.Id, message, true);
                }
                else
                {
                    var shipperDb = await accountRepository.GetById(cancelDeliveringOrderRequest.ShipperRequestId);
                    if (shipperDb == null)
                    {
                        return BuildAppActionResultError(result, $"Không tìm thấy tài khoản shipper với id {cancelDeliveringOrderRequest.ShipperRequestId}");
                    }

                    var orderAssignedRequest = new OrderAssignedRequest
                    {
                        OrderAssignedRequestId = Guid.NewGuid(),
                        RequestTime = currentTime,
                        OrderId = order.OrderId,
                        ShipperAssignedId = shipperDb.Id,
                        StatusId = OrderAssignedStatus.Pending,
                        Reasons = cancelDeliveringOrderRequest.CancelledReasons
                    };

                    await orderAssignedRequestRepository!.Insert(orderAssignedRequest);

                    order.CancelDeliveryReason = cancelDeliveringOrderRequest.CancelledReasons;

                    string message = $"Đơn hàng ID {order.OrderId} đã được yêu cầu hủy bởi Shipper {shipperDb.FirstName} {shipperDb.LastName}";
                    await notificationService!.SendNotificationToRoleAsync(SD.RoleName.ROLE_ADMIN, message);
                    await _hubServices.SendAsync(SD.SignalMessages.LOAD_RE_DELIVERING_REQUEST);
                }

                order.StatusId = OrderStatus.Cancelled;
                order.CancelledTime = currentTime;

                if (!BuildAppActionResultIsError(result))
                {
                    await accountRepository.Update(accountDb);
                    await _repository.Update(order);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }
        public async Task<AppActionResult> GetBestSellerDishesAndCombo(int topNumber, DateTime? startTime, DateTime? endTime)
        {
            var result = new AppActionResult();
            var dishRepository = Resolve<IGenericRepository<Dish>>();
            var comboRepository = Resolve<IGenericRepository<Combo>>();
            var utility = Resolve<Utility>();
            var currentTime = utility.GetCurrentDateTimeInTimeZone();
            try
            {
                var bestSellerDishDictionary = new Dictionary<Guid?, int>();
                var bestSellerComboDictionary = new Dictionary<Guid?, int>();
                BestSellerResponse bestSellerResponse = new BestSellerResponse();

                var listBestSellerDishes = new List<Dish>();
                var listBestSellerCombo = new List<Combo>();

                PagedResult<OrderDetail> orderDetailsList = new PagedResult<OrderDetail>();
                if (startTime.HasValue && endTime.HasValue && startTime.Value > endTime.Value)
                {
                    return result;
                }
                if (startTime.HasValue && endTime == null)
                {
                    orderDetailsList = await _detailRepository.GetAllDataByExpression(p => p.OrderTime.Date >= startTime.Value.Date && p.OrderTime.Date <= currentTime.Date, 0, 0, p => p.OrderTime, false, p => p.DishSizeDetail.Dish,
                                                                                                                                          p => p.Combo
                    );
                }
                else if (startTime.HasValue && endTime.HasValue)
                {
                    orderDetailsList = await _detailRepository.GetAllDataByExpression(p => p.OrderTime.Date >= startTime.Value.Date && p.OrderTime.Date <= endTime.Value.Date, 0, 0, p => p.OrderTime, false, p => p.DishSizeDetail.Dish,
                                                                                                                                          p => p.Combo
                    );
                }
                else
                {
                    orderDetailsList = await _detailRepository.GetAllDataByExpression(null, 0, 0, p => p.OrderTime, false, p => p.DishSizeDetail.Dish,
                                                                                                                                          p => p.Combo
                    );
                }
                var bestSellerDishesList = orderDetailsList.Items.Where(p => p.DishSizeDetailId.HasValue).GroupBy(p => p.DishSizeDetail.DishId).ToDictionary(p => p.Key, p => p.ToList());
                foreach (var bestSellerDish in bestSellerDishesList)
                {
                    bestSellerDishDictionary.Add(bestSellerDish.Key, bestSellerDish.Value.Sum(p => p.Quantity));

                }
                var bestSellerComboList = orderDetailsList.Items.Where(p => p.ComboId.HasValue).GroupBy(p => p.ComboId).ToDictionary(p => p.Key, p => p.ToList());
                foreach (var bestComboSeller in bestSellerComboList)
                {
                    bestSellerComboDictionary.Add(bestComboSeller.Key, bestComboSeller.Value.Sum(p => p.Quantity));
                }

                var topBestSellerDishes = bestSellerDishDictionary.OrderByDescending(p => p.Value).Select(p => p.Key).Take(topNumber);
                var topBestSellerCombo = bestSellerComboDictionary.OrderByDescending(p => p.Value).Select(p => p.Key).Take(topNumber);

                foreach (var dishItem in topBestSellerDishes)
                {
                    var dishDb = await dishRepository!.GetById(dishItem.Value);
                    if (dishDb != null)
                    {
                        listBestSellerDishes.Add(dishDb);
                    }
                }

                foreach (var comboItem in topBestSellerCombo)
                {
                    var comboDb = await comboRepository.GetById(comboItem.Value);
                    if (comboDb != null)
                    {
                        listBestSellerCombo.Add(comboDb);
                    }
                }

                bestSellerResponse.Dishes = listBestSellerDishes;
                bestSellerResponse.Combos = listBestSellerCombo;

                result.Result = bestSellerResponse;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        [Hangfire.Queue("cancel-order")]
        public async Task CancelOrder()
        {
            var emailService = Resolve<IEmailService>();
            var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
            var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
            var orderSessionRepository = Resolve<IGenericRepository<OrderSession>>();
            var utility = Resolve<Utility>();
            var configurationRepository = Resolve<IGenericRepository<Configuration>>();
            var dishManagementService = Resolve<IDishManagementService>();
            var timeToKeepUnpaidDeliveryOrderConfig = configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.TIME_TO_KEEP_UNPAID_DELIVERY_ORDER).Result;
            var timeToKeepReservationConfig = configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.TIME_TO_KEEP_RESERVATION).Result;
            if (timeToKeepUnpaidDeliveryOrderConfig == null)
            {
                return;
            }
            List<DishSizeDetail> updateDishSizeDetailList = new List<DishSizeDetail>();
            var currentTime = utility.GetCurrentDateTimeInTimeZone();
            double keepTime = double.Parse(timeToKeepUnpaidDeliveryOrderConfig.CurrentValue);
            double keepReservationTime = double.Parse(timeToKeepReservationConfig.CurrentValue);
            var unpaidDeliveryOrder = await _repository.GetAllDataByExpression(o => (o.OrderTypeId == OrderType.Delivery
                                                                                    && o.StatusId == OrderStatus.Pending
                                                                                    && o.OrderDate.AddMinutes(keepTime) < currentTime) //Does not pay total on time
                                                                                    ||
                                                                                    (o.OrderTypeId == OrderType.Reservation
                                                                                        &&
                                                                                        (
                                                                                            (o.StatusId == OrderStatus.DepositPaid
                                                                                            && o.MealTime.Value.AddMinutes(keepReservationTime) < currentTime) //Does not show up on time
                                                                                            ||
                                                                                            (o.StatusId == OrderStatus.TableAssigned
                                                                                            && o.ReservationDate.Value.AddMinutes(keepTime) < currentTime
                                                                                            && o.MealTime.Value.Date == o.ReservationDate.Value.Date) //Reservation for today, Depisit Unpaid
                                                                                        )
                                                                                    )
                                                                                    , 0, 0, null, false, o => o.Account);
            foreach (var order in unpaidDeliveryOrder.Items)
            {
                await UpdateCancelledOrderDishQuantity(order, updateDishSizeDetailList, currentTime);
            }
            await dishManagementService.UpdateComboAvailability();
            await dishManagementService.UpdateDishAvailability();
            await dishSizeDetailRepository.UpdateRange(updateDishSizeDetailList);
            await _repository.UpdateRange(unpaidDeliveryOrder.Items);
            await _unitOfWork.SaveChangesAsync();
            Task.CompletedTask.Wait();
        }

        [Hangfire.Queue("remind-order-reservation")]
        public async Task RemindOrderReservation()
        {
            var utility = Resolve<Utility>();
            var emailService = Resolve<IEmailService>();
            try
            {
                var currentTime = utility!.GetCurrentDateTimeInTimeZone();
                var reservationDb = await _repository.GetAllDataByExpression(
                    (p => p.ReservationDate.HasValue && p.ReservationDate.Value.AddMinutes(30) == currentTime &&
                    (p.StatusId == OrderStatus.Pending || p.StatusId == OrderStatus.TableAssigned || p.StatusId == OrderStatus.TemporarilyCompleted
                    )), 0, 0, null, false, p => p.Account
                    );

                if (reservationDb!.Items!.Count > 0 && reservationDb.Items != null)
                {
                    foreach (var reservation in reservationDb.Items)
                    {
                        var username = reservation!.Account!.FirstName + " " + reservation.Account.LastName;
                        emailService!.SendEmail(reservation.Account.Email, SD.SubjectMail.NOTIFY_RESERVATION, TemplateMappingHelper.GetTemplateNotification(username, reservation));
                    }
                }
            }
            catch (Exception ex)
            {
            }
            Task.CompletedTask.Wait();
        }

        [Hangfire.Queue("cancel-over-reservation")]
        public async Task CancelOverReservation()
        {
            var utility = Resolve<Utility>();
            var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
            var emailService = Resolve<IEmailService>();
            try
            {
                var configurationRepository = Resolve<IGenericRepository<Configuration>>();

                var currentTime = utility!.GetCurrentDateTimeInTimeZone();
                var pastReservationDb = await _repository.GetAllDataByExpression(
                    (p => p.MealTime.HasValue && p.MealTime.Value.AddMinutes(30) <= currentTime &&
                    p.StatusId == OrderStatus.DepositPaid), 0, 0, null, false, p => p.Account!, p => p.Status!
                    );

                if (pastReservationDb!.Items!.Count > 0)
                {
                    foreach (var reservation in pastReservationDb.Items)
                    {
                        var reservationDetailsDb = await orderDetailRepository!.GetAllDataByExpression(p => p.OrderId == reservation.OrderId, 0, 0, null, false, null);
                        if (reservationDetailsDb!.Items!.Count > 0 && reservationDetailsDb.Items != null)
                        {
                            foreach (var orderDetail in reservationDetailsDb.Items)
                            {
                                orderDetail.OrderDetailStatusId = OrderDetailStatus.Cancelled;
                            }

                            await orderDetailRepository.UpdateRange(reservationDetailsDb.Items);
                        }

                        reservation.CancelledTime = currentTime;
                        reservation.StatusId = OrderStatus.Cancelled;
                        //await ChangeOrderStatus(reservation.OrderId, false, null);
                        var username = reservation.Account.FirstName + " " + reservation.Account.LastName;
                        //emailService.SendEmail(reservation.Account.Email, SD.SubjectMail.NOTIFY_RESERVATION, TemplateMappingHelper.GetTemplateMailToCancelReservation(username, reservation));
                    }
                    await _repository.UpdateRange(pastReservationDb.Items);
                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, this);
            }
            Task.CompletedTask.Wait();
        }

        private async Task<AppActionResult> UpdateCancelledOrderDishQuantity(Order order, List<DishSizeDetail> updateDishSizeDetailList, DateTime currentTime, bool refillAllow = true)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                var orderSessionRepository = Resolve<IGenericRepository<OrderSession>>();
                var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                var emailService = Resolve<IEmailService>();
                var notificationMessageReposiory = Resolve<IGenericRepository<NotificationMessage>>();
                order.StatusId = OrderStatus.Cancelled;
                bool dishQuantityAccounted = order.MealTime.HasValue ? order.MealTime.Value == currentTime.Date : order.OrderDate.Date == currentTime.Date;
                var orderDetailDb = await _detailRepository.GetAllDataByExpression(o => o.OrderId == order.OrderId && o.OrderDetailStatusId != OrderDetailStatus.ReadyToServe && o.OrderDetailStatusId != OrderDetailStatus.Cancelled, 0, 0, null, false, o => o.DishSizeDetail);
                foreach (var orderDetail in orderDetailDb.Items.Where(o => o.DishSizeDetailId.HasValue))
                {
                    if (orderDetail.OrderDetailStatusId == OrderDetailStatus.Processing || orderDetail.OrderDetailStatusId == OrderDetailStatus.LateWarning)
                    {
                        orderDetail.OrderDetailStatusId = OrderDetailStatus.Cancelled;
                        continue;
                    }

                    if (refillAllow && dishQuantityAccounted)
                    {
                        var existedDishSizeDetail = updateDishSizeDetailList.FirstOrDefault(u => u.DishSizeDetailId == orderDetail.DishSizeDetailId);
                        if (existedDishSizeDetail != null)
                        {
                            existedDishSizeDetail.QuantityLeft += orderDetail.Quantity;
                            if (!existedDishSizeDetail.IsAvailable && existedDishSizeDetail.QuantityLeft > 0)
                            {
                                existedDishSizeDetail.IsAvailable = true;
                            }
                        }
                        else
                        {
                            var dishSizeDetailDb = await dishSizeDetailRepository.GetById(orderDetail.DishSizeDetailId);
                            dishSizeDetailDb.QuantityLeft += orderDetail.Quantity;
                            if (!dishSizeDetailDb.IsAvailable && dishSizeDetailDb.QuantityLeft > 0)
                            {
                                dishSizeDetailDb.IsAvailable = true;
                            }
                            updateDishSizeDetailList.Add(dishSizeDetailDb);
                        }
                        await dishSizeDetailRepository.UpdateRange(updateDishSizeDetailList);
                    }
                    orderDetail.OrderDetailStatusId = OrderDetailStatus.Cancelled;
                }

                foreach (var orderDetail in orderDetailDb.Items.Where(o => o.ComboId.HasValue))
                {
                    if (refillAllow && dishQuantityAccounted)
                    {
                        var comboOrderDetailDb = await comboOrderDetailRepository.GetAllDataByExpression(c => c.OrderDetailId == orderDetail.OrderDetailId, 0, 0, null, false, o => o.OrderDetail, o => o.DishCombo);
                        if (comboOrderDetailDb.Items.Any(c => c.StatusId != DishComboDetailStatus.ReadyToServe && c.StatusId != DishComboDetailStatus.Cancelled))
                        {
                            foreach (var comboOrderDetail in comboOrderDetailDb.Items.Where(c => c.StatusId == DishComboDetailStatus.Reserved || c.StatusId == DishComboDetailStatus.Unchecked))
                            {
                                var existedDishSizeDetail = updateDishSizeDetailList.FirstOrDefault(u => u.DishSizeDetailId == comboOrderDetail.DishCombo.DishSizeDetailId);
                                if (existedDishSizeDetail != null)
                                {
                                    existedDishSizeDetail.QuantityLeft += orderDetail.Quantity * comboOrderDetail.DishCombo.Quantity;
                                    if (!existedDishSizeDetail.IsAvailable && existedDishSizeDetail.QuantityLeft > 0)
                                    {
                                        existedDishSizeDetail.IsAvailable = true;
                                    }
                                }
                                else
                                {
                                    var dishSizeDetailDb = await dishSizeDetailRepository.GetById(comboOrderDetail.DishCombo.DishSizeDetailId);
                                    dishSizeDetailDb.QuantityLeft += orderDetail.Quantity * comboOrderDetail.DishCombo.Quantity;
                                    if (!dishSizeDetailDb.IsAvailable && dishSizeDetailDb.QuantityLeft > 0)
                                    {
                                        dishSizeDetailDb.IsAvailable = true;
                                    }
                                    updateDishSizeDetailList.Add(dishSizeDetailDb);
                                }
                            }
                            comboOrderDetailDb.Items.Where(c => c.StatusId != DishComboDetailStatus.ReadyToServe).ToList().ForEach(c => c.StatusId = DishComboDetailStatus.Cancelled);
                            await comboOrderDetailRepository.UpdateRange(comboOrderDetailDb.Items);
                        }
                    }
                    orderDetail.OrderDetailStatusId = OrderDetailStatus.Cancelled;
                }

                var orderSessionDb = await orderSessionRepository.GetAllDataByExpression(o => orderDetailDb.Items.Select(o => o.OrderSessionId).Contains(o.OrderSessionId), 0, 0, null, false, null);
                foreach (var session in orderSessionDb.Items)
                {
                    if (session.OrderSessionStatusId != OrderSessionStatus.Completed)
                    {
                        session.OrderSessionStatusId = OrderSessionStatus.Cancelled;
                        session.CancelTime = currentTime;
                    }
                }

                await orderSessionRepository.UpdateRange(orderSessionDb.Items);
                await _detailRepository.UpdateRange(orderDetailDb.Items);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAllOrderDetailByAccountId(string accountId, int feedbackStatus, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var accountDb = await accountRepository.GetById(accountId);
                if (accountDb == null)
                {
                    result = BuildAppActionResultError(result, $"Không tìm thấy tài khoản với id {accountId}");
                }
                var orderDetailDb = await _detailRepository.GetAllDataByExpression(o => !string.IsNullOrEmpty(o.Order.AccountId)
                                                                                        && o.Order.AccountId.Equals(accountId)
                                                                                        && (feedbackStatus == 1) == o.IsRated
                                                                                        && o.Order.StatusId == OrderStatus.Completed
                                                                                        , pageNumber, pageSize, o => o.Order.OrderTypeId == OrderType.Delivery ? o.Order.DeliveredTime : o.Order.MealTime, false,
                                                                                        o => o.DishSizeDetail.Dish.DishItemType,
                                                                                        o => o.DishSizeDetail.DishSize,
                                                                                        o => o.Combo.Category,
                                                                                        o => o.OrderDetailStatus);
                var data = await GetReservationDishes2(Guid.NewGuid(), orderDetailDb.Items);

                result.Result = new PagedResult<OrderDishDto>
                {
                    Items = data,
                    TotalPages = orderDetailDb.TotalPages
                };
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
        [Hangfire.Queue("cancel-reservation")]
        public async Task CancelReservation()
        {
            var emailService = Resolve<IEmailService>();
            var utility = Resolve<Utility>();
            var configurationRepository = Resolve<IGenericRepository<Configuration>>();
            var timeLastForReservationWithDishesConfig = configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.TIME_TO_RESERVATION_WITH_DISHES_LAST).Result;
            var timeLastForReservationConfig = configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.TIME_TO_RESERVATION_LAST).Result;
            var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
            var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
            var notificationMessageReposiory = Resolve<IGenericRepository<NotificationMessage>>();
            try
            {
                var currentTime = utility!.GetCurrentDateTimeInTimeZone();

                var orderReservationDb = await _repository.GetAllDataByExpression(p => p.OrderTypeId == OrderType.Reservation && p.StatusId == OrderStatus.TableAssigned, 0, 0, p => p.OrderDate, false, null);
                if (orderReservationDb.Items != null)
                {
                    foreach (var orderReservation in orderReservationDb.Items)
                    {
                        var orderReservationDetailDb = await orderDetailRepository!.GetAllDataByExpression(p => p.OrderId == orderReservation.OrderId, 0, 0, null, false, null);
                        if (orderReservationDb!.Items!.Count > 0 && orderReservationDb.Items != null)
                        {
                            if (orderReservation.ReservationDate!.Value.AddHours(int.Parse(timeLastForReservationWithDishesConfig!.CurrentValue)) <= currentTime)
                            {
                                var orderReservationDetailList = new List<OrderDetail>();
                                foreach (var orderReservationDetail in orderReservationDetailDb.Items!)
                                {
                                    orderReservationDetail.OrderDetailStatusId = OrderDetailStatus.Cancelled;
                                    orderReservationDetailList.Add(orderReservationDetail);
                                }
                                orderReservation.StatusId = OrderStatus.Cancelled;

                                await _repository.Update(orderReservation);
                                await orderDetailRepository.UpdateRange(orderReservationDetailList);
                            }
                        }
                        else
                        {
                            if (orderReservation.ReservationDate.Value.AddHours(int.Parse(timeLastForReservationConfig!.CurrentValue)) <= currentTime)
                            {
                                orderReservation.StatusId = OrderStatus.Cancelled;
                                await _repository.Update(orderReservation);
                            }
                        }

                        var tableDetailDb = await _tableDetailRepository.GetAllDataByExpression(p => p.OrderId == orderReservation.OrderId, 0, 0, null, false, p => p!.Table!, p => p.Table!.Room!, p => p.Table!.TableSize!);
                        var tableDetail = tableDetailDb!.Items!.FirstOrDefault();
                        if (!string.IsNullOrEmpty(orderReservation.Account?.Email))
                        {
                            var username = orderReservation.Account?.FirstName + " " + orderReservation.Account?.LastName;
                            emailService.SendEmail(orderReservation.Account?.Email, SD.SubjectMail.NOTIFY_RESERVATION, TemplateMappingHelper.GetTemplateMailToCancelReservation(username, orderReservation, tableDetail));
                            
                            var message = $"Đơn đặt bàn của bạn vào lúc {orderReservation.MealTime} đã bị hủy, để biết thêm thông tin chi tiết vui lòng kiểm tra hộp thư mail của bạn";
                            var notificationMessage = new NotificationMessage
                            {
                                NotificationId = Guid.NewGuid(),
                                AccountId = orderReservation.AccountId,
                                NotificationName = "Bạn có thông báo mới",
                                Messages = message,
                                NotifyTime = currentTime
                            };

                            await notificationMessageReposiory!.Insert(notificationMessage);
                        }

                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }
            Task.CompletedTask.Wait();
        }
    }
}
