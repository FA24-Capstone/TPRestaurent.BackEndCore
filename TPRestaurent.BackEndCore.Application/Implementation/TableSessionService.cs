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
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class TableSessionService : GenericBackendService, ITableSessionService
    {
        private IGenericRepository<TableSession> _tableSessionRepository;
        private IGenericRepository<PrelistOrder> _prelistOrderRepository;
        private IMapper _mapper;
        private IUnitOfWork _unitOfWork;
        public TableSessionService(IGenericRepository<TableSession> tableSessionRepository,
                                   IGenericRepository<PrelistOrder> prelistOrderRepository,
                                   IMapper mapper,
                                   IUnitOfWork unitOfWork, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _tableSessionRepository = tableSessionRepository;
            _prelistOrderRepository = prelistOrderRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<AppActionResult> AddNewPrelistOrder(PrelistOrderDto dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var tableSessionDb = await _tableSessionRepository.GetById(dto.TableSessionId);
                if (tableSessionDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy phiên dùng bữa với id {dto.TableSessionId}");
                }

                var preOrderListList = new List<PrelistOrder>();

                dto.PrelistOrderDtos.ForEach(p =>
                {
                    if (p.Combo != null)
                    {

                    }
                    else
                    {
                        preOrderListList.Add(new PrelistOrder()
                        {
                            PrelistOrderId = Guid.NewGuid(),
                            DishSizeDetailId = p.DishSizeDetailId,
                            ReservationDishId = p.ReservationDishId,
                            ComboId = null,
                            OrderTime = dto.OrderTime,
                            TableSessionId = dto.TableSessionId,
                            Quantity = p.Quantity
                        });
                    }
                });
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public async Task<AppActionResult> AddTableSession(TableSessionDto dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {

            }
            catch (Exception ex)
            {
            }
            return result;
        }

        public async Task<AppActionResult> GetLatestPrelistOrder(double? minute, bool IsReadyToServed, int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            try
            {
                var preOrderListRepository = Resolve<IGenericRepository<PrelistOrder>>();
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var targetTimeCompleted = currentTime.AddMinutes(-minute.GetValueOrDefault(0));
                var targetTimeToOrder = currentTime.AddMinutes(+minute.GetValueOrDefault(0));    
                if (IsReadyToServed)
                {
                    var preOrderListDb = await preOrderListRepository!.GetAllDataByExpression(p => p.ReadyToServeTime >= currentTime && p.ReadyToServeTime <= targetTimeCompleted, pageNumber, pageSize, null, false, p => p.TableSession!.Table!.TableRating!);
                    result.Result = preOrderListDb;
                }
                else if (!IsReadyToServed)
                {
                       var preOrderListDb = await preOrderListRepository!.GetAllDataByExpression(
                       p => p.ReadyToServeTime >= targetTimeToOrder && p.ReadyToServeTime <= currentTime,
                       pageNumber,
                       pageSize,
                       null,
                       false,
                       p => p.TableSession!.Table!.TableRating!
                      );
                    result.Result = preOrderListDb; 
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetTableSessionById(Guid Id)
        {
            var result = new AppActionResult();
            try
            {
                var tableSessionResponse = new TableSessionResponse();
                var preOrderListRepository = Resolve<IGenericRepository<PrelistOrder>>();
                var tableSessionDb = await _tableSessionRepository.GetByExpression(p => p.TableSessionId == Id, p => p.Table!.TableRating!);
                if (tableSessionDb == null)
                {
                    return BuildAppActionResultError(result, $"Phiên đặt bàn với {Id} không tồn tại");
                }
                var preOrderListDb = await preOrderListRepository!.GetAllDataByExpression(p => p.TableSessionId == Id, 0, 0, null, false, p => p.TableSession!.Table!);
                if (preOrderListDb == null)
                {
                    return BuildAppActionResultError(result, $"Những món đang đặt ở phiên đặt bàn {Id} không tồn tại");
                }

                tableSessionResponse.TableSession = tableSessionDb;
                tableSessionResponse.PrelistOrders = preOrderListDb.Items!;
                result.Result = tableSessionResponse;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public Task<AppActionResult> UpdatePrelistOrderStatus(List<Guid> prelistOrderIds)
        {
            throw new NotImplementedException();
        }
    }
}
