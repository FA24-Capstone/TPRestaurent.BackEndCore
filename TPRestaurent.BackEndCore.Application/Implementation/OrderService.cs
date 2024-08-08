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

        public async Task<AppActionResult> CreateOrder(OrderRequestDto orderRequestDto)
        {
            var result = new AppActionResult();
            try
            {
                var accountRepository = Resolve<IGenericRepository<Account>>();
                var customerInfoRepository = Resolve<IGenericRepository<CustomerInfo>>();
                var reservationRepository = Resolve<IGenericRepository<Reservation>>();
                var reservationDishRepository = Resolve<IGenericRepository<ReservationDish>>();
                var loyalPointsHistoryRepository = Resolve<IGenericRepository<LoyalPointsHistory>>();
                var customerSavedCouponRepository = Resolve<IGenericRepository<CustomerSavedCoupon>>();
                var dishSizeDetailRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                var orderDetailRepository = Resolve<IGenericRepository<OrderDetail>>();
                var comboRepository = Resolve<IGenericRepository<Combo>>();

                if (orderRequestDto.isDelivering == false)
                {
                    var reservationDb = await reservationRepository!.GetById(orderRequestDto.ReservationId!);
                    if (reservationDb != null)
                    {
                        var customerInfoDb = await customerInfoRepository!.GetByExpression(p => p.CustomerId == orderRequestDto.CustomerId, p => p.Account!);
                        var reservationDishDb = await reservationDishRepository!.GetAllDataByExpression(p => p.ReservationId == orderRequestDto.ReservationId, 0, 0, null, false, p => p.DishSizeDetail!.Dish!);


                        var orderDb = new Order
                        {
                            OrderId = Guid.NewGuid(),
                            CustomerId = customerInfoDb!.CustomerId,
                            Note = orderRequestDto.Note,
                            OrderDate = orderRequestDto.OrderDate,
                            Status = OrderStatus.Pending,
                            ReservationId = reservationDb.ReservationId,
                            PaymentMethodId = orderRequestDto.PaymentMethodId,
                        };

                        List<OrderDetail> orderDetailsDto = new List<OrderDetail>();
                        double total = 0;

                        foreach (var orderDetail in orderRequestDto.OrderDetailsDtos)
                        {
                            var orderDetailDb = new OrderDetail
                            {
                                OrderDetailId = Guid.NewGuid(),
                                Note = orderDetail.Note,
                                Quantity = orderDetail.Quantity,
                                OrderId = orderDb.OrderId,
                            };

                            if (orderDetail.ComboId.HasValue)
                            {
                                var comboDb = await comboRepository!.GetById(orderDetail.ComboId!);
                                if (comboDb == null)
                                {
                                    return BuildAppActionResultError(result, $"Combo với id {orderDetail.ComboId} không tồn tại");
                                }
                                else
                                {
                                    orderDetailDb.ComboId = orderDetail.ComboId;
                                    orderDetailDb.Price = comboDb!.Price * orderDetail.Quantity;
                                }

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
                            total += orderDetailDb.Price;
                        }
                        orderDb.TotalAmount = total;


                    }
                    else if ()
                    {

                    }
                }
                else if (orderRequestDto.isDelivering == true)
                {

                }

            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
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
