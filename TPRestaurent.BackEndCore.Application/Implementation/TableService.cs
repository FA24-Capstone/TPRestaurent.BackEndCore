using AutoMapper;
using Castle.Core.Internal;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using TPRestaurent.BackEndCore.Application.Contract.IServices;
using TPRestaurent.BackEndCore.Application.IRepositories;
using TPRestaurent.BackEndCore.Common.DTO.Request;
using TPRestaurent.BackEndCore.Common.DTO.Response.BaseDTO;
using TPRestaurent.BackEndCore.Common.Utils;
using TPRestaurent.BackEndCore.Domain.Enums;
using TPRestaurent.BackEndCore.Domain.Models;

namespace TPRestaurent.BackEndCore.Application.Implementation
{
    public class TableService : GenericBackendService, ITableService
    {
        private readonly IGenericRepository<Table> _repository;
        private readonly IUnitOfWork _unitOfWork;
        private IMapper _mapper;
        public TableService(IGenericRepository<Table> repository, IUnitOfWork unitOfWork, IMapper mapper, IServiceProvider service) : base(service) 
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AppActionResult> CreateTable(TableDto dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var congfigurationRepository = Resolve<IGenericRepository<Configuration>>();
                //Name validation (if needed)
                if (string.IsNullOrEmpty(dto.TableName)) {
                    result = BuildAppActionResultError(result, "Tên bàn không được để trống");
                    return result;
                }

                if ((int)dto.TableSizeId < 1)
                {
                    result = BuildAppActionResultError(result, "Số ghế ngồi phải lớn hơn 0");
                    return result;
                }

                var tableRatingRepository = Resolve<IGenericRepository<Room>>();
                if((await tableRatingRepository.GetById(dto.TableRatingId) == null))
                {
                    result = BuildAppActionResultError(result, $"Không tìm thấy phòng với id {dto.TableRatingId}");
                    return result;
                }

                var newTable = _mapper.Map<Table>(dto);
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.DevicePassword);
                newTable.TableId = Guid.NewGuid();
                newTable.IsDeleted = false;
                newTable.DeviceCode = dto.DeviceCode;
                newTable.DevicePassword = hashedPassword;
                newTable.TableStatusId = TableStatus.NEW;
                newTable.Coordinates = "(0,0)";
                await _repository.Insert(newTable);

                var tableSetUpConfig = await congfigurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.TABLE_IS_SET_UP), null);
                if (tableSetUpConfig != null)
                {
                    tableSetUpConfig.CurrentValue = SD.DefaultValue.NEW;
                    await congfigurationRepository.Update(tableSetUpConfig);
                }

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> FindTable(FindTableDto dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var orderService = Resolve<IOrderService>();
                var availableTables = await GetAvailableTable(dto.StartTime, dto.EndTime, dto.IsPrivate, dto.NumOfPeople, 0, 0);
                if (availableTables.Count == 0)
                {
                    result.Messages.Add("Không tìm thấy bàn cho yêu cầu đặt bàn");
                    return result;
                }
                var bestCaseFindTableResult = await FindTableWithCase(dto, availableTables, false);
                if(bestCaseFindTableResult.IsSuccess && bestCaseFindTableResult.Result != null)
                {
                    result.Result = bestCaseFindTableResult.Result as List<TableArrangementResponseItem>;
                    return result;
                }

                var badCaseFindTableResult = await FindTableWithCase(dto, availableTables, true);
                if (badCaseFindTableResult.IsSuccess && badCaseFindTableResult.Result != null)
                {
                    result.Result = badCaseFindTableResult.Result as List<TableArrangementResponseItem>;
                    if(badCaseFindTableResult.Messages.Count > 0)
                    {
                        result.Messages.AddRange(badCaseFindTableResult.Messages);
                    }
                    return result;
                }
                var bestCaseFindTableResult = await FindTableWithCase(dto, availableTables, false);
                if(bestCaseFindTableResult.IsSuccess && bestCaseFindTableResult.Result != null)
                {
                    result.Result = bestCaseFindTableResult.Result as List<TableArrangementResponseItem>;
                    return result;
                }

                var badCaseFindTableResult = await FindTableWithCase(dto, availableTables, true);
                if (badCaseFindTableResult.IsSuccess && badCaseFindTableResult.Result != null)
                {
                    result.Result = badCaseFindTableResult.Result as List<TableArrangementResponseItem>;
                    if(badCaseFindTableResult.Messages.Count > 0)
                    {
                        result.Messages.AddRange(badCaseFindTableResult.Messages);
                    }
                    return result;
                }
                result.Messages.Add($"Nhà hàng không xếp bàn cho quý khách. Xin lỗi và xin hẹn quý khách lần sau");
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message );
            }
            return result;
        }

        private async Task<AppActionResult> FindTableWithCase(FindTableDto dto, List<Table> availableTables, bool allowAddChair)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var data = new List<TableArrangementResponseItem>();
                int[] sizes = null;
                if (allowAddChair)
                {
                   sizes = new int[]{ 2, 4, 6, 9, 11 }; // Possible sizes
                }
                else
                {
                    sizes = new int[]{ 2, 4, 6, 8, 10 }; // Possible sizes
                }
                int target = dto.NumOfPeople + 2;
                List<List<int>> possibleTableSet = new List<List<int>>();
                await Backtrack(possibleTableSet, new List<int>(), sizes, dto.NumOfPeople, 0, target);

                // Filter out subsets that are not optimal
                possibleTableSet = await FilterOptimalSubsets(possibleTableSet, dto.NumOfPeople);
                var tableSizeDictionary = availableTables.GroupBy(a => (int)a.TableSizeId).ToDictionary(a => a.Key, a => a.Count());
                var adjustedTableSizeDictionary = new Dictionary<int, int>();
                if (allowAddChair)
                {
                    foreach (var table in tableSizeDictionary)
                    {
                        if (table.Key == 8 || table.Key == 10)
                        {
                            adjustedTableSizeDictionary.Add(table.Key + 1, table.Value);
                        }
                        else
                        {
                            adjustedTableSizeDictionary.Add(table.Key, table.Value);
                        }
                    }
                }
                else
                {
                    adjustedTableSizeDictionary = tableSizeDictionary;
                }
                possibleTableSet = await FilterAvailableQuantity(possibleTableSet, adjustedTableSizeDictionary);
                foreach (var possibleTable in possibleTableSet)
                {
                    for (int i = 0; i < possibleTable.Count; i++)
                    {
                        if (possibleTable[i] == 9)
                        {
                            possibleTable[i] = 8;
                        }
                        else if (possibleTable[i] == 11)
                        {
                            possibleTable[i] = 10;
                        }
                    }
                    var tableList = await FindBestTables(new List<Table>(availableTables), possibleTable);
                    if (tableList != null && tableList.Count > 0)
                    {
                        int tableSizeCheck = 0;
                        foreach (var table in tableList)
                        {
                            var tableResponse = _mapper.Map<TableArrangementResponseItem>(table);
                            List<(int, int)> tableCoordinates = DeserializeList(table.Coordinates);
                            if (tableCoordinates.Count > 0)
                            {
                                tableResponse.Position.X = tableCoordinates.FirstOrDefault().Item1;
                                tableResponse.Position.Y = tableCoordinates.FirstOrDefault().Item2;
                            }
                            data.Add(tableResponse);
                            tableSizeCheck += (int)table.TableSizeId;
                        }
                        result.Result = data;
                        if (tableSizeCheck < dto.NumOfPeople)
                        {
                            result.Messages.Add("Lịch đặt bàn hiện tại đang khá dày nên nhà hàng sẽ phải kê thêm ghế cho quý khách");
                        }

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }
        private async Task<List<List<int>>> FilterAvailableQuantity(List<List<int>> possibleTableSet, Dictionary<int, int> dictionary)
        {
            List<List<int>> reducedList = new List<List<int>>();
            try
            {
                bool isValid = true;
                foreach (var possibleTable in possibleTableSet)
                {
                    isValid = true;
                    var dictionaryKey = dictionary.Select(x => x.Key).OrderBy(x => x).ToList();
                    var possibleTableKey = possibleTable.Distinct().OrderBy(x => x).ToList();
                    if (dictionaryKey.All(possibleTableKey.Contains))
                    {
                        continue;
                    }
                    foreach (var kvp in dictionary)
                    {
                        if (possibleTable.Count(p => p == kvp.Key) > kvp.Value)
                        {
                            isValid = false;
                            break;
                        }
                    }
                    if (isValid)
                    {
                        reducedList.Add(possibleTable);
                    }
                }
            }
            catch (Exception ex)
            {
                return possibleTableSet;
            }
            return reducedList;
        }

        private async Task Backtrack(List<List<int>> result, List<int> current, int[] sizes, int number, int start, int target)
        {
            int sum = current.Sum();

            // If the sum is within the acceptable range, add to the result
            if (sum >= number && sum <= target)
            {
                result.Add(new List<int>(current));
                return;
            }

            // Try to add each size to the current subset
            for (int i = start; i < sizes.Length; i++)
            {
                if (sum + sizes[i] > target)
                {
                    break;
                }
                current.Add(sizes[i]);
                Backtrack(result, current, sizes, number, i, target);
                current.RemoveAt(current.Count - 1); // Backtrack step
            }
        }

        private async Task<List<List<int>>> FilterOptimalSubsets(List<List<int>> subsets, int NumOfPeople)
        {
            List<List<int>> optimalSubsets = new List<List<int>>();

            foreach (var subset in subsets)
            {
                bool isOptimal = true;

                // Check if any element can be removed while still satisfying the condition
                foreach (var element in subset)
                {
                    var reducedSubset = new List<int>(subset);
                    reducedSubset.Remove(element);

                    int reducedSum = reducedSubset.Sum();
                    if (reducedSum > subset.Sum() - element)
                    {
                        isOptimal = false;
                        break;
                    }
                }

                if (isOptimal)
                {
                    optimalSubsets.Add(subset);
                }
            }

            return optimalSubsets.OrderBy(o => Math.Abs(o.Sum(i => i) - NumOfPeople)).ThenBy(o => o.Count()).ThenBy(o =>
            {
                double mean = o.Average();
                return o.Average(or => Math.Pow(or - mean, 2));
            }).ToList();
        }

        private async Task<List<Table>> FindBestTables(List<Table> tables, List<int> requestedSizes)
        {
            List<Table> bestCombination = null;
            Backtrack(new List<Table>(tables.OrderBy(t => t.Coordinates)), requestedSizes.OrderBy(r => r).ToList(), new List<Table>(), ref bestCombination, 0);
            return bestCombination;
        }

        private void Backtrack(List<Table> tables, List<int> sizes, List<Table> currentTables, ref List<Table> bestCombination, int startIndex)
        {
            // If all requested sizes are used, check if this combination is valid
            if (sizes.Count == 0)
            {
                if (bestCombination == null || currentTables.Count < bestCombination.Count)
                {
                    bestCombination = new List<Table>(currentTables);
                }
                return;
            }

            // Try to find the best table of the current requested size
            int maxProximity = -1;
            Table bestTable = null;

            for (int i = startIndex; i < tables.Count; i++)
            {
                int tableSize = (int)tables[i].TableSizeId; // Each cell represents 2 units

                if (tableSize == sizes[0])
                {
                    int proximity = CalculateProximity(currentTables, tables[i]);

                    // If this table is closer to the existing tables, update the best choice
                    if (proximity > maxProximity)
                    {
                        maxProximity = proximity;
                        bestTable = tables[i];
                    }
                }
            }

            // If a best table is found, proceed with backtracking
            if (bestTable != null)
            {
                currentTables.Add(bestTable);
                Backtrack(tables.Where(t => t.TableId != bestTable.TableId).ToList(), sizes.Skip(1).ToList(), currentTables, ref bestCombination, startIndex);
                currentTables.RemoveAt(currentTables.Count - 1); // Backtrack step
            }
        }

        private int CalculateProximity(List<Table> currentTables, Table newTable)
        {
            int proximity = 0;

            foreach (var currentTable in currentTables)
            {
                foreach (var cell in DeserializeList(currentTable.Coordinates))
                {
                    foreach (var newCell in DeserializeList(newTable.Coordinates))
                    {
                        // Check if cells are adjacent (horizontally or vertically)
                        if (Math.Abs(cell.Item1 - newCell.Item1) + Math.Abs(cell.Item2 - newCell.Item2) == 1)
                        {
                            proximity++;
                        }
                    }
                }
            }
            return proximity;
        }

        public async Task<List<Table>> GetAvailableTable(DateTime startTime, DateTime? endTime, bool isPrivate, int? numOfPeople, int pageNumber, int pageSize)
        {
            List<Table> result = new List<Table>();
            try
            {
                var _orderRepository = Resolve<IGenericRepository<Order>>();
                var _configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var conditions = new List<Func<Expression<Func<Order, bool>>>>();

                // !(endTime < r.ReservationDate || r.EndTime < startTime)
                var configurationDb = await _configurationRepository.GetAllDataByExpression(c => c.Name.Equals(SD.DefaultValue.AVERAGE_MEAL_DURATION), 0, 0, null, false, null);
                if (configurationDb.Items.Count == 0 || configurationDb.Items.Count > 1)
                {
                    return new List<Table>();
                }
                if (!endTime.HasValue)
                {

                    endTime = startTime.AddHours(double.Parse(configurationDb.Items[0].CurrentValue));
                }

                conditions.Add(() => r => !(endTime < r.ReservationDate || (r.EndTime.HasValue && r.EndTime.Value < startTime || !r.EndTime.HasValue && r.ReservationDate.Value.AddHours(double.Parse(configurationDb.Items[0].CurrentValue)) < startTime))
                                          && r.StatusId != OrderStatus.Cancelled);

                Expression<Func<Order, bool>> expression = r => true; // Default expression to match all

                if (conditions.Count > 0)
                {
                    expression = DynamicLinqBuilder<Order>.BuildExpression(conditions);
                }

                // Get all collided reservations
                var unavailableReservation = await _orderRepository.GetAllDataByExpression(expression, pageNumber, pageSize, null, false, null);
                if (unavailableReservation!.Items.Count > 0)
                {
                    var unavailableReservationIds = unavailableReservation.Items.Select(x => x.OrderId);
                    var reservationTableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                    var reservedTableDb = await reservationTableDetailRepository!.GetAllDataByExpression(r => unavailableReservationIds.Contains(r.OrderId), 0, 0, null, false, r => r.Table.Room);
                    var reservedTableIds = reservedTableDb.Items!.Select(x => x.TableId);
                    var availableTableDb = await _repository!.GetAllDataByExpression(t => !reservedTableIds.Contains(t.TableId) && t.Room.IsPrivate == isPrivate && !t.IsDeleted, 0, 0, null, false, t => t.Room);
                    result = availableTableDb.Items;
                }
                else
                {
                    result = (await _repository!.GetAllDataByExpression(t => t.Room.IsPrivate == isPrivate && !t.IsDeleted, 0, 0, null, false, r => r.Room)).Items;
                }
                //result.Result = availableReservation.Items.Select(x => x.Table);
            }
            catch (Exception ex)
            {
                result = new List<Table>();
            }
            return result;
        }
        private List<(int, int)> DeserializeList(string str)
        {
            // Split the string by ';' and convert each substring to a tuple
            if (string.IsNullOrEmpty(str))
            {
                return new List<(int, int)>();
            }
            return str.Split(';')
                      .Select(s => s.Trim('(', ')')) // Remove parentheses
                      .Select(s => s.Split(','))    // Split by ','
                      .Select(arr => (int.Parse(arr[0]), int.Parse(arr[1])))
                      .ToList();
        }

        public async Task<AppActionResult> GetAllTable(int pageNumber, int pageSize)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var tableDb = await _repository.GetAllDataByExpression(t => !t.IsDeleted, pageNumber, pageSize, null, false, t => t.Room, t => t.TableSize);
                if (tableDb.Items.Count > 0)
                {
                    var data = new List<TableArrangementResponseItem>();
                    var unavailableTableIds = await GetUnavailableTableIds();
                    foreach (var item in tableDb.Items)
                    {
                        var tableResponse = _mapper.Map<TableArrangementResponseItem>(item);
                        List<(int, int)> tableCoordinates = DeserializeList(item.Coordinates);
                        if (tableCoordinates.Count > 0)
                        {
                            tableResponse.Position.X = tableCoordinates.FirstOrDefault().Item1;
                            tableResponse.Position.Y = tableCoordinates.FirstOrDefault().Item2;
                        }

                        if(item.TableStatusId == TableStatus.NEW)
                        {
                            tableResponse.TableStatusId = TableStatus.NEW;
                        }
                        else if (unavailableTableIds.Contains(tableResponse.Id))
                        {
                            tableResponse.TableStatusId = TableStatus.CURRENTLYUSED;
                        } else
                        {
                            tableResponse.TableStatusId = TableStatus.AVAILABLE;
                        }



                        data.Add(tableResponse);
                    }



                    result.Result = new PagedResult<TableArrangementResponseItem>
                    {
                        Items = data,
                        TotalPages = tableDb.TotalPages
                    };
                }
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> UpdateTableCoordinates(List<TableArrangementResponseItem> request, bool? isForce = false)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var congfigurationRepository = Resolve<IGenericRepository<Configuration>>();
                var tableSetUpConfig = await congfigurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.TABLE_IS_SET_UP), null);
                if(!isForce.HasValue || !isForce.Value)
                {
                    if (tableSetUpConfig != null)
                    {
                        if (tableSetUpConfig.CurrentValue.Equals(SD.DefaultValue.IS_SET_UP))
                        {
                            return BuildAppActionResultError(result, $"Sơ đồ bàn đã được thiết lập từ trước. Nếu thay đổi có ảnh hưởng đến lịch đặt bàn sau này");
                        }
                    }
                }
                var tableIds = request.Select(r => r.Id).ToList();
                var tableDb = await _repository.GetAllDataByExpression(r => tableIds.Contains(r.TableId) && !r.IsDeleted, 0, 0, null, false, null);
                if (tableIds.Count != tableDb.Items.Count)
                {
                    return BuildAppActionResultError(result, $"Danh sách chứa id bàn không tồn tại");
                }
                foreach (var table in tableDb.Items)
                {
                    if(table.TableStatusId == TableStatus.NEW)
                    {
                        table.TableStatusId = TableStatus.AVAILABLE;
                    }
                    var inputTable = request.FirstOrDefault(i => i.Id == table.TableId);
                    List<(int, int)> coordinate = new List<(int, int)>();
                    coordinate.Add((inputTable.Position.X, inputTable.Position.Y));
                    if(inputTable.TableSizeId != TableSize.EIGHT && inputTable.TableSizeId != TableSize.TEN)
                    {
                        for (int i = 1; i < (int)inputTable.TableSizeId / 2; i++)
                        {
                            coordinate.Add((inputTable.Position.X, inputTable.Position.Y + i));
                        }
                    } else
                    {
                        if(inputTable.TableSizeId == TableSize.TEN)
                        {
                            coordinate.Add((inputTable.Position.X, inputTable.Position.Y + 1));
                            coordinate.Add((inputTable.Position.X, inputTable.Position.Y + 2));
                            coordinate.Add((inputTable.Position.X, inputTable.Position.Y + 3));

                            coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y));
                            coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y + 1));
                            coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y + 2));
                            coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y + 3));
                        } else
                        {
                            coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y));

                            coordinate.Add((inputTable.Position.X, inputTable.Position.Y + 1));
                            coordinate.Add((inputTable.Position.X + 1, inputTable.Position.Y + 1));
                        }
                    }
                    table.TableSizeId = inputTable.TableSizeId; 
                    table.Coordinates = ParseListToString(coordinate);  
                }
                if(tableSetUpConfig != null)
                {
                    tableSetUpConfig.CurrentValue = SD.DefaultValue.IS_SET_UP;
                    await congfigurationRepository.Update(tableSetUpConfig);
                }
                await _repository.UpdateRange(tableDb.Items);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
            return result;
        }

        private string ParseListToString(List<(int, int)> list)
        {
            // Convert the list of tuples to a string format "(x,y);(x,y);..."
            return string.Join(";", list.Select(tuple => $"({tuple.Item1},{tuple.Item2})"));
        }

        public async Task<List<Guid>> GetUnavailableTableIds()
        {
            List<Guid> result = new List<Guid>();
            try
            {
                var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                var configurationRepository = Resolve<IGenericRepository<Configuration>>();
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var configurationDb = await configurationRepository.GetByExpression(c => c.Name.Equals(SD.DefaultValue.AVERAGE_MEAL_DURATION), null);
                var averageTime = double.Parse(configurationDb.CurrentValue);
                var unavailableDetailDb = await tableDetailRepository.GetAllDataByExpression(t => t.Order.OrderTypeId != OrderType.Delivery
                                                                                                  && t.Order.MealTime <= currentTime
                                                                                                  && (t.Order.EndTime.HasValue && t.Order.EndTime.Value >= currentTime
                                                                                                      || !t.Order.EndTime.HasValue && t.Order.MealTime.Value.AddHours(averageTime) >= currentTime), 
                                                                                                  0, 0, null, false, null);

                result = unavailableDetailDb.Items.DistinctBy(u => u.TableId).Select(u => u.TableId).ToList();
            }
            catch (Exception ex)
            {
                result = new List<Guid>();
            }
            return result;
        }

        public async Task<AppActionResult> GetAllTableRating(int pageNumber, int pageSize)
        {
            var result = new AppActionResult();
            var tableRatingRepository = Resolve<IGenericRepository<Room>>();
            try
            {
                result.Result = await tableRatingRepository!.GetAllDataByExpression(null, pageNumber, pageSize, null, false, null);
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> UpdateTable(UpdateTableDto dto)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var congfigurationRepository = Resolve<IGenericRepository<Configuration>>();
                //Name validation (if needed)
                if (string.IsNullOrEmpty(dto.TableName) || string.IsNullOrEmpty(dto.DeviceCode) || string.IsNullOrEmpty(dto.DevicePassword))
                {
                    result = BuildAppActionResultError(result, "Thông tin không được để trống");
                    return result;
                }

                var tableDb = await _repository.GetById(dto.TableId);
                if(tableDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy bàn với id {dto.TableId}");
                }

                tableDb.TableName = dto.TableName;
                tableDb.DeviceCode = dto.DeviceCode;

                if (dto.DevicePassword.Length > 0)
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.DevicePassword);
                    tableDb.DevicePassword = hashedPassword;
                }
                await _repository.Update(tableDb);

                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        public async Task<AppActionResult> DeleteTable(Guid id)
        {
            AppActionResult result = new AppActionResult();
            try
            {
                var tableDb = await _repository.GetById(id);
                if (tableDb == null)
                {
                    return BuildAppActionResultError(result, $"Không tìm thấy bàn với id {id}");
                }
                var tableHasCurrentOrUpcomingReservation = await TableHasCurrentOrUpcomingReservation(id);
                if (tableHasCurrentOrUpcomingReservation)
                {
                    return BuildAppActionResultError(result, $"Bàn đang được sử dụng hoặc có lịch đặt trong tương lai nên không thề xoá");
                }
                tableDb.IsDeleted = true;
                await _repository.Update(tableDb);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                result = BuildAppActionResultError(result, ex.Message);
            }
            return result;
        }

        private async Task<bool> TableHasCurrentOrUpcomingReservation(Guid id)
        {
            bool tableHasCurrentOrUpcomingReservation = false;
            try
            {
                var tableDetailRepository = Resolve<IGenericRepository<TableDetail>>();
                var utility = Resolve<Utility>();
                var currentTime = utility.GetCurrentDateTimeInTimeZone();
                var tableDetailDb = await tableDetailRepository.GetAllDataByExpression(t => t.TableId == id &&
                                                                                          t.Order.MealTime.Value >= currentTime,
                                                                                          0, 0, null, false, null);
                tableHasCurrentOrUpcomingReservation = tableDetailDb.Items.Count() > 0;
            }
            catch (Exception ex)
            {
                tableHasCurrentOrUpcomingReservation= true;
            }
            return tableHasCurrentOrUpcomingReservation;
        }
    }
}