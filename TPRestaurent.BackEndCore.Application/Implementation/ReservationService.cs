using AutoMapper;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ReservationService(IGenericRepository<Reservation> reservationRepository, IGenericRepository<ReservationDish> reservationDishRepository, IUnitOfWork unitOfWork, IMapper mapper, IServiceProvider service) : base(service)
        {
            _reservationRepository = reservationRepository;
            _reservationDishRepository = reservationDishRepository;
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

                    var accountRepository = Resolve<IGenericRepository<Account>>();
                    if ((await accountRepository!.GetById(dto.CustomerAccountId!)) == null)
                    {
                        result = BuildAppActionResultError(result, $"Không tìm thấy tài khoản khách hàng với id {dto.CustomerAccountId}");
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

                    var collidedTable = await CheckTableBooking(dto.ReservationTableIds, dto.ReservationDate, dto.EndTime);

                    if (collidedTable.Count > 0)
                    {
                        result = BuildAppActionResultError(result, $"Danh sách bàn đặt bị. Vui lòng thử lại");
                        return result;
                    }

                    var reservation = _mapper.Map<Reservation>(dto);
                    reservation.ReservationId = Guid.NewGuid();

                    await _reservationRepository.Insert(reservation);

                    var reservationDishDtos = JsonConvert.DeserializeObject<List<ReservationDishDto>>(dto.ReservationDishDtos);
                    if (reservationDishDtos == null)
                    {
                        result = BuildAppActionResultError(result, $"Tạo danh sách món ăn đặt bàn xảy ra lỗi. Vui lòng thử lại");
                        return result;
                    }

                    if (reservationDishDtos.Count() > 0)
                    {
                        var reservationDishes = _mapper.Map<List<ReservationDish>>(reservationDishDtos);
                        reservationDishes.ForEach(r =>
                        {
                            r.ReservationDishId = Guid.NewGuid();
                            r.ReservationId = reservation.ReservationId;
                        });

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

        public async Task<AppActionResult> CalculateDeposit(string ReservationDishDtos)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                double total = 0;
                var reservationDishDtos = JsonConvert.DeserializeObject<List<ReservationDishDto>>(ReservationDishDtos);
                if (reservationDishDtos!.Count() > 0)
                {
                    var dishRepository = Resolve<IGenericRepository<Dish>>();
                    var comboRepository = Resolve<IGenericRepository<Combo>>();
                    var utility = Resolve<Utility>();

                    Dish dishDb = null;
                    Combo comboDb = null;

                    foreach (var reservationDish in reservationDishDtos!)
                    {
                        if (reservationDish.ComboId.HasValue)
                        {
                            comboDb = await comboRepository!.GetByExpression(c => c.ComboId == reservationDish.ComboId.Value && c.EndDate > utility.GetCurrentDateTimeInTimeZone(), null);
                            if (comboDb == null)
                            {
                                result = BuildAppActionResultError(result, $"Không tìm thấy combo với id {reservationDish.ComboId}");
                                return result;
                            }
                            total += reservationDish.Quantity * comboDb.Price;
                        }
                        else
                        {
                            dishDb = await dishRepository!.GetByExpression(c => c.DishId == reservationDish.DishId.Value && c.isAvailable, null);
                            if (comboDb == null)
                            {
                                result = BuildAppActionResultError(result, $"Không tìm thấy món ăn với id {reservationDish.DishId}");
                                return result;
                            }
                            //total += reservationDish.Quantity * dishDb.Price;
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

                if (unavailableReservation!.Items.Count > 0)
                {
                    var unavailableReservationIds = unavailableReservation.Items.Select(x => x.ReservationId);
                    var reservationTableDetailRepository = Resolve<IGenericRepository<ReservationTableDetail>>();
                    var tableRepository = Resolve<IGenericRepository<Table>>();
                    var reservedTableDb = await reservationTableDetailRepository!.GetAllDataByExpression(r => unavailableReservationIds.Contains(r.ReservationId), 0, 0, null, false, r => r.Table);
                    var reservedTableIds = reservedTableDb.Items.Select(x => x.TableId);
                    var availableTableDb = await tableRepository.GetAllDataByExpression(t => !reservedTableIds.Contains(t.TableId), 0, 0, null, false, null);
                    result.Result = availableTableDb;
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
            AppActionResult result = null;
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
                result.Result = await _reservationRepository.GetAllDataByExpression((Expression<Func<Reservation, bool>>?)expression, pageNumber, pageSize, r => r.ReservationDate, true, p => p.CustomerAccount!);

            }
            catch (Exception ex) 
            { 
            
            }
            return result;
        }
    }
}
