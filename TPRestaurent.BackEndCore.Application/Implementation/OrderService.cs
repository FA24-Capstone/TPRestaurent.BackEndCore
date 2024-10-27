using Castle.DynamicProxy.Generators;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Transactions;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;
using Utility = TPRestaurent.BackEndCore.Common.Utils.Utility;

namespace TPRestaurent.BackEndCore.Application.Implementation;

public class NotificationMessageService : GenericBackendService, INotificationMessageService
{
    private IGenericRepository<NotificationMessage> _repository;
    private IUnitOfWork _unitOfWork;
    public NotificationMessageService(IServiceProvider serviceProvider, IGenericRepository<NotificationMessage> repository, IUnitOfWork unitOfWork) : base(serviceProvider)
    {
        _unitOfWork = unitOfWork;
        _repository = repository;
    }

    public async Task<AppActionResult> GetNotificationMessageByAccountId(string accountId)
    {
        AppActionResult result = new AppActionResult();
        try
        {
            var notificationDb = await _repository.GetAllDataByExpression(n => n.AccountId.Equals(accountId), 0, 0, n => n.NotifyTime, false, null);
            result.Result = notificationDb;
        }
        catch (Exception ex)
        {
            result = BuildAppActionResultError(result, ex.Message);
        }
        return result;
    }

    public async Task<AppActionResult> GetNotificationMessageById(Guid notifiId)
    {
        var result = new AppActionResult();
        try
        {
            var notificationMessageDb = await _repository.GetByExpression(p => p.NotificationId == notifiId);
            if (notificationMessageDb == null)
            {
                return BuildAppActionResultError(result, $"Không tim thấy thông báo");
            }

            result.Result = notificationMessageDb;
        }
        catch (Exception ex)
        {
            result = BuildAppActionResultError(result, $"Có lỗi xảy ra khi sử dụng API với GoongMap {ex.Message} ");
        }
        return result;
    }

        private async Task ApplyLoyalTyPoint(Order order)
        {
            try
            {
                var loyaltyPointRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var accountDb = order.Account;
                var loyaltyPointDb = await loyaltyPointRepository.GetAllDataByExpression(l => l.OrderId == order.OrderId, 0, 0, l => l.PointChanged, true, null);
                foreach(var loyaltyPoint in loyaltyPointDb.Items)
                {
                    accountDb.LoyaltyPoint += loyaltyPoint.PointChanged;
                    loyaltyPoint.NewBalance = accountDb.LoyaltyPoint;
                }
                await accountRepository.Update(accountDb);
                await loyaltyPointRepository.UpdateRange(loyaltyPointDb.Items);
                await _unitOfWork.SaveChangesAsync();

            } catch(Exception ex)
            {
                
            }
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
                    var notificationService = Resolve<INotificationMessageService>();
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

                var accountDb = await accountRepository!.GetById(accountId);
                if (accountDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy tài khoản với id {accountId}");
                }

                        order.AccountId = orderRequestDto.CustomerId.ToString();
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
                    else if (orderRequestDto.OrderType == OrderType.Delivery)
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
                    else
                    {
                        return BuildAppActionResultError(result, $"Thiếu loại đơn hàng (orderType) trong yêu cầu tạo");
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

                                await notificationService!.SendNotificationToRoleAsync(SD.RoleName.ROLE_CHEF, messageBody.ToString());

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
        }
        return result;
    }

    public async Task<AppActionResult> SendNotificationToRoleAsync(string roleName, string message)
    {
        var result = new AppActionResult();
        var utility = Resolve<Utility>();
        var roleRepository = Resolve<IGenericRepository<IdentityRole>>();
        var fireBaseService = Resolve<IFirebaseService>();
        var userRoleRepository = Resolve<IGenericRepository<IdentityUserRole<string>>>();
        var tokenRepository = Resolve<IGenericRepository<Token>>();
        var notificationMessageRepository = Resolve<IGenericRepository<NotificationMessage>>();
        var tokenList = new List<string>();
        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            try
            {
                var roleDb = await roleRepository!.GetByExpression(p => p.Name == roleName);
                if (roleDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy vai trò {roleName}");
                }
                var userRoleDb = await userRoleRepository!.GetAllDataByExpression(p => p.RoleId == roleDb!.Id, 0, 0, null, false, null);
                if (userRoleDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy danh sách user với od {roleDb.Id}");
                }
                foreach (var user in userRoleDb!.Items!)
                {
                    var tokenDb = await tokenRepository!.GetAllDataByExpression(p => p.AccountId == user.UserId, 0, 0, null, false, p => p.Account);
                    foreach (var token in tokenDb.Items)
                    {
                        if (token.DeviceToken != null)
                        {
                            tokenList.Add(token.DeviceToken);
                        }
                    }

                    var notificationList = new List<NotificationMessage>();
                    var currentTime = utility.GetCurrentDateTimeInTimeZone();
                    var notification = new NotificationMessage
                    {
                        NotificationId = Guid.NewGuid(),
                        NotificationName = "Nhà hàng có thông báo mới",
                        Messages = message,
                        NotifyTime = currentTime,
                        AccountId = user.UserId,
                    };
                    notificationList.Add(notification);

                    await notificationMessageRepository!.InsertRange(notificationList);
                    if (tokenList.Count() > 0)
                    {
                        await fireBaseService!.SendMulticastAsync(tokenList, "Nhà hàng có một thông báo mới", message, result);
                    }
                }

                if (!BuildAppActionResultIsError(result))
                {
                    await _unitOfWork.SaveChangesAsync();
                }
                scope.Complete();

            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
        }
        return result;
    }
}