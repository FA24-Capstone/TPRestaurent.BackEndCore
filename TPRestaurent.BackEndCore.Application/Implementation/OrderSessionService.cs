using AutoMapper;
using NPOI.POIFS.Crypt.Agile;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IHubServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;
using static TPRestaurent.BackEndCore.Common.DTO.Response.MapInfo;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class OrderSessionService : GenericBackendService , IOrderSessionService
    {
        private readonly IGenericRepository<OrderSession> _orderSessionRepository;
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private IHubServices.IHubServices _hubServices;

        public OrderSessionService(IGenericRepository<OrderSession> orderSessionRepository, IGenericRepository<Order> orderRepository, IUnitOfWork unitOfWork, IMapper mapper, IHubServices.IHubServices hubService, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _orderSessionRepository = orderSessionRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hubServices = hubService;
        }
        public async Task DeleteOrderSession()
        {
            try
            {
                var orderSessionDb = await _orderSessionRepository.GetAllDataByExpression(null, 0, 0, null, false, null);
                if (orderSessionDb!.Items!.Count > 0 && orderSessionDb.Items != null)
                {
                    await _orderSessionRepository.DeleteRange(orderSessionDb.Items);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex) 
            {
            }
            Task.CompletedTask.Wait();  
        }
        public async Task<AppActionResult> GetAllOrderSession(OrderSessionStatus? orderSessionStatus, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var orderRepository = Resolve<IGenericRepository<Order>>();
                var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                var existOrderDetailDb = await orderDetailRepository.GetAllDataByExpression(o => o.OrderSessionId.HasValue, 0, 0, null, false, null);
                var existOrderSessionId = existOrderDetailDb.Items.Select(o => o.OrderSessionId.Value);
                var orderSessionDb = await _orderSessionRepository.GetAllDataByExpression(null, 0, 0, null, false, null);
                var displayOrderSessionDb = await _orderSessionRepository.GetAllDataByExpression(s => ((orderSessionStatus.HasValue && s.OrderSessionStatusId == orderSessionStatus)
                                                                                      || (!orderSessionStatus.HasValue
                                                                                          && (s.OrderSessionStatusId != OrderSessionStatus.Completed
                                                                                              || s.OrderSessionStatusId != OrderSessionStatus.Cancelled
                                                                                              || s.OrderSessionStatusId != OrderSessionStatus.PreOrder)
                                                                                         )) && existOrderSessionId.Contains(s.OrderSessionId),
                                                                                    pageNumber, pageSize, s => s.OrderSessionTime, false, s => s.OrderSessionStatus!);
                var orderSessionResponseList = new List<OrderSessionResponse>();
                if (displayOrderSessionDb!.Items!.Count == 0 && displayOrderSessionDb.Items != null)
                {
                    return BuildAppActionResultError(result, $"Hiện tại không có phiên đặt bàn");
                }

                Dictionary<Guid, Order> orders = new Dictionary<Guid, Order>();

                foreach (var orderSession in displayOrderSessionDb.Items!)
                {
                    var orderSessionResponse = new OrderSessionResponse();
                    var orderDetailDb = await orderDetailRepository!.GetAllDataByExpression(p => p.OrderSessionId == orderSession.OrderSessionId, 0, 0, null, false, 
                                                                                                                                        o => o.DishSizeDetail!.Dish.DishItemType, 
                                                                                                                                        o => o.DishSizeDetail!.DishSize,
                                                                                                                                        o => o.OrderDetailStatus!,
                                                                                                                                        o => o.Combo!.Category
                                                                                                                                        );
                    if(orderDetailDb.Items.Count > 0)
                    {
                        if (orders.ContainsKey(orderDetailDb.Items[0].OrderId))
                        {
                            orderSessionResponse.Order = orders[orderDetailDb.Items[0].OrderId];
                        }
                        else
                        {
                            orderSessionResponse.Order = await _orderRepository.GetByExpression(o => o.OrderId == orderDetailDb.Items.FirstOrDefault().OrderId, o => o.OrderType, o => o.Status); 
                            orders.Add(orderSessionResponse.Order.OrderId, orderSessionResponse.Order);
                        }

                        if (orderSessionResponse.Order != null)
                        {
                            var tableDb = (await tableDetailRepository.GetAllDataByExpression(t => t.OrderId == orderSessionResponse.Order.OrderId, 0, 0, t => t.TableId, false, t => t.Table.Room, t => t.Table.TableSize));
                            if(tableDb.Items.Count() > 0)
                            {
                                orderSessionResponse.Table = tableDb.Items[0]?.Table;
                            }
                        }

                        orderSessionResponse.OrderSession = orderSession;
                        orderSessionResponse.OrderDetails = orderDetailDb.Items;

                        orderSessionResponseList.Add(orderSessionResponse);
                    }
                   
                }
                var data = new OrderSessionListReponseWithStatus
                {
                    Items = orderSessionResponseList,
                    TotalPages = displayOrderSessionDb.TotalPages,
                    PreOrderQuantity = orderSessionDb.Items.Count(o => o.OrderSessionStatusId == OrderSessionStatus.PreOrder),
                    ConfirmedQuantity = orderSessionDb.Items.Count(o => o.OrderSessionStatusId == OrderSessionStatus.Confirmed),
                    ProcessingQuantity = orderSessionDb.Items.Count(o => o.OrderSessionStatusId == OrderSessionStatus.Processing),
                    LateWarningQuantity = orderSessionDb.Items.Count(o => o.OrderSessionStatusId == OrderSessionStatus.LateWarning),
                    CompletedQuantity = orderSessionDb.Items.Count(o => o.OrderSessionStatusId == OrderSessionStatus.Completed),
                    CancelledQuantity = orderSessionDb.Items.Count(o => o.OrderSessionStatusId == OrderSessionStatus.Cancelled),
                };

                result.Result = data;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
        public async Task<AppActionResult> GetGroupedDish(DateTime?[]? groupedTime)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(o => (
                                                                                                groupedTime == null 
                                                                                                || (
                                                                                                    groupedTime != null 
                                                                                                    && (
                                                                                                        !groupedTime[0].HasValue
                                                                                                        || (
                                                                                                            groupedTime[0].HasValue
                                                                                                            && groupedTime[0] < o.OrderSession.OrderSessionTime
                                                                                                            )
                                                                                                        )
                                                                                                    && groupedTime[1] > o.OrderSession.OrderSessionTime
                                                                                                   )
                                                                                            )
                                                                                            && o.OrderSession.OrderSessionStatusId != OrderSessionStatus.Completed 
                                                                                            && o.OrderSession.OrderSessionStatusId != OrderSessionStatus.Cancelled
                                                                                            && o.OrderSession.OrderSessionStatusId != OrderSessionStatus.PreOrder
                                                                                            && (o.OrderDetailStatusId == OrderDetailStatus.Unchecked
                                                                                                || o.OrderDetailStatusId == OrderDetailStatus.Processing)
                                                                                            && (o.Order.OrderTypeId != OrderType.Delivery   
                                                                                                || o.Order.OrderTypeId == OrderType.Delivery && o.Order.StatusId == OrderStatus.Processing),
                                                                                            0, 0, null, false, 
                                                                                            o => o.OrderDetailStatus,
                                                                                            o => o.Order.Shipper, 
                                                                                            o => o.Order.OrderType,
                                                                                            o => o.Order.Status,
                                                                                            o => o.OrderSession.OrderSessionStatus,
                                                                                            o => o.OrderDetailStatus, 
                                                                                            o => o.DishSizeDetail.Dish.DishItemType, 
                                                                                            o => o.DishSizeDetail.DishSize,
                                                                                            o => o.Combo);
                if(orderDetailDb.Items.Count > 0)
                {
                    var data = new List<KitchenGroupedDishItemResponse>();
                    var dishQuantity = await GetDishQuantity(orderDetailDb.Items.Where(o => o.DishSizeDetailId.HasValue));
                    var extractComboQuantity = await GetExtractComboQuantity(orderDetailDb.Items.Where(o => o.ComboId.HasValue));

                    data.AddRange(dishQuantity);
                    data.AddRange(extractComboQuantity);
                    data = await RefineGroupDishData(data);

                    var groupedDish = new KitchenGroupedDishResponse();
                    groupedDish.OrderDetailIds = orderDetailDb.Items.DistinctBy(o => o.OrderDetailId).Select(o => o.OrderDetailId).ToList();
                    foreach (var dish in data)
                    {
                        if(dish.Dish.Total.Count() == 1 
                            && dish.Dish.Total.Sum(d => d.UncheckedQuantity) + dish.Dish.Total.Sum(d => d.ProcessingQuantity) == 1)
                        {
                            groupedDish.SingleOrderDishes.Add(dish);
                        } else
                        {
                            groupedDish.MutualOrderDishes.Add(dish);
                        }
                    }

                    result.Result = groupedDish;
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
        private async Task<List<KitchenGroupedDishItemResponse>> RefineGroupDishData(List<KitchenGroupedDishItemResponse> data)
        {
            try
            {
                Dictionary<string, Domain.Models.EnumModels.DishSize> sizes = new Dictionary<string, Domain.Models.EnumModels.DishSize>();
                Dictionary<Guid, DishQuantityResponse> dishes = new Dictionary<Guid, DishQuantityResponse>();

                data.ForEach(d =>
                {
                    if (!dishes.ContainsKey(d.Dish.DishId))
                    {
                        dishes.Add(d.Dish.DishId, d.Dish);
                    }
                });

                data.ForEach(d => d.Dish.Total.ForEach(t =>
                {
                    if (!sizes.ContainsKey(t.DishSize.Name))
                    {
                        sizes.Add(t.DishSize.Name, t.DishSize);
                    }
                }));
                return data
     .GroupBy(d => d.Dish.DishId) // Group by Dish
     .Select(group =>
     {
         var kitchenGroupedDishItemResponse = new KitchenGroupedDishItemResponse();
         kitchenGroupedDishItemResponse.Dish = dishes[group.Key];

         kitchenGroupedDishItemResponse.Dish.Total = group
             .SelectMany(d => d.Dish.Total)
             .GroupBy(q => q.DishSize.Name) // Group by DishSize
             .Select(g => new QuantityBySize
             {
                 DishSize = sizes[g.Key],
                 UncheckedQuantity = g.Sum(q => q.UncheckedQuantity), // Sum quantities for each size
                 ProcessingQuantity = g.Sum(q => q.ProcessingQuantity) // Sum quantities for each size
             })
             .OrderBy(g => g.DishSize.Id)
             .ToList();

             kitchenGroupedDishItemResponse.UncheckedDishFromTableOrders = group
              .SelectMany(d => d.UncheckedDishFromTableOrders)
              .ToList();

             kitchenGroupedDishItemResponse.ProcessingDishFromTableOrders = group
             .SelectMany(d => d.ProcessingDishFromTableOrders)
             .ToList();
         return kitchenGroupedDishItemResponse;
     })
     .ToList();
            }
            catch (Exception ex)
            {
                data = new List<KitchenGroupedDishItemResponse>();
            }
            return data;
        }
        private async Task<List<KitchenGroupedDishItemResponse>> GetExtractComboQuantity(IEnumerable<OrderDetail> orderDetails)
        {
            List<KitchenGroupedDishItemResponse> result = new List<KitchenGroupedDishItemResponse>();
            try
            {
                var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                foreach(var orderDetail in orderDetails)
                {
                    var dishDetails = await comboOrderDetailRepository.GetAllDataByExpression(c => c.OrderDetailId == orderDetail.OrderDetailId, 0, 0, null, false, 
                                                                                              c => c.DishCombo.DishSizeDetail.Dish.DishItemType,
                                                                                              c => c.DishCombo.DishSizeDetail.DishSize,
                                                                                              c => c.OrderDetail.Order.OrderType,
                                                                                              c => c.OrderDetail.Order.Shipper,
                                                                                              c => c.OrderDetail.Order.Status,
                                                                                              c => c.OrderDetail.OrderDetailStatus,
                                                                                              c => c.OrderDetail.OrderSession.OrderSessionStatus);

                    var dishDictionary = dishDetails.Items.GroupBy(d => d.DishCombo.DishSizeDetail.DishId).ToDictionary(d => d.Key, d => d.ToList());
                    foreach(var dish in dishDictionary)
                    {
                        var groupedDishItem = new KitchenGroupedDishItemResponse();
                        groupedDishItem.Dish = _mapper.Map<DishQuantityResponse>(dish.Value.FirstOrDefault().DishCombo.DishSizeDetail.Dish);
                        var totalDictionary = dish.Value.GroupBy(d => d.DishCombo.DishSizeDetail.DishSize).ToDictionary(t => t.Key, t => t.ToList()).ToList();
                        foreach (var sizeTotal in totalDictionary)
                        {
                            groupedDishItem.Dish.Total.Add(new QuantityBySize
                            {
                                DishSize = sizeTotal.Key,
                                UncheckedQuantity = sizeTotal.Value.Where(d => d.OrderDetail.OrderDetailStatusId == OrderDetailStatus.Unchecked).Sum(d => d.OrderDetail.Quantity * d.DishCombo.Quantity),
                                ProcessingQuantity = sizeTotal.Value.Where(d => d.OrderDetail.OrderDetailStatusId == OrderDetailStatus.Processing).Sum(d => d.OrderDetail.Quantity * d.DishCombo.Quantity)
                            });
                        }
                        var dishData = await GetListDishFromTableOrder(dish.Value);
                        groupedDishItem.UncheckedDishFromTableOrders = dishData.Where(d => d.OrderDetail.OrderDetailStatusId == OrderDetailStatus.Unchecked).ToList();
                        groupedDishItem.ProcessingDishFromTableOrders = dishData.Where(d => d.OrderDetail.OrderDetailStatusId == OrderDetailStatus.Processing).ToList();
                        result.Add(groupedDishItem);
                    }
                }
            }
            catch (Exception ex)
            {
                result = new List<KitchenGroupedDishItemResponse>();
            }
            return result;
        }
        private async Task<List<KitchenGroupedDishItemResponse>> GetDishQuantity(IEnumerable<OrderDetail> orderDetails)
        {
            List<KitchenGroupedDishItemResponse> result = new List<KitchenGroupedDishItemResponse>();
            try
            {
                var dishDictionary = orderDetails.GroupBy(d => d.DishSizeDetail.DishId).ToDictionary(d => d.Key, d => d.ToList());
                foreach (var dish in dishDictionary)
                {
                    var groupedDishItem = new KitchenGroupedDishItemResponse();

                    groupedDishItem.Dish = _mapper.Map<DishQuantityResponse>(dish.Value.FirstOrDefault().DishSizeDetail.Dish);
                    var totalDictionary = dish.Value.GroupBy(d => d.DishSizeDetail.DishSize).ToDictionary(t => t.Key, t => t.ToList()).ToList();
                    foreach(var sizeTotal in totalDictionary)
                    {
                        groupedDishItem.Dish.Total.Add(new QuantityBySize
                        {
                            DishSize = sizeTotal.Key,
                            UncheckedQuantity = sizeTotal.Value.Where(d => d.OrderDetailStatusId == OrderDetailStatus.Unchecked).Sum(d => d.Quantity),
                            ProcessingQuantity = sizeTotal.Value.Where(d => d.OrderDetailStatusId == OrderDetailStatus.Processing).Sum(d => d.Quantity),
                        });
                    }

                    var listDish = await GetListDishFromTableOrder(dish.Value);
                    groupedDishItem.UncheckedDishFromTableOrders = listDish.Where(o =>o.OrderDetail.OrderDetailStatusId == OrderDetailStatus.Unchecked).ToList();
                    groupedDishItem.ProcessingDishFromTableOrders = listDish.Where(o => o.OrderDetail.OrderDetailStatusId == OrderDetailStatus.Processing).ToList();

                    result.Add(groupedDishItem);
                }
            }
            catch (Exception ex)
            {
                result = new List<KitchenGroupedDishItemResponse>();
            }
            return result;
        }
        private async Task<List<DishFromTableOrder>> GetListDishFromTableOrder(List<OrderDetail> orderDetails)
        {
            List<DishFromTableOrder> result = new List<DishFromTableOrder>();
            try
            {
                var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                foreach (var orderDetail in orderDetails)
                {
                    var dishFromTableOrder = new DishFromTableOrder();
                    dishFromTableOrder.OrderDetail = orderDetail;
                    dishFromTableOrder.Order = orderDetail.Order;
                    dishFromTableOrder.OrderSession = orderDetail.OrderSession;
                    dishFromTableOrder.Table = (await tableDetailRepository.GetAllDataByExpression(t => t.OrderId == orderDetail.OrderDetailId, 0, 0, t => t.TableId, false, t => t.Table)).Items
                                               .FirstOrDefault()?.Table;
                    dishFromTableOrder.Quantity = new OrderDetailQuantityBySize
                    {
                        Quantity = orderDetail.Quantity,
                        DishSize = orderDetail.DishSizeDetail.DishSize
                    };
                    result.Add(dishFromTableOrder);
                }
            }
            catch (Exception ex)
            {
                result = new List<DishFromTableOrder>();
            }
            return result;
        }
        private async Task<List<DishFromTableOrder>> GetListDishFromTableOrder(List<ComboOrderDetail> comboOrderDetails)
        {
            List<DishFromTableOrder> result = new List<DishFromTableOrder>();
            try
            {
                var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                foreach (var comboOrderDetail in comboOrderDetails)
                {
                    var dishFromTableOrder = new DishFromTableOrder();
                    dishFromTableOrder.OrderDetail = comboOrderDetail.OrderDetail;
                    dishFromTableOrder.Order = comboOrderDetail.OrderDetail.Order;
                    dishFromTableOrder.OrderSession = comboOrderDetail.OrderDetail.OrderSession;
                    dishFromTableOrder.Table = (await tableDetailRepository.GetAllDataByExpression(t => t.OrderId == comboOrderDetail.OrderDetail.OrderId, 0, 0, t => t.TableId, false, t => t.Table)).Items
                                               .FirstOrDefault()?.Table;
                    dishFromTableOrder.Quantity = new OrderDetailQuantityBySize
                    {
                        Quantity = comboOrderDetail.DishCombo.Quantity,
                        DishSize = comboOrderDetail.DishCombo.DishSizeDetail.DishSize
                    };
                    result.Add(dishFromTableOrder);
                }
            }
            catch (Exception ex)
            {
                result = new List<DishFromTableOrder>();
            }
            return result;
        }
        public async Task<AppActionResult> GetOrderSessionById(Guid orderSessionId)
        {
            var result = new AppActionResult(); 
            var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
            var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
            var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
            try
            {
                var orderSessionDb = await _orderSessionRepository.GetByExpression(p => p.OrderSessionId == orderSessionId, p => p.OrderSessionStatus!);
                var orderSessionResponse = new OrderSessionDetailResponse();
                if (orderSessionDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy phiên đặt bàn với id {orderSessionId}");
                }
                var orderDetailDb = await orderDetailRepository!.GetAllDataByExpression(p => p.OrderSessionId == orderSessionDb.OrderSessionId, 0, 0, null, false, 
                                                                                                                        o => o.Combo!.Category, 
                                                                                                                        o => o.DishSizeDetail.Dish.DishItemType,
                                                                                                                        o => o.DishSizeDetail.DishSize,
                                                                                                                        o => o.OrderDetailStatus
                                                                                                                        );
                var orderDetailReponseList = new List<OrderDetailResponse>();
                foreach (var o in orderDetailDb!.Items!)
                {
                    var comboOrderDetailsDb = await comboOrderDetailRepository!.GetAllDataByExpression(
                        c => c.OrderDetailId == o.OrderDetailId,
                        0,
                        0,
                        null,
                        false,
                        c => c.DishCombo!.DishSizeDetail!.Dish!.DishItemType,
                        c => c.DishCombo!.DishSizeDetail!.DishSize
                    );
                    orderDetailReponseList.Add(new OrderDetailResponse
                    {
                        OrderDetail = o,
                        ComboOrderDetails = comboOrderDetailsDb.Items!
                    });
                }

                orderSessionResponse.Order = await _orderRepository.GetByExpression(o => o.OrderId == orderDetailDb.Items.FirstOrDefault().OrderId, o => o.OrderType, o => o.Status);

                if (orderSessionResponse.Order != null)
                {
                    var tableDb = (await tableDetailRepository.GetAllDataByExpression(t => t.OrderId == orderSessionResponse.Order.OrderId, 0, 0, t => t.TableId, false, t => t.Table.Room, t => t.Table.TableSize));
                    if (tableDb.Items.Count > 0)
                    {
                        orderSessionResponse.Table = tableDb.Items[0]?.Table;
                    }
                }

                orderSessionResponse.OrderSession = orderSessionDb;
                orderSessionResponse.OrderDetails = orderDetailReponseList; 
                result.Result = orderSessionResponse;   
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
        public async Task<AppActionResult> UpdateOrderSessionStatus(Guid orderSessionId, OrderSessionStatus orderSessionStatus, bool sendSignalR)
        {
            var result = new AppActionResult(); 
            try
            {
                var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                var orderSessionDb = await _orderSessionRepository.GetById(orderSessionId);
                if (orderSessionId == null)
                {
                    return BuildAppActionResultError(result, $"Phiên đặt món với id {orderSessionId} không tồn tại");
                }
                orderSessionDb.OrderSessionStatusId = orderSessionStatus;
                await _orderSessionRepository.Update(orderSessionDb);

                await _unitOfWork.SaveChangesAsync();

                if (sendSignalR)
                {
                    await _hubServices.SendAsync(SD.SignalMessages.LOAD_ORDER_SESIONS);
                    await _hubServices.SendAsync(SD.SignalMessages.LOAD_GROUPED_DISHES);
                }

                if (orderSessionStatus == OrderSessionStatus.Completed)
                {
                    var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(o => o.OrderSessionId == orderSessionId, 0, 0, null, false, o => o.Order);
                    if (orderDetailDb.Items.Count > 0)
                    {
                        var orderService = Resolve<IOrderService>();
                        var orderDb = orderDetailDb.Items.FirstOrDefault().Order;
                        if (orderDb.StatusId == OrderStatus.Processing)
                        {
                            await orderService.ChangeOrderStatus(orderDb.OrderId, true);
                            if (sendSignalR)
                            {
                                await _hubServices.SendAsync(SD.SignalMessages.LOAD_ORDER);
                            }
                        }

                        var orderDetailToCompleteId = orderDetailDb.Items.Where(o => o.OrderDetailStatusId != OrderDetailStatus.Reserved && o.OrderDetailStatusId == OrderDetailStatus.Cancelled).Select(o => o.OrderDetailId).ToList();
                        await orderService.UpdateOrderDetailStatusForce(orderDetailToCompleteId, OrderDetailStatus.ReadyToServe);
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
}
