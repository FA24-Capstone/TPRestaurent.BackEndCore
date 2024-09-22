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

        public async Task<AppActionResult> GetAllOrderSession(DateTime? time, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
            var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
            try
            {
                if (time.HasValue && time != null)
                {
                    var orderSessionDb = await _orderSessionRepository.GetAllDataByExpression(p => p.OrderSessionTime <= time, pageNumber, pageSize, p => p.OrderSessionTime, false, p => p.OrderSessionStatus!);
                    var orderSessionResponseList = new List<OrderSessionResponse>();
                    if (orderSessionDb!.Items!.Count > 0 && orderSessionDb.Items != null)
                    {
                        return BuildAppActionResultError(result, $"Hiện tại không có phiên đặt bàn nào trong");
                    }

                    foreach (var orderSession in orderSessionDb.Items!)
                    {
                        var orderSessionResponse = new OrderSessionResponse();
                        var orderDetailDb = await orderDetailRepository!.GetAllDataByExpression(p => p.OrderSessionId == orderSession.OrderSessionId, 0, 0, null, false, o => o.Combo!);
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
                else
                {
                    var orderSessionDb = await _orderSessionRepository.GetAllDataByExpression(null, pageNumber, pageSize, p => p.OrderSessionTime, false, p => p.OrderSessionStatus!);
                    var orderSessionResponseList = new List<OrderSessionResponse>();
                    if (orderSessionDb!.Items!.Count > 0 && orderSessionDb.Items != null)
                    {
                        return BuildAppActionResultError(result, $"Hiện tại không có phiên đặt bàn nào trong");
                    }
                    foreach (var orderSession in orderSessionDb.Items!)
                    {
                        var orderSessionResponse = new OrderSessionResponse();
                        var orderDetailDb = await orderDetailRepository!.GetAllDataByExpression(p => p.OrderSessionId == orderSession.OrderSessionId, 0, 0, null, false, o => o.Combo!);
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
                    result = BuildAppActionResultError(result, $"Không tìm thấy phiên đặt bàn với id {orderSessionId}");
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

        public Task<AppActionResult> UpdateOrderSession()
        {
            throw new NotImplementedException();
        }

        public Task<AppActionResult> UpdateOrderSessionStatus(Guid orderSessionId, OrderSessionStatus orderSessionStatus)
        {
            throw new NotImplementedException();
        }
    }
}
