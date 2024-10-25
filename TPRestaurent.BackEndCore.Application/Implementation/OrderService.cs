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
                    var dishRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                    var comboRepository = Resolve<IGenericRepository<Combo>>();
                    var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                    var orderSessionRepository = Resolve<IGenericRepository<OrderSession>>();
                    var orderDb = await _repository.GetById(dto.OrderId);
                    var utility = Resolve<Utility>();
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

                    foreach (var o in dto.OrderDetailsDtos)
                    {
                        var orderDetail = new OrderDetail
                        {
                            OrderDetailId = Guid.NewGuid(),
                            OrderId = dto.OrderId,
                        };

                        if (o.Combo != null)
                        {
                            var combo = await comboRepository!.GetById(o.Combo.ComboId);
                            orderDetail.Price = combo.Price;
                            orderDetail.ComboId = combo.ComboId;

                            foreach (var dishComboId in o.Combo.DishComboIds)
                            {
                                comboOrderDetails.Add(new ComboOrderDetail
                                {
                                    ComboOrderDetailId = Guid.NewGuid(),
                                    DishComboId = dishComboId,
                                    OrderDetailId = orderDetail.OrderDetailId
                                });
                            }

                            orderDetail.OrderDetailStatusId = OrderDetailStatus.Unchecked;
                            orderDetail.OrderTime = utility!.GetCurrentDateTimeInTimeZone();
                            orderDetail.Quantity = o.Quantity;
                            orderDetail.Note = o.Note;
                        }
                        else
                        {
                            var dish = await dishRepository!.GetById(o.DishSizeDetailId!);
                            orderDetail.Price = dish.Price;
                            orderDetail.DishSizeDetailId = o.DishSizeDetailId;
                            orderDetail.Quantity = o.Quantity;
                            orderDetail.OrderDetailStatusId = OrderDetailStatus.Unchecked;
                            orderDetail.OrderTime = utility!.GetCurrentDateTimeInTimeZone();
                            orderDetail.Note = o.Note;
                        }

                        orderDb.TotalAmount += orderDetail.Price * orderDetail.Quantity;
                        orderDetails.Add(orderDetail);
                    }

                    await _repository.Update(orderDb);
                    await orderSessionRepository.Insert(orderSession);
                    await orderDetailRepository.InsertRange(orderDetails);
                    await comboOrderDetailRepository!.InsertRange(comboOrderDetails);
                    await _unitOfWork.SaveChangesAsync();
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

        public async Task<AppActionResult> ChangeOrderStatus(Guid orderId, bool IsSuccessful)
        {
            var result = new AppActionResult();
            try
            {
                var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                var orderDb = await _repository.GetById(orderId);
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
                            orderDb.StatusId = IsSuccessful ? OrderStatus.DepositPaid : OrderStatus.Cancelled;
                        }
                        else if (orderDb.StatusId == OrderStatus.DepositPaid)
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
                            }
                            orderDb.StatusId = IsSuccessful ? OrderStatus.TemporarilyCompleted : OrderStatus.Cancelled;
                        }
                        else if (orderDb.StatusId == OrderStatus.TemporarilyCompleted || orderDb.StatusId == OrderStatus.Processing)
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
                            else
                            {
                                orderDb.StatusId = OrderStatus.Cancelled;
                            }
                        }
                        else
                        {
                            result = BuildAppActionResultError(result, $"Đơn hàng không thể cập nhật những trạng thái khác");
                        }
                    }
                    else if (orderDb.OrderTypeId == OrderType.Delivery)
                    {
                        var utility = Resolve<Utility>();
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
                            }
                        }
                        else if (orderDb.StatusId == OrderStatus.Processing)
                        {
                            orderDb.StatusId = IsSuccessful ? OrderStatus.ReadyForDelivery : OrderStatus.Cancelled;

                        }
                        else if (orderDb.StatusId == OrderStatus.ReadyForDelivery)
                        {
                            orderDb.StatusId = IsSuccessful ? OrderStatus.AssignedToShipper : OrderStatus.Cancelled;
                            if (IsSuccessful)
                            {
                                orderDb.AssignedTime = utility.GetCurrentDateTimeInTimeZone();
                            }
                        }
                        else if (orderDb.StatusId == OrderStatus.AssignedToShipper)
                        {
                            orderDb.StatusId = IsSuccessful ? OrderStatus.Delivering : OrderStatus.Cancelled;
                            if (IsSuccessful)
                            {
                                orderDb.StartDeliveringTime = utility.GetCurrentDateTimeInTimeZone();
                            }
                        }
                        else if (orderDb.StatusId == OrderStatus.Delivering)
                        {
                            orderDb.StatusId = IsSuccessful ? OrderStatus.Completed : OrderStatus.Cancelled;
                            if (IsSuccessful)
                            {
                                orderDb.DeliveredTime = utility.GetCurrentDateTimeInTimeZone();
                            }
                        }
                        else
                        {
                            result = BuildAppActionResultError(result, $"Đơn hàng không thể cập nhật những trạng thái khác");
                        }
                    }
                    else
                    {
                        if (orderDb.StatusId == OrderStatus.TemporarilyCompleted)
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
                            else
                            {
                                orderDb.StatusId = OrderStatus.Cancelled;
                            }
                        }
                        else
                        {
                            result = BuildAppActionResultError(result, $"Đơn hàng không thể cập nhật những trạng thái khác");
                        }
                    }

                    await _repository.Update(orderDb);
                    await _unitOfWork.SaveChangesAsync();
                    if (orderDb.StatusId == OrderStatus.Processing || orderDb.StatusId == OrderStatus.ReadyForDelivery)
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

        public async Task<AppActionResult> CreateOrder(OrderRequestDto orderRequestDto)
        {
            var result = new AppActionResult();
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var utility = Resolve<Utility>();
                    var accountRepository = Resolve<IGenericRepository<Account>>();
                    var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                    var loyalPointsHistoryRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
                    var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                    var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                    var comboRepository = Resolve<IGenericRepository<Combo>>();
                    var couponProgramRepository = Resolve<IGenericRepository<CouponProgram>>();
                    var fireBaseService = Resolve<IFirebaseService>();
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
                    var mapService = Resolve<IMapService>();
                    var createdOrderId = new Guid();
                    var dishSizeDetail = new DishSizeDetail();
                    var combo = new Combo();
                    var orderWithPayment = new OrderWithPaymentResponse();

                    var order = new Order()
                    {
                        OrderId = Guid.NewGuid(),
                        OrderTypeId = orderRequestDto.OrderType,
                        Note = orderRequestDto.Note,
                    };

                    Account accountDb = null;
                    if (orderRequestDto.CustomerId.HasValue)
                    {
                        accountDb = await accountRepository.GetByExpression(c => c.Id == orderRequestDto.CustomerId.Value.ToString(), null);
                        if (accountDb == null)
                        {
                            return BuildAppActionResultError(result, $"Xảy ra lỗi");
                        }

                        order.AccountId = orderRequestDto.CustomerId.ToString();
                    }

                    if ((orderRequestDto.OrderType == OrderType.Reservation && orderRequestDto.ReservationOrder == null)
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

                    List<OrderDetail> orderDetails = new List<OrderDetail>();
                    List<ComboOrderDetail> comboOrderDetails = new List<ComboOrderDetail>();
                    double money = 0;
                    var orderSessionDb = await orderSessionRepository!.GetAllDataByExpression(null, 0, 0, null, false, null);
                    var latestOrderSession = orderSessionDb.Items!.Count() + 1;
                    var orderSession = new OrderSession()
                    {
                        OrderSessionId = Guid.NewGuid(),
                        OrderSessionTime = utility!.GetCurrentDateTimeInTimeZone(),
                        OrderSessionStatusId = orderRequestDto.OrderType == OrderType.Reservation ? OrderSessionStatus.PreOrder : OrderSessionStatus.Confirmed,
                        OrderSessionNumber = latestOrderSession
                    };
                    if (orderRequestDto.OrderDetailsDtos != null && orderRequestDto.OrderDetailsDtos.Count > 0)
                    {
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
                                dishSizeDetail = await dishSizeDetailRepository.GetById(item.DishSizeDetailId.Value);

                                if (dishSizeDetail == null)
                                {
                                    return BuildAppActionResultError(result, $"Không tìm thấy món ăn có size với id {item.DishSizeDetailId.Value}");
                                }

                                orderDetail.DishSizeDetailId = item.DishSizeDetailId.Value;
                                orderDetail.Price = dishSizeDetail.Price;
                            }
                            else if (item.Combo != null)
                            {
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
                                    comboOrderDetails.Add(new ComboOrderDetail
                                    {
                                        ComboOrderDetailId = Guid.NewGuid(),
                                        OrderDetailId = orderDetail.OrderDetailId,
                                        DishComboId = dishComboId
                                    });
                                }
                            }
                            else
                            {
                                return BuildAppActionResultError(result, $"Không tìm thấy thông tin món ăn");
                            }

                            orderDetails.Add(orderDetail);
                        }
                        money = orderDetails.Sum(o => o.Quantity * o.Price);

                    }

                    if (orderDetails.Count > 0)
                    {
                        order.TotalAmount = money;
                        await orderDetailRepository.InsertRange(orderDetails);
                        if (comboOrderDetails.Count > 0)
                        {
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
                        order.Deposit = orderRequestDto.ReservationOrder.Deposit;

                        var suggestTableDto = new SuggestTableDto
                        {
                            StartTime = orderRequestDto.ReservationOrder.MealTime,
                            EndTime = orderRequestDto.ReservationOrder.EndTime,
                            IsPrivate = orderRequestDto.ReservationOrder.IsPrivate,
                            NumOfPeople = orderRequestDto.ReservationOrder.NumberOfPeople,
                        };

                        var suggestedTables = await GetSuitableTable(suggestTableDto);
                        if (suggestedTables == null || suggestedTables.Count == 0)
                        {
                            result = BuildAppActionResultError(result, $"Không có bàn trống cho {orderRequestDto.ReservationOrder.NumberOfPeople} người " +
                                                                       $"vào lúc {orderRequestDto.ReservationOrder.MealTime.Hour}h{orderRequestDto.ReservationOrder.MealTime.Minute}p " +
                                                                       $"ngày {orderRequestDto.ReservationOrder.MealTime.Date}");
                            return result;
                        }
                        //Add busniness rule for reservation time(if needed)
                        List<TableDetail> reservationTableDetails = new List<TableDetail>();

                        //foreach(var suggestedTable in suggestedTables)
                        //{
                        //    reservationTableDetails.Add(new TableDetail
                        //    {
                        //        TableDetailId = Guid.NewGuid(),
                        //        OrderId = order.OrderId,
                        //        TableId = suggestedTable.TableId
                        //    });
                        //}

                        reservationTableDetails.Add(new TableDetail
                        {
                            TableDetailId = Guid.NewGuid(),
                            OrderId = order.OrderId,
                            TableId = suggestedTables[0].TableId,
                            StartTime = orderRequestDto.ReservationOrder.MealTime
                        });

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
                        order.TotalAmount = money;
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
                        }
                        else
                        {
                            return BuildAppActionResultError(result, "Không có thông tin bàn");
                        }
                    }
                    else
                    {
                        order.OrderTypeId = OrderType.Delivery;
                        order.StatusId = OrderStatus.Pending;
                        order.TotalAmount = money;
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

                        var customerAddressDb = customerInfoAddressListDb.Items.FirstOrDefault();

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

                        orderDetails.ForEach(o => o.OrderDetailStatusId = OrderDetailStatus.Reserved);
                        order.TotalAmount = money;

                        await orderDetailRepository.InsertRange(orderDetails);



                        if (!BuildAppActionResultIsError(result))
                        {
                            await accountRepository.Update(accountDb);
                        }
                    }
                    if (!BuildAppActionResultIsError(result))
                    {
                        await orderSessionRepository.Insert(orderSession);
                        await _repository.Insert(order);
                        await _unitOfWork.SaveChangesAsync();

                        if (order.OrderTypeId == OrderType.MealWithoutReservation)
                        {

                            await _hubServices.SendAsync(SD.SignalMessages.LOAD_ORDER_SESIONS);
                            await _hubServices.SendAsync(SD.SignalMessages.LOAD_GROUPED_DISHES);
                            await _hubServices.SendAsync(SD.SignalMessages.LOAD_ORDER_DETAIL_STATUS);
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
                                return BuildAppActionResultError(result, "Tạo thanh toán thất bại");
                            }
                            if (linkPaymentDb.Result != null && !string.IsNullOrEmpty(linkPaymentDb.Result.ToString()))
                            {
                                orderWithPayment.PaymentLink = linkPaymentDb!.Result!.ToString();
                            }

                            var chefRole = await roleRepository!.GetByExpression(p => p.Name == SD.RoleName.ROLE_CHEF);
                            var userRole = await userRoleRepository!.GetAllDataByExpression(p => p.RoleId == chefRole.Id.ToString(), 0, 0, null, false, null);
                            var tokenList = new List<string>();
                            foreach (var user in userRole.Items)
                            {
                                var tokenDb = await tokenRepostiory!.GetAllDataByExpression(p => p.AccountId == user.UserId, 0, 0, null, false, p => p.Account);
                                foreach (var token in tokenDb.Items)
                                {
                                    if (token.DeviceToken != null)
                                    {
                                        tokenList.Add(token.DeviceToken);
                                    }
                                }

                            }

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

                                var notificationList = new List<NotificationMessage>();
                                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                                foreach (var user in userRole.Items)
                                {
                                    var notification = new NotificationMessage
                                    {
                                        NotificationId = Guid.NewGuid(),
                                        NotificationName = "Nhà hàng có thông báo mới",
                                        Messages = messageBody.ToString(),
                                        NotifyTime = currentTime,
                                        AccountId = user.UserId,
                                    };
                                    notificationList.Add(notification);
                                }

                                await notificationMessageRepository!.InsertRange(notificationList);
                                if (tokenList.Count() > 0)
                                {
                                    await fireBaseService!.SendMulticastAsync(tokenList, "Nhà hàng có một thông báo mới", messageBody.ToString(), result);
                                }
                            }


                            await _unitOfWork.SaveChangesAsync();
                        }
                        scope.Complete();
                    }
                    result.Result = orderWithPayment;
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
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
                    (o.OrderTypeId == orderType)), pageNumber, pageSize, o => o.OrderDate, false,
                     p => p.Status!,
                     p => p.Account!,
                     p => p.LoyalPointsHistory!,
                     p => p.OrderType!
                    );
                }
                else
                {
                    data = await _repository.GetAllDataByExpression(o => o.Account.Id.Equals(customerId), pageNumber, pageSize, o => o.OrderDate, false,
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
                    Items = mappedData,
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
                    var orderDb = await _repository.GetById(orderRequestDto.OrderId);
                    if (orderDb == null)
                    {
                        return BuildAppActionResultError(result, $"Không tìm thấy đơn với id {orderRequestDto.OrderId}");
                    }

                    if (orderDb.OrderTypeId == OrderType.Delivery)
                    {
                        return BuildAppActionResultError(result, $"Đơn hàng giao tận nơi đã có giao dịch");
                    }

                    var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(o => o.OrderId == orderRequestDto.OrderId && o.OrderDetailStatusId != OrderDetailStatus.Cancelled, 0, 0, p => p.OrderTime, false, null);
                    double money = 0;
                    orderDetailDb.Items.ForEach(o => money += o.Price * o.Quantity);

                    money -= ((orderDb.Deposit.HasValue && orderDb.Deposit.Value > 0) ? orderDb.Deposit.Value : 0);

                    if (!string.IsNullOrEmpty(orderDb.AccountId))
                    {
                        Account accountDb = await accountRepository.GetByExpression(c => c.Id == orderDb.AccountId.ToString(), null);
                        if (accountDb == null)
                        {
                            return BuildAppActionResultError(result, $"Không tìm thấy thông tin khách hàng. Đặt hàng thất bại");
                        }

                        if (string.IsNullOrEmpty(accountDb.Address))
                        {
                            return BuildAppActionResultError(result, $"Địa chỉ của bạn không tồn tại. Vui lòng cập nhập địa chỉ");
                        }

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
                            OrderId = orderDb.OrderId,
                            PointChanged = (int)money / 100,
                            NewBalance = accountDb.LoyaltyPoint + (int)money / 100
                        };

                        await loyalPointsHistoryRepository!.Insert(newLoyalPointHistory);

                        accountDb.LoyaltyPoint = newLoyalPointHistory.NewBalance;
                        await customerInfoRepository.Update(accountDb);
                    }

                    if (!BuildAppActionResultIsError(result))
                    {
                        orderDb.TotalAmount = money;
                        await _repository.Update(orderDb);
                        await _unitOfWork.SaveChangesAsync();

                        var paymentRequest = new PaymentRequestDto
                        {
                            OrderId = orderDb.OrderId,
                            PaymentMethod = orderRequestDto.PaymentMethod,
                        };
                        var linkPaymentDb = await transactionService!.CreatePayment(paymentRequest);
                        if (!linkPaymentDb.IsSuccess)
                        {
                            return BuildAppActionResultError(result, "Tạo thanh toán thất bại");
                        }
                        var orderWithPayment = new OrderWithPaymentResponse();
                        orderWithPayment.Order = orderDb;
                        orderWithPayment.PaymentLink = linkPaymentDb.Result.ToString();
                        result.Result = orderWithPayment;
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
                    var orderListDb = await _repository.GetAllDataByExpression(o => o.StatusId == status && o.OrderTypeId == orderType, pageNumber, pageSize, o => o.OrderDate, false, p => p.Account!,
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
                    var orderListDb = await _repository.GetAllDataByExpression(o => o.StatusId == status, pageNumber, pageSize, o => o.OrderDate, false, p => p.Account!,
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
                    var orderListDb = await _repository.GetAllDataByExpression(o => o.OrderTypeId == orderType, pageNumber, pageSize, o => o.OrderDate, false, p => p.Account!,
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
                        _repository.GetAllDataByExpression(p => p.Account!.PhoneNumber == phoneNumber, pageNumber, pageSize, p => p.OrderDate, false,
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
                    data = await _repository.GetAllDataByExpression(o => o.StatusId == status && o.OrderTypeId == orderType, pageNumber, pageSize, o => o.OrderDate, false, p => p.Account!,
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
                    data = await _repository.GetAllDataByExpression(o => o.StatusId == status, pageNumber, pageSize, o => o.OrderDate, false, p => p.Account!,
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
                    data = await _repository.GetAllDataByExpression(o => o.OrderTypeId == orderType, pageNumber, pageSize, o => o.OrderDate, false, p => p.Account!,
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
                    data = await _repository.GetAllDataByExpression(null, pageNumber, pageSize, o => o.OrderDate, false, p => p.Account!,
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
                    Items = mappedData,
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

        public async Task<AppActionResult> SuggestTable(SuggestTableDto dto)
        {
            AppActionResult result = new AppActionResult();
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
                        result.Result = suitableTables.Count == 0 ? new List<Table>() : suitableTables;
                    }
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }

            return result;
        }
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

        [Hangfire.Queue("cancel-over-reservation")]
        public async Task CancelOverReservation()
        {
            var utility = Resolve<Utility>();
            var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
            try
            {
                var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var configWithReservationDish = await configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.TIME_TO_RESERVATION_WITH_DISHES_LAST);
                var configWithoutReservationDish = await configurationRepository.GetByExpression(p => p.Name == SD.DefaultValue.TIME_TO_RESERVATION_LAST);

                var currentTime = utility!.GetCurrentDateTimeInTimeZone();
                var pastReservationDb = await _repository.GetAllDataByExpression(
                    (p => p.ReservationDate.HasValue &&
                    (p.StatusId == OrderStatus.Pending || p.StatusId == OrderStatus.TableAssigned
                    )), 0, 0, null, false, null
                    );

                if (pastReservationDb!.Items!.Count > 0 && pastReservationDb.Items != null)
                {
                    foreach (var reservation in pastReservationDb.Items)
                    {
                        var reservationDetailsDb = await orderDetailRepository!.GetAllDataByExpression(p => p.OrderId == reservation.OrderId, 0, 0, null, false, null);
                        if (reservationDetailsDb!.Items!.Count > 0 && reservationDetailsDb.Items != null)
                        {
                            var pastReservationTime = reservation!.ReservationDate!.Value.AddHours(double.Parse(configWithReservationDish!.CurrentValue));
                            if (pastReservationTime < currentTime)
                            {
                                reservation.StatusId = OrderStatus.Cancelled;
                                foreach (var orderDetail in reservationDetailsDb.Items)
                                {
                                    orderDetail.OrderDetailStatusId = OrderDetailStatus.Cancelled;
                                    await orderDetailRepository.Update(orderDetail);
                                }
                            }
                        }
                        else
                        {
                            var pastReservationTime = reservation!.ReservationDate!.Value.AddHours(double.Parse(configWithoutReservationDish!.CurrentValue));
                            if (pastReservationTime < currentTime)
                            {
                                reservation.StatusId = OrderStatus.Cancelled;
                            }
                        }

                        await _unitOfWork.SaveChangesAsync();
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, this);
            }
            Task.CompletedTask.Wait();
        }

        [Hangfire.Queue("update-order-status-before-meal-time")]
        public async Task UpdateOrderStatusBeforeMealTime()
        {
            var utility = Resolve<Utility>();
            try
            {
                var currentTime = utility!.GetCurrentDateTimeInTimeZone();
                var orderListDb = await _repository.GetAllDataByExpression(p => p.MealTime!.Value.AddHours(-1) <= currentTime && p.StatusId == OrderStatus.DepositPaid, 0, 0, null, false, null);
                if (orderListDb!.Items!.Count > 0 && orderListDb.Items != null)
                {
                    foreach (var order in orderListDb.Items)
                    {
                        order.StatusId = OrderStatus.TemporarilyCompleted;
                        await _repository.Update(order);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, this);
            }
            Task.CompletedTask.Wait();
        }

        [Hangfire.Queue("update-order-detail-status-before-dining")]
        public async Task UpdateOrderDetailStatusBeforeDining()
        {
            var utility = Resolve<Utility>();
            var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
            try
            {
                var currentTime = utility!.GetCurrentDateTimeInTimeZone();
                var orderListDb = await _repository.GetAllDataByExpression(p => p.MealTime!.Value.AddMinutes(-30) <= currentTime && p.StatusId == OrderStatus.TemporarilyCompleted, 0, 0, null, false, null);
                if (orderListDb!.Items!.Count > 0 && orderListDb.Items != null)
                {
                    var orderIds = orderListDb.Items.Select(o => o.OrderId).ToList();
                    var orderDetailsDb = await orderDetailRepository!.GetAllDataByExpression(p => orderIds.Contains(p.OrderId) && p.OrderDetailStatusId == OrderDetailStatus.Unchecked, 0, 0, null, false, null);
                    if (orderDetailsDb!.Items!.Count > 0 && orderListDb.Items != null)
                    {
                        foreach (var orderDetail in orderDetailsDb.Items)
                        {
                            orderDetail.OrderDetailStatusId = OrderDetailStatus.Unchecked;
                        }
                        await orderDetailRepository.UpdateRange(orderDetailsDb.Items);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, this);
            }
            Task.CompletedTask.Wait();
        }
        public async Task<AppActionResult> GetUpdateCartComboDto(string cartComboJson)
        {
            AppActionResult result = new AppActionResult();
            try
            {

                ComboChoice cart = JsonConvert.DeserializeObject<ComboChoice>(cartComboJson);
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
                foreach (var cartItem in cart.items)
                {
                    comboDb = await comboRepository.GetById(Guid.Parse(cartItem.comboId));
                    if (comboDb == null)
                    {
                        cart.items.Remove(cartItem);
                        if (cart.items.Count == 0) break;
                        continue;
                    }

                    if (comboDb.EndDate <= currentTime)
                    {
                        cart.items.Remove(cartItem);
                        if (cart.items.Count == 0) break;
                        continue;
                    }

                    cartItem.price = comboDb.Price;
                    foreach (var dishDetailList in cartItem.selectedDishes.Values)
                    {
                        foreach (var dishDetail in dishDetailList)
                        {
                            dishComboDb = await dishComboRepository.GetById(Guid.Parse(dishDetail.dishComboId));
                            if (dishComboDb == null)
                            {
                                dishDetailList.Remove(dishDetail);
                                if (dishDetailList.Count == 0) break;
                                continue;
                            }

                            dishSizeDetailDb = await dishSizeDetailRepository.GetById(Guid.Parse(dishDetail.dishSizeDetail.dishSizeDetailId));
                            if (dishSizeDetailDb == null)
                            {
                                cart.items.Remove(cartItem);
                                if (dishDetailList.Count == 0) break;
                                continue;
                            }

                            if (!dishSizeDetailDb.IsAvailable)
                            {
                                dishDetailList.Remove(dishDetail);
                                if (dishDetailList.Count == 0) break;
                                continue;
                            }

                            dishDetail.dishSizeDetail.price = dishSizeDetailDb.Price;
                            dishDetail.dishSizeDetail.discount = dishSizeDetailDb.Discount;

                            dishDb = await dishRepository.GetById(Guid.Parse(dishDetail.dishSizeDetail.dish.dishId));
                            if (dishDb == null)
                            {
                                cart.items.Remove(cartItem);
                                if (dishDetailList.Count == 0) break;
                                continue;
                            }

                            if (!dishDb.isAvailable)
                            {
                                dishDetailList.Remove(dishDetail);
                                if (dishDetailList.Count == 0) break;
                                continue;
                            }

                            dishDetail.dishSizeDetail.dish.name = dishDb.Name;
                            dishDetail.dishSizeDetail.dish.description = dishDb.Description;
                            dishDetail.dishSizeDetail.dish.image = dishDb.Image;
                        }
                    }
                    total += cartItem.price * cartItem.quantity;
                }
                cart.total = total;
                string unProcessedJson = JsonConvert.SerializeObject(cart);
                string formattedJson = unProcessedJson.Replace("\\", "");
                result.Result = formattedJson;
                //for each check dish dishSizeDetailId Price Isavailable, dishId Hin2h anh3 is Available
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
        public async Task<AppActionResult> GetUpdateCartDishDto(string cartDishJson)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                List<CartDishItem> cart = JsonConvert.DeserializeObject<List<CartDishItem>>(cartDishJson);

                var dishComboRepository = Resolve<IGenericRepository<DishCombo>>();
                var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                var dishRepository = Resolve<IGenericRepository<Dish>>();
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                Combo comboDb = null;
                DishCombo dishComboDb = null;
                DishSizeDetail dishSizeDetailDb = null;
                Dish dishDb = null;
                foreach (var dish in cart)
                {
                    dishDb = await dishRepository.GetById(Guid.Parse(dish.dish.dishId));
                    if (dishDb == null)
                    {
                        cart.Remove(dish);
                        if (cart.Count == 0) break;
                        continue;
                    }

                    if (!dishDb.isAvailable)
                    {
                        cart.Remove(dish);
                        if (cart.Count == 0) break;
                        continue;
                    }

                    dish.dish.name = dishDb.Name;
                    dish.dish.description = dishDb.Description;
                    dish.dish.image = dishDb.Image;

                    dishSizeDetailDb = await dishSizeDetailRepository.GetById(Guid.Parse(dish.size.dishSizeDetailId));
                    if (dishSizeDetailDb == null)
                    {
                        cart.Remove(dish);
                        if (cart.Count == 0) break;
                        continue;
                    }

                    if (!dishSizeDetailDb.IsAvailable)
                    {
                        cart.Remove(dish);
                        if (cart.Count == 0) break;
                        continue;
                    }

                    dish.size.price = dishSizeDetailDb.Price;
                    dish.size.discount = dishSizeDetailDb.Discount;

                    dish.size.dish.name = dishDb.Name;
                    dish.size.dish.description = dishDb.Description;
                    dish.size.dish.image = dishDb.Image;

                    string unProcessedJson = JsonConvert.SerializeObject(cart);
                    string formattedJson = unProcessedJson.Replace("\\\"", "\"");
                    result.Result = formattedJson;
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
        public async Task<AppActionResult> UpdateOrderDetailStatus(List<Guid> orderDetailIds, bool isSuccessful)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                var orderSessionRepository = Resolve<IGenericRepository<OrderSession>>();
                var orderSessionService = Resolve<IOrderSessionService>();
                var groupedDishCraftService = Resolve<IGroupedDishCraftService>();
                var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(p => orderDetailIds.Contains(p.OrderDetailId) && !(p.OrderDetailStatusId == OrderDetailStatus.Reserved || p.OrderDetailStatusId == OrderDetailStatus.ReadyToServe || p.OrderDetailStatusId == OrderDetailStatus.Cancelled), 0, 0, null, false, null);
                if (orderDetailDb.Items.Count != orderDetailIds.Count)
                {
                    return BuildAppActionResultError(result, $"Tồn tại id gọi món không nằm trong hệ thống hoặc không thể ập nhập trạng thái được");
                }

                var utility = Resolve<Utility>();
                var time = utility.GetCurrentDateTimeInTimeZone();
                bool orderSessionUpdated = false;

                foreach (var orderDetail in orderDetailDb.Items)
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
                            orderDetailDb.Items.ForEach(p => p.OrderDetailStatusId = OrderDetailStatus.ReadyToServe);
                        }
                        else
                        {
                            return BuildAppActionResultError(result, $"Chi tiết đơn hàng đang ở trạng thái dang xử lí, không thể huỷ");
                        }
                    }
                }

                await orderDetailRepository.UpdateRange(orderDetailDb.Items);
                var orderSessionIds = orderDetailDb.Items.DistinctBy(o => o.OrderSessionId).Select(o => o.OrderSessionId).ToList();
                var orderSessionDb = await orderSessionRepository.GetAllDataByExpression(o => orderDetailIds.Contains(o.OrderSessionId), 0, 0, null, false, null);
                var orderSessionSet = new HashSet<Guid>();
                foreach (var session in orderSessionDb.Items)
                {
                    if (orderSessionSet.Contains(session.OrderSessionId))
                    {
                        continue;
                    }

                    if (session.OrderSessionStatusId == OrderSessionStatus.Confirmed)
                    {
                        await orderSessionService.UpdateOrderSessionStatus(session.OrderSessionId, OrderSessionStatus.Processing, false);
                        orderSessionUpdated = true;
                    }
                    else if (orderDetailDb.Items.Where(o => o.OrderSessionId == session.OrderSessionId).All(o => o.OrderDetailStatusId == OrderDetailStatus.Cancelled))
                    {
                        await orderSessionService.UpdateOrderSessionStatus(session.OrderSessionId, OrderSessionStatus.Cancelled, false);
                        orderSessionUpdated = true;
                    }
                    else if (orderDetailDb.Items.Where(o => o.OrderSessionId == session.OrderSessionId).All(o => o.OrderDetailStatusId == OrderDetailStatus.ReadyToServe))
                    {
                        await orderSessionService.UpdateOrderSessionStatus(session.OrderSessionId, OrderSessionStatus.Completed, false);
                        orderSessionUpdated = true;
                    }

                    orderSessionSet.Add(session.OrderSessionId);
                }


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

                result.Result = orderDetailDb;

            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
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
                                                                && r.Order.OrderTypeId == OrderType.Reservation, 0, 0, r => r.Order!.ReservationDate, true, null);
                if (nearReservationDb.Items.Count > 0)
                {
                    result = await GetAllOrderDetail(nearReservationDb.Items[0].OrderId);
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

        private async Task<List<Common.DTO.Response.OrderDishDto>> GetReservationDishes2(Guid reservationId)
        {
            var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
            var reservationDishDb = await orderDetailRepository.GetAllDataByExpression(
                o => o.OrderId == reservationId,
                0, 0, null, false,
                o => o.DishSizeDetail.Dish,
                o => o.DishSizeDetail.DishSize,
                o => o.Combo,
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
                        ComboDish = comboDishDto,
                        Quantity = r.Quantity,
                        StatusId = r.OrderDetailStatusId,
                        Status = r.OrderDetailStatus,
                        OrderTime = r.OrderTime,
                        Note = r.Note
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
                        Note = r.Note
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
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var accountRepository = Resolve<IGenericRepository<Account>>();
                try
                {
                    var orderList = new List<Order>();
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
                        await ChangeOrderStatus(orderDb.OrderId, true);
                        orderList.Add(orderDb);
                    }
                    if (!BuildAppActionResultIsError(result))
                    {
                        await _repository.UpdateRange(orderList);
                        await _unitOfWork.SaveChangesAsync();
                        scope.Complete();
                        await _hubServices.SendAsync(SD.SignalMessages.LOAD_ORDER);
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
                    var orderDb = await _repository.GetByExpression(p => p.OrderId == confirmedOrderRequest.OrderId);
                    if (orderDb == null)
                    {
                        return BuildAppActionResultError(result, $"Không tìm thấy đơn hàng với id {confirmedOrderRequest.OrderId}");
                    }
                    var pathName = SD.FirebasePathName.ORDER_PREFIX + $"{confirmedOrderRequest.OrderId}{Guid.NewGuid()}.jpg";
                    var upload = await firebaseService!.UploadFileToFirebase(confirmedOrderRequest.Image, pathName);

                    if (!upload.IsSuccess)
                    {
                        return BuildAppActionResultError(result, "Upload hình ảnh không thành công");
                    }

                    orderDb.ValidatingImg = upload.Result!.ToString();
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

        [Hangfire.Queue("cancel-reservation")]
        public async Task CancelReservation()
        {
            var emailService = Resolve<IEmailService>();
            var utility = Resolve<Utility>();
            var configurationRepository = Resolve<IGenericRepository<Configuration>>();
            var timeLastForReservationWithDishesConfig = configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.TIME_TO_RESERVATION_WITH_DISHES_LAST).Result;
            var timeLastForReservationConfig = configurationRepository!.GetByExpression(p => p.Name == SD.DefaultValue.TIME_TO_RESERVATION_LAST).Result;
            var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
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

                        var username = orderReservation.Account.FirstName + "" + orderReservation.Account.LastName;
                        emailService.SendEmail(orderReservation.Account.Email, SD.SubjectMail.CANCEL_RESERVATION, TemplateMappingHelper.GetTemplateMailToCancelReservation(username, orderReservation));
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }
            Task.CompletedTask.Wait();
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
                    return BuildAppActionResultError(result, $"Các chi tiết đơn hàng không thể cập nhật trạng thái v2i đều không ở trạn thái chờ hay đang xừ lí");
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
                orderDetails.ForEach(o => o.OrderDetailStatusId = OrderDetailStatus.Unchecked);
                await _detailRepository.UpdateRange(orderDetails);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public async Task<AppActionResult> GetReservationTable(ReservationTableRequest request)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var reservationDb = await _tableDetailRepository.GetAllDataByExpression(o => o.Order.OrderTypeId == OrderType.Reservation
                                                                                  && (o.Order.StatusId != OrderStatus.Completed && o.Order.StatusId != OrderStatus.Cancelled)
                                                                                  && (o.Order.MealTime >= request.StartTime && o.Order.MealTime <= request.EndTime)
                                                                                  && (!request.Status.HasValue
                                                                                        || request.Status.HasValue && o.Order.StatusId == request.Status.Value)
                                                                                  && (!request.TableId.HasValue
                                                                                        || request.TableId.HasValue && o.TableId == request.TableId.Value),
                                                                                  0, 0, null, false, 
                                                                                  o => o.Order.Status,
                                                                                  o => o.Order.OrderType,
                                                                                  o => o.Order.Account);

                if(reservationDb.Items.Count == 0)
                {
                    return result;
                }
                var groupedReservation = reservationDb.Items.Select(o => o.Order).GroupBy(r => r.MealTime.Value.Date).ToDictionary(r => r.Key, r => r.OrderBy(r => r.MealTime).ToList());

                List<ReservationTableResponse> data = new List<ReservationTableResponse>();
                foreach(var reservation in groupedReservation.OrderBy(g => g.Key))
                {
                    var reservationByDate = new ReservationTableResponse
                    {
                        Date = reservation.Key
                    };

                    var reservationList = await GetReservationListDetailByOrder(reservation.Value);
                    if(reservationList == null)
                    {
                        result.Messages.Add($"Xảy ra lỗi khi truy vấn đặt bàn ngày {reservation.Key}");
                    } else
                    {
                        reservationByDate.Reservations = reservationList.OrderBy(o => o.MealTime).ToList();
                    }
                    data.Add(reservationByDate);
                }
                result.Result = data;
            }
            catch(Exception ex)
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
                if(orders.Count == 0)
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
            catch(Exception ex)
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

        public Task RemindOrderReservation()
        {
            throw new NotImplementedException();
        }
    }
}
