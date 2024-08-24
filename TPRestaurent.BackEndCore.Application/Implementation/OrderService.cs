using AutoMapper;
using MailKit.Search;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public OrderService(IGenericRepository<Order> repository, IUnitOfWork unitOfWork, IMapper mapper, IServiceProvider service) : base(service)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
                    if (orderDb == null)
                    {
                        result = BuildAppActionResultError(result, $"Không tìm thấy đơn hàng với id {dto.OrderId}");
                        return result;
                    }

                    var orderDetailDb = await orderDetailRepository!.GetAllDataByExpression(o => o.OrderId == dto.OrderId, 0, 0, null, false, null);
                    int orderBatch = 1;
                    if (orderDetailDb.Items.Count > 0)
                    {
                        orderBatch = orderDetailDb.Items.OrderByDescending(o => o.OrderBatch).Select(o => o.OrderBatch).FirstOrDefault() + 1;
                    }
                    var orderDetails = new List<OrderDetail>();
                    List<ComboOrderDetail> comboOrderDetails = new List<ComboOrderDetail>();

                    dto.OrderDetailsDtos.ForEach(async o =>
                    {
                        var orderDetail = _mapper.Map<OrderDetail>(o);
                        orderDetail.OrderId = dto.OrderId;
                        orderDetail.OrderDetailId = Guid.NewGuid();
                        orderDetail.OrderBatch = orderBatch;
                        orderDb.TotalAmount += orderDetail.Price * orderDetail.Quantity;
                        if (o.Combo != null)
                        {
                            orderDetail.Price = (await comboRepository!.GetById(o.Combo.ComboId)).Price;
                            o.Combo.DishComboIds.ForEach(o => comboOrderDetails.Add(new ComboOrderDetail
                            {
                                ComboOrderDetailId = Guid.NewGuid(),
                                DishComboId = o,
                                OrderDetailId = orderDetail.OrderDetailId
                            }));
                        }
                        else
                        {
                            orderDetail.Price = (await dishRepository!.GetById(o.DishSizeDetailId!)).Price;
                        }
                    });

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
                    if (orderDb.IsDelivering == true)
                    {
                        if (orderDb.Status == OrderStatus.Pending)
                        {
                            if (IsSuccessful)
                            {
                                orderDb.Status = OrderStatus.Processing;
                            }
                            else
                            {
                                orderDb.Status = OrderStatus.Cancelled;
                            }
                        }
                        else if (orderDb.Status == OrderStatus.Processing)
                        {
                            if (IsSuccessful)
                            {
                                orderDb.Status = OrderStatus.Delivering;
                            }
                            else
                            {
                                orderDb.Status = OrderStatus.Cancelled;
                            }
                        }
                        else if (orderDb.Status == OrderStatus.Delivering)
                        {
                            if (IsSuccessful)
                            {
                                orderDb.Status = OrderStatus.Completed;
                            }
                            else
                            {
                                orderDb.Status = OrderStatus.Cancelled;
                            }
                        }
                        else
                        {
                            result = BuildAppActionResultError(result, $"Đơn hàng không thể cập nhật những trạng thái khác");
                        }
                    }
                    else
                    {
                        if (IsSuccessful)
                        {
                            orderDb.Status = OrderStatus.Completed;
                        }
                        else
                        {
                            orderDb.Status = OrderStatus.Cancelled;
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
        public async Task<AppActionResult> CreateOrder(OrderRequestDto orderRequestDto)
        {
            var result = new AppActionResult();
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var utility = Resolve<Utility>();
                    var accountRepository = Resolve<IGenericRepository<Account>>();
                    var customerInfoRepository = Resolve<IGenericRepository<CustomerInfo>>();
                    var reservationRepository = Resolve<IGenericRepository<Reservation>>();
                    var reservationDishRepository = Resolve<IGenericRepository<ReservationDish>>();
                    var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                    var loyalPointsHistoryRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
                    var customerSavedCouponRepository = Resolve<IGenericRepository<CustomerSavedCoupon>>();
                    var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                    var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                    var comboRepository = Resolve<IGenericRepository<Combo>>();
                    var couponRepository = Resolve<IGenericRepository<Coupon>>();
                    var tableRepository = Resolve<IGenericRepository<Table>>();

                    if (orderRequestDto.isDelivering == false)
                    {
                        var tableDb = await tableRepository!.GetById(orderRequestDto.TableId);
                        if (tableDb == null)
                        {
                            return BuildAppActionResultError(result, $"Không tìm thấy thông tin bàn với id {orderRequestDto.TableId}");
                        }

                        var createdOrderDb = await _repository.GetAllDataByExpression(o => o.ReservationId == orderRequestDto.ReservationId, 0, 0, null, false, null);
                        if (createdOrderDb.Items.Count > 0)
                        {
                            return BuildAppActionResultError(result, $"Không thể tạo order mới vì đã có đơn hàng cho lịch đặt bàn {orderRequestDto.ReservationId}");
                        }
                        var reservationDb = await reservationRepository!.GetById(orderRequestDto.ReservationId!);
                        if (reservationDb != null)
                        {
                            var customerInfoDb = await customerInfoRepository!.GetByExpression(p => p.CustomerId == orderRequestDto.CustomerId, p => p.Account!);
                            var reservationDishDb = await reservationDishRepository!.GetAllDataByExpression(p => p.ReservationId == orderRequestDto.ReservationId, 0, 0, null, false, p => p.DishSizeDetail!.Dish!);
                            var customerSavedCouponDb = await customerSavedCouponRepository!.GetAllDataByExpression(p => p.AccountId == customerInfoDb.AccountId, 0, 0, null, false, p => p.Coupon!);

                            var orderDb = new Order
                            {
                                OrderId = Guid.NewGuid(),
                                CustomerId = customerInfoDb!.CustomerId,
                                Note = orderRequestDto.Note,
                                OrderDate = utility.GetCurrentDateTimeInTimeZone(),
                                Status = OrderStatus.Processing,
                                ReservationId = reservationDb.ReservationId,
                                PaymentMethodId = orderRequestDto.PaymentMethodId,
                                IsDelivering = false,
                                TableId = orderRequestDto.TableId
                            };

                            List<OrderDetail> orderDetailsDto = new List<OrderDetail>();
                            List<ComboOrderDetail> comboOrderDetails = new List<ComboOrderDetail>();
                            double money = 0;

                            foreach (var item in reservationDishDb.Items!)
                            {
                                var orderDetailReservation = new OrderDetail
                                {
                                    OrderDetailId = Guid.NewGuid(),
                                    Note = item.Note,
                                    OrderBatch = 1,
                                    OrderId = orderDb.OrderId,
                                };

                                if (orderDetailReservation.ComboId.HasValue)
                                {
                                    var comboReservationDb = await comboRepository!.GetById(orderDetailReservation.ComboId!);
                                    if (comboReservationDb == null)
                                    {
                                        return BuildAppActionResultError(result, $"Combo với id {orderDetailReservation.ComboId} không tồn tại");
                                    }
                                    else
                                    {
                                        orderDetailReservation.ComboId = item.ComboId;
                                        orderDetailReservation.Price = comboReservationDb!.Price * item.Quantity;
                                    }

                                    //Update with OrderDetailId
                                    var comboOrderDetailDb = await comboOrderDetailRepository!.GetAllDataByExpression(c => c.ReservationDishId == item.ReservationDishId, 0, 0, null, false, null);
                                    comboOrderDetailDb.Items!.ForEach(c => c.OrderDetailId = orderDetailReservation.OrderDetailId);
                                    await comboOrderDetailRepository.UpdateRange(comboOrderDetailDb.Items);
                                }
                                else
                                {
                                    var dishSizeReservationDetailDb = await dishSizeDetailRepository!.GetByExpression(p => p.DishSizeDetailId == item.DishSizeDetailId, p => p.Dish!);
                                    if (dishSizeReservationDetailDb == null)
                                    {
                                        return BuildAppActionResultError(result, $"Món ăn với id {item.DishSizeDetailId} không tồn tại");
                                    }
                                    else
                                    {
                                        orderDetailReservation.DishSizeDetailId = dishSizeReservationDetailDb.DishSizeDetailId;
                                        orderDetailReservation.Price = dishSizeReservationDetailDb!.Price * item.Quantity;
                                    }
                                }
                                orderDetailsDto.Add(orderDetailReservation);
                                await orderDetailRepository!.InsertRange(orderDetailsDto);
                                money += orderDetailReservation.Price;
                            }

                            if (orderRequestDto.OrderDetailsDtos.Count > 0)
                            {
                                foreach (var orderDetail in orderRequestDto.OrderDetailsDtos)
                                {
                                    var orderDetailDb = new OrderDetail
                                    {
                                        OrderDetailId = Guid.NewGuid(),
                                        Note = orderDetail.Note,
                                        Quantity = orderDetail.Quantity,
                                        OrderId = orderDb.OrderId,
                                    };

                                    if (orderDetail.Combo != null)
                                    {
                                        var comboDb = await comboRepository!.GetById(orderDetail.Combo.ComboId!);
                                        if (comboDb == null)
                                        {
                                            return BuildAppActionResultError(result, $"Combo với id {orderDetail.Combo.ComboId} không tồn tại");
                                        }
                                        else
                                        {
                                            orderDetailDb.ComboId = orderDetail.Combo.ComboId;
                                            orderDetailDb.Price = comboDb!.Price * orderDetail.Quantity;
                                        }
                                        //Create ComboOrderDetailHere
                                        orderDetail.Combo.DishComboIds.ForEach(o => comboOrderDetails.Add(new ComboOrderDetail
                                        {
                                            ComboOrderDetailId = Guid.NewGuid(),
                                            DishComboId = o,
                                            OrderDetailId = orderDetailDb.OrderDetailId
                                        }));
                                    }
                                    else
                                    {
                                        var dishSizeDetailDb = await dishSizeDetailRepository!.GetByExpression(p => p.DishSizeDetailId == orderDetail.DishSizeDetailId, p => p.Dish!);
                                        if (dishSizeDetailDb == null)
                                        {
                                            return BuildAppActionResultError(result, $"Món ăn với id {orderDetail.DishSizeDetailId} không tồn tại");
                                        }
                                        else
                                        {
                                            orderDetailDb.DishSizeDetailId = dishSizeDetailDb.DishSizeDetailId;
                                            orderDetailDb.Price = dishSizeDetailDb!.Price * orderDetail.Quantity;
                                        }
                                    }
                                    orderDetailsDto.Add(orderDetailDb);
                                    await orderDetailRepository!.InsertRange(orderDetailsDto);
                                    money += orderDetailDb.Price;
                                }
                                await comboOrderDetailRepository!.InsertRange(comboOrderDetails);
                            }
                            orderDb.TotalAmount = money - reservationDb.Deposit;

                            if (customerSavedCouponDb.Items!.Count > 0 && customerSavedCouponDb.Items != null)
                            {
                                if (orderRequestDto.CouponId.HasValue)
                                {
                                    var customerSavedCoupon = customerSavedCouponDb.Items.FirstOrDefault(c => c.CouponId == orderRequestDto.CouponId);
                                    if (customerSavedCoupon != null && customerSavedCoupon.IsUsedOrExpired == false)
                                    {
                                        var coupon = await couponRepository!.GetById(customerSavedCoupon.CouponId);
                                        if (coupon != null && coupon.ExpiryDate > utility.GetCurrentDateTimeInTimeZone())
                                        {
                                            double discountMoney = money * (coupon.DiscountPercent * 0.01);
                                            money -= discountMoney;
                                        }else if (coupon!.ExpiryDate < utility.GetCurrentDateTimeInTimeZone())
                                        {
                                            return BuildAppActionResultError(result, $"Mã giảm giá của bạn đã hết hạn");
                                        }
                                        //NEED BUSINESS RULE HERE
                                        money = Math.Max(0, money);

                                        orderDb.CustomerSavedCouponId = customerSavedCoupon.CustomerSavedCouponId;
                                        // Update the coupon usage
                                        customerSavedCoupon.IsUsedOrExpired = true;
                                        await customerSavedCouponRepository!.Update(customerSavedCoupon);

                                    }
                                }
                            }
                            if (customerInfoDb.Account != null && orderRequestDto.LoyalPointsToUse.HasValue && orderRequestDto.LoyalPointsToUse > 0)
                            {
                                // Check if the user has enough points
                                if (customerInfoDb.Account.LoyaltyPoint >= orderRequestDto.LoyalPointsToUse)
                                {
                                    // Calculate the discount (assuming 1 point = 1 currency unit)
                                    double loyaltyDiscount = Math.Min(orderRequestDto.LoyalPointsToUse.Value, money);
                                    money -= loyaltyDiscount;

                                    // Ensure the total doesn't go below zero
                                    money = Math.Max(0, money);

                                    // Update the customer's loyalty points
                                    customerInfoDb.Account.LoyaltyPoint -= (int)loyaltyDiscount;

                                    // Create a new loyalty point history entry for the point usage
                                    var loyalPointUsageHistory = new LoyalPointsHistory
                                    {
                                        LoyalPointsHistoryId = Guid.NewGuid(),
                                        TransactionDate = utility!.GetCurrentDateTimeInTimeZone(),
                                        OrderId = orderDb.OrderId,
                                        PointChanged = -(int)loyaltyDiscount,
                                        NewBalance = customerInfoDb.Account.LoyaltyPoint
                                    };

                                    await loyalPointsHistoryRepository!.Insert(loyalPointUsageHistory);
                                }
                                else
                                {
                                    // Handle the case where the user doesn't have enough points
                                    return BuildAppActionResultError(result, "Không đủ điểm tích lũy để sử dụng.");
                                }
                            }

                            // Calculate the final total amount
                            orderDb.TotalAmount = money - reservationDb.Deposit;

                            // The rest of your existing loyalty point earning logic
                            var newLoyalPointHistory = new LoyalPointsHistory
                            {
                                LoyalPointsHistoryId = Guid.NewGuid(),
                                TransactionDate = utility!.GetCurrentDateTimeInTimeZone(),
                                OrderId = orderDb.OrderId,
                                PointChanged = (int)money / 100,
                                NewBalance = customerInfoDb.Account.LoyaltyPoint + (int)money / 100
                            };

                            await loyalPointsHistoryRepository!.Insert(newLoyalPointHistory);

                            //orderDb.LoyalPointsHistoryId = newLoyalPointHistory.LoyalPointsHistoryId;
                            if(customerInfoDb.Account != null)
                            {
                                customerInfoDb.Account.LoyaltyPoint = newLoyalPointHistory.NewBalance;
                            }
                            await customerInfoRepository.Update(customerInfoDb);

                            await _repository.Insert(orderDb);
                        }
                        else if (reservationDb == null)
                        {
                            var orderDb = new Order
                            {
                                OrderId = Guid.NewGuid(),
                                IsDelivering = false,
                                Note = orderRequestDto.Note,
                                PaymentMethodId = orderRequestDto.PaymentMethodId,
                                Status = OrderStatus.Processing,
                                OrderDate = utility.GetCurrentDateTimeInTimeZone(),
                            };

                            List<OrderDetail> orderDetailsDto = new List<OrderDetail>();
                            List<ComboOrderDetail> comboOrderDetails = new List<ComboOrderDetail>();
                            double money = 0;

                            foreach (var orderDetail in orderRequestDto.OrderDetailsDtos)
                            {
                                var orderDetailDb = new OrderDetail
                                {
                                    OrderDetailId = Guid.NewGuid(),
                                    Note = orderDetail.Note,
                                    Quantity = orderDetail.Quantity,
                                    OrderId = orderDb.OrderId,
                                };

                                if (orderDetail.Combo != null)
                                {
                                    var comboDb = await comboRepository!.GetById(orderDetail.Combo.ComboId!);
                                    if (comboDb == null)
                                    {
                                        return BuildAppActionResultError(result, $"Combo với id {orderDetail.Combo.ComboId} không tồn tại");
                                    }
                                    else
                                    {
                                        orderDetailDb.ComboId = orderDetail.Combo.ComboId;
                                        orderDetailDb.Price = comboDb!.Price * orderDetail.Quantity;
                                    }
                                    orderDetail.Combo.DishComboIds.ForEach(o => comboOrderDetails.Add(new ComboOrderDetail
                                    {
                                        ComboOrderDetailId = Guid.NewGuid(),
                                        DishComboId = o,
                                        OrderDetailId = orderDetailDb.OrderDetailId
                                    }));
                                }
                                else
                                {
                                    var dishSizeDetailDb = await dishSizeDetailRepository!.GetByExpression(p => p.DishSizeDetailId == orderDetail.DishSizeDetailId, p => p.Dish!);
                                    if (dishSizeDetailDb == null)
                                    {
                                        return BuildAppActionResultError(result, $"Món ăn với id {orderDetail.DishSizeDetailId} không tồn tại");
                                    }
                                    else
                                    {
                                        orderDetailDb.DishSizeDetailId = dishSizeDetailDb.DishSizeDetailId;
                                        orderDetailDb.Price = dishSizeDetailDb!.Price * orderDetail.Quantity;
                                    }
                                }
                                orderDetailsDto.Add(orderDetailDb);
                                await orderDetailRepository!.InsertRange(orderDetailsDto);
                                money += orderDetailDb.Price;
                            }

                            await comboOrderDetailRepository!.InsertRange(comboOrderDetails);
                            orderDb.TotalAmount = money;

                            var customerInfoDb = await customerInfoRepository!.GetByExpression(p => p.CustomerId == orderRequestDto.CustomerId, p => p.Account!);
                            if (customerInfoDb.Account != null && orderRequestDto.CustomerId.HasValue)
                            {
                                var customerSavedCouponDb = await customerSavedCouponRepository!.GetAllDataByExpression(p => string.IsNullOrEmpty(customerInfoDb.AccountId) && p.AccountId == customerInfoDb.AccountId, 0, 0, null, false, p => p.Coupon!);

                                if (customerSavedCouponDb.Items!.Count > 0 && customerSavedCouponDb.Items != null)
                                {
                                    if (orderRequestDto.CouponId.HasValue)
                                    {
                                        var customerSavedCoupon = customerSavedCouponDb.Items.FirstOrDefault(c => c.CouponId == orderRequestDto.CouponId);
                                        if (customerSavedCoupon != null && customerSavedCoupon.IsUsedOrExpired == false)
                                        {
                                            var coupon = await couponRepository!.GetById(customerSavedCoupon.CouponId);
                                            if (coupon != null && coupon.ExpiryDate > utility.GetCurrentDateTimeInTimeZone())
                                            {
                                                double discountMoney = money * (coupon.DiscountPercent * 0.01);
                                                money -= discountMoney;
                                            }else if (coupon!.ExpiryDate < utility.GetCurrentDateTimeInTimeZone())
                                            {
                                                return BuildAppActionResultError(result, $"Mã giảm giá của bạn đã hết hạn");
                                            }

                                            money = Math.Max(0, money);

                                            orderDb.CustomerSavedCouponId = customerSavedCoupon.CustomerSavedCouponId;
                                            // Update the coupon usage
                                            customerSavedCoupon.IsUsedOrExpired = true;
                                            await customerSavedCouponRepository!.Update(customerSavedCoupon);

                                        }
                                    }
                                }
                                if (customerInfoDb.Account != null && orderRequestDto.LoyalPointsToUse.HasValue && orderRequestDto.LoyalPointsToUse > 0)
                                {
                                    // Check if the user has enough points
                                    if (customerInfoDb.Account.LoyaltyPoint >= orderRequestDto.LoyalPointsToUse)
                                    {
                                        // Calculate the discount (assuming 1 point = 1 currency unit)
                                        double loyaltyDiscount = Math.Min(orderRequestDto.LoyalPointsToUse.Value, money);
                                        money -= loyaltyDiscount;

                                        // Ensure the total doesn't go below zero
                                        money = Math.Max(0, money);

                                        // Update the customer's loyalty points
                                        customerInfoDb.Account.LoyaltyPoint -= (int)loyaltyDiscount;

                                        // Create a new loyalty point history entry for the point usage
                                        var loyalPointUsageHistory = new LoyalPointsHistory
                                        {
                                            LoyalPointsHistoryId = Guid.NewGuid(),
                                            TransactionDate = utility!.GetCurrentDateTimeInTimeZone(),
                                            OrderId = orderDb.OrderId,
                                            PointChanged = -(int)loyaltyDiscount,
                                            NewBalance = customerInfoDb.Account.LoyaltyPoint
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
                                    NewBalance = customerInfoDb!.Account.LoyaltyPoint + (int)money / 100
                                };

                                await loyalPointsHistoryRepository!.Insert(newLoyalPointHistory);

                                //orderDb.LoyalPointsHistoryId = newLoyalPointHistory.LoyalPointsHistoryId;

                                customerInfoDb.Account.LoyaltyPoint = newLoyalPointHistory.NewBalance;
                                await customerInfoRepository.Update(customerInfoDb);
                            }
                            await _repository.Insert(orderDb);
                        }
                    }
                    else if (orderRequestDto.isDelivering == true)
                    {
                        var orderDb = new Order
                        {
                            OrderId = Guid.NewGuid(),
                            Note = orderRequestDto.Note,
                            OrderDate = utility.GetCurrentDateTimeInTimeZone(),
                            Status = OrderStatus.Pending,
                            PaymentMethodId = orderRequestDto.PaymentMethodId,
                            IsDelivering = true,
                        };
                        List<OrderDetail> orderDetailsDto = new List<OrderDetail>();
                        List<ComboOrderDetail> comboOrderDetails = new List<ComboOrderDetail>();

                        double money = 0;

                        foreach (var orderDetail in orderRequestDto.OrderDetailsDtos)
                        {
                            if (orderDetail.Quantity < 1)
                            {
                                return BuildAppActionResultError(result, "Số lượng món ăn/ combo phải lớn hơn 0");
                            }
                            var orderDetailDb = new OrderDetail
                            {
                                OrderDetailId = Guid.NewGuid(),
                                Note = orderDetail.Note,
                                Quantity = orderDetail.Quantity,
                                OrderId = orderDb.OrderId,
                            };

                            if (orderDetail.Combo != null)
                            {
                                var comboDb = await comboRepository!.GetById(orderDetail.Combo.ComboId!);
                                if (comboDb == null)
                                {
                                    return BuildAppActionResultError(result, $"Combo với id {orderDetail.Combo.ComboId} không tồn tại");
                                }
                                else
                                {
                                    orderDetailDb.ComboId = orderDetail.Combo.ComboId;
                                    orderDetailDb.Price = comboDb!.Price * orderDetail.Quantity;
                                }
                                orderDetail.Combo.DishComboIds.ForEach(o => comboOrderDetails.Add(new ComboOrderDetail
                                {
                                    ComboOrderDetailId = Guid.NewGuid(),
                                    DishComboId = o,
                                    OrderDetailId = orderDetailDb.OrderDetailId
                                }));
                            }
                            else
                            {
                                var dishSizeDetailDb = await dishSizeDetailRepository!.GetByExpression(p => p.DishSizeDetailId == orderDetail.DishSizeDetailId, p => p.Dish!);
                                if (dishSizeDetailDb == null)
                                {
                                    return BuildAppActionResultError(result, $"Món ăn với id {orderDetail.DishSizeDetailId} không tồn tại");
                                }
                                else
                                {
                                    orderDetailDb.DishSizeDetailId = dishSizeDetailDb.DishSizeDetailId;
                                    orderDetailDb.Price = dishSizeDetailDb!.Price * orderDetail.Quantity;
                                }
                            }
                            orderDetailsDto.Add(orderDetailDb);
                            await orderDetailRepository!.InsertRange(orderDetailsDto);
                            money += orderDetailDb.Price;
                        }

                        await comboOrderDetailRepository!.InsertRange(comboOrderDetails);
                        orderDb.TotalAmount = money;

                        if (orderRequestDto.CustomerId.HasValue)
                        {
                            var customerInfoDb = await customerInfoRepository!.GetByExpression(p => p.CustomerId == orderRequestDto.CustomerId, p => p.Account!);
                            if (string.IsNullOrEmpty(customerInfoDb!.Address))
                            {
                                return BuildAppActionResultError(result, $"Địa chỉ của bạn không tồn tại. Vui lòng cập nhập địa chỉ");
                            }
                            orderDb.CustomerId = customerInfoDb.CustomerId;
                            var customerSavedCouponDb = await customerSavedCouponRepository!.GetAllDataByExpression(p => string.IsNullOrEmpty(customerInfoDb.AccountId) && p.AccountId == customerInfoDb.AccountId, 0, 0, null, false, p => p.Coupon!);
                            if (customerSavedCouponDb.Items!.Count < 0 && customerSavedCouponDb.Items != null)
                            {
                                if (orderRequestDto.CouponId.HasValue)
                                {
                                    var customerSavedCoupon = customerSavedCouponDb.Items.FirstOrDefault(c => c.CouponId == orderRequestDto.CouponId);
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

                                        orderDb.CustomerSavedCouponId = customerSavedCoupon.CustomerSavedCouponId;
                                        // Update the coupon usage
                                        customerSavedCoupon.IsUsedOrExpired = true;
                                        await customerSavedCouponRepository!.Update(customerSavedCoupon);

                                    }
                                }
                            }
                            if (customerInfoDb.Account != null && orderRequestDto.LoyalPointsToUse.HasValue && orderRequestDto.LoyalPointsToUse > 0)
                            {
                                // Check if the user has enough points
                                if (customerInfoDb.Account!.LoyaltyPoint >= orderRequestDto.LoyalPointsToUse)
                                {
                                    // Calculate the discount (assuming 1 point = 1 currency unit)
                                    double loyaltyDiscount = Math.Min(orderRequestDto.LoyalPointsToUse.Value, money);
                                    money -= loyaltyDiscount;

                                    // Ensure the total doesn't go below zero
                                    money = Math.Max(0, money);

                                    // Update the customer's loyalty points
                                    customerInfoDb.Account.LoyaltyPoint -= (int)loyaltyDiscount;

                                    // Create a new loyalty point history entry for the point usage
                                    var loyalPointUsageHistory = new LoyalPointsHistory
                                    {
                                        LoyalPointsHistoryId = Guid.NewGuid(),
                                        TransactionDate = utility!.GetCurrentDateTimeInTimeZone(),
                                        OrderId = orderDb.OrderId,
                                        PointChanged = -(int)loyaltyDiscount,
                                        NewBalance = customerInfoDb.Account.LoyaltyPoint
                                    };

                                    await loyalPointsHistoryRepository!.Insert(loyalPointUsageHistory);
                                }
                                else
                                {
                                    // Handle the case where the user doesn't have enough points
                                    return BuildAppActionResultError(result, "Không đủ điểm tích lũy để sử dụng.");
                                }
                            }

                            if(customerInfoDb.Account != null)
                            {
                                var newLoyalPointHistory = new LoyalPointsHistory
                                {
                                    LoyalPointsHistoryId = Guid.NewGuid(),
                                    TransactionDate = utility!.GetCurrentDateTimeInTimeZone(),
                                    OrderId = orderDb.OrderId,
                                    PointChanged = (int)money / 100,
                                    NewBalance = customerInfoDb!.Account.LoyaltyPoint + (int)money / 100
                                };

                                await loyalPointsHistoryRepository!.Insert(newLoyalPointHistory);

                                //orderDb.LoyalPointsHistoryId = newLoyalPointHistory.LoyalPointsHistoryId;

                                customerInfoDb.Account.LoyaltyPoint = newLoyalPointHistory.NewBalance;
                            }
                            await customerInfoRepository.Update(customerInfoDb);
                        }
                        await _repository.Insert(orderDb);
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
        public async Task<AppActionResult> GetAllOrderByAccountId(string accountId, OrderStatus? status, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                if (status.HasValue)
                {
                    result.Result = await _repository.GetAllDataByExpression(o => o.CustomerInfo.AccountId.Equals(accountId) && o.Status == status, pageNumber, pageSize, o => o.OrderDate, false, null);
                }
                else
                {
                    result.Result = await _repository.GetAllDataByExpression(o => o.CustomerInfo.AccountId.Equals(accountId), pageNumber, pageSize, o => o.OrderDate, false, null);
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
                var orderDb = await _repository.GetAllDataByExpression(p => p.OrderId == orderId, 0, 0 , null, false, p => p.CustomerInfo!.Account!,
                    p => p.PaymentMethod!,
                    p => p.Reservation!.ReservationStatus!,
                    p => p.LoyalPointsHistory!,
                    p => p.CustomerSavedCoupon!.Coupon!,
                    p => p.CustomerSavedCoupon!.Account!,
                    p => p.Table!.TableRating!,
                    p => p.Table!.TableSize!);
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
        public async Task<AppActionResult> CompleteOrder(OrderPaymentRequestDto orderRequestDto)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                AppActionResult result = new AppActionResult();
                try
                {
                    var orderDb = await _repository.GetById(orderRequestDto.OrderId);
                    if (orderDb == null)
                    {
                        return BuildAppActionResultError(result, $"Không tìm thấy đơn với id {orderRequestDto.OrderId}");
                    }
                    if (orderDb.CustomerId.HasValue)
                    {
                        var couponRepository = Resolve<IGenericRepository<Coupon>>();
                        var customerSavedCouponRepository = Resolve<IGenericRepository<CustomerSavedCoupon>>();
                        var customerInfoRepository = Resolve<IGenericRepository<CustomerInfo>>();
                        var loyalPointsHistoryRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
                        var customerInfoDb = await customerInfoRepository!.GetByExpression(p => p.CustomerId == orderDb.CustomerId, p => p.Account!);
                        var utility = Resolve<Utility>();
                        var customerSavedCouponDb = await customerSavedCouponRepository!.GetAllDataByExpression(p => string.IsNullOrEmpty(customerInfoDb.AccountId) && p.AccountId == customerInfoDb.AccountId, 0, 0, null, false, p => p.Coupon!);
                        if (customerSavedCouponDb.Items!.Count < 0 && customerSavedCouponDb.Items != null)
                        {
                            if (orderRequestDto.CouponId.HasValue)
                            {
                                var customerSavedCoupon = customerSavedCouponDb.Items.FirstOrDefault(c => c.CouponId == orderRequestDto.CouponId);
                                if (customerSavedCoupon != null && customerSavedCoupon.IsUsedOrExpired == false)
                                {
                                    var coupon = await couponRepository!.GetById(customerSavedCoupon.CouponId);
                                    if (coupon != null && coupon.ExpiryDate > utility!.GetCurrentDateTimeInTimeZone())
                                    {
                                        double discountAmount = orderDb.TotalAmount * (coupon.DiscountPercent * 0.01);
                                        orderDb.TotalAmount -= discountAmount;
                                    }
                                    else if (coupon.ExpiryDate < utility!.GetCurrentDateTimeInTimeZone())
                                    {
                                        return BuildAppActionResultError(result, $"Mã giảm giá của bạn đã hết hạn");
                                    }
                                    //NEED BUSINESS RULE HERE
                                    orderDb.TotalAmount = Math.Max(0, orderDb.TotalAmount);

                                    orderDb.CustomerSavedCouponId = customerSavedCoupon.CustomerSavedCouponId;
                                    // Update the coupon usage
                                    customerSavedCoupon.IsUsedOrExpired = true;
                                    await customerSavedCouponRepository!.Update(customerSavedCoupon);

                                }
                            }
                        }
                        if (customerInfoDb.Account != null && orderRequestDto.LoyalPointsToUse.HasValue && orderRequestDto.LoyalPointsToUse > 0)
                        {
                            // Check if the user has enough points
                            if (customerInfoDb.Account.LoyaltyPoint >= orderRequestDto.LoyalPointsToUse)
                            {
                                // Calculate the discount (assuming 1 point = 1 currency unit)
                                double loyaltyDiscount = Math.Min(orderRequestDto.LoyalPointsToUse.Value, orderDb.TotalAmount);
                                orderDb.TotalAmount -= loyaltyDiscount;

                                // Ensure the total doesn't go below zero
                                orderDb.TotalAmount = Math.Max(0, orderDb.TotalAmount);

                                // Update the customer's loyalty points
                                customerInfoDb.Account.LoyaltyPoint -= (int)loyaltyDiscount;

                                // Create a new loyalty point history entry for the point usage
                                var loyalPointUsageHistory = new LoyalPointsHistory
                                {
                                    LoyalPointsHistoryId = Guid.NewGuid(),
                                    TransactionDate = utility!.GetCurrentDateTimeInTimeZone(),
                                    OrderId = orderDb.OrderId,
                                    PointChanged = -(int)loyaltyDiscount,
                                    NewBalance = customerInfoDb.Account.LoyaltyPoint
                                };

                                await loyalPointsHistoryRepository!.Insert(loyalPointUsageHistory);
                            }
                            else
                            {
                                // Handle the case where the user doesn't have enough points
                                return BuildAppActionResultError(result, "Không đủ điểm tích lũy để sử dụng.");
                            }
                        }

                        // Calculate the final total amount
                        // The rest of your existing loyalty point earning logic
                        var newLoyalPointHistory = new LoyalPointsHistory
                        {
                            LoyalPointsHistoryId = Guid.NewGuid(),
                            TransactionDate = utility!.GetCurrentDateTimeInTimeZone(),
                            OrderId = orderDb.OrderId,
                            PointChanged = (int)orderDb.TotalAmount / 100,
                            NewBalance = customerInfoDb.Account.LoyaltyPoint + (int)orderDb.TotalAmount / 100
                        };

                        await loyalPointsHistoryRepository!.Insert(newLoyalPointHistory);

                        //orderDb.LoyalPointsHistoryId = newLoyalPointHistory.LoyalPointsHistoryId;
                        if (customerInfoDb.Account != null)
                        {
                            customerInfoDb.Account.LoyaltyPoint = newLoyalPointHistory.NewBalance;
                        }
                        await customerInfoRepository.Update(customerInfoDb);
                        await _repository.Update(orderDb);
                        await _unitOfWork.SaveChangesAsync();
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
        public async Task<AppActionResult> GetOrderTotal(CalculateOrderRequest orderRequestDto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var utility = Resolve<Utility>();
                var customerInfoRepository = Resolve<IGenericRepository<CustomerInfo>>();
                if (orderRequestDto.CustomerId.HasValue)
                {
                    var data = new CalculateOrderResponse();
                    data.Amount = orderRequestDto.Total;
                    var customerInfoDb = await customerInfoRepository!.GetById(orderRequestDto.CustomerId);
                    if (customerInfoDb == null)
                    {
                        return BuildAppActionResultError(result, $"Không tìm thấy thông tin khách hàng với id {orderRequestDto.CustomerId}");
                    }
                    var customerSavedCouponRepository = Resolve<IGenericRepository<CustomerSavedCoupon>>();
                    var reservationRepository = Resolve<IGenericRepository<Reservation>>();
                    var loyalPointsHistoryRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
                    var reservationDb = await reservationRepository.GetById(orderRequestDto.ReservationId);
                    if (reservationDb != null)
                    {
                        data.PaidDeposit = reservationDb.Deposit;
                    }
                    var customerSavedCouponDb = await customerSavedCouponRepository!.GetAllDataByExpression(p => string.IsNullOrEmpty(customerInfoDb.AccountId) && p.AccountId == customerInfoDb.AccountId, 0, 0, null, false, p => p.Coupon!);
                    if (customerSavedCouponDb.Items!.Count > 0 && customerSavedCouponDb.Items != null)
                    {
                        if (orderRequestDto.CouponId.HasValue)
                        {
                            var customerSavedCoupon = customerSavedCouponDb.Items.FirstOrDefault(c => c.CouponId == orderRequestDto.CouponId);
                            if (customerSavedCoupon != null && customerSavedCoupon.IsUsedOrExpired == false)
                            {
                                if (customerSavedCoupon.Coupon != null && customerSavedCoupon.Coupon.MinimumAmount < orderRequestDto.Total && customerSavedCoupon.Coupon.ExpiryDate > utility!.GetCurrentDateTimeInTimeZone())
                                {
                                    data.CouponDiscount = orderRequestDto.Total * (customerSavedCoupon.Coupon.DiscountPercent * 0.01);
                                } 
                                else if (customerSavedCoupon.Coupon.ExpiryDate < utility!.GetCurrentDateTimeInTimeZone())
                                {
                                    return BuildAppActionResultError(result, $"Mã giảm giá của bạn đã hết hạn");
                                }
                                else
                                {
                                    result.Messages.Add($"Coupon {customerSavedCoupon.Coupon.Code} yều cầu hoá đơn phải trên {customerSavedCoupon.Coupon.MinimumAmount}");
                                }

                            }
                        }
                    }
                    if (customerInfoDb.Account != null && orderRequestDto.LoyalPointsToUse.HasValue && orderRequestDto.LoyalPointsToUse > 0)
                    {
                        // Check if the user has enough points
                        if (customerInfoDb.Account.LoyaltyPoint >= orderRequestDto.LoyalPointsToUse)
                        {
                            // Calculate the discount (assuming 1 point = 1 currency unit)
                            data.LoyalPointUsed = (double)orderRequestDto.LoyalPointsToUse;
                        }
                        else
                        {
                            // Handle the case where the user doesn't have enough points
                            return BuildAppActionResultError(result, $"Không đủ điểm tích lũy để sử dụng. Bạn còn {customerInfoDb.Account.LoyaltyPoint} điểm");
                        }
                    }

                    data.FinalPrice = Math.Max(0, data.Amount - data.PaidDeposit - data.CouponDiscount - data.LoyalPointUsed);
                    data.LoyalPointAdded = Math.Floor(data.FinalPrice / 100);
                    result.Result = data;
                }
                else
                {
                    result.Result = new CalculateOrderResponse
                    {
                        Amount = orderRequestDto.Total,
                        FinalPrice = orderRequestDto.Total
                    };
                }

            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAllOrderByPhoneNumber(string phoneNumber, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            try
            {
                var orderListDb = await 
                    _repository.GetAllDataByExpression(p => p.CustomerInfo!.PhoneNumber == phoneNumber, pageNumber, pageSize, p => p.OrderDate, false,
                    p => p.CustomerInfo!.Account!,
                    p => p.PaymentMethod!,
                    p => p.Reservation!.ReservationStatus!,
                    p => p.LoyalPointsHistory!,
                    p => p.CustomerSavedCoupon!.Coupon!,
                    p => p.CustomerSavedCoupon!.Account!,
                    p => p.Table!.TableRating!,
                    p => p.Table!.TableSize!
                    );
                result.Result = orderListDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAllOrderByStatus(OrderStatus? status, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                if (status.HasValue)
                {
                    result.Result = await _repository.GetAllDataByExpression(o => o.Status == status, pageNumber, pageSize, o => o.OrderDate, false, null);
                }
                else
                {
                    result.Result = await _repository.GetAllDataByExpression(null, pageNumber, pageSize, o => o.OrderDate, false, null);
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
