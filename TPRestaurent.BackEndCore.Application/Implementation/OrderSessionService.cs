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
                                                                                            o => o.OrderSession, 
                                                                                            o => o.OrderDetailStatus, 
                                                                                            o => o.DishSizeDetail.Dish, 
                                                                                            o => o.Combo);
                if(orderDetailDb.Items.Count > 0)
                {
                    var data = new KitchenGroupedDishResponse();
                    var groupedOrder = orderDetailDb.Items.GroupBy(o => {
                        if (o.DishSizeDetailId.HasValue)
                        {
                            return o.DishSizeDetail.DishId!;
                        }
                        return o.ComboId!;
                    }).ToDictionary(o => o.Key, o => o.ToList());
                    foreach (var item in groupedOrder)
                    {
                        if (item.Value[0].DishSizeDetailId.HasValue)
                        {
                            var total = item.Value.Count;
                            var orderDetailResponse = new List<OrderDetailResponse>();
                            foreach (var orderDetail in item.Value)
                            {
                                orderDetailResponse.Add(new OrderDetailResponse
                                {
                                    OrderDetail = orderDetail
                                });
                            }

                            if (total > 1)
                            {
                                data.MutualOrderDishes.Add(new KitchenGroupedDishItemResponse
                                {
                                    Total = total,
                                    orderDetailResponses = orderDetailResponse
                                });

                            }
                            else
                            {
                                data.SingleOrderDishes.Add(new KitchenGroupedDishItemResponse
                                {
                                    Total = total,
                                    orderDetailResponses = orderDetailResponse
                                });
                            }
                        }
                        else if (item.Value[0].ComboId.HasValue)
                        {
                            var total = item.Value.Count;
                            var orderDetailResponse = new List<OrderDetailResponse>();
                            foreach (var orderDetail in item!.Value)
                            {
                                var comboOrderDetailsDb = await comboOrderDetailRepository!.GetAllDataByExpression(
                                    c => c.OrderDetailId == orderDetail.OrderDetailId,
                                    0,
                                    0,
                                    null,
                                    false,
                                    c => c.DishCombo!.DishSizeDetail!.Dish!
                                );
                                orderDetailResponse.Add(new OrderDetailResponse
                                {
                                    OrderDetail = orderDetail,
                                    ComboOrderDetails = comboOrderDetailsDb.Items
                                });
                            }
                            if (total > 1)
                            {
                                data.MutualOrderDishes.Add(new KitchenGroupedDishItemResponse
                                {
                                    Total = total,
                                    orderDetailResponses = orderDetailResponse
                                });
                            }
                            else
                            {
                                data.SingleOrderDishes.Add(new KitchenGroupedDishItemResponse
                                {
                                    Total = total,
                                    orderDetailResponses = orderDetailResponse
                                });
                            }
                        }
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
                var orderSessionDb = await _orderSessionRepository.GetById(orderSessionId);
                if (orderSessionId == null)
                {
                    return BuildAppActionResultError(result, $"Phiên đặt món với id {orderSessionId} không tồn tại");
                }
                orderSessionDb.OrderSessionStatusId = orderSessionStatus;
                await _orderSessionRepository.Update(orderSessionDb);
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
