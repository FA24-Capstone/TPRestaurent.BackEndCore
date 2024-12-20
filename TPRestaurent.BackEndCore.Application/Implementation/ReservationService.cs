﻿//using AutoMapper;
//using MailKit.Search;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;
//using Newtonsoft.Json;
//using NPOI.SS.Formula.Functions;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;
//using System.Transactions;
//using TPRestaurent.BackEndCore.Application.Contract.IServices;
//using TPRestaurent.BackEndCore.Application.IRepositories;
//using TPRestaurent.BackEndCore.Common.DTO.Request;
//using TPRestaurent.BackEndCore.Common.DTO.Response;
//using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
//using TPRestaurent.BackEndCore.Common.Utils;
//using TPRestaurent.BackEndCore.Domain.Enums;
//using TPRestaurent.BackEndCore.Domain.Models;
//using static TPRestaurent.BackEndCore.Common.Utils.SD;
//using Table = TPRestaurent.BackEndCore.Domain.Models.Table;

//namespace TPRestaurent.BackEndCore.Application.Implementation
//{
//    public class ReservationService : GenericBackendService, IReservationService
//    {
//        //private readonly IGenericRepository<Reservation> _reservationRepository;
//        private readonly IGenericRepository<TableDetail> _reservationTableDetailRepository;
//        //private readonly IGenericRepository<ReservationDish> _reservationDishRepository;
//        private readonly IGenericRepository<ComboOrderDetail> _comboOrderDetailRepository;
//        private readonly IGenericRepository<Configuration> _configurationRepository;
//        private readonly IAccountService _accountService;
//        private readonly IUnitOfWork _unitOfWork;
//        private readonly IMapper _mapper;
//        private BackEndLogger _logger;

//        public ReservationService(/*IGenericRepository<Reservation> reservationRepository,*/
//                                  IGenericRepository<TableDetail> reservationTableDetailRepository,
//                                  //IGenericRepository<ReservationDish> reservationDishRepository,
//                                  IGenericRepository<ComboOrderDetail> comboOrderDetailRepository,
//                                  IGenericRepository<Configuration> configurationRepository,
//                                  IAccountService accountService,
//                                  IUnitOfWork unitOfWork,
//                                  IMapper mapper, IServiceProvider service,
//                                  BackEndLogger logger) : base(service)
//        {
//            //_reservationRepository = reservationRepository;
//            _reservationTableDetailRepository = reservationTableDetailRepository;
//            //_reservationDishRepository = reservationDishRepository;
//            _comboOrderDetailRepository = comboOrderDetailRepository;
//            _configurationRepository = configurationRepository;
//            _accountService = accountService;
//            _unitOfWork = unitOfWork;
//            _mapper = mapper;
//            _logger = logger;
//        }

//        public async Task<AppActionResult> AddReservation(ReservationDto dto)
//        {
//            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
//            {
//                AppActionResult result = new AppActionResult();
//                try
//                {
//                    //Validate
//                    if (dto.Deposit < 0)
//                    {
//                        return BuildAppActionResultError(result, $"Số tiền cọc không hợp lệ");
//                        return result;
//                    }

//                    var suggestTableDto = new SuggestTableDto
//                    {
//                        StartTime = dto.ReservationDate,
//                        EndTime = dto.EndTime,
//                        IsPrivate = dto.IsPrivate,
//                        NumOfPeople = dto.NumberOfPeople,
//                    };

//                    var suitableTable = await GetSuitableTable(suggestTableDto);
//                    if (suitableTable == null)
//                    {
//                        return BuildAppActionResultError(result, $"Không có bàn trống cho {dto.NumberOfPeople} người " +
//                                                                   $"vào lúc {dto.ReservationDate.Hour}h{dto.ReservationDate.Minute}p " +
//                                                                   $"ngày {dto.ReservationDate.Date}");
//                        return result;
//                    }
//                    //Add busniness rule for reservation time(if needed)
//                    var utility = Resolve<Utility>();
//                    if (dto.ReservationDate < utility!.GetCurrentDateTimeInTimeZone())
//                    {
//                        return BuildAppActionResultError(result, "Thời gian đặt bàn không hợp lệ");
//                        return result;
//                    }

//                    var customerInfoRepository = Resolve<IGenericRepository<CustomerInfo>>();

//                    var customerInfoDb = await customerInfoRepository!.GetById(dto.CustomerInfoId!);

//                    if (customerInfoDb == null || !customerInfoDb.IsVerified)
//                    {
//                        return BuildAppActionResultError(result, $"Không tìm thấy thông tin khách hàng với id {dto.CustomerInfoId}");
//                        return result;
//                    }

//                    var reservation = _mapper.Map<Reservation>(dto);
//                    reservation.ReservationId = Guid.NewGuid();
//                    reservation.StatusId = ReservationStatus.TABLEASSIGNED;
//                    reservation.ReservationDate = utility.GetCurrentDateTimeInTimeZone();

//                    await _reservationRepository.Insert(reservation);

//                    var reservationTableDetail = new TableDetail
//                    {
//                        ReservationTableDetailId = Guid.NewGuid(),
//                        ReservationId = reservation.ReservationId,
//                        TableId = suitableTable.TableId
//                    };

//                    await _reservationTableDetailRepository.Insert(reservationTableDetail);

//                    if (dto.ReservationDishDtos.Count() > 0)
//                    {
//                        var reservationDishes = new List<ReservationDish>();
//                        var dishComboComboDetailList = new List<ComboOrderDetail>();
//                        Guid reservationDishId = Guid.NewGuid();
//                        dto.ReservationDishDtos.ForEach(r =>
//                        {
//                            reservationDishId = Guid.NewGuid();
//                            reservationDishes.Add(new ReservationDish
//                            {
//                                DishSizeDetailId = r.DishSizeDetailId,
//                                ComboId = (r.Combo != null) ? r.Combo.ComboId : (Guid?)null,
//                                Note = r.Note,
//                                Quantity = r.Quantity,
//                                ReservationId = reservation.ReservationId,
//                                ReservationDishId = reservationDishId
//                            });

//                            if (r.Combo != null)
//                            {
//                                r.Combo.DishComboIds.ForEach(d => dishComboComboDetailList.Add(new ComboOrderDetail
//                                {
//                                    ComboOrderDetailId = Guid.NewGuid(),
//                                    DishComboId = d,
//                                    ReservationDishId = reservationDishId
//                                }));
//                            }
//                        });

//                        await _comboOrderDetailRepository.InsertRange(dishComboComboDetailList);
//                        await _reservationDishRepository.InsertRange(reservationDishes);
//                    }

//                    //List<ReservationTableDetail> reservationTableDetails = new List<ReservationTableDetail>();
//                    //dto.ReservationTableIds.ForEach(r => reservationTableDetails.Add(new ReservationTableDetail
//                    //{
//                    //    ReservationTableDetailId = Guid.NewGuid(),
//                    //    ReservationId = reservation.ReservationId,
//                    //    TableId = r
//                    //}));

//                    //var reservationTableDetailRepository = Resolve<IGenericRepository<ReservationTableDetail>>();
//                    //await reservationTableDetailRepository!.InsertRange(reservationTableDetails);
//                    await _unitOfWork.SaveChangesAsync();
//                    scope.Complete();
//                }
//                catch (Exception ex)
//                {
//                    result = BuildAppActionResultError(result, ex.Message);
//                }
//                return result;
//            }
//        }

//        //private async Task<List<ColidedTableDto>> CheckTableBooking(List<Guid> reservationTableIds, DateTime reservationDate, DateTime? endTime)
//        //{
//        //    throw new NotImplementedException();
//        //}

//        //public async Task<AppActionResult> CalculateDeposit(ReservationDto dto)
//        //{
//        //    AppActionResult result = new AppActionResult();
//        //    try
//        //    {
//        //        double total = 0;
//        //        if (dto.ReservationDishDtos!.Count() > 0)
//        //        {
//        //            var dishRepository = Resolve<IGenericRepository<DishSizeDetail>>();
//        //            var comboRepository = Resolve<IGenericRepository<Combo>>();
//        //            var utility = Resolve<Utility>();

//        //            DishSizeDetail dishDb = null;
//        //            Combo comboDb = null;

//        //            foreach (var reservationDish in dto.ReservationDishDtos!)
//        //            {
//        //                if (reservationDish.Combo != null)
//        //                {
//        //                    comboDb = await comboRepository!.GetByExpression(c => c.ComboId == reservationDish.Combo.ComboId && c.EndDate > utility.GetCurrentDateTimeInTimeZone(), null);
//        //                    if (comboDb == null)
//        //                    {
//        //                        return BuildAppActionResultError(result, $"Không tìm thấy combo với id {reservationDish.Combo.ComboId}");
//        //                        return result;
//        //                    }
//        //                    total += reservationDish.Quantity * comboDb.Price;
//        //                }
//        //                else
//        //                {
//        //                    dishDb = await dishRepository!.GetByExpression(c => c.DishSizeDetailId == reservationDish.DishSizeDetailId.Value && c.IsAvailable, null);
//        //                    if (dishDb == null)
//        //                    {
//        //                        return BuildAppActionResultError(result, $"Không tìm thấy món ăn với id {reservationDish.DishSizeDetailId}");
//        //                        return result;
//        //                    }
//        //                    total += reservationDish.Quantity * dishDb.Price;
//        //                }
//        //            }
//        //        }
//        //        var configurationDb = await _configurationRepository.GetAllDataByExpression(c => c.Name.Equals(SD.DefaultValue.DEPOSIT_PERCENT), 0, 0, null, false, null);
//        //        if (configurationDb.Items.Count == 0 || configurationDb.Items.Count > 1)
//        //        {
//        //            return BuildAppActionResultError(result, $"Xảy ra lỗi khi lấy thông số cấu hình {SD.DefaultValue.DEPOSIT_PERCENT}");
//        //        }

//        //        double deposit = total * double.Parse(configurationDb.Items[0].PreValue);
//        //        string tableTypeDeposit = SD.DefaultValue.DEPOSIT_FOR_NORMAL_TABLE;
//        //        if (dto.IsPrivate)
//        //        {
//        //            tableTypeDeposit = SD.DefaultValue.DEPOSIT_FOR_PRIVATE_TABLE;
//        //        }
//        //        else
//        //        {
//        //            tableTypeDeposit = SD.DefaultValue.DEPOSIT_FOR_NORMAL_TABLE;

//        //        }
//        //        var tableConfigurationDb = await _configurationRepository.GetAllDataByExpression(c => c.Name.Equals(tableTypeDeposit), 0, 0, null, false, null);
//        //        if (tableConfigurationDb.Items.Count == 0 || tableConfigurationDb.Items.Count > 1)
//        //        {
//        //            return BuildAppActionResultError(result, $"Xảy ra lỗi khi lấy thông số cấu hình {tableTypeDeposit}");
//        //        }
//        //        deposit += double.Parse(tableConfigurationDb.Items[0].PreValue);
//        //        dto.Deposit = deposit;
//        //        result.Result = dto;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        return BuildAppActionResultError(result, ex?.Message);
//        //    }
//        //    return result;
//        //}

//        //public async Task<AppActionResult> GetAvailableTable(DateTime startTime, DateTime? endTime, int? numOfPeople, int pageNumber, int pageSize)
//        //{
//        //    AppActionResult result = new AppActionResult();
//        //    try
//        //    {
//        //        var conditions = new List<Func<Expression<Func<Reservation, bool>>>>();

//        //        // !(endTime < r.ReservationDate || r.EndTime < startTime)
//        //        var configurationDb = await _configurationRepository.GetAllDataByExpression(c => c.Name.Equals(SD.DefaultValue.AVERAGE_MEAL_DURATION), 0, 0, null, false, null);
//        //        if (configurationDb.Items.Count == 0 || configurationDb.Items.Count > 1)
//        //        {
//        //            return BuildAppActionResultError(result, $"Xảy ra lỗi khi lấy thông số cấu hình {SD.DefaultValue.AVERAGE_MEAL_DURATION}");
//        //        }
//        //        if (!endTime.HasValue)
//        //        {
//        //            endTime = startTime.AddHours(double.Parse(configurationDb.Items[0].PreValue));
//        //        }

//        //        conditions.Add(() => r => !(endTime < r.ReservationDate || (r.EndTime.HasValue && r.EndTime.Value < startTime || !r.EndTime.HasValue && r.ReservationDate.AddHours(double.Parse(configurationDb.Items[0].PreValue)) < startTime))
//        //                                  && r.StatusId != OrderStatus.Cancelled);

//        //        Expression<Func<Order, bool>> expression = r => true; // Default expression to match all

//        //        if (conditions.Count > 0)
//        //        {
//        //            expression = DynamicLinqBuilder<Or>.BuildExpression(conditions);
//        //        }

//        //        // Get all collided reservations
//        //        var unavailableReservation = await _reservationRepository.GetAllDataByExpression(expression, pageNumber, pageSize, null, false, null);
//        //        var tableRepository = Resolve<IGenericRepository<Table>>();
//        //        if (unavailableReservation!.Items.Count > 0)
//        //        {
//        //            var unavailableReservationIds = unavailableReservation.Items.Select(x => x.ReservationId);
//        //            var reservationTableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
//        //            var reservedTableDb = await reservationTableDetailRepository!.GetAllDataByExpression(r => unavailableReservationIds.Contains(r.ReservationId), 0, 0, null, false, r => r.Table.TableRating);
//        //            var reservedTableIds = reservedTableDb.Items!.Select(x => x.TableId);
//        //            var availableTableDb = await tableRepository!.GetAllDataByExpression(t => !reservedTableIds.Contains(t.TableId), 0, 0, null, false, null);
//        //            result.Result = availableTableDb;
//        //        }
//        //        else
//        //        {
//        //            result.Result = await tableRepository!.GetAllDataByExpression(null, 0, 0, null, false, r => r.TableRating);
//        //        }
//        //        //result.Result = availableReservation.Items.Select(x => x.Table);
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        result = BuildAppActionResultError(result, ex.Message);
//        //    }
//        //    return result;
//        //}

//        //public async Task<AppActionResult> UpdateReservation(UpdateReservationDto dto)
//        //{
//        //    using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
//        //    {
//        //        AppActionResult result = new AppActionResult();
//        //        try
//        //        {
//        //            //Validate
//        //            var reservationRepository = Resolve<IGenericRepository<Reservation>>();
//        //            var reservationTableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
//        //            var reservationDishRepository = Resolve<IGenericRepository<ReservationDish>>();
//        //            var tableRepository = Resolve<IGenericRepository<Table>>();
//        //            var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();

//        //            var reservationDb = await reservationRepository!.GetById(dto.ReservationId);
//        //            if (reservationDb == null)
//        //            {
//        //                return BuildAppActionResultError(result, $"Không tìm thấy lịch đặt bàn với id {reservationDb!.ReservationId}");
//        //                return result;
//        //            }

//        //            if (dto.AdditionalDeposit.HasValue && dto.AdditionalDeposit < 0)
//        //            {
//        //                return BuildAppActionResultError(result, $"Số tiền cọc không hợp lệ");
//        //                return result;
//        //            }

//        //            //Add busniness rule for reservation time(if needed)
//        //            var utility = Resolve<Utility>();
//        //            if (dto.ReservationDate < utility!.GetCurrentDateTimeInTimeZone())
//        //            {
//        //                return BuildAppActionResultError(result, "Thời gian đặt bàn không hợp lệ");
//        //                return result;
//        //            }

//        //            var accountRepository = Resolve<IGenericRepository<CustomerInfo>>();
//        //            if ((await accountRepository!.GetById(reservationDb.CustomerInfoId!)) == null)
//        //            {
//        //                return BuildAppActionResultError(result, $"Không tìm thấy  thông tin khách hàng với id {reservationDb.CustomerInfoId}");
//        //                return result;
//        //            }

//        //            foreach (var tableId in dto.ReservationTableIds)
//        //            {
//        //                if ((await tableRepository!.GetById(tableId)) == null)
//        //                {
//        //                    return BuildAppActionResultError(result, $"Không tìm thấy bàn với id {tableId}");
//        //                    return result;
//        //                }
//        //            }

//        //            var reservationTableDb = await reservationTableDetailRepository!.GetAllDataByExpression(r => r.ReservationId == dto.ReservationId, 0, 0, null, false, null);
//        //            //Not yet has table => first time add table from
//        //            if (reservationTableDb.Items.Count == 0)
//        //            {
//        //                if (dto.ReservationTableIds.Count() == 0)
//        //                {
//        //                    return BuildAppActionResultError(result, $"Danh sách đặt bàn không được phép trống");
//        //                    return result;
//        //                }
//        //            }
//        //            else
//        //            {
//        //                //Already has table reservation -> delete old ones
//        //                if (dto.ReservationDishDtos.Count() > 0)
//        //                {
//        //                    await reservationTableDetailRepository.DeleteRange(reservationTableDb.Items);
//        //                }
//        //            }

//        //            reservationDb.NumberOfPeople = dto.NumberOfPeople;
//        //            reservationDb.ReservationDate = dto.ReservationDate;
//        //            reservationDb.EndTime = dto.EndTime;
//        //            reservationDb.Deposit += Math.Max(0, (double)dto.AdditionalDeposit);

//        //            await _reservationRepository.Update(reservationDb);

//        //            if (dto.ReservationDishDtos.Count() > 0)
//        //            {
//        //                //Remove old dishes
//        //                var reservationDishDetailDb = await reservationDishRepository!.GetAllDataByExpression(r => r.ReservationId == dto.ReservationId, 0, 0, null, false, null);
//        //                if (reservationDishDetailDb.Items.Count > 0)
//        //                {
//        //                    var reservationComboIds = reservationDishDetailDb.Items.Where(r => r.ComboId.HasValue).Select(r => r.ReservationDishId).ToList();
//        //                    var comboOrderDetailDb = await comboOrderDetailRepository!.GetAllDataByExpression(r => reservationComboIds.Contains((Guid)r.ReservationDishId), 0, 0, null, false, null);
//        //                    if (comboOrderDetailDb.Items.Count > 0)
//        //                    {
//        //                        await comboOrderDetailRepository.DeleteRange(comboOrderDetailDb.Items);
//        //                    }
//        //                    await reservationDishRepository.DeleteRange(reservationDishDetailDb.Items);
//        //                }

//        //                var reservationDishes = new List<ReservationDish>();
//        //                var dishComboComboDetailList = new List<ComboOrderDetail>();
//        //                Guid reservationDishId = Guid.NewGuid();
//        //                dto.ReservationDishDtos.ForEach(r =>
//        //                {
//        //                    reservationDishId = Guid.NewGuid();
//        //                    reservationDishes.Add(new ReservationDish
//        //                    {
//        //                        DishSizeDetailId = r.DishSizeDetailId,
//        //                        ComboId = (r.Combo != null) ? r.Combo.ComboId : (Guid?)null,
//        //                        Note = r.Note,
//        //                        Quantity = r.Quantity,
//        //                        ReservationId = reservationDb.ReservationId,
//        //                        ReservationDishId = reservationDishId
//        //                    });

//        //                    if (r.Combo != null)
//        //                    {
//        //                        r.Combo.DishComboIds.ForEach(d => dishComboComboDetailList.Add(new ComboOrderDetail
//        //                        {
//        //                            ComboOrderDetailId = Guid.NewGuid(),
//        //                            DishComboId = d,
//        //                            ReservationDishId = reservationDishId
//        //                        }));
//        //                    }
//        //                });

//        //                await _comboOrderDetailRepository.InsertRange(dishComboComboDetailList);
//        //                await _reservationDishRepository.InsertRange(reservationDishes);
//        //            }

//        //            if (dto.ReservationTableIds.Count > 0)
//        //            {
//        //                var reservationTableDetails = new List<TableDetail>();
//        //                dto.ReservationTableIds.ForEach(r => reservationTableDetails.Add(new TableDetail
//        //                {
//        //                    TableId = r,
//        //                    ReservationTableDetailId = Guid.NewGuid(),
//        //                    ReservationId = dto.ReservationId
//        //                }));
//        //                await reservationTableDetailRepository!.InsertRange(reservationTableDetails);
//        //            }
//        //            await _unitOfWork.SaveChangesAsync();
//        //            scope.Complete();
//        //        }
//        //        catch (Exception ex)
//        //        {
//        //            result = BuildAppActionResultError(result, ex.Message);
//        //        }
//        //        return result;
//        //    }
//        //}

//        //public async Task<AppActionResult> SuggestTable(SuggestTableDto dto)
//        //{
//        //    AppActionResult result = new AppActionResult();
//        //    try
//        //    {
//        //        //Validate
//        //        List<Guid> guids = new List<Guid>();
//        //        var conditions = new List<Func<Expression<Func<Reservation, bool>>>>();
//        //        if (dto.NumOfPeople <= 0)
//        //        {
//        //            return BuildAppActionResultError(result, "Số người phải lớn hơn 0!");
//        //        }
//        //        //Get All Available Table
//        //        var availableTableResult = await GetAvailableTable(dto.StartTime, dto.EndTime, dto.NumOfPeople, 0, 0);
//        //        if (availableTableResult.IsSuccess)
//        //        {
//        //            var availableTable = (PagedResult<Table>)availableTableResult.Result!;
//        //            if (availableTable.Items!.Count > 0)
//        //            {
//        //                result.Result = await GetTables(availableTable.Items, dto.NumOfPeople, dto.IsPrivate);
//        //            }
//        //        }

//        //        //Get Table with condition:
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        result = BuildAppActionResultError(result, ex.Message);
//        //    }

//        //    return result;
//        //}
//        //public async Task<Table> GetSuitableTable(SuggestTableDto dto)
//        //{
//        //    Table result = null;
//        //    try
//        //    {
//        //        if (dto.NumOfPeople <= 0)
//        //        {
//        //            return null;
//        //        }
//        //        //Get All Available Table
//        //        var availableTableResult = await GetAvailableTable(dto.StartTime, dto.EndTime, dto.NumOfPeople, 0, 0);
//        //        if (availableTableResult.IsSuccess)
//        //        {
//        //            var availableTable = (PagedResult<Table>)availableTableResult.Result!;
//        //            if (availableTable.Items!.Count > 0)
//        //            {
//        //                var suitableTables = await GetTables(availableTable.Items, dto.NumOfPeople, dto.IsPrivate);
//        //                result = suitableTables[0];
//        //            }
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        result = null;
//        //    }
//        //    return result;
//        //}

//        //public async Task<List<Table>> GetTables(List<Table> allAvailableTables, int quantity, bool isPrivate)
//        //{
//        //    List<Table> result = new List<Table>();
//        //    try
//        //    {
//        //        string tableCode = "T";
//        //        if (isPrivate)
//        //        {
//        //            tableCode = "V";
//        //        }

//        //        var tableType = allAvailableTables.Where(t => t.TableName.Contains(tableCode)).GroupBy(t => t.TableSizeId)
//        //                                          .ToDictionary(k => k.Key, k => k.ToList());

//        //        if (!tableType.Equals("V"))
//        //        {
//        //            if (quantity <= 2)
//        //            {
//        //                if (tableType.ContainsKey(TableSize.TWO) && tableType[TableSize.TWO].Count > 0)
//        //                {
//        //                    result.AddRange(tableType[TableSize.TWO].ToList());
//        //                    return result;
//        //                }
//        //            }

//        //            if (quantity <= 4)
//        //            {
//        //                if (tableType.ContainsKey(TableSize.FOUR) && tableType[TableSize.SIX].Count > 0)
//        //                {
//        //                    result.AddRange(tableType[TableSize.FOUR]);
//        //                    return result;
//        //                }
//        //            }

//        //            if (quantity <= 6)
//        //            {
//        //                if (tableType.ContainsKey(TableSize.SIX) && tableType[TableSize.SIX].Count > 0)
//        //                {
//        //                    result.AddRange(tableType[TableSize.SIX]);
//        //                    return result;
//        //                }
//        //            }
//        //        }

//        //        if (quantity <= 8)
//        //        {
//        //            if (tableType.ContainsKey(TableSize.EIGHT) && tableType[TableSize.EIGHT].Count > 0)
//        //            {
//        //                result.AddRange(tableType[TableSize.EIGHT]);
//        //                return result;
//        //            }
//        //        }

//        //        if (quantity <= 10)
//        //        {
//        //            if (tableType.ContainsKey(TableSize.TEN) && tableType[TableSize.TEN].Count > 0)
//        //            {
//        //                result.AddRange(tableType[TableSize.TEN]);
//        //                return result;
//        //            }
//        //        }
//        //        return result;
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        result = new List<Table>();
//        //    }
//        //    return result;

//        //}

//        //public async Task<AppActionResult> GetAllReservation(int? time, ReservationStatus? status, int pageNumber, int pageSize)
//        //{
//        //    AppActionResult result = new AppActionResult();
//        //    try
//        //    {
//        //        var conditions = new List<Func<Expression<Func<Reservation, bool>>>>();

//        //        if (time.HasValue)
//        //        {
//        //            //This week
//        //            if (time == 0)
//        //            {
//        //                var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
//        //                var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1);

//        //                // Add the condition to the list
//        //                conditions.Add(() => r =>
//        //                    r.ReservationDate >= startOfWeek && r.ReservationDate <= endOfWeek);
//        //            }
//        //        }

//        //        if (status.HasValue)
//        //        {
//        //            conditions.Add(() => r => r.StatusId == status.Value);
//        //        }

//        //        Expression expression = DynamicLinqBuilder<Reservation>.BuildExpression(conditions);
//        //        result.Result = await _reservationRepository.GetAllDataByExpression((Expression<Func<Reservation, bool>>?)expression, pageNumber, pageSize, r => r.ReservationDate, true, p => p.CustomerInfo.Account!);

//        //    }
//        //    catch (Exception ex)
//        //    {
//        //    }
//        //    return result;
//        //}

//        //public async Task<AppActionResult> GetAllReservationByAccountId(Guid customerInfoId, ReservationStatus? status, int pageNumber, int pageSize)
//        //{
//        //    AppActionResult result = new AppActionResult();
//        //    try
//        //    {
//        //        if (status.HasValue)
//        //        {
//        //            result.Result = await _reservationRepository.GetAllDataByExpression(o => o.CustomerInfoId.Equals(customerInfoId) && o.StatusId == status, pageNumber, pageSize, o => o.ReservationDate, false, null);
//        //        }
//        //        else
//        //        {
//        //            result.Result = await _reservationRepository.GetAllDataByExpression(o => o.CustomerInfoId.Equals(customerInfoId), pageNumber, pageSize, o => o.ReservationDate, false, null);
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        result = BuildAppActionResultError(result, ex.Message);
//        //    }
//        //    return result;
//        //}

//        //public async Task<AppActionResult> GetAllReservationDetail(Guid reservationId)
//        //{
//        //    try
//        //    {
//        //        var reservation = await _reservationRepository.GetByExpression(r => r.ReservationId == reservationId, r => r.CustomerInfo);
//        //        if (reservation == null)
//        //        {
//        //            return BuildAppActionResultError(new AppActionResult(), $"Không tìm thấy thông tin đặt bàn với id {reservationId}");
//        //        }

//        //        var reservationTableDetails = await GetReservationTableDetails(reservationId);
//        //        var reservationDishes = await GetReservationDishes(reservationId);

//        //        var data = new ReservationReponse
//        //        {
//        //            Reservation = reservation,
//        //            ReservationTableDetails = reservationTableDetails,
//        //            ReservationDishes = reservationDishes
//        //        };

//        //        return new AppActionResult { Result = data };
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        return BuildAppActionResultError(new AppActionResult(), ex.Message);
//        //    }
//        //}

//        //private async Task<List<TableDetail>> GetReservationTableDetails(Guid reservationId)
//        //{
//        //    var reservationTableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
//        //    var result = await reservationTableDetailRepository!.GetAllDataByExpression(
//        //        o => o.ReservationId == reservationId,
//        //        0, 0, null, false,
//        //        o => o.Table
//        //    );
//        //    return result.Items!;
//        //}

//        //private async Task<List<Common.DTO.Response.ReservationDishDto>> GetReservationDishes(Guid reservationId)
//        //{
//        //    var reservationDishDb = await _reservationDishRepository!.GetAllDataByExpression(
//        //        o => o.ReservationId == reservationId,
//        //        0, 0, null, false,
//        //        o => o.DishSizeDetail.Dish,
//        //        o => o.DishSizeDetail.DishSize,
//        //        o => o.Combo
//        //    );

//        //    var reservationDishes = new List<Common.DTO.Response.ReservationDishDto>();
//        //    var comboOrderDetailRepository = Resolve<IGenericRepository<ComboOrderDetail>>();

//        //    foreach (var r in reservationDishDb.Items)
//        //    {
//        //        if (r.Combo != null)
//        //        {
//        //            var comboDishDto = await CreateComboDishDto(r, comboOrderDetailRepository);
//        //            reservationDishes.Add(new Common.DTO.Response.ReservationDishDto
//        //            {
//        //                ReservationDishId = r.ReservationDishId,
//        //                ComboDish = comboDishDto
//        //            });
//        //        }
//        //        else
//        //        {
//        //            reservationDishes.Add(new Common.DTO.Response.ReservationDishDto
//        //            {
//        //                ReservationDishId = r.ReservationDishId,
//        //                DishSizeDetailId = r.DishSizeDetailId,
//        //                DishSizeDetail = r.DishSizeDetail
//        //            });
//        //        }
//        //    }

//        //    return reservationDishes;
//        //}

//        //private async Task<ComboDishDto> CreateComboDishDto(ReservationDish r, IGenericRepository<ComboOrderDetail> comboOrderDetailRepository)
//        //{
//        //    var comboOrderDetails = await comboOrderDetailRepository!.GetAllDataByExpression(
//        //        d => d.ReservationDishId == r.ReservationDishId,
//        //        0, 0, null, false,
//        //        d => d.DishCombo.DishSizeDetail!.Dish,
//        //        d => d.DishCombo.DishSizeDetail.DishSize
//        //    );

//        //    return new ComboDishDto
//        //    {
//        //        ComboId = r.Combo.ComboId,
//        //        Combo = r.Combo,
//        //        DishCombos = comboOrderDetails.Items.Select(d => d.DishCombo).ToList()
//        //    };
//        //}
//        //public async Task<AppActionResult> UpdateReservationStatus(Guid reservationId, ReservationStatus status)
//        //{
//        //    AppActionResult result = new AppActionResult();
//        //    try
//        //    {
//        //        var transactionService = Resolve<ITransactionService>();
//        //        var transactionRepository = Resolve<IGenericRepository<Domain.Models.Transaction>>();
//        //        var reservationDb = await _reservationRepository.GetById(reservationId);
//        //        if (reservationDb == null)
//        //        {
//        //            return BuildAppActionResultError(result, $"Không tìm thấy yêu cầu đặt bàn với id {reservationId}");
//        //            return result;
//        //        }
//        //        bool updated = false;
//        //        if (reservationDb.StatusId == ReservationStatus.PENDING)
//        //        {
//        //            if (status != ReservationStatus.TABLEASSIGNED && status != ReservationStatus.CANCELLED)
//        //            {
//        //                return BuildAppActionResultError(result, $"Yêu cầu đặt bàn với id {reservationId} chưa được xử lí, không thể diễn ra");
//        //                return result;
//        //            }
//        //            reservationDb.StatusId = status;
//        //            updated = true;
//        //        }
//        //        else if (reservationDb.StatusId == ReservationStatus.TABLEASSIGNED)
//        //        {
//        //            if (status != ReservationStatus.PAID && status != ReservationStatus.CANCELLED)
//        //            {
//        //                return BuildAppActionResultError(result, $"Yêu cầu đặt bàn với id {reservationId} chưa được thanh toán cọc, không thể diễn ra");
//        //                return result;
//        //            }

//        //            if(status == ReservationStatus.PAID)
//        //            {
//        //                var reservationTransactionDb = await transactionRepository.GetByExpression(t => t.ReservationId == reservationId && t.TransationStatusId == TransationStatus.PENDING, null);
//        //                if (reservationTransactionDb == null)
//        //                {
//        //                    return BuildAppActionResultError(result, $"Không tìm thấy giao dịch cho đặt bàn với id {reservationId}");
//        //                    return result;
//        //                }

//        //                var transactionUpdatedSuccessFully = await transactionService.UpdateTransactionStatus(reservationTransactionDb.Id, TransationStatus.SUCCESSFUL);
//        //                if (!transactionUpdatedSuccessFully.IsSuccess)
//        //                {
//        //                    return BuildAppActionResultError(result, $"Cập nhật trạng thái giao dịch {reservationTransactionDb.Id} cho đặt bàn {reservationId} thất bại. Vui lòng cập nhật lại sau");
//        //                }
//        //            }
//        //            reservationDb.StatusId = status;
//        //            updated = true;
//        //        }
//        //        else if (reservationDb.StatusId == ReservationStatus.PAID)
//        //        {
//        //            if (status != ReservationStatus.DINING && status != ReservationStatus.CANCELLED)
//        //            {
//        //                return BuildAppActionResultError(result, $"Yêu cầu đặt bàn với id {reservationId} đã được thanh toán cọc,không thể diễn ra");
//        //                return result;
//        //            }
//        //            reservationDb.StatusId = status;
//        //            updated = true;
//        //        }
//        //        await _reservationRepository.Update(reservationDb);
//        //        await _unitOfWork.SaveChangesAsync();
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        result = BuildAppActionResultError(result, ex.Message);
//        //    }
//        //    return result;
//        //}

//        //public async Task<AppActionResult> GetAllReservationByPhoneNumber(string phoneNumber, Domain.Enums.ReservationStatus? status, int pageNumber, int pageSize)
//        //{
//        //    var result = new AppActionResult();
//        //    try
//        //    {
//        //        if (status.HasValue)
//        //        {
//        //            result.Result = await _reservationRepository.GetAllDataByExpression(o => o.CustomerInfo!.PhoneNumber == phoneNumber && o.StatusId == status, pageNumber, pageSize, o => o.ReservationDate, false, r => r.CustomerInfo);
//        //        }
//        //        else
//        //        {
//        //            result.Result = await _reservationRepository.GetAllDataByExpression(o => o.CustomerInfo!.PhoneNumber == phoneNumber, pageNumber, pageSize, o => o.ReservationDate, false, r => r.CustomerInfo);
//        //        }
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        result = BuildAppActionResultError(result, ex.Message);
//        //    }
//        //    return result;
//        //}

//        //public async Task<AppActionResult> GetTableReservationWithTime(Guid tableId, DateTime? time)
//        //{
//        //    AppActionResult result = new AppActionResult();
//        //    try
//        //    {
//        //        var tableRepository = Resolve<IGenericRepository<Table>>();
//        //        var reservationTableRepository = Resolve<IGenericRepository<TableDetail>>();
//        //        var tableDb = await tableRepository!.GetById(tableId);
//        //        if(tableDb == null)
//        //        {
//        //            return BuildAppActionResultError(result, $"Không tìm thấy bàn với id {tableId}");
//        //        }

//        //        var configurationRepository = Resolve<IGenericRepository<Configuration>>();
//        //        var configDb = await configurationRepository!.GetByExpression(c => c.Name.Equals(SD.DefaultValue.TIME_TO_LOOK_UP_FOR_RESERVATION), null);
//        //        if (configDb == null)
//        //        {
//        //            return BuildAppActionResultError(result, $"Không tìm thấy cấu hình với tên {SD.DefaultValue.TIME_TO_LOOK_UP_FOR_RESERVATION}");
//        //        }

//        //        if (!time.HasValue)
//        //        {
//        //            var utility = Resolve<Utility>();
//        //            time = utility.GetCurrentDateTimeInTimeZone();
//        //        }
//        //        var nearReservationDb = await reservationTableRepository.GetAllDataByExpression(r => r.TableId == tableId
//        //                                                        && r.Reservation.ReservationDate <= time.Value.AddHours(double.Parse(configDb.PreValue))
//        //                                                        && r.Reservation.ReservationDate.AddHours(double.Parse(configDb.PreValue)) >= time, 0, 0, r => r.Reservation.ReservationDate, true, null);
//        //        if(nearReservationDb.Items.Count > 0)
//        //        {
//        //            result = await GetAllReservationDetail(nearReservationDb.Items[0].ReservationId);
//        //        }

//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        result = BuildAppActionResultError(result, ex.Message);
//        //    }
//        //    return result;
//        //}

//        //public async Task<AppActionResult> AddTableToReservation(Guid reservationId, List<Guid> tableIds)
//        //{
//        //    AppActionResult result = new AppActionResult();
//        //    try
//        //    {
//        //        var reservationDb = await _reservationRepository.GetById(reservationId);
//        //        if (reservationDb == null)
//        //        {
//        //            return BuildAppActionResultError(result, $"Không tìm thấy đặt bàn với id {reservationId}");
//        //        }

//        //        var tableRepository = Resolve<IGenericRepository<Table>>();
//        //        var tableDb = await tableRepository.GetAllDataByExpression(t => tableIds.Contains(t.TableId), 0, 0, null, false, null);
//        //        if (tableIds.Count != tableDb.Items.Count)
//        //        {
//        //            return BuildAppActionResultError(result, $"Một số id bàn không tồn tại trong hệ thống. Vui lòng kiểm tra lại");
//        //        }

//        //        var reservationTableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
//        //        var reservationTableDetail = new List<TableDetail>();

//        //        foreach (var tableId in tableIds)
//        //        {
//        //            reservationTableDetail.Add(new TableDetail()
//        //            {
//        //                ReservationId = reservationId,
//        //                TableId = tableId,
//        //                ReservationTableDetailId = Guid.NewGuid()
//        //            });
//        //        }
//        //        reservationDb.StatusId = ReservationStatus.TABLEASSIGNED;
//        //        await reservationTableDetailRepository!.InsertRange(reservationTableDetail);
//        //        await _reservationRepository.Update(reservationDb);
//        //        await _unitOfWork.SaveChangesAsync();
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        return BuildAppActionResultError(result, ex.Message );
//        //    }
//        //    return result;
//        //}

//        //public async Task<AppActionResult> UpdateExpireReservation()
//        //{
//        //    AppActionResult result = new AppActionResult();
//        //    try
//        //    {
//        //        var unPaidReservationDb = await _reservationRepository.GetAllDataByExpression(r => r.StatusId == ReservationStatus.TABLEASSIGNED || r.StatusId == ReservationStatus.PENDING, 0, 0, null, false, null);
//        //        if(unPaidReservationDb.Items.Count > 0)
//        //        {
//        //            var utility = Resolve<Utility>();
//        //            var today = utility.GetCurrentDateInTimeZone();
//        //            var configurationRepository = Resolve<IGenericRepository<Configuration>>();
//        //            var configurationDb = await configurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.TIME_TO_KEEP_RESERVATION), null);
//        //            if(configurationDb == null)
//        //            {
//        //                return BuildAppActionResultError(result, $"Không tìm thấy cấu hình cho {SD.DefaultValue.TIME_TO_KEEP_RESERVATION}");
//        //            }

//        //            var timeToKeepReservation = double.Parse(configurationDb.PreValue);
//        //            var reservationToExpire = unPaidReservationDb.Items.Where(u => u.CreateDate.AddHours(timeToKeepReservation) <= today).ToList();
//        //            reservationToExpire.ForEach(r => r.StatusId = ReservationStatus.CANCELLED);
//        //            await _reservationRepository.UpdateRange(reservationToExpire);
//        //            await _unitOfWork.SaveChangesAsync();
//        //        }
//        //    } catch(Exception ex)
//        //    {
//        //        result = BuildAppActionResultError(result, ex.Message);
//        //    }
//        //    return result;
//        //}

//        //public async Task CancelOverdueReservations()
//        //{
//        //    var utility = Resolve<Utility>();
//        //    try
//        //    {
//        //        var currentTime = utility!.GetCurrentDateTimeInTimeZone().AddHours(24);
//        //        var pastReservationDb = await _reservationRepository.GetAllDataByExpression(
//        //            (p => p.ReservationDate.AddHours(24) < currentTime && (p.StatusId == ReservationStatus.PENDING || p.StatusId == ReservationStatus.TABLEASSIGNED)), 0, 0, null, false, null);
//        //        if (pastReservationDb!.Items!.Count > 0 && pastReservationDb.Items != null)
//        //        {
//        //            foreach (var reservation in pastReservationDb.Items)
//        //            {
//        //                reservation.StatusId = ReservationStatus.CANCELLED;
//        //                await _reservationRepository.Update(reservation);
//        //            }
//        //        }
//        //        await _unitOfWork.SaveChangesAsync();
//        //    }
//        //    catch(Exception ex)
//        //    {
//        //        _logger.LogError(ex.Message, this);
//        //    }
//        //    Task.CompletedTask.Wait();
//        //}
//    }
//}