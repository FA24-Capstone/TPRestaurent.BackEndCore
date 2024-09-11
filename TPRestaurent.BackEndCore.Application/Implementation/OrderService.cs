using AutoMapper;
using Castle.Core.Logging;
using Humanizer;
using MailKit.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;
using Utility = TPRestaurent.BackEndCore.Common.Utils.Utility;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class OrderService : GenericBackendService, IOrderService
    {
        private readonly IGenericRepository<Order> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private BackEndLogger _logger;

        public OrderService(IGenericRepository<Order> repository, IUnitOfWork unitOfWork, IMapper mapper, IServiceProvider service, BackEndLogger logger) : base(service)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
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
                    var orderDb = await _repository.GetById(dto.OrderId);
                    var utility = Resolve<Utility>();
                    if (orderDb == null)
                    {
                        result = BuildAppActionResultError(result, $"Không tìm thấy đơn hàng với id {dto.OrderId}");
                        return result;
                    }

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
                        }
                        else
                        {
                            var dish = await dishRepository!.GetById(o.DishSizeDetailId!);
                            orderDetail.Price = dish.Price;
                            orderDetail.DishSizeDetailId = o.DishSizeDetailId;
                            orderDetail.Quantity = o.Quantity;
                            orderDetail.OrderDetailStatusId = OrderDetailStatus.Unchecked;
                            orderDetail.OrderTime = utility!.GetCurrentDateTimeInTimeZone();
                        }

                        orderDb.TotalAmount += orderDetail.Price * orderDetail.Quantity;
                        orderDetails.Add(orderDetail);
                    }

                    await _repository.Update(orderDb);
                    await orderDetailRepository.InsertRange(orderDetails);
                    await comboOrderDetailRepository!.InsertRange(comboOrderDetails);
                    await _unitOfWork.SaveChangesAsync();
                    scope.Complete();
                    //AddOrderMessageToChef
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
                return result;
            }
        }
        //public async Task<AppActionResult> ChangeOrderStatus(Guid orderId, bool IsSuccessful)
        //{
        //    var result = new AppActionResult();
        //    try
        //    {
        //        var orderDb = await _repository.GetById(orderId);
        //        if (orderDb == null)
        //        {
        //            result = BuildAppActionResultError(result, $"Đơn hàng với id {orderId} không tồn tại");
        //        }
        //        else
        //        {
        //            if (orderDb.IsDelivering == true)
        //            {
        //                if (orderDb.StatusId == OrderStatus.Pending)
        //                {
        //                    if (IsSuccessful)
        //                    {
        //                        orderDb.StatusId = OrderStatus.Processing;
        //                    }
        //                    else
        //                    {
        //                        orderDb.StatusId = OrderStatus.Cancelled;
        //                    }
        //                }
        //                else if (orderDb.StatusId == OrderStatus.Processing)
        //                {
        //                    if (IsSuccessful)
        //                    {
        //                        orderDb.StatusId = OrderStatus.Delivering;
        //                    }
        //                    else
        //                    {
        //                        orderDb.StatusId = OrderStatus.Cancelled;
        //                    }
        //                }
        //                else if (orderDb.StatusId == OrderStatus.Delivering)
        //                {
        //                    if (IsSuccessful)
        //                    {
        //                        orderDb.StatusId = OrderStatus.Completed;
        //                        var transactionService = Resolve<ITransactionService>();
        //                        var transactionRepository = Resolve<IGenericRepository<Domain.Models.Transaction>>();
        //                        var reservationTransactionDb = await transactionRepository.GetByExpression(t => t.OrderId == orderId && t.TransationStatusId == TransationStatus.PENDING, null);
        //                        if (reservationTransactionDb == null)
        //                        {
        //                            result = BuildAppActionResultError(result, $"Không tìm thấy giao dịch cho đơn hàng với id {orderId}");
        //                            return result;
        //                        }
        //            await _repository.Update(orderDb);
        //            await orderDetailRepository.InsertRange(orderDetails);
        //            await comboOrderDetailRepository!.InsertRange(comboOrderDetails);
        //            await _unitOfWork.SaveChangesAsync();
        //            scope.Complete();
        //            //AddOrderMessageToChef
        //        }
        //        catch (Exception ex)
        //        {
        //            result = BuildAppActionResultError(result, ex.Message);
        //        }
        //        return result;
        //    }
        //}
        public async Task<AppActionResult> ChangeOrderStatus(Guid orderId, bool IsSuccessful)
        {
            var result = new AppActionResult();
            try
            {
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
                            orderDb.StatusId = IsSuccessful ? OrderStatus.Dining : OrderStatus.Cancelled;
                        }
                        else if (orderDb.StatusId == OrderStatus.Dining)
                        {
                            if (IsSuccessful)
                            {
                                orderDb.StatusId = OrderStatus.Completed;
                                var transactionService = Resolve<ITransactionService>();
                                var transactionRepository = Resolve<IGenericRepository<Domain.Models.Transaction>>();
                                var reservationTransactionDb = await transactionRepository.GetByExpression(t => t.OrderId == orderId && t.TransationStatusId == TransationStatus.PENDING, null);
                                if (reservationTransactionDb == null)
                                {
                                    result = BuildAppActionResultError(result, $"Không tìm thấy giao dịch cho đơn hàng với id {orderId}");
                                    return result;
                                }

                                var transactionUpdatedSuccessFully = await transactionService.UpdateTransactionStatus(reservationTransactionDb.Id, TransationStatus.SUCCESSFUL);
                                if (!transactionUpdatedSuccessFully.IsSuccess)
                                {
                                    result = BuildAppActionResultError(result, $"Cập nhật trạng thái giao dịch {reservationTransactionDb.Id} cho đơn hàng {orderId} thất bại. Vui lòng cập nhật lại sau");
                                }
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
                        if (orderDb.StatusId == OrderStatus.Pending)
                        {
                            orderDb.StatusId = IsSuccessful ? OrderStatus.Processing : OrderStatus.Cancelled;
                        }
                        else if (orderDb.StatusId == OrderStatus.Processing)
                        {
                            orderDb.StatusId = IsSuccessful ? OrderStatus.Delivering : OrderStatus.Cancelled;
                        }
                        else if (orderDb.StatusId == OrderStatus.Delivering)
                        {
                            orderDb.StatusId = IsSuccessful ? OrderStatus.Completed : OrderStatus.Cancelled;
                            if (orderDb.StatusId == OrderStatus.Completed)
                            {
                                var transactionService = Resolve<ITransactionService>();
                                var transactionRepository = Resolve<IGenericRepository<Domain.Models.Transaction>>();
                                var reservationTransactionDb = await transactionRepository.GetByExpression(t => t.OrderId == orderId && t.TransationStatusId == TransationStatus.PENDING, null);
                                if (reservationTransactionDb == null)
                                {
                                    result = BuildAppActionResultError(result, $"Không tìm thấy giao dịch cho đơn hàng với id {orderId}");
                                    return result;
                                }

                                var transactionUpdatedSuccessFully = await transactionService.UpdateTransactionStatus(reservationTransactionDb.Id, TransationStatus.SUCCESSFUL);
                                if (!transactionUpdatedSuccessFully.IsSuccess)
                                {
                                    result = BuildAppActionResultError(result, $"Cập nhật trạng thái giao dịch {reservationTransactionDb.Id} cho đơn hàng {orderId} thất bại. Vui lòng cập nhật lại sau");
                                }
                            }
                        }
                        else
                        {
                            result = BuildAppActionResultError(result, $"Đơn hàng không thể cập nhật những trạng thái khác");
                        }
                    }
                    else
                    {
                        if (orderDb.StatusId == OrderStatus.Dining)
                        {
                            if (IsSuccessful)
                            {
                                orderDb.StatusId = OrderStatus.Completed;
                                var transactionService = Resolve<ITransactionService>();
                                var transactionRepository = Resolve<IGenericRepository<Domain.Models.Transaction>>();
                                var reservationTransactionDb = await transactionRepository.GetByExpression(t => t.OrderId == orderId && t.TransationStatusId == TransationStatus.PENDING, null);
                                if (reservationTransactionDb == null)
                                {
                                    result = BuildAppActionResultError(result, $"Không tìm thấy giao dịch cho đơn hàng với id {orderId}");
                                    return result;
                                }

                                var transactionUpdatedSuccessFully = await transactionService.UpdateTransactionStatus(reservationTransactionDb.Id, TransationStatus.SUCCESSFUL);
                                if (!transactionUpdatedSuccessFully.IsSuccess)
                                {
                                    result = BuildAppActionResultError(result, $"Cập nhật trạng thái giao dịch {reservationTransactionDb.Id} cho đơn hàng {orderId} thất bại. Vui lòng cập nhật lại sau");
                                }
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
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> CreateOrder(OrderRequestDto orderRequestDto, HttpContext httpContext)
        {
            var result = new AppActionResult();
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var utility = Resolve<Utility>();
                    var accountRepository = Resolve<IGenericRepository<Account>>();
                    var customerInfoRepository = Resolve<IGenericRepository<CustomerInfo>>();
                    var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                    var loyalPointsHistoryRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
                    var customerSavedCouponRepository = Resolve<IGenericRepository<CustomerSavedCoupon>>();
                    var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                    var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                    var comboRepository = Resolve<IGenericRepository<Combo>>();
                    var couponRepository = Resolve<IGenericRepository<Coupon>>();
                    var tableRepository = Resolve<IGenericRepository<Table>>();
                    var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                    var transcationService = Resolve<ITransactionService>();
                    var createdOrderId = new Guid();
                    var dishSizeDetail = new DishSizeDetail();
                    var combo = new Combo();
                    var orderWithPayment = new OrderWithPaymentResponse();

                    var order = new Order()
                    {
                        OrderId = Guid.NewGuid(),
                        CustomerId = orderRequestDto.CustomerId,
                        OrderTypeId = orderRequestDto.OrderType,
                        Note = orderRequestDto.Note,
                        PaymentMethodId = PaymentMethod.VNPAY
                    };

                    PagedResult<CustomerInfo> customerInfoListDb = new PagedResult<CustomerInfo>();
                    if (orderRequestDto.CustomerId.HasValue)
                    {
                        customerInfoListDb = await customerInfoRepository.GetAllDataByExpression(c => c.CustomerId == orderRequestDto.CustomerId.Value, 0, 0, null, false, null);
                        if (customerInfoListDb.Items.Count != 1)
                        {
                            return BuildAppActionResultError(result, $"Xảy ra lỗi");
                        }
                    }

                    List<OrderDetail> orderDetails = new List<OrderDetail>();
                    List<ComboOrderDetail> comboOrderDetails = new List<ComboOrderDetail>();
                    double money = 0;
                    if (orderRequestDto.OrderDetailsDtos != null && orderRequestDto.OrderDetailsDtos.Count > 0)
                    {
                        foreach (var item in orderRequestDto.OrderDetailsDtos)
                        {
                            var orderDetail = new OrderDetail()
                            {
                                OrderDetailId = Guid.NewGuid(),
                                Quantity = item.Quantity,
                                Note = item.Note,
                                OrderId = order.OrderId
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
                                        OrderDetailId = order.OrderId,
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
                        order.PaymentMethodId = orderRequestDto.ReservationOrder.PaymentMethod;

                        var suggestTableDto = new SuggestTableDto
                        {
                            StartTime = orderRequestDto.ReservationOrder.MealTime,
                            EndTime = orderRequestDto.ReservationOrder.EndTime,
                            IsPrivate = orderRequestDto.ReservationOrder.IsPrivate,
                            NumOfPeople = orderRequestDto.ReservationOrder.NumberOfPeople,
                        };

                        var suitableTable = await GetSuitableTable(suggestTableDto);
                        if (suitableTable == null)
                        {
                            result = BuildAppActionResultError(result, $"Không có bàn trống cho {orderRequestDto.ReservationOrder.NumberOfPeople} người " +
                                                                       $"vào lúc {orderRequestDto.ReservationOrder.MealTime.Hour}h{orderRequestDto.ReservationOrder.MealTime.Minute}p " +
                                                                       $"ngày {orderRequestDto.ReservationOrder.MealTime.Date}");
                            return result;
                        }
                        //Add busniness rule for reservation time(if needed)
                        var reservationTableDetail = new TableDetail
                        {
                            TableDetailId = Guid.NewGuid(),
                            OrderId = order.OrderId,
                            TableId = suitableTable.TableId
                        };

                        await tableDetailRepository.Insert(reservationTableDetail);

                        if (orderDetails.Count > 0)
                        {
                            orderDetails.ForEach(o => o.OrderDetailStatusId = OrderDetailStatus.Pending);
                        }
                        orderWithPayment.Order = order;
                    }
                    else if (orderRequestDto.OrderType == OrderType.MealWithoutReservation)
                    {
                        order.OrderTypeId = OrderType.MealWithoutReservation;
                        order.StatusId = OrderStatus.Dining;
                        order.MealTime = utility.GetCurrentDateTimeInTimeZone();
                        order.NumOfPeople = orderRequestDto.MealWithoutReservation.NumberOfPeople;
                        order.TotalAmount = money;
                        if (orderDetails.Count > 0)
                        {
                            orderDetails.ForEach(o => o.OrderDetailStatusId = OrderDetailStatus.Unchecked);
                        }
                        else
                        {
                            result = BuildAppActionResultError(result, "Bàn không thực hiện gọi món.");
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
                        }
                        else
                        {
                            result = BuildAppActionResultError(result, "Không có thông tin bàn");
                        }
                    }
                    else
                    {
                        order.OrderTypeId = OrderType.Delivery;
                        order.StatusId = OrderStatus.Pending;
                        order.TotalAmount = money;
                        order.OrderDate = utility.GetCurrentDateInTimeZone();

                        if (customerInfoListDb.Items.Count == 0)
                        {
                            return BuildAppActionResultError(result, $"Không tìm thấy thông tin khách hàng. Đặt hàng thất bại");
                        }

                        if (customerInfoListDb.Items.Count > 1)
                        {
                            return BuildAppActionResultError(result, $"Xảy ra lỗi khi tìm thông tin khách hàng. Đặt hàng thất bại");
                        }

                        var address = customerInfoListDb.Items.Select(c => c.Address).FirstOrDefault();
                        var accountId = customerInfoListDb.Items.Select(c => c.AccountId).FirstOrDefault();
                        if (orderRequestDto.CustomerId.HasValue)
                        {
                            if (string.IsNullOrEmpty(address))
                            {
                                return BuildAppActionResultError(result, $"Địa chỉ của bạn không tồn tại. Vui lòng cập nhập địa chỉ");
                            }

                            var customerSavedCouponDb = await customerSavedCouponRepository!.GetAllDataByExpression(p => !string.IsNullOrEmpty(accountId) && p.AccountId == accountId, 0, 0, null, false, p => p.Coupon!);
                            if (customerSavedCouponDb.Items!.Count > 0 && customerSavedCouponDb.Items != null
                                && orderRequestDto.DeliveryOrder != null && orderRequestDto.DeliveryOrder.CouponIds.Count > 0)
                            {
                                foreach (var couponId in orderRequestDto.DeliveryOrder.CouponIds)
                                {
                                    if (money <= 0)
                                    {
                                        break;
                                    }

                                    var customerSavedCoupon = customerSavedCouponDb.Items.FirstOrDefault(c => c.CouponId == couponId);
                                    if (customerSavedCoupon != null && customerSavedCoupon.IsUsedOrExpired == false)
                                    {
                                        var coupon = await couponRepository!.GetById(customerSavedCoupon.CouponId);
                                        if (coupon != null && coupon.ExpiryDate > utility.GetCurrentDateTimeInTimeZone())
                                        {
                                            double discountMoney = money * (coupon.DiscountPercent * 0.01);
                                            money -= discountMoney;
                                        }
                                        else if (coupon.ExpiryDate < utility.GetCurrentDateTimeInTimeZone())
                                        {
                                            return BuildAppActionResultError(result, $"Mã giảm giá của bạn đã hết hạn");
                                        }

                                        money = Math.Max(0, money);

                                        // Update the coupon usage
                                        customerSavedCoupon.IsUsedOrExpired = true;
                                        customerSavedCoupon.OrderId = order.OrderId;
                                        await customerSavedCouponRepository!.Update(customerSavedCoupon);

                                    }
                                }
                            }

                            if (!string.IsNullOrEmpty(accountId))
                            {
                                var accountDb = await accountRepository!.GetById(accountId);
                                if (accountDb != null && orderRequestDto.DeliveryOrder.LoyalPointToUse.HasValue && orderRequestDto.DeliveryOrder.LoyalPointToUse > 0)
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

                                if (accountDb != null)
                                {
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
                                }
                            }
                        }
                    }

                    await _repository.Insert(order);
                    orderWithPayment.Order = order;

                    if (orderDetails.Count > 0)
                    {
                        orderDetails.ForEach(o => o.OrderDetailStatusId = OrderDetailStatus.Pending);
                    }
                    await orderDetailRepository.InsertRange(orderDetails);

                    if (!BuildAppActionResultIsError(result))
                    {
                        await _unitOfWork.SaveChangesAsync();
                        if (orderRequestDto.DeliveryOrder != null && orderRequestDto.DeliveryOrder.PaymentMethod != PaymentMethod.Cash ||
                            orderRequestDto.ReservationOrder != null && orderRequestDto.ReservationOrder.PaymentMethod != PaymentMethod.Cash)
                        {

                            var paymentRequest = new PaymentRequestDto
                            {
                                OrderId = order.OrderId,
                                PaymentMethod = orderRequestDto.DeliveryOrder != null ? orderRequestDto.DeliveryOrder.PaymentMethod : orderRequestDto.ReservationOrder.PaymentMethod,
                            };
                            var linkPaymentDb = await transcationService!.CreatePayment(paymentRequest, httpContext);
                            if (!linkPaymentDb.IsSuccess)
                            {
                                return BuildAppActionResultError(result, "Tạo thanh toán thất bại");
                            }
                            orderWithPayment.PaymentLink = linkPaymentDb.Result.ToString();
                        }
                        result.Result = orderWithPayment;
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

        public async Task<AppActionResult> GetAllOrderByAccountId(string accountId, OrderStatus? status, OrderType? orderType, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                if (status.HasValue)
                {
                    result.Result = await _repository.GetAllDataByExpression((o => o.CustomerInfo.AccountId.Equals(accountId) && (
                    o.StatusId == status && o.OrderTypeId == orderType) ||
                    (o.StatusId == status) ||
                    (o.OrderTypeId == orderType)), pageNumber, pageSize, o => o.OrderDate, false,
                     p => p.Status!,
                     p => p.CustomerInfo!.Account!,
                     p => p.PaymentMethod!,
                     p => p.LoyalPointsHistory!,
                     p => p.OrderType!
                    );
                }
                else
                {
                    result.Result = await _repository.GetAllDataByExpression(o => o.CustomerInfo.AccountId.Equals(accountId), pageNumber, pageSize, o => o.OrderDate, false, p => p.PaymentMethod!,
                        p => p.Status!,
                        p => p.CustomerInfo!.Account!,
                        p => p.PaymentMethod!,
                        p => p.LoyalPointsHistory!,
                        p => p.OrderType!
                        );
                }
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
                var orderDb = await _repository.GetAllDataByExpression(p => p.OrderId == orderId, 0, 0, null, false, p => p.CustomerInfo!.Account!,
                        p => p.Status!,
                        p => p.CustomerInfo!.Account!,
                        p => p.PaymentMethod!,
                        p => p.LoyalPointsHistory!,
                        p => p.OrderType!
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

        //public async Task<AppActionResult> CompleteOrder(OrderPaymentRequestDto orderRequestDto)
        //{
        //    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        //    {
        //        AppActionResult result = new AppActionResult();
        //        try
        //        {
        //            var orderDb = await _repository.GetById(orderRequestDto.OrderId);
        //            if (orderDb == null)
        //            {
        //                return BuildAppActionResultError(result, $"Không tìm thấy đơn với id {orderRequestDto.OrderId}");
        //            }
        //            if (orderDb.CustomerId.HasValue)
        //            {
        //                var couponRepository = Resolve<IGenericRepository<Coupon>>();
        //                var customerSavedCouponRepository = Resolve<IGenericRepository<CustomerSavedCoupon>>();
        //                var customerInfoRepository = Resolve<IGenericRepository<CustomerInfo>>();
        //                var loyalPointsHistoryRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
        //                var customerInfoDb = await customerInfoRepository!.GetByExpression(p => p.CustomerId == orderDb.CustomerId, p => p.Account!);
        //                var utility = Resolve<Utility>();
        //                var customerSavedCouponDb = await customerSavedCouponRepository!.GetAllDataByExpression(p => !string.IsNullOrEmpty(customerInfoDb.AccountId) && p.AccountId == customerInfoDb.AccountId, 0, 0, null, false, p => p.Coupon!);
        //                if (customerSavedCouponDb.Items!.Count < 0 && customerSavedCouponDb.Items != null)
        //                {
        //                    if (orderRequestDto.CouponId.HasValue)
        //                    {
        //                        var customerSavedCoupon = customerSavedCouponDb.Items.FirstOrDefault(c => c.CouponId == orderRequestDto.CouponId);
        //                        if (customerSavedCoupon != null && customerSavedCoupon.IsUsedOrExpired == false)
        //                        {
        //                            var coupon = await couponRepository!.GetById(customerSavedCoupon.CouponId);
        //                            if (coupon != null && coupon.ExpiryDate > utility!.GetCurrentDateTimeInTimeZone())
        //                            {
        //                                double discountAmount = orderDb.TotalAmount * (coupon.DiscountPercent * 0.01);
        //                                orderDb.TotalAmount -= discountAmount;
        //                            }
        //                            else if (coupon.ExpiryDate < utility!.GetCurrentDateTimeInTimeZone())
        //                            {
        //                                return BuildAppActionResultError(result, $"Mã giảm giá của bạn đã hết hạn");
        //                            }
        //                            //NEED BUSINESS RULE HERE
        //                            orderDb.TotalAmount = Math.Max(0, orderDb.TotalAmount);

        //                            orderDb.CustomerSavedCouponId = customerSavedCoupon.CustomerSavedCouponId;
        //                            // Update the coupon usage
        //                            customerSavedCoupon.IsUsedOrExpired = true;
        //                            await customerSavedCouponRepository!.Update(customerSavedCoupon);

        //                        }
        //                    }
        //                }
        //                if (customerInfoDb.Account != null && orderRequestDto.LoyalPointsToUse.HasValue && orderRequestDto.LoyalPointsToUse > 0)
        //                {
        //                    // Check if the user has enough points
        //                    if (customerInfoDb.Account.LoyaltyPoint >= orderRequestDto.LoyalPointsToUse)
        //                    {
        //                        // Calculate the discount (assuming 1 point = 1 currency unit)
        //                        double loyaltyDiscount = Math.Min(orderRequestDto.LoyalPointsToUse.Value, orderDb.TotalAmount);
        //                        orderDb.TotalAmount -= loyaltyDiscount;

        //                        // Ensure the total doesn't go below zero
        //                        orderDb.TotalAmount = Math.Max(0, orderDb.TotalAmount);

        //                        // Update the customer's loyalty points
        //                        customerInfoDb.Account.LoyaltyPoint -= (int)loyaltyDiscount;

        //                        // Create a new loyalty point history entry for the point usage
        //                        var loyalPointUsageHistory = new LoyalPointsHistory
        //                        {
        //                            LoyalPointsHistoryId = Guid.NewGuid(),
        //                            TransactionDate = utility!.GetCurrentDateTimeInTimeZone(),
        //                            OrderId = orderDb.OrderId,
        //                            PointChanged = -(int)loyaltyDiscount,
        //                            NewBalance = customerInfoDb.Account.LoyaltyPoint
        //                        };

        //                        await loyalPointsHistoryRepository!.Insert(loyalPointUsageHistory);
        //                    }
        //                    else
        //                    {
        //                        // Handle the case where the user doesn't have enough points
        //                        return BuildAppActionResultError(result, "Không đủ điểm tích lũy để sử dụng.");
        //                    }
        //                }

        //                // Calculate the final total amount
        //                // The rest of your existing loyalty point earning logic
        //                var newLoyalPointHistory = new LoyalPointsHistory
        //                {
        //                    LoyalPointsHistoryId = Guid.NewGuid(),
        //                    TransactionDate = utility!.GetCurrentDateTimeInTimeZone(),
        //                    OrderId = orderDb.OrderId,
        //                    PointChanged = (int)orderDb.TotalAmount / 100,
        //                    NewBalance = customerInfoDb.Account.LoyaltyPoint + (int)orderDb.TotalAmount / 100
        //                };

        //                await loyalPointsHistoryRepository!.Insert(newLoyalPointHistory);

        //                //orderDb.LoyalPointsHistoryId = newLoyalPointHistory.LoyalPointsHistoryId;
        //                if (customerInfoDb.Account != null)
        //                {
        //                    customerInfoDb.Account.LoyaltyPoint = newLoyalPointHistory.NewBalance;
        //                }
        //                await customerInfoRepository.Update(customerInfoDb);
        //                await _repository.Update(orderDb);
        //                await _unitOfWork.SaveChangesAsync();
        //                scope.Complete();
        //                result.Result = orderDb;
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            result = BuildAppActionResultError(result, ex.Message);
        //        }
        //        return result;
        //    }
        //}
        //public async Task<AppActionResult> GetOrderTotal(CalculateOrderRequest orderRequestDto)
        //{
        //    AppActionResult result = new AppActionResult();
        //    try
        //    {
        //        var utility = Resolve<Utility>();
        //        var customerInfoRepository = Resolve<IGenericRepository<CustomerInfo>>();
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
        //            var customerSavedCouponDb = await customerSavedCouponRepository!.GetAllDataByExpression(p => !string.IsNullOrEmpty(customerInfoDb.AccountId) && p.AccountId == customerInfoDb.AccountId, 0, 0, null, false, p => p.Coupon!);
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

        public async Task<AppActionResult> GetAllOrderByPhoneNumber(string phoneNumber, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            try
            {
                var orderListDb = await
                    _repository.GetAllDataByExpression(p => p.CustomerInfo!.PhoneNumber == phoneNumber, pageNumber, pageSize, p => p.OrderDate, false,
                        p => p.Status!,
                        p => p.CustomerInfo!.Account!,
                        p => p.PaymentMethod!,
                        p => p.LoyalPointsHistory!,
                        p => p.OrderType!
                    );
                result.Result = orderListDb;
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
                if (status.HasValue || orderType.HasValue)
                {
                    result.Result = await _repository.GetAllDataByExpression(o => o.StatusId == status || o.OrderTypeId == orderType, pageNumber, pageSize, o => o.OrderDate, false, p => p.CustomerInfo!.Account!,
                        p => p.Status!,
                        p => p.CustomerInfo!.Account!,
                        p => p.PaymentMethod!,
                        p => p.LoyalPointsHistory!,
                        p => p.OrderType!
                  );
                }
                else if (status.HasValue && orderType.HasValue)
                {
                    result.Result = await _repository.GetAllDataByExpression(o => o.OrderTypeId == orderType && o.OrderTypeId == orderType, pageNumber, pageSize, o => o.OrderDate, false, p => p.CustomerInfo!.Account!,
                        p => p.Status!,
                        p => p.CustomerInfo!.Account!,
                        p => p.PaymentMethod!,
                        p => p.LoyalPointsHistory!,
                        p => p.OrderType!
                        );
                }
                else
                {
                    result.Result = await _repository.GetAllDataByExpression(null, pageNumber, pageSize, o => o.OrderDate, false, p => p.CustomerInfo!.Account!,
                        p => p.Status!,
                        p => p.CustomerInfo!.Account!,
                        p => p.PaymentMethod!,
                        p => p.LoyalPointsHistory!,
                        p => p.OrderType!
                        );
                }
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
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var targetTimeCompleted = currentTime.AddMinutes(-minute.GetValueOrDefault(0));
                var orderDb = await _repository.GetAllDataByExpression(p => p.MealTime == targetTimeCompleted && p.StatusId == OrderStatus.Dining,
                    pageNumber, pageSize, p => p.MealTime, false, p => p.CustomerInfo!.Account!,
                        p => p.Status!,
                        p => p.CustomerInfo!.Account!,
                        p => p.PaymentMethod!,
                        p => p.LoyalPointsHistory!,
                        p => p.OrderType!);
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

                double deposit = total * double.Parse(configurationDb.Items[0].PreValue);
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
                deposit += double.Parse(tableConfigurationDb.Items[0].PreValue);
                request.Deposit = deposit;
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

                    endTime = startTime.AddHours(double.Parse(configurationDb.Items[0].PreValue));
                }

                conditions.Add(() => r => !(endTime < r.ReservationDate || (r.EndTime.HasValue && r.EndTime.Value < startTime || !r.EndTime.HasValue && r.ReservationDate.Value.AddHours(double.Parse(configurationDb.Items[0].PreValue)) < startTime))
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
                    var availableTableDb = await tableRepository!.GetAllDataByExpression(t => !reservedTableIds.Contains(t.TableId), 0, 0, null, false, null);
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
                //Validate
                List<Guid> guids = new List<Guid>();
                if (dto.NumOfPeople <= 0)
                {
                    return BuildAppActionResultError(result, "Số người phải lớn hơn 0!");
                }
                //Get All Available Table
                var availableTableResult = await GetAvailableTable(dto.StartTime, dto.EndTime, dto.NumOfPeople, 0, 0);
                if (availableTableResult.IsSuccess)
                {
                    var availableTable = (PagedResult<Table>)availableTableResult.Result!;
                    if (availableTable.Items!.Count > 0)
                    {
                        result.Result = await GetTables(availableTable.Items, dto.NumOfPeople, dto.IsPrivate);
                    }
                }


                //Get Table with condition: 
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

                if (!tableType.Equals("V"))
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
        public async Task<Table> GetSuitableTable(SuggestTableDto dto)
        {
            Table result = null;
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
                        result = suitableTables[0];
                    }
                }
            }
            catch (Exception ex)
            {
                result = null;
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
        public async Task CancelOverReservation()
        {
            var utility = Resolve<Utility>();
            var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
            try
            {
                var currentTime = utility!.GetCurrentDateTimeInTimeZone().AddHours(24);
                var pastReservationDb = await _repository.GetAllDataByExpression(
                    (p => p.ReservationDate.HasValue && p.ReservationDate.Value.AddHours(24) < currentTime &&
                    (p.StatusId == OrderStatus.Pending || p.StatusId == OrderStatus.TableAssigned
                    )), 0, 0, null, false, null
                    );
                if (pastReservationDb!.Items!.Count > 0 && pastReservationDb.Items != null)
                {
                    foreach (var reservation in pastReservationDb.Items)
                    {
                        reservation.StatusId = OrderStatus.Cancelled;
                        await _repository.Update(reservation);
                        var orderDetailsDb = await orderDetailRepository!.GetAllDataByExpression(p => p.OrderId == reservation.OrderId, 0, 0, null, false, null);
                        if (orderDetailsDb!.Items!.Count > 0 && orderDetailsDb.Items != null)
                        {
                            foreach (var orderDetail in orderDetailsDb.Items)
                            {
                                orderDetail.OrderDetailStatusId = OrderDetailStatus.Cancelled;
                                await orderDetailRepository.Update(orderDetail);
                            }
                        }
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
                        order.StatusId = OrderStatus.Dining;
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

        public async Task UpdateOrderDetailStatusBeforeDining()
        {
            var utility = Resolve<Utility>();
            var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
            try
            {
                var currentTime = utility!.GetCurrentDateTimeInTimeZone();
                var orderListDb = await _repository.GetAllDataByExpression(p => p.MealTime!.Value.AddMinutes(-30) <= currentTime && p.StatusId == OrderStatus.Dining, 0, 0, null, false, null);
                if (orderListDb!.Items!.Count > 0 && orderListDb.Items != null)
                {
                    var orderIds = orderListDb.Items.Select(o => o.OrderId).ToList();
                    var orderDetailsDb = await orderDetailRepository!.GetAllDataByExpression(p => orderIds.Contains(p.OrderId) && p.OrderDetailStatusId == OrderDetailStatus.Pending, 0, 0, null, false, null);
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
    }
}
