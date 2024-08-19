using AutoMapper;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
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

                var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();

                var preOrderListList = new List<PrelistOrder>();
                var comboOrderDetailList = new List<ComboOrderDetail>();
                var prelistOrderId = Guid.NewGuid();
                dto.PrelistOrderDtos!.ForEach(p =>
                {
                    if (p.Combo != null)
                    {
                        prelistOrderId = Guid.NewGuid();
                        preOrderListList.Add(new PrelistOrder()
                        {
                            PrelistOrderId = prelistOrderId,
                            DishSizeDetailId = p.DishSizeDetailId,
                            ReservationDishId = p.ReservationDishId,
                            ComboId = p.Combo.ComboId,
                            OrderTime = dto.OrderTime,
                            TableSessionId = dto.TableSessionId,
                            Quantity = p.Quantity
                        });
                        p.Combo.DishComboIds.ForEach(
                            c => comboOrderDetailList.Add(
                                new ComboOrderDetail
                                {
                                    ComboOrderDetailId = Guid.NewGuid(),
                                    DishComboId = c,
                                    PrelistOrderId = prelistOrderId
                                }
                            )
                        );
                    }
                    else
                    {
                        preOrderListList.Add(new PrelistOrder()
                        {
                            PrelistOrderId= Guid.NewGuid(), 
                            DishSizeDetailId = p.DishSizeDetailId,
                            ReservationDishId = p.ReservationDishId,
                            ComboId = null,
                            OrderTime = dto.OrderTime,
                            TableSessionId = dto.TableSessionId,
                            Quantity = p.Quantity
                        });
                    }
                });
                await _prelistOrderRepository.InsertRange(preOrderListList);
                await comboOrderDetailRepository!.InsertRange(comboOrderDetailList);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> AddTableSession(TableSessionDto dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var tableRepository = Resolve<IGenericRepository<Table>>();
                var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();
                var tableDb = await tableRepository!.GetById(dto.TableId);
                if(tableDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy bàn với id {dto.TableId}");
                }

                var tableSessionDb = new TableSession
                {
                    TableSessionId = Guid.NewGuid(),
                    TableId = dto.TableId,
                    ReservationId = dto.ReservationId,
                    StartTime = dto.StartTime,
                };

                var preOrderListList = new List<PrelistOrder>();
                var comboOrderDetailList = new List<ComboOrderDetail>();
                var prelistOrderId = Guid.NewGuid();

                dto.PrelistOrderDtos!.ForEach(p =>
                {
                    if (p.Combo != null)
                    {
                        prelistOrderId = Guid.NewGuid();
                        preOrderListList.Add(new PrelistOrder()
                        {
                            PrelistOrderId = prelistOrderId,
                            DishSizeDetailId = p.DishSizeDetailId,
                            ReservationDishId = p.ReservationDishId,
                            ComboId = p.Combo.ComboId,
                            OrderTime = dto.StartTime,
                            TableSessionId = tableSessionDb.TableSessionId,
                            Quantity = p.Quantity
                        });
                        p.Combo.DishComboIds.ForEach(
                            c => comboOrderDetailList.Add(
                                new ComboOrderDetail
                                {
                                    ComboOrderDetailId = Guid.NewGuid(),
                                    DishComboId = c,
                                    PrelistOrderId = prelistOrderId
                                }
                            )
                        );
                    }
                    else
                    {
                        preOrderListList.Add(new PrelistOrder()
                        {
                            PrelistOrderId = Guid.NewGuid(),
                            DishSizeDetailId = p.DishSizeDetailId,
                            ReservationDishId = p.ReservationDishId,
                            ComboId = null,
                            OrderTime = dto.StartTime,
                            TableSessionId = tableSessionDb.TableSessionId,
                            Quantity = p.Quantity
                        });
                    }
                });
                await _tableSessionRepository.Insert(tableSessionDb);
                await _prelistOrderRepository.InsertRange(preOrderListList);
                await comboOrderDetailRepository!.InsertRange(comboOrderDetailList);

                await _unitOfWork.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public Task<AppActionResult> GetLatestPrelistOrder(double? minute, bool IsOrdered)
        {
            throw new NotImplementedException();
        }

        public Task<AppActionResult> GetTableSessionById(Guid Id)
        {
            throw new NotImplementedException();
        }

        public async Task<AppActionResult> UpdatePrelistOrderStatus(List<Guid> prelistOrderIds)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var prelistOrderDb = await _prelistOrderRepository.GetAllDataByExpression(p => prelistOrderIds.Contains(p.PrelistOrderId), 0, 0, null, false, null);
                if (prelistOrderDb.Items.Count != prelistOrderIds.Count)
                {
                    return BuildAppActionResultError(result, $"Tồn tại id gọi món hông nằm trong hệ thống");
                }

                var utility = Resolve<Utility>();
                var time = utility.GetCurrentDateTimeInTimeZone();
                prelistOrderDb.Items.ForEach(p => p.ReadyToServeTime = time);
                await _prelistOrderRepository.UpdateRange(prelistOrderDb.Items);
                await _unitOfWork.SaveChangesAsync();
                result.Result = prelistOrderDb;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
    }
}
