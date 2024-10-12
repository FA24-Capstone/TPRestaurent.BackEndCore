using NPOI.POIFS.Crypt.Agile;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;
using static TPRestaurent.BackEndCore.Common.DTO.Response.MapInfo;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class OrderSessionService : GenericBackendService , IOrderSessionService
    {
        private readonly IGenericRepository<OrderSession> _orderSessionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OrderSessionService(IGenericRepository<OrderSession> orderSessionRepository, IUnitOfWork unitOfWork, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _orderSessionRepository = orderSessionRepository;
            _unitOfWork = unitOfWork;
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
                var orderSessionDb = await _orderSessionRepository.GetAllDataByExpression(s => (orderSessionStatus.HasValue && s.OrderSessionStatusId == orderSessionStatus)
                                                                                      || (!orderSessionStatus.HasValue
                                                                                          && (s.OrderSessionStatusId != OrderSessionStatus.Completed
                                                                                              || s.OrderSessionStatusId != OrderSessionStatus.Cancelled
                                                                                              || s.OrderSessionStatusId != OrderSessionStatus.PreOrder)
                                                                                         ),
                                                                                    pageNumber, pageSize, s => s.OrderSessionTime, false, s => s.OrderSessionStatus!);
                var orderSessionResponseList = new List<OrderSessionResponse>();
                if (orderSessionDb!.Items!.Count == 0 && orderSessionDb.Items != null)
                {
                    return BuildAppActionResultError(result, $"Hiện tại không có phiên đặt bàn");
                }

                Dictionary<Guid, Order> orders = new Dictionary<Guid, Order>();

                foreach (var orderSession in orderSessionDb.Items!)
                {
                    var orderSessionResponse = new OrderSessionResponse();
                    var orderDetailDb = await orderDetailRepository!.GetAllDataByExpression(p => p.OrderSessionId == orderSession.OrderSessionId, 0, 0, null, false, o => o.Combo!);
                    if(orderDetailDb.Items.Count > 0)
                    {
                        if (orders.ContainsKey(orderDetailDb.Items[0].OrderId))
                        {
                            orderSessionResponse.Order = orders[orderDetailDb.Items[0].OrderId];
                        }
                        else
                        {
                            orderSessionResponse.Order = (await orderDetailRepository.GetByExpression(o => o.OrderDetailId == orderDetailDb.Items[0].OrderDetailId, o => o.Order)).Order;
                            orders.Add(orderSessionResponse.Order.OrderId, orderSessionResponse.Order);
                        }
                    }
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
                    orderSessionResponse.OrderSession = orderSession;
                    orderSessionResponse.OrderDetails = orderDetailReponseList;

                    orderSessionResponseList.Add(orderSessionResponse);
                }
                result.Result = orderSessionResponseList;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetGroupedDish()
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(o => o.OrderSession.OrderSessionStatusId != OrderSessionStatus.Completed 
                                                                                            && o.OrderSession.OrderSessionStatusId != OrderSessionStatus.Cancelled, 
                                                                                            0, 0, null, false, 
                                                                                            o => o.OrderDetailStatus,
                                                                                            o => o.Order.Shipper, 
                                                                                            o => o.Order.OrderType,
                                                                                            o => o.Order.Status,
                                                                                            o => o.OrderSession.OrderSessionStatus,
                                                                                            o => o.OrderDetailStatus, 
                                                                                            o => o.DishSizeDetail.Dish, 
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
                    foreach (var dish in data)
                    {
                        if(dish.Total.Count() == 1 && dish.Total.FirstOrDefault().Quantity == 1)
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
                Dictionary<Guid, Dish> dishes = new Dictionary<Guid, Dish>();

                data.ForEach(d =>
                {
                    if (!dishes.ContainsKey(d.Dish.DishId))
                    {
                        dishes.Add(d.Dish.DishId, d.Dish);
                    }
                });

                data.ForEach(d => d.Total.ForEach(t =>
                {
                    if (!sizes.ContainsKey(t.DishSize.Name))
                    {
                        sizes.Add(t.DishSize.Name, t.DishSize);
                    }
                }));
                return data
            .GroupBy(d => d.Dish.DishId) // Group by Dish
            .Select(group => new KitchenGroupedDishItemResponse
            {
                Dish = dishes[group.Key],
                Total = group
                    .SelectMany(d => d.Total)
                    .GroupBy(q => q.DishSize.Name) // Group by DishSize
                    .Select(g => new QuantityBySize
                    {
                        DishSize = sizes[g.Key],
                        Quantity = g.Sum(q => q.Quantity) // Sum quantities for each size
                    }).OrderBy(g => g.DishSize.Id)
                    .ToList(),
                DishFromTableOrders = group
                    .SelectMany(d => d.DishFromTableOrders)
                    .ToList() // Append all DishFromTableOrders
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
                                                                                              c => c.DishCombo.DishSizeDetail.Dish,
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
                        groupedDishItem.Dish = dish.Value.FirstOrDefault().DishCombo.DishSizeDetail.Dish;
                        var totalDictionary = dish.Value.GroupBy(d => d.DishCombo.DishSizeDetail.DishSize).ToDictionary(t => t.Key, t => t.Count()).ToList();
                        foreach (var sizeTotal in totalDictionary)
                        {
                            groupedDishItem.Total.Add(new QuantityBySize
                            {
                                DishSize = sizeTotal.Key,
                                Quantity = sizeTotal.Value
                            });
                        }
                        groupedDishItem.DishFromTableOrders.AddRange(await GetListDishFromTableOrder(dish.Value));
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

                    groupedDishItem.Dish = dish.Value.FirstOrDefault().DishSizeDetail.Dish;
                    var totalDictionary = dish.Value.GroupBy(d => d.DishSizeDetail.DishSize).ToDictionary(t => t.Key, t => t.Count()).ToList();
                    foreach(var sizeTotal in totalDictionary)
                    {
                        groupedDishItem.Total.Add(new QuantityBySize
                        {
                            DishSize = sizeTotal.Key,
                            Quantity = sizeTotal.Value
                        });
                    }

                    groupedDishItem.DishFromTableOrders = await GetListDishFromTableOrder(dish.Value);

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
                    dishFromTableOrder.Quantity = new QuantityBySize
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
                    dishFromTableOrder.Quantity = new QuantityBySize
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
            try
            {
                var orderSessionDb = await _orderSessionRepository.GetByExpression(p => p.OrderSessionId == orderSessionId, p => p.OrderSessionStatus!);
                var orderSessionResponse = new OrderSessionResponse();
                if (orderSessionDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy phiên đặt bàn với id {orderSessionId}");
                }
                var orderDetailDb = await orderDetailRepository!.GetAllDataByExpression(p => p.OrderSessionId == orderSessionDb.OrderSessionId, 0, 0, null, false, o => o.Combo!);
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


        public async Task<AppActionResult> UpdateOrderSessionStatus(Guid orderSessionId, OrderSessionStatus orderSessionStatus)
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

                if(orderSessionStatus == OrderSessionStatus.Completed)
                {
                    var orderDetailDb = await orderDetailRepository.GetAllDataByExpression(o => o.OrderSessionId == orderSessionId, 0, 0, null, false, o => o.Order);
                    if (orderDetailDb.Items.Count > 0)
                    {
                        var orderDb = orderDetailDb.Items.FirstOrDefault().Order;
                        if (orderDb.StatusId == OrderStatus.Processing)
                        {
                            var orderService = Resolve<IOrderService>();
                            await orderService.ChangeOrderStatus(orderDb.OrderId, true);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
    }
}
