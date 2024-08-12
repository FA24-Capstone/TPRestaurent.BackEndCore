﻿using AutoMapper;
using MailKit.Search;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
using static TPRestaurent.BackEndCore.Common.Utils.SD;
using Table = TPRestaurent.BackEndCore.Domain.Models.Table;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class ReservationService : GenericBackendService, IReservationService
    {
        private readonly IGenericRepository<Reservation> _reservationRepository;
        private readonly IGenericRepository<ReservationDish> _reservationDishRepository;
        private readonly IGenericRepository<ComboOrderDetail> _comboOrderDetailRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ReservationService(IGenericRepository<Reservation> reservationRepository, 
                                  IGenericRepository<ReservationDish> reservationDishRepository, 
                                  IGenericRepository<ComboOrderDetail> comboOrderDetailRepository,
                                  IUnitOfWork unitOfWork, 
                                  IMapper mapper, IServiceProvider service) : base(service)
        {
            _reservationRepository = reservationRepository;
            _reservationDishRepository = reservationDishRepository;
            _comboOrderDetailRepository = comboOrderDetailRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AppActionResult> AddReservation(ReservationDto dto)
        {
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                AppActionResult result = new AppActionResult();
                try
                {
                    //Validate
                    if (dto.Deposit < 0)
                    {
                        result = BuildAppActionResultError(result, $"Số tiền cọc không hợp lệ");
                        return result;
                    }

                    if (dto.ReservationTableIds.Count() == 0)
                    {
                        result = BuildAppActionResultError(result, $"Danh sách đặt bàn không được phép trống");
                        return result;
                    }

                    //Add busniness rule for reservation time(if needed)
                    var utility = Resolve<Utility>();
                    if (dto.ReservationDate < utility!.GetCurrentDateTimeInTimeZone())
                    {
                        result = BuildAppActionResultError(result, "Thời gian đặt bàn không hợp lệ");
                        return result;
                    }

                    var accountRepository = Resolve<IGenericRepository<CustomerInfo>>();
                    if ((await accountRepository!.GetById(dto.CustomerInfoId!)) == null)
                    {
                        result = BuildAppActionResultError(result, $"Không tìm thấy  thông tin khách hàng với id {dto.CustomerInfoId}");
                        return result;
                    }

                    var tableRepository = Resolve<IGenericRepository<Table>>();
                    foreach (var tableId in dto.ReservationTableIds)
                    {
                        if ((await tableRepository!.GetById(tableId)) == null)
                        {
                            result = BuildAppActionResultError(result, $"Không tìm thấy bàn với id {tableId}");
                            return result;
                        }
                    }

                    //var collidedTable = await CheckTableBooking(dto.ReservationTableIds, dto.ReservationDate, dto.EndTime);

                    //if (collidedTable.Count > 0)
                    //{
                    //    result = BuildAppActionResultError(result, $"Danh sách bàn đặt bị. Vui lòng thử lại");
                    //    return result;
                    //}

                    var reservation = _mapper.Map<Reservation>(dto);
                    reservation.ReservationId = Guid.NewGuid();
                    await _reservationRepository.Insert(reservation);

                    if (dto.ReservationDishDtos.Count() > 0)
                    {
                        var reservationDishes = new List<ReservationDish>();
                        var dishComboComboDetailList = new List<ComboOrderDetail>();
                        Guid reservationDishId = Guid.NewGuid();
                        dto.ReservationDishDtos.ForEach(r =>
                        {
                            reservationDishId = Guid.NewGuid();
                            reservationDishes.Add(new ReservationDish
                            {
                                DishSizeDetailId = r.DishSizeDetailId,
                                ComboId = (r.Combo != null) ? r.Combo.ComboId : (Guid?)null,
                                Note = r.Note,
                                Quantity = r.Quantity,
                                ReservationId = reservation.ReservationId,
                                ReservationDishId = reservationDishId
                            });

                            if (r.Combo != null)
                            {
                                r.Combo.DishComboIds.ForEach(d => dishComboComboDetailList.Add(new ComboOrderDetail
                                {
                                    ComboOrderDetailId = Guid.NewGuid(),
                                    DishComboId = d,
                                    ReservationDishId = reservationDishId
                                }));
                            }
                        });

                        await _comboOrderDetailRepository.InsertRange(dishComboComboDetailList);
                        await _reservationDishRepository.InsertRange(reservationDishes);
                    }

                    List<ReservationTableDetail> reservationTableDetails = new List<ReservationTableDetail>();
                    dto.ReservationTableIds.ForEach(r => reservationTableDetails.Add(new ReservationTableDetail
                    {
                        ReservationTableDetailId = Guid.NewGuid(),
                        ReservationId = reservation.ReservationId,
                        TableId = r
                    }));

                    var reservationTableDetailRepository = Resolve<IGenericRepository<ReservationTableDetail>>();
                    await reservationTableDetailRepository!.InsertRange(reservationTableDetails);
                    await _unitOfWork.SaveChangesAsync();
                    scope.Complete();
                }
                catch (Exception ex)
                {
                    result = BuildAppActionResultError(result, ex.Message);
                }
                return result;
            }
        }

        private async Task<List<ColidedTableDto>> CheckTableBooking(List<Guid> reservationTableIds, DateTime reservationDate, DateTime? endTime)
        {
            throw new NotImplementedException();
        }

        public async Task<AppActionResult> CalculateDeposit(List<ReservationDishDto> reservationDishDtos)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                double total = 0;
                if (reservationDishDtos!.Count() > 0)
                {
                    var dishRepository = Resolve<IGenericRepository<DishSizeDetail>>();
                    var comboRepository = Resolve<IGenericRepository<Combo>>();
                    var utility = Resolve<Utility>();

                    DishSizeDetail dishDb = null;
                    Combo comboDb = null;

                    foreach (var reservationDish in reservationDishDtos!)
                    {
                        if (reservationDish.Combo != null)
                        {
                            comboDb = await comboRepository!.GetByExpression(c =>  c.ComboId == reservationDish.Combo.ComboId && c.EndDate > utility.GetCurrentDateTimeInTimeZone(), null);
                            if (comboDb == null)
                            {
                                result = BuildAppActionResultError(result, $"Không tìm thấy combo với id {reservationDish.Combo.ComboId}");
                                return result;
                            }
                            total += reservationDish.Quantity * comboDb.Price;
                        }
                        else
                        {
                            dishDb = await dishRepository!.GetByExpression(c => c.DishSizeDetailId == reservationDish.DishSizeDetailId.Value && c.IsAvailable, null);
                            if (dishDb == null)
                            {
                                result = BuildAppActionResultError(result, $"Không tìm thấy món ăn với id {reservationDish.DishSizeDetailId}");
                                return result;
                            }
                            total += reservationDish.Quantity * dishDb.Price;
                        }
                    }
                }
                result.Result = total * SD.DefaultValue.DEPOSIT_PERCENT;
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex?.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAvailableTable(DateTime? startTime, DateTime? endTime, int? numOfPeople, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var conditions = new List<Func<Expression<Func<Reservation, bool>>>>();
                if (startTime.HasValue)
                {
                    if (endTime.HasValue)
                    {
                        conditions.Add(() => r => (r.ReservationDate < endTime
                                                    && (r.EndTime != null && r.EndTime < startTime || r.EndTime == null
                                                        && r.ReservationDate.AddHours(SD.DefaultValue.AVERAGE_MEAL_DURATION) < startTime)));
                    }
                    else
                    {
                        conditions.Add(() => r => (r.ReservationDate < startTime.Value.AddHours(SD.DefaultValue.AVERAGE_MEAL_DURATION)
                                                    && (r.EndTime != null && r.EndTime < startTime || r.EndTime == null
                                                        && r.ReservationDate.AddHours(SD.DefaultValue.AVERAGE_MEAL_DURATION) < startTime)));
                    }
                }

                Expression expression = DynamicLinqBuilder<Reservation>.BuildExpression(conditions);

                //Get all collided reservations

                var unavailableReservation = await _reservationRepository.GetAllDataByExpression((Expression<Func<Reservation, bool>>?)expression, pageNumber, pageSize, null, false, null);

                var tableRepository = Resolve<IGenericRepository<Table>>();
                if (unavailableReservation!.Items.Count > 0)
                {
                    var unavailableReservationIds = unavailableReservation.Items.Select(x => x.ReservationId);
                    var reservationTableDetailRepository = Resolve<IGenericRepository<ReservationTableDetail>>();
                    var reservedTableDb = await reservationTableDetailRepository!.GetAllDataByExpression(r => unavailableReservationIds.Contains(r.ReservationId), 0, 0, null, false, r => r.Table);
                    var reservedTableIds = reservedTableDb.Items!.Select(x => x.TableId);
                    var availableTableDb = await tableRepository!.GetAllDataByExpression(t => !reservedTableIds.Contains(t.TableId), 0, 0, null, false, null);
                    result.Result = availableTableDb;
                } else
                {
                    result.Result = await tableRepository!.GetAllDataByExpression(null, 0, 0, null, false, null);
                }
                //result.Result = availableReservation.Items.Select(x => x.Table);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public Task<AppActionResult> RemoveReservation(Guid reservationId)
        {
            throw new NotImplementedException();
        }


        public Task<AppActionResult> UpdateReservation(ReservationDto dto)
        {
            throw new NotImplementedException();
        }

        public async Task<AppActionResult> SuggestTable(SuggestTableDto dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                //Validate
                List<Guid> guids = new List<Guid>();
                var conditions = new List<Func<Expression<Func<Reservation, bool>>>>();
                //Get All Available Table
                var availableTableResult = await GetAvailableTable(dto.StartTime, dto.EndTime, null, 0, 0);
                if (availableTableResult.IsSuccess)
                {
                    var availableTable = (PagedResult<Table>)availableTableResult.Result!;
                    if (availableTable.Items!.Count > 0)
                    {
                        result.Result = await GetTables(availableTable.Items, dto.NumOfPeople, dto.RequiredAirConditioner, dto.IsPrivate);
                    }
                }


                //Get Table with condition: 
            }
            catch (Exception ex)
            {

            }

            return result;
        }

        public async Task<List<Table>> GetTables(List<Table> allAvailableTables, int quantity, bool requiredAirCondition, bool isPrivate)
        {
            List<Table> result = new List<Table>();
            try
            {
                string tableCode = "T";
                if (isPrivate)
                {
                    tableCode = "V";
                }
                else if (requiredAirCondition)
                {
                    tableCode = "L";
                }

                var tableType = allAvailableTables.Where(t => t.TableName.Contains(tableCode)).GroupBy(t => t.TableSizeId)
                                                  .ToDictionary(k => k.Key, k => k.ToList());

                if (!tableType.Equals("V"))
                {
                    if (quantity <= 2)
                    {
                        if (tableType.ContainsKey(TableSize.TWO) && tableType[TableSize.TWO].Count > 0)
                        {
                            result.Add(tableType[TableSize.TWO].FirstOrDefault());
                            return result;
                        }
                    }

                    if (quantity <= 4)
                    {
                        if (tableType.ContainsKey(TableSize.FOUR) && tableType[TableSize.SIX].Count > 0)
                        {
                            result.Add(tableType[TableSize.FOUR].FirstOrDefault());
                            return result;
                        }
                    }

                    if (quantity <= 6)
                    {
                        if (tableType.ContainsKey(TableSize.SIX) && tableType[TableSize.SIX].Count > 0)
                        {
                            result.Add(tableType[TableSize.SIX].FirstOrDefault());
                            return result;
                        }
                    }
                }

                if (quantity <= 8)
                {
                    if (tableType.ContainsKey(TableSize.EIGHT) && tableType[TableSize.EIGHT].Count > 0)
                    {
                        result.Add(tableType[TableSize.EIGHT].FirstOrDefault());
                        return result;
                    }
                }

                if (quantity <= 10)
                {
                    if (tableType.ContainsKey(TableSize.TEN) && tableType[TableSize.TEN].Count > 0)
                    {
                        result.Add(tableType[TableSize.TEN].FirstOrDefault());
                        return result;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                result = new List<Table>();
            }
            return result;


        }

        public async Task<AppActionResult> GetAllReservation(int? time, ReservationStatus? status, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var conditions = new List<Func<Expression<Func<Reservation, bool>>>>();
                
                if (time.HasValue)
                {
                    //This week
                    if(time == 0)
                    {
                        var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                        var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);

                        // Add the condition to the list
                        conditions.Add(() => r =>
                            r.ReservationDate >= startOfWeek && r.ReservationDate <= endOfWeek);
                    }
                }

                if (status.HasValue)
                {
                    conditions.Add(() => r => r.StatusId == status.Value);
                }

                Expression expression = DynamicLinqBuilder<Reservation>.BuildExpression(conditions);
                result.Result = await _reservationRepository.GetAllDataByExpression((Expression<Func<Reservation, bool>>?)expression, pageNumber, pageSize, r => r.ReservationDate, true, p => p.CustomerInfo.Account!);

            }
            catch (Exception ex) 
            { 
            
            }
            return result;
        }

        public async Task<AppActionResult> GetAllReservationByAccountId(Guid customerInfoId, ReservationStatus? status, int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                if (status.HasValue)
                {
                    result.Result = await _reservationRepository.GetAllDataByExpression(o => o.CustomerInfoId.Equals(customerInfoId) && o.StatusId == status, pageNumber, pageSize, o => o.ReservationDate, false, null);
                }
                else
                {
                    result.Result = await _reservationRepository.GetAllDataByExpression(o => o.CustomerInfoId.Equals(customerInfoId), pageNumber, pageSize, o => o.ReservationDate, false, null);
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> GetAllReservationDetail(Guid reservationId)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var reservationDb = await _reservationRepository.GetById(reservationId);
                if (reservationDb == null)
                {
                    result = BuildAppActionResultError(result, $"Không tìm thấy thông tin đặt bàn với id {reservationId}");
                    return result;
                }

                var reservationTableDetailRepository = Resolve<IGenericRepository<ReservationTableDetail>>();
                var reservationTableDetailDb = await reservationTableDetailRepository!.GetAllDataByExpression(o => o.ReservationId == reservationId, 0, 0, null, false, o => o.Table);
                var reservationDishDb = await _reservationDishRepository!.GetAllDataByExpression(o => o.ReservationId == reservationId, 0, 0, null, false, o => o.DishSizeDetail.Dish, o => o.Combo);
                result.Result = new ReservationReponse
                {
                    Reservation = reservationDb,
                    ReservationDishes = reservationDishDb.Items!,
                    ReservationTableDetails = reservationTableDetailDb.Items!
                };
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> UpdateReservationStatus(Guid reservationId, ReservationStatus status)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var reservationDb = await _reservationRepository.GetById(reservationId);
                if(reservationDb == null)
                {
                    result = BuildAppActionResultError(result, $"Không tìm thấy yêu cầu đặt bàn với id {reservationId}");
                    return result;
                }
                bool updated = false;
                if(reservationDb.StatusId == ReservationStatus.PENDING)
                {
                    if(status == ReservationStatus.DINING)
                    {
                        result = BuildAppActionResultError(result, $"Yêu cầu đặt bàn với id {reservationId} chưa được xử lí,không thể diễn ra");
                        return result;
                    }
                    reservationDb.StatusId = status;
                    updated = true; 
                } else if (reservationDb.StatusId == ReservationStatus.PAID && status != ReservationStatus.PENDING  && status != ReservationStatus.PAID)
                {
                    reservationDb.StatusId = status;
                    updated = true;
                }
                await _reservationRepository.Update(reservationDb);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            
            }
            return result;
        }
    }
}
