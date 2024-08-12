﻿using AutoMapper;
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
                //AddOrderMessageToChef
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> ChangeOrderStatus(string orderId, bool IsSuccessful)
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
                        }else if (orderDb.Status == OrderStatus.Processing)
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

                    if (orderRequestDto.isDelivering == false)
                    {
                        var reservationDb = await reservationRepository!.GetById(orderRequestDto.ReservationId!);
                        if (reservationDb != null)
                        {
                            var customerInfoDb = await customerInfoRepository!.GetByExpression(p => p.CustomerId == orderRequestDto.CustomerId, p => p.Account!);
                            var reservationDishDb = await reservationDishRepository!.GetAllDataByExpression(p => p.ReservationId == orderRequestDto.ReservationId, 0, 0, null, false, p => p.DishSizeDetail!.Dish!);
                            //var comboOrderDetailDb = await comboOrderDetailRepository!.GetAllDataByExpression(p => p.ReservationDish.ReservationId == orderRequestDto.ReservationId, 0, 0, null, false, p => p.ReservationDish.Combo!);
                            var customerSavedCouponDb = await customerSavedCouponRepository!.GetAllDataByExpression(p => p.CustomerInfoId == orderRequestDto.CustomerId, 0, 0, null, false, p => p.Coupon!);

                            var orderDb = new Order
                            {
                                OrderId = Guid.NewGuid(),
                                CustomerId = customerInfoDb!.CustomerId,
                                Note = orderRequestDto.Note,
                                OrderDate = orderRequestDto.OrderDate,
                                Status = OrderStatus.Processing,
                                ReservationId = reservationDb.ReservationId,
                                PaymentMethodId = orderRequestDto.PaymentMethodId,
                                IsDelivering = false,
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
                                    var dishSizeReservationDetailDb = await dishSizeDetailRepository!.GetByExpression(p => p.DishSizeDetailId == orderDetailReservation.DishSizeDetailId, p => p.Dish!);
                                    if (dishSizeReservationDetailDb == null)
                                    {
                                        return BuildAppActionResultError(result, $"Món ăn với id {orderDetailReservation.DishSizeDetailId} không tồn tại");
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

                            if(orderRequestDto.OrderDetailsDtos.Count > 0)
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

                            if (customerSavedCouponDb.Items!.Count < 0 && customerSavedCouponDb.Items != null)
                            {
                                if (orderRequestDto.CouponId.HasValue)
                                {
                                    var customerSavedCoupon = customerSavedCouponDb.Items.FirstOrDefault(c => c.CouponId == orderRequestDto.CouponId);
                                    if (customerSavedCoupon != null && customerSavedCoupon.IsUsedOrExpired == false)
                                    {
                                        var coupon = await couponRepository!.GetById(customerSavedCoupon.CouponId);
                                        if (coupon != null)
                                        {
                                            double discountMoney = money * (coupon.DiscountPercent / 100);
                                            money -= discountMoney;
                                        }

                                        money = Math.Max(0, money);

                                        orderDb.CustomerSavedCouponId = customerSavedCoupon.CustomerSavedCouponId;
                                        // Update the coupon usage
                                        customerSavedCoupon.IsUsedOrExpired = true;
                                        await customerSavedCouponRepository!.Update(customerSavedCoupon);

                                    }
                                }
                            }
                            if (orderRequestDto.LoyalPointsToUse.HasValue && orderRequestDto.LoyalPointsToUse > 0)
                            {
                                // Check if the user has enough points
                                if (customerInfoDb.LoyaltyPoint >= orderRequestDto.LoyalPointsToUse)
                                {
                                    // Calculate the discount (assuming 1 point = 1 currency unit)
                                    double loyaltyDiscount = Math.Min(orderRequestDto.LoyalPointsToUse.Value, money);
                                    money -= loyaltyDiscount;

                                    // Ensure the total doesn't go below zero
                                    money = Math.Max(0, money);

                                    // Update the customer's loyalty points
                                    customerInfoDb.LoyaltyPoint -= (int)loyaltyDiscount;

                                    // Create a new loyalty point history entry for the point usage
                                    var loyalPointUsageHistory = new LoyalPointsHistory
                                    {
                                        LoyalPointsHistoryId = Guid.NewGuid(),
                                        TransactionDate = utility!.GetCurrentDateTimeInTimeZone(),
                                        OrderId = orderDb.OrderId,
                                        PointChanged = -(int)loyaltyDiscount,
                                        NewBalance = customerInfoDb.LoyaltyPoint
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
                                NewBalance = customerInfoDb.LoyaltyPoint + (int)money / 100
                            };

                            await loyalPointsHistoryRepository!.Insert(newLoyalPointHistory);

                            orderDb.LoyalPointsHistoryId = newLoyalPointHistory.LoyalPointsHistoryId;

                            customerInfoDb.LoyaltyPoint = newLoyalPointHistory.NewBalance;
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
                                OrderDate = orderRequestDto.OrderDate,  
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

                                if (orderDetail.Combo !=null)
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
                                var customerSavedCouponDb = await customerSavedCouponRepository!.GetAllDataByExpression(p => p.CustomerInfoId == orderRequestDto.CustomerId, 0, 0, null, false, p => p.Coupon!);

                                if (customerSavedCouponDb.Items!.Count < 0 && customerSavedCouponDb.Items != null)
                                {
                                    if (orderRequestDto.CouponId.HasValue)
                                    {
                                        var customerSavedCoupon = customerSavedCouponDb.Items.FirstOrDefault(c => c.CouponId == orderRequestDto.CouponId);
                                        if (customerSavedCoupon != null && customerSavedCoupon.IsUsedOrExpired == false)
                                        {
                                            var coupon = await couponRepository!.GetById(customerSavedCoupon.CouponId);
                                            if (coupon != null)
                                            {
                                                double discountMoney = money * (coupon.DiscountPercent / 100);
                                                money -= discountMoney;
                                            }

                                            money = Math.Max(0, money);

                                            orderDb.CustomerSavedCouponId = customerSavedCoupon.CustomerSavedCouponId;
                                            // Update the coupon usage
                                            customerSavedCoupon.IsUsedOrExpired = true;
                                            await customerSavedCouponRepository!.Update(customerSavedCoupon);

                                        }
                                    }
                                }
                                if (orderRequestDto.LoyalPointsToUse.HasValue && orderRequestDto.LoyalPointsToUse > 0)
                                {
                                    // Check if the user has enough points
                                    if (customerInfoDb.LoyaltyPoint >= orderRequestDto.LoyalPointsToUse)
                                    {
                                        // Calculate the discount (assuming 1 point = 1 currency unit)
                                        double loyaltyDiscount = Math.Min(orderRequestDto.LoyalPointsToUse.Value, money);
                                        money -= loyaltyDiscount;

                                        // Ensure the total doesn't go below zero
                                        money = Math.Max(0, money);

                                        // Update the customer's loyalty points
                                        customerInfoDb.LoyaltyPoint -= (int)loyaltyDiscount;

                                        // Create a new loyalty point history entry for the point usage
                                        var loyalPointUsageHistory = new LoyalPointsHistory
                                        {
                                            LoyalPointsHistoryId = Guid.NewGuid(),
                                            TransactionDate = utility!.GetCurrentDateTimeInTimeZone(),
                                            OrderId = orderDb.OrderId,
                                            PointChanged = -(int)loyaltyDiscount,
                                            NewBalance = customerInfoDb.LoyaltyPoint
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
                                    NewBalance = customerInfoDb!.LoyaltyPoint + (int)money / 100
                                };

                                await loyalPointsHistoryRepository!.Insert(newLoyalPointHistory);

                                orderDb.LoyalPointsHistoryId = newLoyalPointHistory.LoyalPointsHistoryId;

                                customerInfoDb.LoyaltyPoint = newLoyalPointHistory.NewBalance;
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
                            OrderDate = orderRequestDto.OrderDate,
                            Status = OrderStatus.Pending,
                            PaymentMethodId = orderRequestDto.PaymentMethodId,
                            IsDelivering = true,
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

                        await comboOrderDetailRepository.InsertRange(comboOrderDetails);
                        orderDb.TotalAmount = money;

                        if (orderRequestDto.CustomerId.HasValue)
                        {
                            var customerInfoDb = await customerInfoRepository!.GetByExpression(p => p.CustomerId == orderRequestDto.CustomerId, p => p.Account!);
                            var customerSavedCouponDb = await customerSavedCouponRepository!.GetAllDataByExpression(p => p.CustomerInfoId == orderRequestDto.CustomerId, 0, 0, null, false, p => p.Coupon!);

                            if (customerSavedCouponDb.Items!.Count < 0 && customerSavedCouponDb.Items != null)
                            {
                                if (orderRequestDto.CouponId.HasValue)
                                {
                                    var customerSavedCoupon = customerSavedCouponDb.Items.FirstOrDefault(c => c.CouponId == orderRequestDto.CouponId);
                                    if (customerSavedCoupon != null && customerSavedCoupon.IsUsedOrExpired == false)
                                    {
                                        var coupon = await couponRepository!.GetById(customerSavedCoupon.CouponId);
                                        if (coupon != null)
                                        {
                                            double discountMoney = money * (coupon.DiscountPercent / 100);
                                            money -= discountMoney;
                                        }

                                        money = Math.Max(0, money);

                                        orderDb.CustomerSavedCouponId = customerSavedCoupon.CustomerSavedCouponId;
                                        // Update the coupon usage
                                        customerSavedCoupon.IsUsedOrExpired = true;
                                        await customerSavedCouponRepository!.Update(customerSavedCoupon);

                                    }
                                }
                            }
                            if (orderRequestDto.LoyalPointsToUse.HasValue && orderRequestDto.LoyalPointsToUse > 0)
                            {
                                // Check if the user has enough points
                                if (customerInfoDb!.LoyaltyPoint >= orderRequestDto.LoyalPointsToUse)
                                {
                                    // Calculate the discount (assuming 1 point = 1 currency unit)
                                    double loyaltyDiscount = Math.Min(orderRequestDto.LoyalPointsToUse.Value, money);
                                    money -= loyaltyDiscount;

                                    // Ensure the total doesn't go below zero
                                    money = Math.Max(0, money);

                                    // Update the customer's loyalty points
                                    customerInfoDb.LoyaltyPoint -= (int)loyaltyDiscount;

                                    // Create a new loyalty point history entry for the point usage
                                    var loyalPointUsageHistory = new LoyalPointsHistory
                                    {
                                        LoyalPointsHistoryId = Guid.NewGuid(),
                                        TransactionDate = utility!.GetCurrentDateTimeInTimeZone(),
                                        OrderId = orderDb.OrderId,
                                        PointChanged = -(int)loyaltyDiscount,
                                        NewBalance = customerInfoDb.LoyaltyPoint
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
                                NewBalance = customerInfoDb!.LoyaltyPoint + (int)money / 100
                            };

                            await loyalPointsHistoryRepository!.Insert(newLoyalPointHistory);

                            orderDb.LoyalPointsHistoryId = newLoyalPointHistory.LoyalPointsHistoryId;

                            customerInfoDb.LoyaltyPoint = newLoyalPointHistory.NewBalance;
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
                var orderDb = await _repository.GetById(orderId);
                if (orderDb == null)
                {
                    result = BuildAppActionResultError(result, $"Không tìm thấy đơn hàng với id {orderId}");
                    return result;
                }

                var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                var orderDetailDb = await orderDetailRepository!.GetAllDataByExpression(o => o.OrderId == orderId, 0, 0, null, false, o => o.DishSizeDetail.Dish, o => o.Combo);
                var orderDetailReponseList = new List<OrderDetailResponse>();
                orderDetailDb.Items.ForEach(async o =>
                {
                    orderDetailReponseList.Add(new OrderDetailResponse
                    {
                        OrderDetail = o,
                        ComboOrderDetails = (await comboOrderDetailRepository.GetAllDataByExpression(c => c.OrderDetailId == o.OrderDetailId, 0, 0, null, false, c => c.DishCombo.DishSizeDetail.Dish)).Items
                    });
                });
                result.Result = new OrderReponse
                {
                    Order = orderDb,
                    OrderDetails = orderDetailReponseList
                };
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
    }
}
