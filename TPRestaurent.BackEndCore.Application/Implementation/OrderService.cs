using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class OrderService : GenericBackendService, IOrderService
    {
        private readonly IGenericRepository<Order> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public OrderService(IGenericRepository<Order> repository, IUnitOfWork unitOfWork, IMapper mapper, IServiceProvider service): base(service)
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
                var orderDetail = _mapper.Map<List<OrderDetail>>(dto.OrderDetailsDtos);
                orderDetail.ForEach(async o =>
                {
                    o.OrderId = dto.OrderId;
                    if (o.ComboId.HasValue)
                    {
                        o.Price = (await comboRepository.GetById(o.ComboId)).Price;
                    } else
                    {
                        o.Price = (await dishRepository.GetById(o.DishSizeDetailId)).Price;
                    }
                    o.OrderBatch = orderBatch;
                    orderDb.TotalAmount += o.Price * o.Quantity;
                });
                await _repository.Update(orderDb);
                await orderDetailRepository.InsertRange(orderDetail);
                await _unitOfWork.SaveChangesAsync();
                //AddOrderMessageToChef
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public Task<AppActionResult> ChangeOrderStatus(string orderId, bool? isDelivering)
        {
            throw new NotImplementedException();
        }

        public Task<AppActionResult> CreateOrder(OrderRequestDto orderRequestDto)
        {
            throw new NotImplementedException();
        }

        public Task<AppActionResult> DeleteOrderDetail(Guid orderDetailId)
        {
            throw new NotImplementedException();
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

        public async Task<AppActionResult> GetOrderById(Guid orderId)
        {
            var result = new AppActionResult();
            try
            {
                var orderDb = await _repository.GetByExpression(p => p.OrderId == orderId);
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
                var orderDetailDb = await orderDetailRepository!.GetAllDataByExpression(o => o.OrderId == orderId, 0, 0, null, false, o => o.DishSizeDetail.Dish, o => o.Combo);
                result.Result = new OrderReponse
                {
                    Order = orderDb,
                    OrderDetails = orderDetailDb.Items!
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
